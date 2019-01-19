using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.CachingService;
using Remit.Web.Helpers;
using System.Xml;
using Remit.ClientModel;

namespace Remit.Web.Controllers
{
    public class RoleSubModuleItemController : Controller
    {
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IModuleService moduleService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:roleSubModuleItem" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /RoleSubModuleItem/
        public ActionResult Index()
        {
            var url = Request.RawUrl;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:roleSubModuleItem" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("RoleSubModuleItem");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");  
        }

        public RoleSubModuleItemController(IRoleSubModuleItemService roleSubModuleItemService, ISubModuleItemService subModuleItemService, IModuleService moduleService)
        {
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.subModuleItemService = subModuleItemService;
            this.moduleService = moduleService;
        }

        [HttpPost]
        public JsonResult CreateRoleSubModuleItem(List<RoleSubModuleItem> roleSubModuleItemlist)
        {
            const string url = "/RoleSubModuleItem/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);
            
            var isSuccess = false;
            var message = string.Empty;

            List<int> moduleIdList = new List<int>();

            if (permission.UpdateOperation == true)
            {
                foreach (var itemList in roleSubModuleItemlist)
                {
                    var roleSubModuleItem = this.roleSubModuleItemService.GetRoleSubModuleItem(itemList.Id);
                    if (roleSubModuleItem != null && roleSubModuleItem.RoleId == itemList.RoleId)
                    {
                        roleSubModuleItem.CreateOperation = itemList.CreateOperation;
                        roleSubModuleItem.DeleteOperation = itemList.DeleteOperation;
                        roleSubModuleItem.ReadOperation = itemList.ReadOperation;
                        roleSubModuleItem.UpdateOperation = itemList.UpdateOperation;
                        if (this.roleSubModuleItemService.UpdateRoleSubModuleItem(roleSubModuleItem))
                        {
                            isSuccess = true;
                        }
                    }
                    else
                    {
                        if (this.roleSubModuleItemService.CreateRoleSubModuleItem(itemList))
                        {
                            isSuccess = true;
                        }
                    }
                }
                message = string.Format(isSuccess ? Resources.ResourceCommon.CMsg_update : Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceModule.LblRoleSubModuleItem);
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
            }

            if (roleSubModuleItemlist != null && roleSubModuleItemlist.Count() > 0)
            {
                int roleId = (int)roleSubModuleItemlist[0].RoleId;
                ClearPermissionCache(roleId);
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        private void ClearPermissionCache(int roleId)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Server.MapPath("~/CacheKeys/CacheKeys.xml"));
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                cacheProvider.Invalidate(node.Attributes["name"].Value + roleId);
            }
            cacheProvider.Invalidate("module" + roleId);
            var moduleIdList = this.moduleService.GetAllModuleByRoleId(roleId).Select(a => a.Id).Distinct();
            foreach (var moduleId in moduleIdList)
            {
                cacheProvider.Invalidate("submodule" +moduleId.ToString()+ roleId);
            }
        }

        [HttpPost]
        public JsonResult DeleteRoleSubModuleItem(RoleSubModuleItem roleSubModuleItem)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/RoleSubModuleItem/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.roleSubModuleItemService.DeleteRoleSubModuleItem(roleSubModuleItem.Id);
                if (isSuccess)
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_delete, Resources.ResourceModule.LblRoleSubModuleItem);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceModule.LblRoleSubModuleItem);
                }
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToDelete;
            }
            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRoleSubModuleItemList()
        {
            var roleSubModuleItemListObj = this.roleSubModuleItemService.GetAllRoleSubModuleItem();
            List<RoleSubModuleItemModel> roleSubModuleItemVMList = new List<RoleSubModuleItemModel>();

            foreach (var roleSubModuleItem in roleSubModuleItemListObj)
            {
                RoleSubModuleItemModel roleSubModuleItemTemp = new RoleSubModuleItemModel();
                roleSubModuleItemTemp.Id = roleSubModuleItem.Id;
                roleSubModuleItemTemp.RoleId = roleSubModuleItem.RoleId;
                if (roleSubModuleItem.RoleId != null)
                    roleSubModuleItemTemp.RoleIdName = roleSubModuleItem.Role.Name;

                roleSubModuleItemTemp.SubModuleItemId = roleSubModuleItem.SubModuleItemId;
                if (roleSubModuleItem.SubModuleItemId != null) 
                    roleSubModuleItemTemp.SubModuleItemIdName = roleSubModuleItem.SubModuleItem.Name;

                if (roleSubModuleItem.SubModuleItem != null)
                {
                    if (roleSubModuleItem.SubModuleItem.SubModule != null) 
                    {
                        roleSubModuleItemTemp.SubModuleId = roleSubModuleItem.SubModuleItem.SubModule.Id;
                        roleSubModuleItemTemp.ModuleId = roleSubModuleItem.SubModuleItem.SubModule.ModuleId.Value;
                    }
                }
                
                roleSubModuleItemTemp.CreateOperation = roleSubModuleItem.CreateOperation;
                if (roleSubModuleItem.CreateOperation == true)
                    roleSubModuleItemTemp.CreateOperationName = "True";
                else
                    roleSubModuleItemTemp.CreateOperationName = "False";

                roleSubModuleItemTemp.ReadOperation = roleSubModuleItem.ReadOperation;
                if (roleSubModuleItem.ReadOperation == true)
                    roleSubModuleItemTemp.ReadOperationName = "True";
                else
                    roleSubModuleItemTemp.ReadOperationName = "False";

                roleSubModuleItemTemp.DeleteOperation = roleSubModuleItem.DeleteOperation;
                if (roleSubModuleItem.DeleteOperation == true)
                    roleSubModuleItemTemp.DeleteOperationName = "True";
                else
                    roleSubModuleItemTemp.DeleteOperationName = "False";

                roleSubModuleItemTemp.UpdateOperation = roleSubModuleItem.UpdateOperation;
                if (roleSubModuleItem.UpdateOperation == true)
                    roleSubModuleItemTemp.UpdateOperationName = "True";
                else
                    roleSubModuleItemTemp.UpdateOperationName = "False";

                roleSubModuleItemVMList.Add(roleSubModuleItemTemp);
            }
            return Json(roleSubModuleItemVMList, JsonRequestBehavior.AllowGet);
        }


        private string GetMenuResourceValueByDatabaseId(string resourceName)
        {
            try
            {
                return HttpContext.GetGlobalResourceObject("ResourceMenu", resourceName).ToString();
            }
            catch (Exception e)
            {
                //
            }
            return null;
        }



        public JsonResult GetRoleSubModuleItemByRole(int id)
        {
            var roleSubModuleItemListObj = roleSubModuleItemService.GetRoleSubModuleItemByRole(id);
            List<RoleSubModuleItemModel> roleSubModuleItemVMList = new List<RoleSubModuleItemModel>();

            var subModuleItemListObj = this.subModuleItemService.GetAllBaseSubModuleItem();
            foreach (var subModuleItem in subModuleItemListObj)
            {
                var roleSubModuleItem=roleSubModuleItemListObj.FirstOrDefault(rsm=>rsm.SubModuleItemId==subModuleItem.Id);
                RoleSubModuleItemModel roleSubModuleItemTemp = new RoleSubModuleItemModel();
                
                if (roleSubModuleItem != null) 
                {
                    roleSubModuleItemTemp.Id = roleSubModuleItem.Id;
                    roleSubModuleItemTemp.RoleId = roleSubModuleItem.RoleId;
                    roleSubModuleItemTemp.SubModuleItemId = roleSubModuleItem.SubModuleItemId;
                    if (roleSubModuleItem.SubModuleItemId != null)
                    {
                        roleSubModuleItemTemp.SubModuleItemIdName = roleSubModuleItem.SubModuleItem.Name;// GetMenuResourceValueByDatabaseId(roleSubModuleItem.SubModuleItem.Name);
                    }
                    if (roleSubModuleItem.SubModuleItem != null)
                    {
                        if (roleSubModuleItem.SubModuleItem.SubModule != null)
                        {
                            roleSubModuleItemTemp.SubModuleId = roleSubModuleItem.SubModuleItem.SubModule.Id;
                            roleSubModuleItemTemp.ModuleId = roleSubModuleItem.SubModuleItem.SubModule.ModuleId.Value;
                        }
                    }
                    roleSubModuleItemTemp.CreateOperation = roleSubModuleItem.CreateOperation;
                    roleSubModuleItemTemp.ReadOperation = roleSubModuleItem.ReadOperation;
                    roleSubModuleItemTemp.DeleteOperation = roleSubModuleItem.DeleteOperation;
                    roleSubModuleItemTemp.UpdateOperation = roleSubModuleItem.UpdateOperation;
                }
                else
                {
                    //roleSubModuleItemTemp.Id = roleSubModuleItem.Id;
                    roleSubModuleItemTemp.RoleId = id;
                    roleSubModuleItemTemp.SubModuleItemId = subModuleItem.Id;
                    if (roleSubModuleItemTemp.SubModuleItemId != null)
                    {
                        roleSubModuleItemTemp.SubModuleItemIdName = subModuleItem.Name;
                    }

                    roleSubModuleItemTemp.SubModuleId = subModuleItem.SubModule.Id;
                    roleSubModuleItemTemp.ModuleId = subModuleItem.SubModule.ModuleId.Value;

                    roleSubModuleItemTemp.CreateOperation = false;
                    roleSubModuleItemTemp.ReadOperation = false;
                    roleSubModuleItemTemp.DeleteOperation = false;
                    roleSubModuleItemTemp.UpdateOperation = false;
                }

                roleSubModuleItemVMList.Add(roleSubModuleItemTemp);
            }
                        
            return Json(roleSubModuleItemVMList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetRoleSubModuleItem(int id)
        {
            var roleSubModuleItem = this.roleSubModuleItemService.GetRoleSubModuleItem(id);
            return Json(roleSubModuleItem);
        }
    }

    
}