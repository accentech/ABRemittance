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
    public class SubModuleItemController : Controller
    {
        public readonly ISubModuleItemService subModuleItemService;
        public readonly ISubModuleService subModuleService;
        public readonly IModuleService moduleService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:subModuleItem" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /SubModuleItem/
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
                    cacheProvider.Set("permission:subModuleItem" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("SubModuleItem");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public SubModuleItemController(ISubModuleItemService subModuleItemService, IModuleService moduleService, ISubModuleService subModuleService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.subModuleItemService = subModuleItemService;
            this.moduleService = moduleService;
            this.subModuleService = subModuleService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateSubModuleItem(SubModuleItem subModuleItem)
        {
            const string url = "/SubModuleItem/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = subModuleItem.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(subModuleItem))
                    {
                        if (this.subModuleItemService.CreateSubModuleItem(subModuleItem))
                        {
                            var moduleIdList = this.moduleService.GetAllModuleByRoleId(Helpers.UserSession.GetUserFromSession().RoleId).Select(a => a.Id).Distinct().ToList();
                            foreach (var moduleId in moduleIdList)
                            {
                                cacheProvider.Invalidate("submodule" + moduleId.ToString() + Helpers.UserSession.GetUserFromSession().RoleId);
                            }


                            isSuccess = true;
                            message = "SubModuleItem saved successfully!";
                        }
                        else
                        {
                            message = "SubModuleItem could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same subModuleItem name found!";
                    }
                }
                else
                {
                    message = Resources.ResourceCommon.MsgNoPermissionToCreate;
                }
            }
            else
            {
                if (permission.UpdateOperation == true)
                {
                    if (this.subModuleItemService.UpdateSubModuleItem(subModuleItem))
                    {
                        isSuccess = true;
                        message = "SubModuleItem updated successfully!";
                        var moduleIdList = this.moduleService.GetAllModuleByRoleId(Helpers.UserSession.GetUserFromSession().RoleId).Select(a => a.Id).Distinct().ToList();
                        foreach (var moduleId in moduleIdList)
                        {                            
                            cacheProvider.Invalidate("submodule" + moduleId.ToString() + Helpers.UserSession.GetUserFromSession().RoleId);
                        }
                    }
                    else
                    {
                        message = "SubModuleItem could not updated!";
                    }
                }
                else
                {
                    message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
                }                
            }
            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }
        private bool CheckIsExist(Model.Models.SubModuleItem subModuleItem)
        {
            return this.subModuleItemService.CheckIsExist(subModuleItem);
        }

        [HttpPost]
        public JsonResult DeleteSubModuleItem(SubModuleItem subModuleItem)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuleItem/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {

                isSuccess = this.subModuleItemService.DeleteSubModuleItem(subModuleItem.Id);
                if (isSuccess)
                {
                    message = "SubModuleItem deleted successfully!";
                }
                else
                {
                    message = "SubModuleItem can't be deleted!";
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

        public JsonResult GetSubModuleItemList()
        {
            var subModuleItemListObj = this.subModuleItemService.GetAllSubModuleItem();
            List<SubModuleItemModel> subModuleItemVMList = new List<SubModuleItemModel>();

            foreach (var subModuleItem in subModuleItemListObj)
            {
                SubModuleItemModel subModuleItemTemp = new SubModuleItemModel();
                subModuleItemTemp.Id = subModuleItem.Id;
                subModuleItemTemp.Name = subModuleItem.Name;
                if (subModuleItem.SubModuleId != null)
                    subModuleItemTemp.ModuleId = subModuleItem.SubModule.ModuleId;
                subModuleItemTemp.SubModuleId = subModuleItem.SubModuleId;
                subModuleItemTemp.Ordering = subModuleItem.Ordering;
                if (subModuleItem.SubModuleId != null)
                {
                    subModuleItemTemp.SubModuleName = subModuleItem.SubModule.Name;
                    if (subModuleItem.SubModule.ModuleId != null)
                        subModuleItemTemp.ModuleName = subModuleItem.SubModule.Module.Name;
                }

                subModuleItemTemp.UrlPath = subModuleItem.UrlPath;
                subModuleItemTemp.IsBaseItem = subModuleItem.IsBaseItem;
                subModuleItemTemp.BaseItemId = subModuleItem.BaseItemId;
                if (subModuleItemTemp.BaseItemId!=null)
                    subModuleItemTemp.BaseItemName = subModuleItem.SubModuleItem2.Name;

                if (subModuleItem.IsActive != null)
                    subModuleItemTemp.IsActive = subModuleItem.IsActive.Value;

                subModuleItemVMList.Add(subModuleItemTemp);
            }
            return Json(subModuleItemVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBaseSubModuleItemList()
        {
            var subModuleItemListObj = this.subModuleItemService.GetAllBaseSubModuleItem();
            List<SubModuleItemModel> subModuleItemVMList = new List<SubModuleItemModel>();

            foreach (var subModuleItem in subModuleItemListObj)
            {
                SubModuleItemModel subModuleItemTemp = new SubModuleItemModel();
                subModuleItemTemp.Id = subModuleItem.Id;
                subModuleItemTemp.Name = subModuleItem.Name;
                if (subModuleItem.SubModuleId != null)
                    subModuleItemTemp.ModuleId = subModuleItem.SubModule.ModuleId;
                subModuleItemTemp.SubModuleId = subModuleItem.SubModuleId;
                subModuleItemTemp.Ordering = subModuleItem.Ordering;
                if (subModuleItem.SubModuleId != null)
                {
                    subModuleItemTemp.SubModuleName = subModuleItem.SubModule.Name;
                    if (subModuleItem.SubModule.ModuleId != null)
                        subModuleItemTemp.ModuleName = subModuleItem.SubModule.Module.Name;
                }

                subModuleItemTemp.UrlPath = subModuleItem.UrlPath;
                subModuleItemTemp.IsBaseItem = subModuleItem.IsBaseItem;
                subModuleItemTemp.BaseItemId = subModuleItem.BaseItemId;
                if (subModuleItemTemp.BaseItemId != null)
                    subModuleItemTemp.BaseItemName = subModuleItem.SubModuleItem2.Name;

                if (subModuleItem.IsActive != null)
                 subModuleItemTemp.IsActive = subModuleItem.IsActive.Value;

                subModuleItemVMList.Add(subModuleItemTemp);
            }
            return Json(subModuleItemVMList, JsonRequestBehavior.AllowGet);
        }

        

        public JsonResult GetSubModuleItem(int id)
        {
            var subModuleItem = this.subModuleItemService.GetSubModuleItem(id);
            return Json(subModuleItem);
        }
    }
}