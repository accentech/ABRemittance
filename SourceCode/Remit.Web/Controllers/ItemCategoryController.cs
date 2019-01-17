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

namespace Remit.Web.Controllers
{
    public class ItemCategoryController : Controller
    {
        public readonly IItemCategoryService itemCategoryService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public ItemCategoryController(IItemCategoryService itemCategoryService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.itemCategoryService = itemCategoryService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string grpType = WebConfigurationManager.AppSettings["GroupType"];
        string cacheKey = "permission:itemCategory" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        
        // GET: /ItemCategory/
        public ActionResult Index()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            Session["GroupT"] = "raw";
            const string url = "/ItemCategory/Index";
            cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            return CommonItemCategoryIndex(url, cacheKey, "ItemCategory");
        }

        public ActionResult SparePartsAndOtherIndex()
        {
            ViewBag.GroupType = "";
            Session["GroupT"] = "spare";
            const string url = "/ItemCategory/SparePartsAndOtherIndex";
            return CommonItemCategoryIndex(url, cacheKey, "ItemCategory");
        }

        private ActionResult CommonItemCategoryIndex(string url, string cacheKey, string viewName)
        {
            RoleSubModuleItem permissionDemand = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                                 roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permissionDemand != null)
            {
                if (permissionDemand.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permissionDemand, 240);
                    return View(viewName);
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult ItemCategory()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateItemCategory(ItemCategory itemCategory)
        {
            var isSuccess = false;
            var message = string.Empty;
            //var isNew = itemCategoryService.GetItemCategory(itemCategory.Id);
            var isNew = itemCategory.Id == 0 ? true : false;
            string url = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                url = "/ItemCategory/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                url = "/ItemCategory/SparePartsAndOtherIndex"; 
            }

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(itemCategory))
                    {
                        if (this.itemCategoryService.CreateItemCategory(itemCategory))
                        {
                            isSuccess = true;
                            message = "ItemCategory saved successfully!";
                        }
                        else
                        {
                            message = "ItemCategory could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same itemCategory name found!";
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
                    if (this.itemCategoryService.UpdateItemCategory(itemCategory))
                    {
                        isSuccess = true;
                        message = "ItemCategory updated successfully!";
                    }
                    else
                    {
                        message = "ItemCategory could not updated!";
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
        private bool CheckIsExist(Model.Models.ItemCategory itemCategory)
        {
            return this.itemCategoryService.CheckIsExist(itemCategory);
        }
        [HttpPost]
        public JsonResult DeleteItemCategory(ItemCategory itemCategory)
        {
            var isSuccess = true;
            var message = string.Empty;
            string url = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                url = "/ItemCategory/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                url = "/ItemCategory/SparePartsAndOtherIndex";
            }
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permission.DeleteOperation == true)
            {
                isSuccess = this.itemCategoryService.DeleteItemCategory(itemCategory.Id);
                if (isSuccess)
                {
                    message = "ItemCategory deleted successfully!";

                }
                else
                {
                    message = "ItemCategory can't be deleted!";
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

        public JsonResult GetItemCategoryListWithoutGroupType()
        {
            var itemCategoryListObj = this.itemCategoryService.GetAllItemCategory().Where(itm => itm.ItemGroup.TypeId != Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"])).OrderBy(a => a.Name);
            List<ItemCategoryViewModel> itemCategoryVMList = new List<ItemCategoryViewModel>();

            foreach (var itemCategory in itemCategoryListObj)
            {
                ItemCategoryViewModel itemCategoryTemp = new ItemCategoryViewModel();
                itemCategoryTemp.Id = itemCategory.Id;
                itemCategoryTemp.Name = itemCategory.Name;
                if (itemCategory.ItemGroup != null)
                {
                    itemCategoryTemp.ItemGroupName = itemCategory.ItemGroup.Name;
                    itemCategoryTemp.ItemGroupId = itemCategory.ItemGroupId;
                }
                itemCategoryTemp.Description = itemCategory.Description;
                itemCategoryVMList.Add(itemCategoryTemp);
            }
            return Json(itemCategoryVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemCategoryListWithGroupType(int groupType)
        {
            var itemCategoryListObj = this.itemCategoryService.GetAllItemCategory().Where(cat => cat.ItemGroup.TypeId == groupType).OrderBy(a => a.Name);
            List<ItemCategoryViewModel> itemCategoryVMList = new List<ItemCategoryViewModel>();

            foreach (var itemCategory in itemCategoryListObj)
            {
                ItemCategoryViewModel itemCategoryTemp = new ItemCategoryViewModel();
                itemCategoryTemp.Id = itemCategory.Id;
                itemCategoryTemp.Name = itemCategory.Name;
                if (itemCategory.ItemGroup != null)
                {
                    itemCategoryTemp.ItemGroupName = itemCategory.ItemGroup.Name;
                    itemCategoryTemp.ItemGroupId = itemCategory.ItemGroupId;
                }
                itemCategoryTemp.Description = itemCategory.Description;
                itemCategoryVMList.Add(itemCategoryTemp);
            }
            return Json(itemCategoryVMList, JsonRequestBehavior.AllowGet);
        }
      
        public JsonResult GetItemCategoryList()
        {
            var itemCategoryListObj = this.itemCategoryService.GetAllItemCategory().OrderBy(a => a.Name);
            List<ItemCategoryViewModel> itemCategoryVMList = new List<ItemCategoryViewModel>();

            foreach (var itemCategory in itemCategoryListObj)
            {
                ItemCategoryViewModel itemCategoryTemp = new ItemCategoryViewModel();
                itemCategoryTemp.Id = itemCategory.Id;
                itemCategoryTemp.Name = itemCategory.Name;
                if (itemCategory.ItemGroup != null)
                {
                    itemCategoryTemp.ItemGroupName = itemCategory.ItemGroup.Name;
                    itemCategoryTemp.ItemGroupId = itemCategory.ItemGroupId;
                }
                itemCategoryTemp.Description = itemCategory.Description;
                itemCategoryVMList.Add(itemCategoryTemp);
            }
            return Json(itemCategoryVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemCategoryList2()
        {
            var itemCategoryListObj = new List<ItemCategory>();
            if ((string)Session["GroupT"] == "raw")
            {
                itemCategoryListObj = this.itemCategoryService.GetAllItemCategory().Where(a => a.ItemGroup.TypeId == Convert.ToInt32(grpType)).OrderBy(a => a.Name).ToList();
            }
            else
            {
                itemCategoryListObj = this.itemCategoryService.GetAllItemCategory().Where(  a => a.ItemGroup != null && a.ItemGroup.TypeId != Convert.ToInt32(grpType)).OrderBy(a => a.Name).ToList();
            }

            List<ItemCategoryViewModel> itemCategoryVMList = new List<ItemCategoryViewModel>();

            foreach (var itemCategory in itemCategoryListObj)
            {
                ItemCategoryViewModel itemCategoryTemp = new ItemCategoryViewModel();
                itemCategoryTemp.Id = itemCategory.Id;
                itemCategoryTemp.Name = itemCategory.Name;
                if (itemCategory.ItemGroup != null)
                {
                    itemCategoryTemp.ItemGroupName = itemCategory.ItemGroup.Name;
                    itemCategoryTemp.ItemGroupId = itemCategory.ItemGroupId;
                }
                itemCategoryTemp.Description = itemCategory.Description;
                itemCategoryVMList.Add(itemCategoryTemp);
            }
            return Json(itemCategoryVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemCategory(int id)
        {
            var itemCategory = this.itemCategoryService.GetItemCategory(id);
            return Json(itemCategory);
        }
        public JsonResult GetItemCategoryListByGroupId(int id)
        {
            var itemCategoryListObj = this.itemCategoryService.GetAllItemCategory().Where(c => c.ItemGroupId == id).OrderBy(a=>a.Name);
            List<ItemCategoryViewModel> itemCategoryVMList = new List<ItemCategoryViewModel>();
            foreach (var itemCategory in itemCategoryListObj)
            {
                ItemCategoryViewModel itemCategoryTemp = new ItemCategoryViewModel();
                itemCategoryTemp.Id = itemCategory.Id;
                itemCategoryTemp.Name = itemCategory.Name;

                itemCategoryVMList.Add(itemCategoryTemp);
            }
            return Json(itemCategoryVMList, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ItemGroupId { get; set; }
        public string ItemGroupName { get; set; }
        public string Description { get; set; }
    }
}