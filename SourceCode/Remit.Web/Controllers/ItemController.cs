using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.CachingService;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class ItemController : Controller
    {
        public readonly IItemCategoryService itemCategoryService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:item" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        public ItemController(IItemService itemService, IItemCategoryService itemCategoryService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.itemService = itemService;
            this.itemCategoryService = itemCategoryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        // GET: /Item/
        public ActionResult Index()
        {
            string url = "/Item/Index";
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            Session["GroupT"] = "raw";
            cacheKey += WebConfigurationManager.AppSettings["GroupType"];

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Item");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ActionResult CompositeItem()
        {
            var url = Request.RawUrl;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("CompositeItem");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ActionResult SparePartsAndOtherIndex()
        {
            ViewBag.GroupType = "";
            Session["GroupT"] = "spare";
            string url = "/Item/SparePartsAndOtherIndex"; 

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Item");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }








        [HttpPost]
        public JsonResult CreateItem(Item item)
        {
            string url= string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                url = "/Item/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                url = "/Item/SparePartsAndOtherIndex";
            }

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = item.Id == Guid.Empty ? true : false;

            if (isNew)
            {
                item.Id = Guid.NewGuid();
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(item))
                    {
                        if (this.itemService.CreateItem(item))
                        {
                            isSuccess = true;
                            message = "Item saved successfully!!!";
                        }
                        else
                        {
                            message = "Item could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same item name found!";
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
                    if (this.itemService.UpdateItem(item))
                    {
                        isSuccess = true;
                        message = "Item updated successfully!";
                    }
                    else
                    {
                        message = "Item could not updated!";
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

        private bool CheckIsExist(Model.Models.Item item)
        {
            return this.itemService.CheckIsExist(item);
        }

        [HttpPost]
        public JsonResult CreateCompositeItem(Item item)
        {
            const string url = "/Item/CompositeItem";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;

            if (permission.UpdateOperation == true)
            {
                var itemObj = this.itemService.GetItem(item.Id);
                if (itemObj.Item1 != null)
                {
                    foreach (var a in itemObj.Item1.ToList())
                    {
                        a.CompositeItemId = null;
                        this.itemService.UpdateItem(a);
                    }
                }

                if (item.Item1 != null)
                {
                    foreach (var a in item.Item1)
                    {
                        var aObjAttach = this.itemService.GetItem(a.Id);
                        if (aObjAttach != null)
                        {
                            aObjAttach.CompositeItemId = item.Id;
                            if (this.itemService.UpdateItem(aObjAttach))
                            {
                                isSuccess = true;
                                message = "Item updated successfully!";
                            }
                            else
                            {
                                message = "Item could not updated!";
                            }
                        }
                    }
                }
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
            }
            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteItem(Item item)
        {
            var isSuccess = true;
            var message = string.Empty;
            string url = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                url = "/Item/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                url = "/Item/SparePartsAndOtherIndex";
            }
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.itemService.DeleteItem(item.Id);
                if (isSuccess)
                {
                    message = "Item deleted successfully!";
                }
                else
                {
                    message = "Item can't be deleted!";
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

        public JsonResult GetItemList()
        {
            var itemListObj = new List<Item>();
            var grpType = WebConfigurationManager.AppSettings["GroupType"];
            if ((string) Session["GroupT"] == "raw")
            {
                itemListObj = this.itemService.GetAllItem().Where(it => it.ItemCategory.ItemGroup.TypeId == Convert.ToInt32(grpType)).ToList();
            }
            else
            {
                itemListObj = this.itemService.GetAllItem().Where(it => it.ItemCategory.ItemGroup.TypeId != Convert.ToInt32(grpType)).ToList();
            }
            
            List<ItemViewModel> itemVMList = new List<ItemViewModel>();

            foreach (var item in itemListObj)
            {
                var itemTemp = AItem(item);
                itemVMList.Add(itemTemp);
            }
            return Json(itemVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompositeItemList()
        {
            var itemListObj = this.itemService.GetAllItem().Where(a=>a.Item1.Count() != 0);
            List<ItemViewModel> itemVMList = new List<ItemViewModel>();

            foreach (var item in itemListObj)
            {
                var itemTemp = AItem(item);
                itemVMList.Add(itemTemp);
            }
            return Json(itemVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemListByCategoryId(int id)
        {
            var itemListObj = this.itemService.GetAllItem().Where(c => c.ItemCategoryId == id);
            List<ItemViewModel> itemVMList = new List<ItemViewModel>();
            foreach (var item in itemListObj)
            {
                var itemTemp = AItem(item);
                List<ItemInventoryViewModel> listInv = new List<ItemInventoryViewModel>();
                foreach (var itemInv in item.ItemInventories)
                {
                    ItemInventoryViewModel aInv = new ItemInventoryViewModel();
                    aInv.BinCardId = itemInv.BinCardId;
                    if (itemInv.BinCard != null)
                    {
                        aInv.BinCardName = itemInv.BinCard.CardNo;
                    }
                    aInv.Quantity = Math.Round((double)itemInv.Quantity, 3);

                    listInv.Add(aInv);
                }
                var sum = listInv.Sum(l => l.Quantity);
                itemTemp.TotalStockBalance = Math.Round((double)sum, 3);

                itemTemp.ItemInventorys = listInv;

                itemVMList.Add(itemTemp);
            }
            return Json(itemVMList.OrderBy(xx=> xx.Name), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemListByCategoryIdFromIssueDemApproveNotRequired(int id)
        {
            var itemListObj = this.itemService.GetAllItem().Where(c => c.ItemCategoryId == id && c.IsApprovalRequiredForDemand != true);
            List<ItemViewModel> itemVMList = new List<ItemViewModel>();
            foreach (var item in itemListObj)
            {
                var itemTemp = AItem(item);
                List<ItemInventoryViewModel> listInv = new List<ItemInventoryViewModel>();
                foreach (var itemInv in item.ItemInventories)
                {
                    ItemInventoryViewModel aInv = new ItemInventoryViewModel();
                    aInv.BinCardId = itemInv.BinCardId;
                    if (itemInv.BinCard != null)
                    {
                        aInv.BinCardName = itemInv.BinCard.CardNo;
                    }
                    aInv.Quantity = Math.Round((double) itemInv.Quantity, 3);

                    listInv.Add(aInv);
                }
                var sum = listInv.Sum(l => l.Quantity);
                itemTemp.TotalStockBalance = Math.Round((double) sum, 3);

                itemTemp.ItemInventorys = listInv;

                itemVMList.Add(itemTemp);
            }
            return Json(itemVMList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetItemListByCategoryIdFromIssue(int id)
        {
            var itemListObj = this.itemService.GetAllItem().Where(c => c.ItemCategoryId == id);
            List<ItemViewModel> itemVMList = new List<ItemViewModel>();
            foreach (var item in itemListObj)
            {
                var itemTemp = AItem(item);
                List<ItemInventoryViewModel> listInv = new List<ItemInventoryViewModel>();
                foreach (var itemInv in item.ItemInventories)
                {
                    ItemInventoryViewModel aInv = new ItemInventoryViewModel();
                    aInv.BinCardId = itemInv.BinCardId;
                    if (itemInv.BinCard != null)
                    {
                        aInv.BinCardName = itemInv.BinCard.CardNo;
                    }
                    aInv.Quantity = Math.Round((double)itemInv.Quantity, 3);
                    
                    listInv.Add(aInv);
                }
                itemTemp.TotalStockBalance = listInv.Sum(l => l.Quantity);
                if (itemTemp.TotalStockBalance != null)
                    itemTemp.TotalStockBalance = System.Math.Round((double) itemTemp.TotalStockBalance, 4);
                itemTemp.ItemInventorys = listInv;

                itemVMList.Add(itemTemp);
            }
            return Json(itemVMList, JsonRequestBehavior.AllowGet);
        }

        public ItemViewModel AItem(Item item)
        {
            ItemViewModel itemTemp = new ItemViewModel();
            itemTemp.Id = item.Id;
            itemTemp.Name = item.Name;
            itemTemp.Specification = item.Specification;
            itemTemp.Size = item.Size;
            itemTemp.Color = item.Color;
            itemTemp.HSCode = item.HSCode;
            itemTemp.PartNumber = item.PartNumber;
            if (item.UnitOfMeasurement != null)
            {
                itemTemp.Purchaseunit = item.UnitOfMeasurement.Name;
                itemTemp.PurchaseunitId = item.PurchaseunitId;
            }
            if (item.UnitOfMeasurement1 != null)
            {
                itemTemp.UsageUnit = item.UnitOfMeasurement1.Name;
                itemTemp.UsageUnitId = item.UsageUnitId;
            }
            itemTemp.PerUnitWeight = item.PerUnitWeight;
            itemTemp.ThresholdLevel = item.ThresholdLevel;
            itemTemp.Priority = item.Priority;
            itemTemp.PurchaseToUsageConversionRatio = item.PurchaseToUsageConversionRatio;
            if (item.ItemCategory != null)
            {
                itemTemp.ItemCategoryName = item.ItemCategory.Name;
                itemTemp.ItemCategoryId = item.ItemCategoryId;
            }
            itemTemp.CompositeItemId = item.CompositeItemId;
            
            itemTemp.IsApprovalRequiredForIssue = item.IsApprovalRequiredForIssue;
            itemTemp.IsApprovalRequiredForDemand = item.IsApprovalRequiredForDemand;
            return itemTemp;
        }

        public JsonResult GetItem(Guid id)
        {
            var item = this.itemService.GetItem(id);
            return Json(item, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompositeItem(Guid id)
        {
            var item = this.itemService.GetItem(id);
            ItemViewModel itemTemp = null;

            if (item != null)
            {
                itemTemp = new ItemViewModel();
                itemTemp.Id = item.Id;
                itemTemp.ItemCategoryId = item.ItemCategoryId;

                if (item.Item1.Count() > 0)
                {
                    List<ItemViewModel> itemDetailVMList = new List<ItemViewModel>();
                    foreach (var itemDetai in item.Item1)
                    {
                        ItemViewModel itemDetailTtemp = new ItemViewModel();
                        itemDetailTtemp.Id = itemDetai.Id;
                        itemDetailTtemp.ItemCategoryId = itemDetai.ItemCategoryId;
                        itemDetailTtemp.CompositeItemId = itemDetai.CompositeItemId;

                        itemDetailVMList.Add(itemDetailTtemp);
                    }
                    itemTemp.Item1 = itemDetailVMList;
                }
            }
            return Json(itemTemp, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCompositeItemForReceive(Guid id)
        {
            var item = this.itemService.GetItem(id);

            List<ItemViewModel> itemDetailVMList = new List<ItemViewModel>();
            if (item != null)
            {
                if (item.Item1.Count() > 0)
                {
                    foreach (var itemDetai in item.Item1)
                    {
                        ItemViewModel itemDetailTtemp = new ItemViewModel();
                        itemDetailTtemp.Id = itemDetai.Id;
                        itemDetailTtemp.Name = itemDetai.Name;
                        itemDetailTtemp.ItemCategoryId = itemDetai.ItemCategoryId;
                        itemDetailTtemp.CompositeItemId = itemDetai.CompositeItemId;

                        itemDetailVMList.Add(itemDetailTtemp);
                    }
                }
                else
                {
                    ItemViewModel itemDetailTtemp = new ItemViewModel();
                    itemDetailTtemp.Id = item.Id;
                    itemDetailTtemp.Name = item.Name;
                    itemDetailTtemp.ItemCategoryId = item.ItemCategoryId;
                    itemDetailTtemp.CompositeItemId = item.CompositeItemId;

                    itemDetailVMList.Add(itemDetailTtemp);
                }
            }
            return Json(itemDetailVMList, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemViewModel
    {
        public ItemViewModel()
        {
            this.ItemInventorys = new List<ItemInventoryViewModel>();
            this.Item1 = new List<ItemViewModel>();
        }
        public System.Guid Id { get; set; }
        public int ItemCategoryId { get; set; }
        public string Name { get; set; }
        public string ItemCategoryName { get; set; }
        public virtual ItemCategory ItemCategory { get; set; }
        public string Specification { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string HSCode { get; set; }
        public string PartNumber { get; set; }
        public int? PurchaseunitId { get; set; }
        public string Purchaseunit { get; set; }
        public int? UsageUnitId { get; set; }
        public double? TotalStockBalance { set; get; }
        public string UsageUnit { get; set; }
        public int? Priority { get; set; }
        public double? PurchaseToUsageConversionRatio { get; set; }
        public double? ThresholdLevel { get; set; }
        public double? PerUnitWeight { get; set; }
        public bool? IsApprovalRequiredForIssue { get; set; }
        public bool? IsApprovalRequiredForDemand { get; set; }
        public Nullable<System.Guid> CompositeItemId { get; set; }
        public List<ItemInventoryViewModel> ItemInventorys { set; get; }
        public List<ItemViewModel> Item1 { set; get; }
    }
}