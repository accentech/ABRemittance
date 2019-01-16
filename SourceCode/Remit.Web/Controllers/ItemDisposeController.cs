using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.Web.Helpers;
using Remit.Service.Enums;
using System.Linq.Dynamic;
using Remit.Web.Models;
using System.Text.RegularExpressions;
using Remit.Service.Utilities;
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class ItemDisposeController : Controller
    {
        public readonly IItemDisposeService itemDisposeService;
        public readonly IItemDisposeDetailService itemDisposeDetailService;
        public readonly IItemInventoryHistoryService itemInventoryHistoryService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly INotificationSettingService notificationSettingService;
        public readonly IItemInventoryService itemInventoryService;

        public readonly ISupplierService supplierService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public ItemDisposeController(ISupplierService supplierService, IItemDisposeService itemDisposeService, IItemInventoryHistoryService itemInventoryHistoryService,
            ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService,
            IItemDisposeDetailService itemDisposeDetailService, IWorkflowactionSettingService workflowactionSettingService,
            INotificationSettingService notificationSettingService, IItemInventoryService itemInventoryService, IItemService itemService)
        {
            this.supplierService = supplierService;
            this.itemDisposeService = itemDisposeService;
            this.itemInventoryHistoryService = itemInventoryHistoryService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.itemDisposeDetailService = itemDisposeDetailService;

            this.workflowactionSettingService = workflowactionSettingService;
            this.notificationSettingService = notificationSettingService;
            this.itemInventoryService = itemInventoryService;
            this.itemService = itemService;
        }

        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:itemdispose" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        //const string url = "/ItemDispose/Index";

        // GET: /ItemDispose/
        public ActionResult Index()
        {
            const string url = "/ItemDispose/Index";
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            Session["GroupT"] = "raw";
            cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            return CommonItemDispose(url, cacheKey, "ItemDispose");
        }

        public ActionResult SparePartsAndOtherIndex()
        {
            ViewBag.GroupType = "";
            Session["GroupT"] = "spare";
            const string url = "/ItemDispose/SparePartsAndOtherIndex";
            return CommonItemDispose(url, cacheKey, "ItemDispose");
        }

        public ActionResult CommonItemDispose(string url, string cacheKey, string viewName)
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

        public ActionResult EditItemDispose(string id, string redirectPage, int status) //edit fields
        {
            ViewBag.ItemDisposeId = id;
            ViewBag.Status = status;
            ViewBag.ForEditOrApproveOrIssueOrReceive = 77; //for edit
            ViewBag.RedirectPage = redirectPage;
            return View("ItemDispose");
        }

        [HttpPost]
        public JsonResult CreateItemDispose(ItemDispose itemdispose)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                var isNew = itemdispose.Id == 0 ? true : false;

                string url = string.Empty;
                if ((string)Session["GroupT"] == "raw")
                {
                    url = "/ItemDispose/Index";
                    cacheKey += WebConfigurationManager.AppSettings["GroupType"];
                }else{
                    url = "/ItemDispose/SparePartsAndOtherIndex";
                }


                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                  roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                      Helpers.UserSession.GetUserFromSession().RoleId);
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {
                        if (this.itemDisposeService.CreateItemDispose(itemdispose))
                        {
                            isSuccess = true;
                            message += string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceItemDispose.LblItemDispose);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceItemDispose.LblItemDispose);
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
                        var itemdisposeObj = this.itemDisposeService.GetItemDispose(itemdispose.Id);
                        if (itemdisposeObj.ItemDisposeDetails != null)
                        {
                            foreach (var a in itemdisposeObj.ItemDisposeDetails.ToList())
                            {
                                InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService,
                                    itemInventoryService);
                                try
                                {
                                    ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(b => b.BinCardId == a.BinCardId && b.ItemId == a.ItemId).FirstOrDefault();
                                    if (itemInventory != null)
                                    {
                                        inventoryUtility.UpdateItmeBalance(itemInventory, a.Quantity, true);
                                    }
                                    inventoryUtility.SaveInventoryHistory(a.ItemId, a.Quantity, a.UnitId, a.BinCardId, a.Id.ToString(), "ItemDispose", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Delete);
                                }
                                catch { }

                                this.itemDisposeDetailService.DeleteItemDisposeDetail(a.Id);
                            }
                        }

                        if (itemdisposeObj.ItemDisposeDetails != null)
                        {
                            foreach (var detin in itemdispose.ItemDisposeDetails)
                            {
                                try
                                {
                                    detin.ItemDisposeId = itemdispose.Id;
                                    this.itemDisposeDetailService.CreateItemDisposeDetail(detin);
                                }
                                catch { };
                            }
                        }

                        itemdisposeObj.Date = itemdispose.Date.Value.ToUniversalTime();

                        if (this.itemDisposeService.UpdateItemDispose(itemdisposeObj))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceItemDispose.LblItemDispose);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceItemDispose.LblItemDispose);
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                message = ex.Message;
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteItemDispose(ItemDispose itemdispose)
        {
            var isSuccess = true;
            var message = string.Empty;

            string url = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                url = "/ItemDispose/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }else{
                url = "/ItemDispose/SparePartsAndOtherIndex";
            }

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {

                foreach (var a in itemdispose.ItemDisposeDetails.ToList())
                {
                    this.itemDisposeDetailService.DeleteItemDisposeDetail(a.Id);
                }
                isSuccess = this.itemDisposeService.DeleteItemDispose(itemdispose.Id);
                message = isSuccess ? "Item Dispose deleted successfully!" : "Item Dispose can't be deleted!";

                itemdispose = this.itemDisposeService.GetItemDispose(itemdispose.Id);
                if (itemdispose != null)
                {
                    foreach (var ItemDisposeDetail in itemdispose.ItemDisposeDetails.ToList())
                    {
                        InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService, itemInventoryService);
                        try
                        {
                            ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(a => a.BinCardId == ItemDisposeDetail.BinCardId && a.ItemId == ItemDisposeDetail.ItemId).FirstOrDefault();
                            if (itemInventory != null)
                            {
                                inventoryUtility.UpdateItmeBalance(itemInventory, ItemDisposeDetail.Quantity, true);
                            }
                            inventoryUtility.SaveInventoryHistory(ItemDisposeDetail.ItemId, ItemDisposeDetail.Quantity, ItemDisposeDetail.Item.UsageUnitId, ItemDisposeDetail.BinCardId, ItemDisposeDetail.Id.ToString(), "Item Dispose", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Delete);
                        }
                        catch { }

                        //this.itemDisposeDetailService.DeleteItemDisposeDetail(ItemDisposeDetail.Id);
                    }

                    itemdispose.IsDeleted = true;
                    itemdispose.DeletedBy = UserSession.GetUserFromSession().EmployeeId;
                    itemdispose.DeletedOn = DateTime.UtcNow;

                    isSuccess = this.itemDisposeService.UpdateItemDispose(itemdispose);
                    message = string.Format(isSuccess ? Resources.ResourceCommon.CMsg_delete : Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceItemDispose.LblItemDispose);
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

        public JsonResult GetItemDisposeListByGroupType()
        {
            var itemdisposeListObj = new List<ItemDispose>();
            if ((string)Session["GroupT"] == "raw")
            {
                itemdisposeListObj = this.itemDisposeService.GetAllItemDispose().Where(dis =>
                {
                    var itemDisposeDetail = dis.ItemDisposeDetails.FirstOrDefault();
                    return itemDisposeDetail != null && itemDisposeDetail.Item.ItemCategory.ItemGroup.TypeId ==
                           Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                }).ToList();
            }
            else
            {
                itemdisposeListObj = this.itemDisposeService.GetAllItemDispose().Where(dis =>
                {
                    var itemDisposeDetail = dis.ItemDisposeDetails.FirstOrDefault();
                    return itemDisposeDetail != null && itemDisposeDetail.Item.ItemCategory.ItemGroup.TypeId !=
                           Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                }).ToList();
            }
           
            List<ItemDisposeViewModel> itemdisposeVMList = new List<ItemDisposeViewModel>();

            foreach (var itemdispose in itemdisposeListObj)
            {
                ItemDisposeViewModel itemdisposeTemp = new ItemDisposeViewModel();

                itemdisposeTemp.Id = itemdispose.Id;

                itemdisposeTemp.Date = itemdispose.Date != null ? itemdispose.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemdisposeTemp.AuthorisedBy = itemdispose.AuthorisedBy;

                if (itemdispose.Employee != null)
                {
                    itemdisposeTemp.AuthorisedByName = itemdispose.Employee.FullName;
                }

                itemdisposeTemp.AuthorisedDate = itemdispose.AuthorisedDate != null ? itemdispose.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemdisposeVMList.Add(itemdisposeTemp);
            }
            return Json(itemdisposeVMList, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetItemDisposeList()
        {
            var itemdisposeListObj = this.itemDisposeService.GetAllItemDispose().Where(a => a.IsDeleted != true);
            List<ItemDisposeViewModel> itemdisposeVMList = new List<ItemDisposeViewModel>();

            foreach (var itemdispose in itemdisposeListObj)
            {
                ItemDisposeViewModel itemdisposeTemp = new ItemDisposeViewModel();

                itemdisposeTemp.Id = itemdispose.Id;

                itemdisposeTemp.Date = itemdispose.Date != null ? itemdispose.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemdisposeTemp.AuthorisedBy = itemdispose.AuthorisedBy;

                if (itemdispose.Employee != null)
                {
                    itemdisposeTemp.AuthorisedByName = itemdispose.Employee.FullName;
                }

                itemdisposeTemp.AuthorisedDate = itemdispose.AuthorisedDate != null ? itemdispose.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemdisposeVMList.Add(itemdisposeTemp);
            }
            return Json(itemdisposeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPiListByYear(int year)
        {

            var itemdisposeListObj  = new List<ItemDispose>();
            if ((string) Session["GroupT"] == "raw")
            {
                itemdisposeListObj = this.itemDisposeService.GetAllItemDispose().Where(a =>
                {
                    var itemDisposeDetail = a.ItemDisposeDetails.FirstOrDefault(); 
                    return itemDisposeDetail != null && (a.Date != null && a.Date.Value.AddMinutes(timeZoneOffset).Year == year && a.IsDeleted != true &&
                                                                                 itemDisposeDetail.Item.ItemCategory.ItemGroup.TypeId ==
                                                                                 Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]));
                }).ToList();
            }
            else
            {
                itemdisposeListObj = this.itemDisposeService.GetAllItemDispose().Where(a =>
                {
                    var itemDisposeDetail = a.ItemDisposeDetails.FirstOrDefault();
                    return itemDisposeDetail != null && (a.Date != null && a.Date.Value.AddMinutes(timeZoneOffset).Year == year && a.IsDeleted != true &&
                                                         itemDisposeDetail.Item.ItemCategory.ItemGroup.TypeId !=
                                                         Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]));
                }).ToList();
            }
            

            List<ItemDisposeViewModel> itemdisposeVMList = new List<ItemDisposeViewModel>();
            foreach (var itemdispose in itemdisposeListObj)
            {
                ItemDisposeViewModel itemdisposeTemp = new ItemDisposeViewModel();
                itemdisposeTemp.Id = itemdispose.Id;
                itemdisposeTemp.Date = itemdispose.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                itemdisposeVMList.Add(itemdisposeTemp);
            }
            return Json(itemdisposeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemDispose(int id)
        {
            var itemdispose = this.itemDisposeService.GetItemDispose(id);
            ItemDisposeViewModel itemdisposeTemp = null;
            var BinCardQty = new ItemInventory();
            if (itemdispose != null)
            {
                itemdisposeTemp = new ItemDisposeViewModel();
                itemdisposeTemp.Id = itemdispose.Id;

                itemdisposeTemp.Date = itemdispose.Date != null ? itemdispose.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemdisposeTemp.AuthorisedBy = itemdispose.AuthorisedBy;
                itemdisposeTemp.AuthorisedDate = itemdispose.AuthorisedDate != null ? itemdispose.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";

                if (itemdispose.ItemDisposeDetails.Count() > 0)
                {
                    List<ItemDisposeDetailViewModel> itemdisposeDetailVMList = new List<ItemDisposeDetailViewModel>();
                    foreach (var itemdisposeDetai in itemdispose.ItemDisposeDetails)
                    {
                        ItemDisposeDetailViewModel itemdisposeDetailTtemp = new ItemDisposeDetailViewModel();
                        itemdisposeDetailTtemp.Id = itemdisposeDetai.Id;
                        itemdisposeDetailTtemp.ItemDisposeId = (int)itemdisposeDetai.ItemDisposeId;
                        itemdisposeDetailTtemp.ItemId = (Guid)itemdisposeDetai.ItemId;
                        itemdisposeDetailTtemp.ItemName = itemdisposeDetai.Item.Name;
                        if (itemdisposeDetai.Item != null)
                        {
                            itemdisposeDetailTtemp.ItemCategoryId = itemdisposeDetai.Item.ItemCategoryId;
                        }

                        itemdisposeDetailTtemp.UnitId = itemdisposeDetai.UnitId;
                        itemdisposeDetailTtemp.UnitName = itemdisposeDetai.UnitOfMeasurement.Name;
                        itemdisposeDetailTtemp.Quantity = itemdisposeDetai.Quantity;
                        if (itemdisposeDetai.BinCard != null)
                        {
                            itemdisposeDetailTtemp.BinCardId = itemdisposeDetai.BinCard.Id;
                            itemdisposeDetailTtemp.BinCardNo = itemdisposeDetai.BinCard.CardNo;
                            BinCardQty = this.itemInventoryService.GetItemInventoryByItemIdAndBinCardId(itemdisposeDetai.ItemId, itemdisposeDetai.BinCard.Id);

                        }
                        if (BinCardQty != null)
                        {

                            itemdisposeDetailTtemp.BinCardQty = BinCardQty.Quantity;
                        }

                        itemdisposeDetailTtemp.DisposeReason = itemdisposeDetai.DisposeReason;

                        itemdisposeDetailVMList.Add(itemdisposeDetailTtemp);
                    }
                    itemdisposeTemp.ItemDisposeDetails = itemdisposeDetailVMList;
                }
            }
            return Json(itemdisposeTemp, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemDisposeViewModel
    {
        public ItemDisposeViewModel()
        {
            this.ItemDisposeDetails = new List<ItemDisposeDetailViewModel>();
        }
        public int Id { get; set; }
        public int? AuthorisedBy { get; set; }
        public string Date { get; set; }
        public string AuthorisedByName { get; set; }
        public string AuthorisedDate { get; set; }
        public virtual ICollection<ItemDisposeDetailViewModel> ItemDisposeDetails { get; set; }
    }

    public partial class ItemDisposeDetailViewModel
    {
        public int Id { get; set; }
        public int ItemDisposeId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemCategoryId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; }
        public double Quantity { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public string BinCardNo { get; set; }
        public double BinCardQty { get; set; }
        public string DisposeReason { get; set; }
    }
}