using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Remit.Service.Enums;


namespace Remit.Web.Controllers
{
    public class ItemGroupController : Controller
    {
        public readonly IItemGroupService itemGroupService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public ItemGroupController(IItemGroupService itemGroupService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.itemGroupService = itemGroupService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:itemGroup" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        
        public ActionResult Index()
        {
            const string urlItemGroup = "/ItemGroup/Index";
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            Session["GroupT"] = "raw";
            cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            return CommonItemGroupList(urlItemGroup, cacheKey, "ItemGroup");
        }

        public ActionResult SparePartsAndOtherIndex()
        {
            ViewBag.GroupType = "";
            Session["GroupT"] = "spare";
            const string urlItemGroup = "/ItemGroup/SparePartsAndOtherIndex";
            return CommonItemGroupList(urlItemGroup, cacheKey, "ItemGroup");
        }


        private ActionResult CommonItemGroupList(string url, string cacheKey, string viewName)
        {
            RoleSubModuleItem permissionGroup = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                                roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permissionGroup != null)
            {
                if (permissionGroup.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permissionGroup, 240);
                    return View(viewName);
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult ItemGroup()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateItemGroup(ItemGroup itemGroup)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = itemGroup.Id == 0 ? true : false;
           // const string url = "/ItemGroup/Index";

            string urlGroup = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                urlGroup = "/ItemGroup/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                urlGroup = "/ItemGroup/SparePartsAndOtherIndex";
            }




            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(urlGroup, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(itemGroup))
                    {
                        if (this.itemGroupService.CreateItemGroup(itemGroup))
                        {
                            isSuccess = true;
                            message = "ItemGroup saved successfully!";
                        }
                        else
                        {
                            message = "ItemGroup could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same itemGroup name found!";
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
                    if (this.itemGroupService.UpdateItemGroup(itemGroup))
                    {
                        isSuccess = true;
                        message = "ItemGroup updated successfully!";
                    }
                    else
                    {
                        message = "ItemGroup could not updated!";
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
        private bool CheckIsExist(Model.Models.ItemGroup itemGroup)
        {
            return this.itemGroupService.CheckIsExist(itemGroup);
        }
        [HttpPost]
        public JsonResult DeleteItemGroup(ItemGroup itemGroup)
        {
            var isSuccess = true;
            var message = string.Empty;
           // const string url = "/ItemGroup/Index";

            string urlGroup = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                urlGroup = "/ItemGroup/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                urlGroup = "/ItemGroup/SparePartsAndOtherIndex";
            }



            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(urlGroup,
                                Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permission.DeleteOperation == true)
            {
                isSuccess = this.itemGroupService.DeleteItemGroup(itemGroup.Id);
                if (isSuccess)
                {
                    message = "ItemGroup deleted successfully!";

                }
                else
                {
                    message = "ItemGroup can't be deleted!";
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

        public JsonResult GetItemGroupListWithGroupType(int groupType)
        {
            var itemGroupListObj = this.itemGroupService.GetAllItemGroup().Where(it => it.TypeId == groupType).OrderBy(a => a.Name);
            List<ItemGroupViewModel> itemGroupVMList = new List<ItemGroupViewModel>();

            foreach (var itemGroup in itemGroupListObj)
            {
                ItemGroupViewModel itemGroupTemp = new ItemGroupViewModel();
                itemGroupTemp.Id = itemGroup.Id;
                itemGroupTemp.Name = itemGroup.Name;
                itemGroupTemp.TypeId = itemGroup.TypeId;
                itemGroupTemp.TypeName = ((ItemGroupTypeEnum)itemGroup.TypeId).ToString();
                itemGroupVMList.Add(itemGroupTemp);
            }
            return Json(itemGroupVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemGroupListWithoutGroupType()
        {
            var itemGroupListObj = this.itemGroupService.GetAllItemGroup().Where(it => it.TypeId != Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"])).OrderBy(a=>a.Name);
            List<ItemGroupViewModel> itemGroupVMList = new List<ItemGroupViewModel>();

            foreach (var itemGroup in itemGroupListObj)
            {
                ItemGroupViewModel itemGroupTemp = new ItemGroupViewModel();
                itemGroupTemp.Id = itemGroup.Id;
                itemGroupTemp.Name = itemGroup.Name;
                itemGroupTemp.TypeId = itemGroup.TypeId;
                itemGroupTemp.TypeName = ((ItemGroupTypeEnum)itemGroup.TypeId).ToString();
                itemGroupVMList.Add(itemGroupTemp);
            }
            return Json(itemGroupVMList, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetItemGroupList()
        {
            var itemGroupListObj = this.itemGroupService.GetAllItemGroup().OrderBy(a => a.Name);
            List<ItemGroupViewModel> itemGroupVMList = new List<ItemGroupViewModel>();

            foreach (var itemGroup in itemGroupListObj)
            {
                ItemGroupViewModel itemGroupTemp = new ItemGroupViewModel();
                itemGroupTemp.Id = itemGroup.Id;
                itemGroupTemp.Name = itemGroup.Name;
                itemGroupTemp.TypeId = itemGroup.TypeId;
                itemGroupTemp.TypeName = ((ItemGroupTypeEnum)itemGroup.TypeId).ToString();
                itemGroupVMList.Add(itemGroupTemp);
            }
            return Json(itemGroupVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemGroup(int id)
        {
            var itemGroup = this.itemGroupService.GetItemGroup(id);
            List<ItemGroupViewModel> itemGroupVMList = new List<ItemGroupViewModel>();
            ItemGroupViewModel itemGroupTemp = new ItemGroupViewModel();
            itemGroupTemp.Id = itemGroup.Id;
            itemGroupTemp.Name = itemGroup.Name;
            itemGroupTemp.TypeId = itemGroup.TypeId;
            itemGroupTemp.TypeName = ((ItemGroupTypeEnum)itemGroup.TypeId).ToString();
            itemGroupVMList.Add(itemGroupTemp);

            return Json(itemGroupVMList, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemGroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? TypeId { get; set; }

        public string TypeName { get; set; }


    }
}