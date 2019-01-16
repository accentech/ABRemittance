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
using Remit.Service.Utilities;
using System.Linq.Dynamic;
using Remit.Web.Models;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class ItemReceiveController : Controller
    {
        public readonly IItemReceivePurchaseRequisitionMappingService itemReceivePurchaseRequisitionMappingService;
        public readonly IItemReceiveService itemReceiveService;
        public readonly IItemReceiveDetailService itemReceiveDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly ICommercialInvoiceService commercialInvoiceService;
        public readonly ICommercialInvoiceDetailService commercialInvoiceDetailService;
        public readonly IPurchaseRequisitionService purchaseRequisitionService;
        public readonly IPurchaseRequisitionDetailService purchaseRequisitionDetailService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly INotificationSettingService notificationSettingService;

        public readonly IItemInventoryService itemInventoryService;
        public readonly IItemInventoryHistoryService itemInventoryHistoryService;

        public readonly ISupplierService supplierService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public ItemReceiveController(ISupplierService supplierService, IItemReceiveService itemReceiveService, IItemReceivePurchaseRequisitionMappingService itemReceivePurchaseRequisitionMappingService,
            ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService, ICommercialInvoiceService commercialInvoiceService, ICommercialInvoiceDetailService commercialInvoiceDetailService,
            IPurchaseRequisitionService purchaseRequisitionService, IPurchaseRequisitionDetailService purchaseRequisitionDetailService, IItemReceiveDetailService itemReceiveDetailService, IWorkflowactionSettingService workflowactionSettingService,
            IItemInventoryService itemInventoryService, IItemInventoryHistoryService itemInventoryHistoryService,
            INotificationSettingService notificationSettingService, IItemService itemService)
        {
            this.supplierService = supplierService;
            this.itemReceivePurchaseRequisitionMappingService = itemReceivePurchaseRequisitionMappingService;
            this.itemReceiveService = itemReceiveService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.commercialInvoiceService = commercialInvoiceService;
            this.commercialInvoiceDetailService = commercialInvoiceDetailService;
            this.purchaseRequisitionService = purchaseRequisitionService;
            this.purchaseRequisitionDetailService = purchaseRequisitionDetailService;
            this.itemReceiveDetailService = itemReceiveDetailService;

            this.itemInventoryHistoryService = itemInventoryHistoryService;
            this.itemInventoryService = itemInventoryService;

            this.workflowactionSettingService = workflowactionSettingService;
            this.notificationSettingService = notificationSettingService;
            this.itemService = itemService;
        }

        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:itemreceive" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        string url = "/ItemReceive/ByLC";

        string grpType = WebConfigurationManager.AppSettings["GroupType"];

        // GET: /ItemReceive/
        public ActionResult ByLC()
        {
            Session["GroupT"] = "raw";
            Session["RCT"] = "LC";
            return CommonView("ItemReceiveByLC");
        }

        public ActionResult SparePartsAndOtherByLC()
        {
            Session["GroupT"] = "spare";
            Session["RCT"] = "LC";
            return CommonView("ItemReceiveByLC");
        }

        public ActionResult NonLC()
        {
            Session["GroupT"] = "raw";
            Session["RCT"] = "NonLC";
            return CommonView("ItemReceiveNonLC");
        }

        public ActionResult SparePartsAndOtherNonLC()
        {
            Session["GroupT"] = "spare";
            Session["RCT"] = "NonLC";
            return CommonView("ItemReceiveNonLC");
        }

        private ActionResult CommonView(string viewName)
        {
            permission = CheckPermission();

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View(viewName);
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        private RoleSubModuleItem CheckPermission()
        {
            if ((string)Session["GroupT"] == "raw")
            {
                cacheKey += grpType;

                if ((string)Session["RCT"] == "LC")
                {
                    url = "/ItemReceive/ByLC";
                }
                else
                {
                    url = "/ItemReceive/NonLC";
                }
            }
            else
            {
                if ((string)Session["RCT"] == "LC")
                {
                    url = "/ItemReceive/SparePartsAndOtherByLC";
                }
                else
                {
                    url = "/ItemReceive/SparePartsAndOtherNonLC";
                }
            }

            return (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);
        }

        [HttpPost]
        public JsonResult CreateItemReceive(ItemReceive itemReceive, List<ItemReceiveDetailViewModel> isReceivedList)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                var isNew = itemReceive.Id == Guid.Empty ? true : false;

                permission = CheckPermission(); 

                if (isNew)
                {
                    itemReceive.Id = Guid.NewGuid();
                    itemReceive.ReceivedBy = UserSession.GetUserFromSession().EmployeeId;
                    itemReceive.ReceivedDate = itemReceive.ReceivedDate != null ? itemReceive.ReceivedDate.Value.ToUniversalTime() : DateTime.UtcNow;

                    foreach (var itemReceiveDetail in itemReceive.ItemReceiveDetails)
                    {
                        itemReceiveDetail.Id = Guid.NewGuid();
                        itemReceiveDetail.ItemReceiveId = itemReceive.Id;
                    }
                    foreach (var IRPRMapping in itemReceive.ItemReceivePurchaseRequisitionMappings)
                    {
                        IRPRMapping.Id = Guid.NewGuid();
                        IRPRMapping.ItemReceiveId = itemReceive.Id;
                    }
                    if (permission.CreateOperation == true)
                    {
                        if (this.itemReceiveService.CreateItemReceive(itemReceive))
                        {
                            foreach (var itemReceiveDetail in itemReceive.ItemReceiveDetails)
                            {
                                var itemDetail = this.itemService.GetItem(itemReceiveDetail.ItemId);
                                double PurchaseToUsageConversionRatio = itemDetail != null? itemDetail.PurchaseToUsageConversionRatio:1.0;
                                int? UsageUnitId = itemDetail != null ? itemDetail.UsageUnitId : null;
                                InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService, itemInventoryService);
                                try
                                {
                                    ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(a => a.BinCardId == itemReceiveDetail.BinCardId && a.ItemId == itemReceiveDetail.ItemId).FirstOrDefault();
                                    if (itemInventory != null)
                                    {
                                        inventoryUtility.UpdateItmeBalance(itemInventory, itemReceiveDetail.ReceivedQuantity * PurchaseToUsageConversionRatio, true);
                                    }
                                    else
                                    {
                                        itemInventory = new ItemInventory();
                                        itemInventory.Id = Guid.NewGuid();
                                        itemInventory.ItemId = itemReceiveDetail.ItemId;
                                        itemInventory.Quantity = itemReceiveDetail.ReceivedQuantity * PurchaseToUsageConversionRatio;
                                        itemInventory.UnitId = UsageUnitId;
                                        itemInventory.BinCardId = itemReceiveDetail.BinCardId;

                                        this.itemInventoryService.CreateItemInventory(itemInventory);
                                    }
                                    inventoryUtility.SaveInventoryHistory(itemReceiveDetail.ItemId, itemReceiveDetail.ReceivedQuantity * PurchaseToUsageConversionRatio, UsageUnitId, itemReceiveDetail.BinCardId, itemReceiveDetail.Id.ToString(), "Item Receive", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Create);
                                }
                                catch { }
                            }
                            
                            isSuccess = true;
                            foreach (var isReceived in isReceivedList)
                            {
                                if (isReceived.IsReceived)
                                {
                                    if (itemReceive.CommercialInvoiceId != null)
                                    {
                                        var cidetails = this.commercialInvoiceService.GetCommercialInvoice((Guid)itemReceive.CommercialInvoiceId).CommercialInvoiceDetails.Where(x => x.ItemId == isReceived.ItemId);
                                        foreach (var b in cidetails)
                                        {
                                            b.IsReceived = true;
                                            this.commercialInvoiceDetailService.UpdateCommercialInvoiceDetail(b);
                                        }
                                    }
                                    else if (itemReceive.ItemReceivePurchaseRequisitionMappings.Any())
                                    {
                                        foreach (var IRPRMapping in itemReceive.ItemReceivePurchaseRequisitionMappings)
                                        {
                                            var prdetails = this.purchaseRequisitionDetailService.GetAllPurchaseRequisitionDetail().Where(x => x.ItemId == isReceived.ItemId && x.PurchaseRequisitionId == IRPRMapping.PurchaseRequisitionId);
                                            foreach (var b in prdetails)
                                            {
                                                b.IsReceived = true;
                                                this.purchaseRequisitionDetailService.UpdatePurchaseRequisitionDetail(b);
                                            }
                                        } 
                                    }
                                }
                            }
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceItemReceive.LblItemReceive);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceItemReceive.LblItemReceive);
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
                        var itemReceiveObj = this.itemReceiveService.GetItemReceive(itemReceive.Id);
                        if (itemReceiveObj.ItemReceiveDetails != null)
                        {
                            foreach (var itemReceiveObjDetail in itemReceiveObj.ItemReceiveDetails.ToList())
                            {
                                InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService, itemInventoryService);
                                try
                                {
                                    ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(a => a.BinCardId == itemReceiveObjDetail.BinCardId && a.ItemId == itemReceiveObjDetail.ItemId).FirstOrDefault();
                                    if (itemInventory != null)
                                    {
                                        inventoryUtility.UpdateItmeBalance(itemInventory, itemReceiveObjDetail.ReceivedQuantity * itemReceiveObjDetail.Item.PurchaseToUsageConversionRatio, false);
                                    }
                                    inventoryUtility.SaveInventoryHistory(itemReceiveObjDetail.ItemId, itemReceiveObjDetail.ReceivedQuantity * itemReceiveObjDetail.Item.PurchaseToUsageConversionRatio, itemReceiveObjDetail.Item.UsageUnitId, itemReceiveObjDetail.BinCardId, itemReceiveObjDetail.Id.ToString(), "Item Receive", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Delete);
                                }
                                catch { }

                                this.itemReceiveDetailService.DeleteItemReceiveDetail(itemReceiveObjDetail.Id);
                            }
                        }

                        if (itemReceive.ItemReceiveDetails != null)
                        {
                            foreach (var itemReceiveDetail in itemReceive.ItemReceiveDetails)
                            {
                                itemReceiveDetail.Id = Guid.NewGuid();
                                itemReceiveDetail.ItemReceiveId = itemReceive.Id;
                                this.itemReceiveDetailService.CreateItemReceiveDetail(itemReceiveDetail);

                                var itemDetail = this.itemService.GetItem(itemReceiveDetail.ItemId);
                                double PurchaseToUsageConversionRatio = itemDetail != null ? itemDetail.PurchaseToUsageConversionRatio : 1.0;
                                int? UsageUnitId = itemDetail != null ? itemDetail.UsageUnitId : null;

                                InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService, itemInventoryService);
                                try
                                {
                                    ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(a => a.BinCardId == itemReceiveDetail.BinCardId && a.ItemId == itemReceiveDetail.ItemId).FirstOrDefault();
                                    if (itemInventory != null)
                                    {
                                        inventoryUtility.UpdateItmeBalance(itemInventory, itemReceiveDetail.ReceivedQuantity * PurchaseToUsageConversionRatio, true);
                                    }
                                    else
                                    {
                                        itemInventory = new ItemInventory();
                                        itemInventory.Id = Guid.NewGuid();
                                        itemInventory.ItemId = itemReceiveDetail.ItemId;
                                        itemInventory.Quantity = itemReceiveDetail.ReceivedQuantity * PurchaseToUsageConversionRatio;
                                        itemInventory.UnitId = UsageUnitId;
                                        itemInventory.BinCardId = itemReceiveDetail.BinCardId;

                                        this.itemInventoryService.CreateItemInventory(itemInventory);
                                    }
                                    inventoryUtility.SaveInventoryHistory(itemReceiveDetail.ItemId, itemReceiveDetail.ReceivedQuantity * PurchaseToUsageConversionRatio, UsageUnitId, itemReceiveDetail.BinCardId, itemReceiveDetail.Id.ToString(), "Item Receive", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Create);
                                }
                                catch { }
                            }
                        }
                        if (itemReceiveObj.ItemReceivePurchaseRequisitionMappings != null)
                        {
                            foreach (var IRPRMapping in itemReceiveObj.ItemReceivePurchaseRequisitionMappings.ToList())
                            {
                                this.itemReceivePurchaseRequisitionMappingService.DeleteItemReceivePurchaseRequisitionMapping(IRPRMapping.Id);
                            }
                        }
                        if (itemReceive.ItemReceivePurchaseRequisitionMappings != null)
                        {
                            foreach (var IRPRMapping in itemReceive.ItemReceivePurchaseRequisitionMappings)
                            {
                                IRPRMapping.Id = Guid.NewGuid();
                                IRPRMapping.ItemReceiveId = itemReceive.Id;
                                this.itemReceivePurchaseRequisitionMappingService.CreateItemReceivePurchaseRequisitionMapping(IRPRMapping);
                            }
                        }

                        itemReceiveObj.SupplierId = itemReceive.SupplierId;
                        itemReceiveObj.InvoiceNo = itemReceive.InvoiceNo;
                        if (itemReceive.ReceivedDate != null)
                            itemReceiveObj.ReceivedDate = itemReceive.ReceivedDate.Value.ToUniversalTime();
                        itemReceiveObj.SupplierId = itemReceive.SupplierId;
                        itemReceiveObj.VehicleNo = itemReceive.VehicleNo;
                        itemReceiveObj.DriverName = itemReceive.DriverName;
                        itemReceiveObj.LoadedTruckWeight = itemReceive.LoadedTruckWeight;
                        itemReceiveObj.EmptyTruckWeight = itemReceive.EmptyTruckWeight;
                        itemReceiveObj.PurchasedFrom = itemReceive.PurchasedFrom;
                        itemReceiveObj.CommercialInvoiceId = itemReceive.CommercialInvoiceId;

                        if (this.itemReceiveService.UpdateItemReceive(itemReceiveObj))
                        {
                            isSuccess = true;
                            foreach (var isReceived in isReceivedList)
                            {
                                if (isReceived.IsReceived)
                                {
                                    if (itemReceive.CommercialInvoiceId != null)
                                    {
                                        var cidetails = this.commercialInvoiceService.GetCommercialInvoice((Guid)itemReceive.CommercialInvoiceId).CommercialInvoiceDetails.Where(x => x.ItemId == isReceived.ItemId);
                                        foreach (var b in cidetails)
                                        {
                                            b.IsReceived = true;
                                            this.commercialInvoiceDetailService.UpdateCommercialInvoiceDetail(b);
                                        }
                                    }
                                    else
                                    {
                                        //**************//
                                    }
                                }
                            }
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceItemReceive.LblItemReceive);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceItemReceive.LblItemReceive);
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
                    }
                }

                if (itemReceive.CommercialInvoiceId == null && !itemReceive.ItemReceivePurchaseRequisitionMappings.Any())
                {
                    PurchaseRequisition purchaseRequisition = new PurchaseRequisition();
                    purchaseRequisition.Id = Guid.NewGuid();
                    purchaseRequisition.RequisitionNo = System.DateTime.UtcNow.ToString("yyyyMMMddhhmmss");
                    purchaseRequisition.RequisitionDate = System.DateTime.UtcNow;
                    purchaseRequisition.ApprovedBy = UserSession.GetUserFromSession().EmployeeId;
                    purchaseRequisition.IsAutoGenerated = true;
                    purchaseRequisition.Status = (int?)CommonEnum.Approved;

                    List<ItemReceivePurchaseRequisitionMapping> itemReceivePurchaseRequisitionMappingVMList = new List<ItemReceivePurchaseRequisitionMapping>();
                    ItemReceivePurchaseRequisitionMapping itemReceivePurchaseRequisitionMapping = new ItemReceivePurchaseRequisitionMapping();
                    itemReceivePurchaseRequisitionMapping.Id = Guid.NewGuid();
                    itemReceivePurchaseRequisitionMapping.PurchaseRequisitionId = purchaseRequisition.Id;
                    itemReceivePurchaseRequisitionMapping.ItemReceiveId = itemReceive.Id;
                    itemReceivePurchaseRequisitionMappingVMList.Add(itemReceivePurchaseRequisitionMapping);
                    purchaseRequisition.ItemReceivePurchaseRequisitionMappings = itemReceivePurchaseRequisitionMappingVMList;

                    List<PurchaseRequisitionDetail> purchaseRequisitionDetailVMList = new List<PurchaseRequisitionDetail>();
                    foreach (var itemReceiveDetail in itemReceive.ItemReceiveDetails)
                    {
                        PurchaseRequisitionDetail purchaseRequisitionDetail = new PurchaseRequisitionDetail();
                        purchaseRequisitionDetail.Id = Guid.NewGuid();
                        purchaseRequisitionDetail.PurchaseRequisitionId = purchaseRequisition.Id;
                        purchaseRequisitionDetail.ItemId = itemReceiveDetail.ItemId;
                        purchaseRequisitionDetail.Quantity = itemReceiveDetail.ReceivedQuantity;
                        purchaseRequisitionDetail.IsReceived = true;
                        purchaseRequisitionDetailVMList.Add(purchaseRequisitionDetail);
                    }
                    purchaseRequisition.PurchaseRequisitionDetails = purchaseRequisitionDetailVMList;
                    this.purchaseRequisitionService.CreatePurchaseRequisition(purchaseRequisition);
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

        public JsonResult DeleteItemReceive(Guid id)
        {
            var isSuccess = true;
            var message = string.Empty;
            
            permission = CheckPermission(); 

            if (permission.DeleteOperation == true)
            {
                var itemReceive = this.itemReceiveService.GetItemReceive(id);
                if (itemReceive != null)
                {
                    foreach (var itemReceiveDetail in itemReceive.ItemReceiveDetails.ToList())
                    {
                        InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService, itemInventoryService);
                        try
                        {
                            ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(a => a.BinCardId == itemReceiveDetail.BinCardId && a.ItemId == itemReceiveDetail.ItemId).FirstOrDefault();
                            if (itemInventory != null)
                            {
                                inventoryUtility.UpdateItmeBalance(itemInventory, itemReceiveDetail.ReceivedQuantity * itemReceiveDetail.Item.PurchaseToUsageConversionRatio, false);
                            }
                            inventoryUtility.SaveInventoryHistory(itemReceiveDetail.ItemId, itemReceiveDetail.ReceivedQuantity * itemReceiveDetail.Item.PurchaseToUsageConversionRatio, itemReceiveDetail.Item.UsageUnitId, itemReceiveDetail.BinCardId, itemReceiveDetail.Id.ToString(), "Item Receive", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Delete);
                        }
                        catch { }

                        //this.itemReceiveDetailService.DeleteItemReceiveDetail(itemReceiveDetail.Id);

                        if (itemReceiveDetail.ItemReceive.CommercialInvoiceId != null)
                        {
                            var cidetail = this.commercialInvoiceDetailService.GetAllCommercialInvoiceDetail().Where(a => a.ItemId == itemReceiveDetail.ItemId & a.CommercialInvoiceId == itemReceiveDetail.ItemReceive.CommercialInvoiceId).FirstOrDefault();
                            if (cidetail != null)
                            {
                                cidetail.IsReceived = false;
                                this.commercialInvoiceDetailService.UpdateCommercialInvoiceDetail(cidetail);
                            }
                        }
                    }

                    var chkPRs = this.itemReceivePurchaseRequisitionMappingService.GetAllItemReceivePurchaseRequisitionMapping().Where(a => a.ItemReceiveId == id);

                    foreach (var chkPR in chkPRs)
                    {
                        this.itemReceivePurchaseRequisitionMappingService.DeleteItemReceivePurchaseRequisitionMapping(chkPR.Id);
                    }

                    itemReceive.IsDeleted = true;
                    itemReceive.DeletedBy = UserSession.GetUserFromSession().EmployeeId;
                    itemReceive.DeletedOn = DateTime.UtcNow;

                    isSuccess = this.itemReceiveService.UpdateItemReceive(itemReceive);
                    message = string.Format(isSuccess ? Resources.ResourceCommon.CMsg_delete : Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceItemReceive.LblItemReceive);
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

        public JsonResult GetItemReceiveList()
        {
            var itemreceiveListObj = new List<ItemReceive>();
            if ((string)Session["GroupT"] == "raw")
            {
                itemreceiveListObj = this.itemReceiveService.GetAllItemReceive().Where(a => a.IsDeleted != true && a.ItemReceiveDetails.Count() > 0 && a.ItemReceiveDetails.FirstOrDefault().Item.ItemCategory.ItemGroup.TypeId == Convert.ToInt32(grpType)).ToList();
            }
            else
            {
                itemreceiveListObj = this.itemReceiveService.GetAllItemReceive().Where(a => a.IsDeleted != true && a.ItemReceiveDetails.Count() > 0 && a.ItemReceiveDetails.FirstOrDefault().Item.ItemCategory.ItemGroup.TypeId != Convert.ToInt32(grpType)).ToList();
            }

            List<ItemReceiveViewModel> itemreceiveVMList = new List<ItemReceiveViewModel>();

            foreach (var itemreceive in itemreceiveListObj)
            {
                ItemReceiveViewModel itemreceiveTemp = new ItemReceiveViewModel();

                itemreceiveTemp.Id = itemreceive.Id;

                itemreceiveTemp.ReceivedDate = itemreceive.ReceivedDate != null ? itemreceive.ReceivedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemreceiveTemp.ReceivedBy = itemreceive.ReceivedBy;

                if (itemreceive.Employee != null)
                {
                    itemreceiveTemp.ReceivedByName = itemreceive.Employee.FullName;
                }

                itemreceiveVMList.Add(itemreceiveTemp);
            }
            return Json(itemreceiveVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReceiveListByYear(int year, int month)
        {
            var itemreceiveListObj = new List<ItemReceive>();
            if ((string)Session["GroupT"] == "raw")
            {
                itemreceiveListObj = this.itemReceiveService.GetAllItemReceive().Where(a => a.ReceivedDate.Value.AddMinutes(timeZoneOffset).Year == year && a.ReceivedDate.Value.AddMinutes(timeZoneOffset).Month == month && a.IsDeleted != true && a.ItemReceiveDetails.Count() > 0 && a.ItemReceiveDetails.FirstOrDefault().Item.ItemCategory.ItemGroup.TypeId == Convert.ToInt32(grpType)).ToList();
            }
            else
            {
                itemreceiveListObj = this.itemReceiveService.GetAllItemReceive().Where(a => a.ReceivedDate.Value.AddMinutes(timeZoneOffset).Year == year && a.ReceivedDate.Value.AddMinutes(timeZoneOffset).Month == month && a.IsDeleted != true && a.ItemReceiveDetails.Count() > 0 && a.ItemReceiveDetails.FirstOrDefault().Item.ItemCategory.ItemGroup.TypeId != Convert.ToInt32(grpType)).ToList();
            }

            List<ItemReceiveViewModel> itemreceiveVMList = new List<ItemReceiveViewModel>();
            foreach (var itemreceive in itemreceiveListObj)
            {
                if (itemreceive.CommercialInvoice == null)
                {
                    ItemReceiveViewModel itemreceiveTemp = new ItemReceiveViewModel();

                    itemreceiveTemp.Id = itemreceive.Id;
                    itemreceiveTemp.CommercialInvoiceId = itemreceive.CommercialInvoiceId;

                    itemreceiveTemp.ReceivedDate = itemreceive.ReceivedDate != null ? itemreceive.ReceivedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";

                    itemreceiveVMList.Add(itemreceiveTemp);
                }
            }
            return Json(itemreceiveVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetReceiveListByCommercialInvoiceId(Guid commercialInvoiceId)
        {
            var itemreceiveListObj = this.itemReceiveService.GetAllItemReceive().Where(a => a.CommercialInvoiceId == commercialInvoiceId && a.IsDeleted != true).OrderByDescending(b => b.ReceivedDate);

            List<ItemReceiveViewModel> itemreceiveVMList = new List<ItemReceiveViewModel>();
            foreach (var itemreceive in itemreceiveListObj)
            {
                ItemReceiveViewModel itemreceiveTemp = new ItemReceiveViewModel();

                itemreceiveTemp.Id = itemreceive.Id;
                itemreceiveTemp.CommercialInvoiceId = itemreceive.CommercialInvoiceId;
                itemreceiveTemp.ReceivedDate = itemreceive.ReceivedDate != null ? itemreceive.ReceivedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";

                itemreceiveVMList.Add(itemreceiveTemp);
            }
            return Json(itemreceiveVMList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetItemReceive(Guid id)
        {
            var itemreceive = this.itemReceiveService.GetItemReceive(id);
            ItemReceiveViewModel itemreceiveTemp = null;

            Guid lastId = this.itemReceiveService.GetAllItemReceive().Where(a => a.IsDeleted != true).OrderByDescending(a => a.ReceivedDate).FirstOrDefault().Id;

            if (itemreceive != null)
            {
                List<Guid> ids = new List<Guid>();
                foreach (var IRPRMapping in itemreceive.ItemReceivePurchaseRequisitionMappings)
                {
                    ids.Add(IRPRMapping.PurchaseRequisitionId);
                }

                var itemDetails = this.purchaseRequisitionDetailService.GetAllPurchaseRequisitionDetail().Where(a => ids.Contains(a.PurchaseRequisitionId)).GroupBy(b => b.Item).Select(c => new { Item = c.Key, Quantity = c.Sum(d => d.Quantity) });

                itemreceiveTemp = new ItemReceiveViewModel();
                itemreceiveTemp.Id = itemreceive.Id;
                
                if(lastId == itemreceive.Id)
                    itemreceiveTemp.IsLast = true;
                else
                    itemreceiveTemp.IsLast = false;

                itemreceiveTemp.ReceivedDate = itemreceive.ReceivedDate != null ? itemreceive.ReceivedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                if (itemreceive.CommercialInvoice != null)
                    itemreceiveTemp.LCId = itemreceive.CommercialInvoice.LCId;
                itemreceiveTemp.CommercialInvoiceId = itemreceive.CommercialInvoiceId;
                itemreceiveTemp.InvoiceNo = itemreceive.InvoiceNo;
                itemreceiveTemp.VehicleNo = itemreceive.VehicleNo;
                itemreceiveTemp.DriverName = itemreceive.DriverName;
                itemreceiveTemp.LoadedTruckWeight = itemreceive.LoadedTruckWeight;
                itemreceiveTemp.EmptyTruckWeight = itemreceive.EmptyTruckWeight;
                itemreceiveTemp.PurchasedFrom = itemreceive.PurchasedFrom;
                itemreceiveTemp.SupplierId = itemreceive.SupplierId;
                itemreceiveTemp.ReceivedBy = itemreceive.ReceivedBy;

                if (itemreceive.ItemReceiveDetails.Count() > 0)
                {
                    List<ItemReceiveDetailViewModel> itemreceiveDetailVMList = new List<ItemReceiveDetailViewModel>();
                    foreach (var itemreceiveDetai in itemreceive.ItemReceiveDetails)
                    {
                        ItemReceiveDetailViewModel itemreceiveDetailTtemp = new ItemReceiveDetailViewModel();
                        itemreceiveDetailTtemp.Id = itemreceiveDetai.Id;
                        itemreceiveDetailTtemp.ItemReceiveId = itemreceiveDetai.ItemReceiveId;
                        itemreceiveDetailTtemp.LCItemId = itemreceiveDetai.LCItemId;
                        if (itemreceiveDetai.Item1 != null)
                        {
                            itemreceiveDetailTtemp.LCItemName = itemreceiveDetai.Item1.Name;
                        }
                        itemreceiveDetailTtemp.ItemId = itemreceiveDetai.ItemId;
                        if (itemreceiveDetai.Item != null)
                        {
                            itemreceiveDetailTtemp.ItemName = itemreceiveDetai.Item.Name;
                            itemreceiveDetailTtemp.ItemSize = itemreceiveDetai.Item.Size;
                            itemreceiveDetailTtemp.ItemSpecification = itemreceiveDetai.Item.Specification;
                            itemreceiveDetailTtemp.ItemCategoryId = itemreceiveDetai.Item.ItemCategoryId;
                        }
                        itemreceiveDetailTtemp.ReceivedQuantity = itemreceiveDetai.ReceivedQuantity;
                        itemreceiveDetailTtemp.UnitId = itemreceiveDetai.UnitId;
                        if (itemreceiveDetai.UnitOfMeasurement != null)
                            itemreceiveDetailTtemp.UnitName = itemreceiveDetai.UnitOfMeasurement.Name;
                        itemreceiveDetailTtemp.PerUnitPrice = itemreceiveDetai.PerUnitPrice;
                        itemreceiveDetailTtemp.CurrencyId = itemreceiveDetai.CurrencyId;
                        itemreceiveDetailTtemp.BinCardId = itemreceiveDetai.BinCardId;
                        itemreceiveDetailTtemp.ExpiryDate = itemreceiveDetai.ExpiryDate != null ? itemreceiveDetai.ExpiryDate.Value.ToString(dateTimeFormat) : "";
                        itemreceiveDetailTtemp.Remarks = itemreceiveDetai.Remarks;
                        if (itemreceive.CommercialInvoice != null)
                        {
                            itemreceiveDetailTtemp.CIQuantity = this.commercialInvoiceDetailService.GetAllCommercialInvoiceDetail().Where(a => a.CommercialInvoiceId == itemreceive.CommercialInvoiceId && a.ItemId == itemreceiveDetai.LCItemId.Value).Sum(b => b.Quantity);
                            itemreceiveDetailTtemp.PrevReceivedQuantity = this.itemReceiveDetailService.GetAllItemReceiveDetail().Where(a => a.ItemReceive.CommercialInvoiceId != null && a.ItemReceive.CommercialInvoiceId == itemreceive.CommercialInvoiceId && a.ItemId == itemreceiveDetai.ItemId).Sum(b => b.ReceivedQuantity) - itemreceiveDetai.ReceivedQuantity;
                        }
                        else
                        {
                            itemreceiveDetailTtemp.PRQuantity = itemDetails.Where(a => a.Item == itemreceiveDetai.Item).Sum(b=>b.Quantity);
                            var recvids = this.itemReceivePurchaseRequisitionMappingService.GetAllItemReceivePurchaseRequisitionMapping().Where(a => ids.Contains(a.PurchaseRequisitionId)).GroupBy(x => x.ItemReceiveId).Select(b => b.Key);
                            itemreceiveDetailTtemp.PrevReceivedQuantity = this.itemReceiveDetailService.GetAllItemReceiveDetail().Where(a => a.ItemId == itemreceiveDetailTtemp.ItemId && recvids.Contains(a.ItemReceiveId)).Sum(b => b.ReceivedQuantity) - itemreceiveDetai.ReceivedQuantity;
                        }

                        itemreceiveDetailVMList.Add(itemreceiveDetailTtemp);
                    }
                    itemreceiveTemp.ItemReceiveDetails = itemreceiveDetailVMList;
                }
                if (itemreceive.ItemReceivePurchaseRequisitionMappings.Count() > 0)
                {
                    List<ItemReceivePurchaseRequisitionMappingViewModel> itemreceivePRVMList = new List<ItemReceivePurchaseRequisitionMappingViewModel>();
                    foreach (var IRPRMapping in itemreceive.ItemReceivePurchaseRequisitionMappings)
                    {
                        ItemReceivePurchaseRequisitionMappingViewModel itemreceivePRTtemp = new ItemReceivePurchaseRequisitionMappingViewModel();
                        itemreceivePRTtemp.Id = IRPRMapping.Id;
                        itemreceivePRTtemp.ItemReceiveId = (Guid)IRPRMapping.ItemReceiveId;
                        itemreceivePRTtemp.PurchaseRequisitionId = (Guid)IRPRMapping.PurchaseRequisitionId;
                        itemreceivePRTtemp.RequisitionNo = IRPRMapping.PurchaseRequisition != null ? IRPRMapping.PurchaseRequisition.RequisitionNo : "";
                        itemreceivePRVMList.Add(itemreceivePRTtemp);
                    }
                    itemreceiveTemp.ItemReceivePurchaseRequisitionMappings = itemreceivePRVMList;
                }
            }
            return Json(itemreceiveTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getNotReceivedItemByCommercialInvoiceId(Guid id)
        {
            var commercialInvoice = this.commercialInvoiceService.GetCommercialInvoice(id);

            CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();

            commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;
            List<CommercialInvoiceDetailViewModel> commercialInvoiceDetailVMList = new List<CommercialInvoiceDetailViewModel>();
            if (commercialInvoice.CommercialInvoiceDetails.Any())
            {
                foreach (var CommercialInvoiceDetai in commercialInvoice.CommercialInvoiceDetails.Where(a => a.IsReceived != true))
                {
                    CommercialInvoiceDetailViewModel commercialInvoiceDetailTtemp = new CommercialInvoiceDetailViewModel();
                    commercialInvoiceDetailTtemp.ItemId = CommercialInvoiceDetai.ItemId;
                    if (CommercialInvoiceDetai.Item != null)
                    {
                        commercialInvoiceDetailTtemp.ItemName = CommercialInvoiceDetai.Item.Name;
                    }
                    commercialInvoiceDetailTtemp.UnitId = CommercialInvoiceDetai.UnitId;
                    commercialInvoiceDetailTtemp.UnitName = CommercialInvoiceDetai.UnitOfMeasurement != null ? CommercialInvoiceDetai.UnitOfMeasurement.Name : "";
                    commercialInvoiceDetailTtemp.UnitPrice = CommercialInvoiceDetai.UnitPrice;
                    commercialInvoiceDetailTtemp.Quantity = CommercialInvoiceDetai.Quantity;
                    commercialInvoiceDetailTtemp.PrevReceivedQuantity = this.itemReceiveDetailService.GetAllItemReceiveDetail().Where(a => a.ItemId == commercialInvoiceDetailTtemp.ItemId && a.ItemReceive.CommercialInvoiceId == id).Sum(b => b.ReceivedQuantity);
                    commercialInvoiceDetailVMList.Add(commercialInvoiceDetailTtemp);
                }
            }
            commercialInvoiceTemp.CommercialInvoiceDetails = commercialInvoiceDetailVMList;

            return Json(commercialInvoiceTemp, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getItemByCommercialInvoiceId(Guid id)
        {
            var commercialInvoice = this.commercialInvoiceService.GetCommercialInvoice(id);

            CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();

            commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;
            List<CommercialInvoiceDetailViewModel> commercialInvoiceDetailVMList = new List<CommercialInvoiceDetailViewModel>();
            if (commercialInvoice.CommercialInvoiceDetails.Any())
            {
                foreach (var CommercialInvoiceDetai in commercialInvoice.CommercialInvoiceDetails)
                {
                    CommercialInvoiceDetailViewModel commercialInvoiceDetailTtemp = new CommercialInvoiceDetailViewModel();
                    commercialInvoiceDetailTtemp.ItemId = CommercialInvoiceDetai.ItemId;
                    if (CommercialInvoiceDetai.Item != null)
                    {
                        commercialInvoiceDetailTtemp.ItemName = CommercialInvoiceDetai.Item.Name;
                    }
                    commercialInvoiceDetailTtemp.UnitId = CommercialInvoiceDetai.UnitId;
                    commercialInvoiceDetailTtemp.UnitName = CommercialInvoiceDetai.UnitOfMeasurement != null ? CommercialInvoiceDetai.UnitOfMeasurement.Name : "";
                    commercialInvoiceDetailTtemp.UnitPrice = CommercialInvoiceDetai.UnitPrice;
                    commercialInvoiceDetailTtemp.Quantity = CommercialInvoiceDetai.Quantity;
                    commercialInvoiceDetailTtemp.PrevReceivedQuantity = this.itemReceiveDetailService.GetAllItemReceiveDetail().Where(a => a.ItemId == commercialInvoiceDetailTtemp.ItemId && a.ItemReceive.CommercialInvoiceId == id).Sum(b => b.ReceivedQuantity);
                    commercialInvoiceDetailVMList.Add(commercialInvoiceDetailTtemp);
                }
            }
            commercialInvoiceTemp.CommercialInvoiceDetails = commercialInvoiceDetailVMList;

            return Json(commercialInvoiceTemp, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getPreviousReceivedQuantityByItemId(Guid ciId, Guid itemId)
        {
            var PrevReceivedQuantity = this.itemReceiveDetailService.GetAllItemReceiveDetail().Where(a => a.ItemId == itemId && a.ItemReceive.CommercialInvoiceId == ciId).Sum(b => b.ReceivedQuantity);

            return Json(PrevReceivedQuantity, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getPRItemByPRIDs(Guid[] ids)
        {
            var itemDetails = this.purchaseRequisitionDetailService.GetAllPurchaseRequisitionDetail().Where(a => ids.Contains(a.PurchaseRequisitionId)).GroupBy(b => b.Item).Select(c => new { Item = c.Key, Quantity = c.Sum(d=>d.Quantity) });

            ItemReceiveViewModel itemReceiveTemp = new ItemReceiveViewModel();

            List<ItemReceiveDetailViewModel> itemReceiveDetailVMList = new List<ItemReceiveDetailViewModel>();
            if (itemDetails.Any())
            {
                foreach (var itemDetai in itemDetails)
                {
                    ItemReceiveDetailViewModel itemReceiveDetailTtemp = new ItemReceiveDetailViewModel();
                    itemReceiveDetailTtemp.ItemId = itemDetai.Item.Id;
                    if (itemDetai.Item != null)
                    {
                        itemReceiveDetailTtemp.ItemName = itemDetai.Item.Name;
                        itemReceiveDetailTtemp.ItemSize = itemDetai.Item.Size;
                        itemReceiveDetailTtemp.ItemSpecification = itemDetai.Item.Specification;
                        itemReceiveDetailTtemp.ItemCategoryId = itemDetai.Item.ItemCategoryId;
                        itemReceiveDetailTtemp.UnitId = itemDetai.Item.PurchaseunitId;
                        if (itemDetai.Item.UnitOfMeasurement != null)
                            itemReceiveDetailTtemp.UnitName = itemDetai.Item.UnitOfMeasurement.Name;
                    }
                    itemReceiveDetailTtemp.PRQuantity = itemDetai.Quantity;
                    var recvids = this.itemReceivePurchaseRequisitionMappingService.GetAllItemReceivePurchaseRequisitionMapping().Where(a => ids.Contains(a.PurchaseRequisitionId)).GroupBy(x => x.ItemReceiveId).Select(b => b.Key);
                    itemReceiveDetailTtemp.PrevReceivedQuantity = this.itemReceiveDetailService.GetAllItemReceiveDetail().Where(a => a.ItemId == itemReceiveDetailTtemp.ItemId && recvids.Contains(a.ItemReceiveId)).Sum(b => b.ReceivedQuantity);
                    itemReceiveDetailVMList.Add(itemReceiveDetailTtemp);
                }
            }
            itemReceiveTemp.ItemReceiveDetails = itemReceiveDetailVMList;

            return Json(itemReceiveTemp, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemReceiveViewModel
    {
        public ItemReceiveViewModel()
        {
            this.ItemReceiveDetails = new List<ItemReceiveDetailViewModel>();
            this.ItemReceivePurchaseRequisitionMappings = new List<ItemReceivePurchaseRequisitionMappingViewModel>();
        }
        public System.Guid Id { get; set; }
        public bool IsLast { get; set; }
        public Nullable<System.Guid> LCId { get; set; }
        public Nullable<System.Guid> CommercialInvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public string ReceivedDate { get; set; }
        public string VehicleNo { get; set; }
        public string DriverName { get; set; }
        public Nullable<double> LoadedTruckWeight { get; set; }
        public Nullable<double> EmptyTruckWeight { get; set; }
        public string PurchasedFrom { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public Nullable<int> ReceivedBy { get; set; }
        public string ReceivedByName { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual CommercialInvoice CommercialInvoice { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<ItemReceiveDetailViewModel> ItemReceiveDetails { get; set; }
        public virtual ICollection<ItemReceivePurchaseRequisitionMappingViewModel> ItemReceivePurchaseRequisitionMappings { get; set; }
    }

    public partial class ItemReceiveDetailViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid ItemReceiveId { get; set; }
        public Nullable<System.Guid> LCItemId { get; set; }
        public string LCItemName { get; set; }
        public int? ItemCategoryId { get; set; }
        public Nullable<double> CIQuantity { get; set; }
        public Nullable<double> PRQuantity { get; set; }
        public System.Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemSize { get; set; }
        public string ItemSpecification { get; set; }
        public Nullable<double> PrevReceivedQuantity { get; set; }
        public Nullable<double> ReceivedQuantity { get; set; }
        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<double> PerUnitPrice { get; set; }
        public Nullable<int> CurrencyId { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public string BinCardNo { get; set; }
        public string ExpiryDate { get; set; }
        public string Remarks { get; set; }
        public bool IsReceived { get; set; }
        public virtual BinCard BinCard { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual Item Item { get; set; }
        public virtual ItemReceive ItemReceive { get; set; }
        public virtual UnitOfMeasurement UnitOfMeasurement { get; set; }
    }
    public partial class ItemReceivePurchaseRequisitionMappingViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid ItemReceiveId { get; set; }
        public System.Guid PurchaseRequisitionId { get; set; }
        public string RequisitionNo { get; set; }
        public virtual ItemReceive ItemReceive { get; set; }
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }
    }
    public partial class IsReceivedViewModel
    {
        public System.Guid ItemId { get; set; }
        public bool IsReceived { get; set; }
    }
}