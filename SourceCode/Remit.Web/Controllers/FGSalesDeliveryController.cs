using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Service.Enums;
using Remit.Service.Utilities;
using Remit.Web.Helpers;
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class FGSalesDeliveryController : Controller
    {
        public readonly ICountryService countryService;
        public readonly IFGSaleService fgSaleService;
        public readonly IFGItemService fgItemService;
        public readonly IFGSalesDetailService fgSalesDetailService;
        public readonly IFGSalesDeliveryService fgSalesDeliveryService;
        public readonly IFGSalesDeliveryDetailService fgSalesDeliveryDetailService;
        public readonly IFGItemInventoryService fgItemInventoryService;
        public readonly IFGItemInventoryHistoryService fgItemInventoryHistoryService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();
        public readonly IWorkflowactionSettingService workflowactionSettingService;

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:fgSalesDelivery" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        const string url = "/FGSalesDelivery/Index";
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];

        // GET: /FGSalesDelivery/
        public ActionResult Index()
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGSalesDelivery");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }


        // for delivery by truck .....
        public ActionResult DeliveryByTruck()
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("DeliveryChallan");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public FGSalesDeliveryController(IFGSalesDetailService fgSalesDetailService, IFGSaleService fgSaleService, IFGItemService fgItemService, IFGSalesDeliveryService fgSalesDeliveryService, IFGSalesDeliveryDetailService fgSalesDeliveryDetailService, IFGItemInventoryService fgItemInventoryService, IFGItemInventoryHistoryService fgItemInventoryHistoryService, ICountryService countryService, IRoleSubModuleItemService roleSubModuleItemService, IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService, IWorkflowactionSettingService workflowactionSettingService)
        {
            this.fgItemService = fgItemService;
            this.fgSaleService = fgSaleService;
            this.fgSalesDetailService = fgSalesDetailService;
            this.fgSalesDeliveryService = fgSalesDeliveryService;
            this.fgSalesDeliveryDetailService = fgSalesDeliveryDetailService;
            this.fgItemInventoryService = fgItemInventoryService;
            this.fgItemInventoryHistoryService = fgItemInventoryHistoryService;
            this.countryService = countryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.fgItemInventoryWithoutBinService = fgItemInventoryWithoutBinService;
            this.workflowactionSettingService = workflowactionSettingService;
        }

        [HttpPost]
        public JsonResult CreateFGSalesDelivery(FGSalesDelivery fgSalesDelivery, List<FGSalesDeliveryDetailViewModel> FGSalesDeliveryDetails, bool isDelivered)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = this.fgSalesDeliveryService.GetFGSalesDelivery(fgSalesDelivery.DeliveryChallanNo);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    List<FGSalesDeliveryDetail> fgSalesDeliveryDetails = new List<FGSalesDeliveryDetail>();
                    foreach (var aFgSalesDeliveryDet in FGSalesDeliveryDetails)
                    {
                        foreach (var inv in aFgSalesDeliveryDet.FGItemInventoryViewModels)
                        {
                            FGSalesDeliveryDetail fgSalesDeliveryDetail = new FGSalesDeliveryDetail();
                            fgSalesDeliveryDetail.Id = Guid.NewGuid();
                            fgSalesDeliveryDetail.FGItemId = aFgSalesDeliveryDet.FGItemId;
                            fgSalesDeliveryDetail.FGGradeId = aFgSalesDeliveryDet.FGGradeId;
                            fgSalesDeliveryDetail.FGSizeId = aFgSalesDeliveryDet.FGSizeId;
                            fgSalesDeliveryDetail.Lot = aFgSalesDeliveryDet.Lot;
                            fgSalesDeliveryDetail.DeliveryChallanNo = fgSalesDelivery.DeliveryChallanNo;
                            fgSalesDeliveryDetail.DeliveryFGUnitId = aFgSalesDeliveryDet.DeliveryFGUnitId;
                            fgSalesDeliveryDetail.BinCardId = inv.BinCardId;
                            fgSalesDeliveryDetail.DeliveryQuantity = inv.DeliveryQuantity;

                            fgSalesDeliveryDetails.Add(fgSalesDeliveryDetail);
                        }
                    }

                    fgSalesDelivery.FGSalesDeliveryDetails = fgSalesDeliveryDetails;
                    fgSalesDelivery.DeliveryDate = fgSalesDelivery.DeliveryDate != null ? fgSalesDelivery.DeliveryDate.Value.ToUniversalTime() : DateTime.UtcNow;
                    fgSalesDelivery.CreatedBy = UserSession.GetUserFromSession().EmployeeId;
                    fgSalesDelivery.CreatedOn = DateTime.UtcNow;

                    if (fgSalesDelivery.FGSalesDeliveryDetails.Any())
                    {
                        foreach (var fgSalesDeliveryDet in fgSalesDelivery.FGSalesDeliveryDetails)
                        {
                            fgSalesDeliveryDet.Id = Guid.NewGuid();

                            var fgItemObj = fgItemService.GetFGItem((int)fgSalesDeliveryDet.FGItemId);

                            var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, (double)fgSalesDeliveryDet.DeliveryQuantity, (int)fgSalesDeliveryDet.DeliveryFGUnitId);
                            if (fgQty != null)
                            {
                                fgSalesDeliveryDet.DeliveryQuantityInSFT = fgQty.QuantityInSFT;
                                fgSalesDeliveryDet.DeliveryQuantityInCTN = fgQty.QuantityInCTN;
                                fgSalesDeliveryDet.DeliveryQuantityInPCs = fgQty.QuantityInPcs;
                                fgSalesDeliveryDet.DeliveryQuantityInSMT = fgQty.QuantityInSMT;
                            }

                            try
                            {
                                var referenceId = fgSalesDeliveryDet.Id.ToString();
                                var type = "FGSalesDeliveryDetail"; bool isCreate = true; bool isPlusBalance = false; var action = actionEnum.Create;

                                var check = fgInventoryUtility.MainFunction(fgSalesDeliveryDet.FGItemId.Value, fgSalesDeliveryDet.FGGradeId.Value,
                                    fgSalesDeliveryDet.Lot, fgSalesDeliveryDet.BinCardId, fgSalesDeliveryDet.DeliveryQuantity, fgSalesDeliveryDet.DeliveryQuantityInSFT,
                                    fgSalesDeliveryDet.DeliveryQuantityInSMT, fgSalesDeliveryDet.DeliveryQuantityInCTN, fgSalesDeliveryDet.DeliveryQuantityInPCs,
                                    fgSalesDeliveryDet.DeliveryFGUnitId, referenceId, type, UserSession.GetUserFromSession().EmployeeId, isPlusBalance, (int)action, isCreate, true, false);
                            }
                            catch { }
                        }
                    }

                    if (this.fgSalesDeliveryService.CreateFGSalesDelivery(fgSalesDelivery))
                    {
                        isSuccess = true;
                        message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                        //if (!string.IsNullOrEmpty(fgSalesDelivery.VATChallanNo) &&
                        //    !string.IsNullOrEmpty(fgSalesDelivery.TruckNo))
                        //{
                        var fgSale = this.fgSaleService.GetFGSale(fgSalesDelivery.InvoiceNo);
                        if (fgSale.isDelivered != isDelivered)
                        {
                            fgSale.isDelivered = isDelivered;
                            this.fgSaleService.UpdateFGSale(fgSale);
                        }
                        //}

                    }
                    else
                    {
                        message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
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
                    var fgSalesDeliveryObj = this.fgSalesDeliveryService.GetFGSalesDelivery(fgSalesDelivery.DeliveryChallanNo);
                    if (fgSalesDeliveryObj != null)
                    {
                        if (fgSalesDeliveryObj.FGSalesDeliveryDetails != null)
                        {
                            foreach (var fgSalesDeliveryDet in fgSalesDeliveryObj.FGSalesDeliveryDetails.ToList())
                            {
                                var fgItemObj = fgItemService.GetFGItem((int)fgSalesDeliveryDet.FGItemId);
                                var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, (double)fgSalesDeliveryDet.DeliveryQuantity, (int)fgSalesDeliveryDet.DeliveryFGUnitId);
                                if (fgQty != null)
                                {
                                    fgSalesDeliveryDet.DeliveryQuantityInSFT = fgQty.QuantityInSFT;
                                    fgSalesDeliveryDet.DeliveryQuantityInCTN = fgQty.QuantityInCTN;
                                    fgSalesDeliveryDet.DeliveryQuantityInPCs = fgQty.QuantityInPcs;
                                    fgSalesDeliveryDet.DeliveryQuantityInSMT = fgQty.QuantityInSMT;
                                }

                                try
                                {
                                    var referenceId = fgSalesDeliveryDet.Id.ToString();
                                    var type = "FGSalesDeliveryDetail"; bool isCreate = true; bool isPlusBalance = true; var action = actionEnum.Delete;

                                    var check = fgInventoryUtility.MainFunction(fgSalesDeliveryDet.FGItemId.Value, fgSalesDeliveryDet.FGGradeId.Value,
                                        fgSalesDeliveryDet.Lot, fgSalesDeliveryDet.BinCardId, fgSalesDeliveryDet.DeliveryQuantity, fgSalesDeliveryDet.DeliveryQuantityInSFT,
                                        fgSalesDeliveryDet.DeliveryQuantityInSMT, fgSalesDeliveryDet.DeliveryQuantityInCTN, fgSalesDeliveryDet.DeliveryQuantityInPCs,
                                        fgSalesDeliveryDet.DeliveryFGUnitId, referenceId, type, UserSession.GetUserFromSession().EmployeeId, isPlusBalance, (int)action, isCreate, true, false);
                                }
                                catch { }
                                this.fgSalesDeliveryDetailService.DeleteFGSalesDeliveryDetail(fgSalesDeliveryDet.Id);
                            }
                        }
                    }

                    foreach (var aFgSalesDeliveryDet in FGSalesDeliveryDetails)
                    {
                        foreach (var inv in aFgSalesDeliveryDet.FGItemInventoryViewModels)
                        {
                            FGSalesDeliveryDetail fgSalesDeliveryDetail = new FGSalesDeliveryDetail();
                            fgSalesDeliveryDetail.Id = Guid.NewGuid();
                            fgSalesDeliveryDetail.FGItemId = aFgSalesDeliveryDet.FGItemId;
                            fgSalesDeliveryDetail.FGGradeId = aFgSalesDeliveryDet.FGGradeId;
                            fgSalesDeliveryDetail.FGSizeId = aFgSalesDeliveryDet.FGSizeId;
                            fgSalesDeliveryDetail.Lot = aFgSalesDeliveryDet.Lot;
                            fgSalesDeliveryDetail.DeliveryChallanNo = fgSalesDelivery.DeliveryChallanNo;
                            fgSalesDeliveryDetail.DeliveryFGUnitId = aFgSalesDeliveryDet.DeliveryFGUnitId;
                            fgSalesDeliveryDetail.BinCardId = inv.BinCardId;
                            fgSalesDeliveryDetail.DeliveryQuantity = inv.DeliveryQuantity;

                            this.fgSalesDeliveryDetailService.CreateFGSalesDeliveryDetail(fgSalesDeliveryDetail);

                            var fgItemObj = fgItemService.GetFGItem((int)fgSalesDeliveryDetail.FGItemId);
                            var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, (double)fgSalesDeliveryDetail.DeliveryQuantity, (int)fgSalesDeliveryDetail.DeliveryFGUnitId);
                            if (fgQty != null)
                            {
                                fgSalesDeliveryDetail.DeliveryQuantityInSFT = fgQty.QuantityInSFT;
                                fgSalesDeliveryDetail.DeliveryQuantityInCTN = fgQty.QuantityInCTN;
                                fgSalesDeliveryDetail.DeliveryQuantityInPCs = fgQty.QuantityInPcs;
                                fgSalesDeliveryDetail.DeliveryQuantityInSMT = fgQty.QuantityInSMT;
                            }

                            try
                            {
                                var referenceId = fgSalesDeliveryDetail.Id.ToString();
                                var type = "FGSalesDeliveryDetail"; bool isCreate = true; bool isPlusBalance = false; var action = actionEnum.Create;

                                var check = fgInventoryUtility.MainFunction(fgSalesDeliveryDetail.FGItemId.Value, fgSalesDeliveryDetail.FGGradeId.Value,
                                    fgSalesDeliveryDetail.Lot, fgSalesDeliveryDetail.BinCardId, fgSalesDeliveryDetail.DeliveryQuantity, fgSalesDeliveryDetail.DeliveryQuantityInSFT,
                                    fgSalesDeliveryDetail.DeliveryQuantityInSMT, fgSalesDeliveryDetail.DeliveryQuantityInCTN, fgSalesDeliveryDetail.DeliveryQuantityInPCs,
                                    fgSalesDeliveryDetail.DeliveryFGUnitId, referenceId, type, UserSession.GetUserFromSession().EmployeeId, isPlusBalance, (int)action, isCreate, true, false);
                            }
                            catch { }
                        }
                    }

                    if (fgSalesDelivery.DeliveryDate != null)
                        fgSalesDeliveryObj.DeliveryDate = fgSalesDelivery.DeliveryDate.Value.ToUniversalTime();
                    fgSalesDeliveryObj.DeliverySite = fgSalesDelivery.DeliverySite;
                    fgSalesDeliveryObj.DriverName = fgSalesDelivery.DriverName;
                    fgSalesDeliveryObj.DriverPhone = fgSalesDelivery.DriverPhone;
                    fgSalesDeliveryObj.TruckNo = fgSalesDelivery.TruckNo;
                    fgSalesDeliveryObj.DriverName = fgSalesDelivery.DriverName;
                    fgSalesDeliveryObj.VATChallanNo = fgSalesDelivery.VATChallanNo;
                    fgSalesDeliveryObj.WeightDelivered = fgSalesDelivery.WeightDelivered;

                    if (this.fgSalesDeliveryService.UpdateFGSalesDelivery(fgSalesDeliveryObj))
                    {
                        isSuccess = true;
                        message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                        //if (!string.IsNullOrEmpty(fgSalesDelivery.VATChallanNo) &&
                        //    !string.IsNullOrEmpty(fgSalesDelivery.TruckNo))
                        //{
                        var fgSale = this.fgSaleService.GetFGSale(fgSalesDelivery.InvoiceNo);
                        if (fgSale.isDelivered != isDelivered)
                        {
                            fgSale.isDelivered = isDelivered;
                            this.fgSaleService.UpdateFGSale(fgSale);
                        }
                        //}
                    }
                    else
                    {
                        message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
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




        // update delivery challan ...............

        [HttpPost]
        public JsonResult UpdateDeliveryChallan(List<FGSalesDelivery> deliveryChallan)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId && a.SubModuleItem.UrlPath == url && a.WorkflowactionId == (int)WorkFlowActionEnum.Review).FirstOrDefault();



            if (permission.UpdateOperation == true && workflowactionSettingObj != null)
            {
                foreach (var adeliveryChallan in deliveryChallan)
                {
                    var DeliveryChallanObj = this.fgSalesDeliveryService.GetFGSalesDelivery(adeliveryChallan.DeliveryChallanNo);
                    if (DeliveryChallanObj != null)
                    {
                        DeliveryChallanObj.DriverName = adeliveryChallan.DriverName;
                        DeliveryChallanObj.DriverPhone = adeliveryChallan.DriverPhone;
                        DeliveryChallanObj.TruckNo = adeliveryChallan.TruckNo;
                        if (adeliveryChallan.DeliveryDate != null)
                        {
                            DeliveryChallanObj.DeliveryDate = adeliveryChallan.DeliveryDate.Value.ToUniversalTime();
                        }

                        DeliveryChallanObj.VATChallanNo = adeliveryChallan.VATChallanNo;

                    }
                    if (this.fgSalesDeliveryService.UpdateFGSalesDelivery(DeliveryChallanObj))
                    {
                        isSuccess = true;
                        message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGSalesDelivery.TitleDeliveryChallan);
                        if (DeliveryChallanObj != null && (!string.IsNullOrEmpty(DeliveryChallanObj.VATChallanNo) &&
                                                           !string.IsNullOrEmpty(DeliveryChallanObj.TruckNo)))
                        {
                            var fgSale = this.fgSaleService.GetFGSale(DeliveryChallanObj.InvoiceNo);
                            var prevDeliveryQuantity = this.fgSalesDeliveryDetailService.GetAllFGSalesDeliveryDetail().Where(a => a.FGSalesDelivery.InvoiceNo == fgSale.InvoiceNo).Sum(b => b.DeliveryQuantityInPCs);
                            if (fgSale.TotalPCs == prevDeliveryQuantity)
                            {
                                fgSale.isDelivered = true;
                                this.fgSaleService.UpdateFGSale(fgSale);
                            }
                        }
                    }
                    else
                    {
                        message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGSalesDelivery.TitleDeliveryChallan);
                    }

                }



            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToReview;
            }
            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult RejectFGSalesDelivery(FGSalesDelivery fgSalesDelivery)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId && a.SubModuleItem.UrlPath == url && a.WorkflowactionId == (int)WorkFlowActionEnum.ApproveReject).FirstOrDefault();

            if (permission.UpdateOperation == true && workflowactionSettingObj != null)
            {
                var fgSalesDeliveryObj = this.fgSalesDeliveryService.GetFGSalesDelivery(fgSalesDelivery.DeliveryChallanNo);
                if (fgSalesDeliveryObj.FGSalesDeliveryDetails != null)
                {
                    foreach (var fgSalesDeliveryDet in fgSalesDeliveryObj.FGSalesDeliveryDetails.ToList())
                    {
                        var fgItemObj = fgItemService.GetFGItem((int)fgSalesDeliveryDet.FGItemId);
                        var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, (double)fgSalesDeliveryDet.DeliveryQuantity, (int)fgSalesDeliveryDet.DeliveryFGUnitId);
                        if (fgQty != null)
                        {
                            fgSalesDeliveryDet.DeliveryQuantityInSFT = fgQty.QuantityInSFT;
                            fgSalesDeliveryDet.DeliveryQuantityInCTN = fgQty.QuantityInCTN;
                            fgSalesDeliveryDet.DeliveryQuantityInPCs = fgQty.QuantityInPcs;
                            fgSalesDeliveryDet.DeliveryQuantityInSMT = fgQty.QuantityInSMT;
                        }

                        try
                        {
                            var referenceId = fgSalesDeliveryDet.Id.ToString();
                            var type = "FGSalesDeliveryDetail"; bool isCreate = true; bool isPlusBalance = true; var action = actionEnum.Delete;

                            var check = fgInventoryUtility.MainFunction(fgSalesDeliveryDet.FGItemId.Value, fgSalesDeliveryDet.FGGradeId.Value,
                                fgSalesDeliveryDet.Lot, fgSalesDeliveryDet.BinCardId, fgSalesDeliveryDet.DeliveryQuantity, fgSalesDeliveryDet.DeliveryQuantityInSFT,
                                fgSalesDeliveryDet.DeliveryQuantityInSMT, fgSalesDeliveryDet.DeliveryQuantityInCTN, fgSalesDeliveryDet.DeliveryQuantityInPCs,
                                fgSalesDeliveryDet.DeliveryFGUnitId, referenceId, type, UserSession.GetUserFromSession().EmployeeId, isPlusBalance, (int)action, isCreate, true, false);
                        }
                        catch { }
                    }
                }

                fgSalesDeliveryObj.Status = (int)CommonEnum.Rejected;
                fgSalesDeliveryObj.DeletedBy = UserSession.GetUserFromSession().EmployeeId;
                fgSalesDeliveryObj.DeletedOn = DateTime.UtcNow;
                fgSalesDeliveryObj.Remarks = fgSalesDelivery.Remarks;

                if (this.fgSalesDeliveryService.UpdateFGSalesDelivery(fgSalesDeliveryObj))
                {
                    isSuccess = true;
                    message = string.Format(Resources.ResourceCommon.CMsg_reject, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notreject, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                }
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToReject;
            }
            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ApproveFGSalesDelivery(FGSalesDelivery fgSalesDelivery)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId && a.SubModuleItem.UrlPath == url && a.WorkflowactionId == (int)WorkFlowActionEnum.Approve).FirstOrDefault();

            if (permission.UpdateOperation == true && workflowactionSettingObj != null)
            {
                var fgSalesDeliveryObj = this.fgSalesDeliveryService.GetFGSalesDelivery(fgSalesDelivery.DeliveryChallanNo);

                fgSalesDeliveryObj.Status = (int)CommonEnum.Approved;
                fgSalesDeliveryObj.ApprovedBy = UserSession.GetUserFromSession().EmployeeId;
                fgSalesDeliveryObj.ApprovedOn = DateTime.UtcNow;
                fgSalesDeliveryObj.Remarks = fgSalesDelivery.Remarks;

                if (this.fgSalesDeliveryService.UpdateFGSalesDelivery(fgSalesDeliveryObj))
                {
                    isSuccess = true;
                    message = string.Format(Resources.ResourceCommon.CMsg_approve, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notapprove, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                }
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToApprove;
            }
            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReviewFGSalesDelivery(FGSalesDelivery fgSalesDelivery)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId && a.SubModuleItem.UrlPath == url && a.WorkflowactionId == (int)WorkFlowActionEnum.Review).FirstOrDefault();

            if (permission.UpdateOperation == true && workflowactionSettingObj != null)
            {
                var fgSalesDeliveryObj = this.fgSalesDeliveryService.GetFGSalesDelivery(fgSalesDelivery.DeliveryChallanNo);

                fgSalesDeliveryObj.Status = (int)CommonEnum.Review;
                fgSalesDeliveryObj.ReviewedBy = UserSession.GetUserFromSession().EmployeeId;
                fgSalesDeliveryObj.ReviewedOn = DateTime.UtcNow;
                fgSalesDeliveryObj.Remarks = fgSalesDelivery.Remarks;

                if (this.fgSalesDeliveryService.UpdateFGSalesDelivery(fgSalesDeliveryObj))
                {
                    isSuccess = true;
                    message = string.Format(Resources.ResourceCommon.CMsg_review, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notreview, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                }
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToReview;
            }
            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteFGSalesDelivery(FGSalesDelivery fgSalesDelivery)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                var fgSalesDeliveryObj = this.fgSalesDeliveryService.GetFGSalesDelivery(fgSalesDelivery.DeliveryChallanNo);
                if (fgSalesDeliveryObj.FGSalesDeliveryDetails != null)
                {
                    foreach (var fgSalesDeliveryDet in fgSalesDeliveryObj.FGSalesDeliveryDetails.ToList())
                    {
                        var fgItemObj = fgItemService.GetFGItem((int)fgSalesDeliveryDet.FGItemId);
                        var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, (double)fgSalesDeliveryDet.DeliveryQuantity, (int)fgSalesDeliveryDet.DeliveryFGUnitId);
                        if (fgQty != null)
                        {
                            fgSalesDeliveryDet.DeliveryQuantityInSFT = fgQty.QuantityInSFT;
                            fgSalesDeliveryDet.DeliveryQuantityInCTN = fgQty.QuantityInCTN;
                            fgSalesDeliveryDet.DeliveryQuantityInPCs = fgQty.QuantityInPcs;
                            fgSalesDeliveryDet.DeliveryQuantityInSMT = fgQty.QuantityInSMT;
                        }

                        try
                        {
                            var referenceId = fgSalesDeliveryDet.Id.ToString();
                            var type = "FGSalesDeliveryDetail"; bool isCreate = true; bool isPlusBalance = true; var action = actionEnum.Delete;

                            var check = fgInventoryUtility.MainFunction(fgSalesDeliveryDet.FGItemId.Value, fgSalesDeliveryDet.FGGradeId.Value,
                                fgSalesDeliveryDet.Lot, fgSalesDeliveryDet.BinCardId, fgSalesDeliveryDet.DeliveryQuantity, fgSalesDeliveryDet.DeliveryQuantityInSFT,
                                fgSalesDeliveryDet.DeliveryQuantityInSMT, fgSalesDeliveryDet.DeliveryQuantityInCTN, fgSalesDeliveryDet.DeliveryQuantityInPCs,
                                fgSalesDeliveryDet.DeliveryFGUnitId, referenceId, type, UserSession.GetUserFromSession().EmployeeId, isPlusBalance, (int)action, isCreate, true, false);
                        }
                        catch { }
                    }
                }

                fgSalesDeliveryObj.IsDelete = true;
                fgSalesDeliveryObj.DeletedBy = UserSession.GetUserFromSession().EmployeeId;
                fgSalesDeliveryObj.DeletedOn = DateTime.UtcNow;
                fgSalesDeliveryObj.Remarks = fgSalesDelivery.Remarks;

                if (this.fgSalesDeliveryService.UpdateFGSalesDelivery(fgSalesDeliveryObj))
                {
                    isSuccess = true;
                    message = string.Format(Resources.ResourceCommon.CMsg_delete, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
                    if (!string.IsNullOrEmpty(fgSalesDelivery.VATChallanNo) &&
                        !string.IsNullOrEmpty(fgSalesDelivery.TruckNo))
                    {
                        var fgSale = this.fgSaleService.GetFGSale(fgSalesDelivery.InvoiceNo);
                        fgSale.isDelivered = false;
                        this.fgSaleService.UpdateFGSale(fgSale);
                    }
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceFGSalesDelivery.TitleFGSalesDelivery);
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

        public WorkflowactionSetting WorkflowactionSettingObj(int? employeeId, string url, int workFlowStatusEnum)
        {
            return this.workflowactionSettingService
                .GetAllWorkflowactionSetting().Where(app => app.EmployeeId == employeeId &&
                                                            app.SubModuleItem.UrlPath == url &&
                                                            app.WorkflowactionId == workFlowStatusEnum).FirstOrDefault();
        }

        // new add for delivery challan truck no or vat c empty

        public JsonResult GetDeliveryChallanNoList()
        {
            var deliveryChallanNoListObj = this.fgSalesDeliveryService.GetAllFGSalesDelivery().Where(b => b.TruckNo == null || b.VATChallanNo == null);
            List<FGSalesDeliveryViewModel> deliveryChallanNoVMList = new List<FGSalesDeliveryViewModel>();

            foreach (var deliveryChallanNo in deliveryChallanNoListObj)
            {
                FGSalesDeliveryViewModel deliveryChallanNoTemp = new FGSalesDeliveryViewModel();
                deliveryChallanNoTemp.DeliveryChallanNo = deliveryChallanNo.DeliveryChallanNo;


                deliveryChallanNoVMList.Add(deliveryChallanNoTemp);
            }
            return Json(deliveryChallanNoVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGSalesDeliveryList()
        {
            var fgSalesDeliveryListObj = this.fgSalesDeliveryService.GetAllFGSalesDelivery().OrderByDescending(b => b.DeliveryDate);
            List<FGSalesDeliveryViewModel> fgSalesDeliveryVMList = new List<FGSalesDeliveryViewModel>();

            foreach (var fgSalesDelivery in fgSalesDeliveryListObj)
            {
                FGSalesDeliveryViewModel fgSalesDeliveryTemp = new FGSalesDeliveryViewModel();
                fgSalesDeliveryTemp.DeliveryChallanNo = fgSalesDelivery.DeliveryChallanNo;

                fgSalesDeliveryVMList.Add(fgSalesDeliveryTemp);
            }
            return Json(fgSalesDeliveryVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGSalesDeliveryListByDate(DateTime filterdate)
        {
            DateTime dt1 = filterdate.ToUniversalTime();
            DateTime dt2 = dt1.AddHours(24);
            var fgSalesDeliveryListObj = this.fgSalesDeliveryService.GetAllFGSalesDelivery().Where(a => (a.DeliveryDate >= dt1 && a.DeliveryDate < dt2) && !a.IsDelete).OrderByDescending(b => b.DeliveryDate);
            List<FGSalesDeliveryViewModel> fgSalesDeliveryVMList = new List<FGSalesDeliveryViewModel>();

            foreach (var fgSalesDelivery in fgSalesDeliveryListObj)
            {
                FGSalesDeliveryViewModel fgSalesDeliveryTemp = new FGSalesDeliveryViewModel();
                fgSalesDeliveryTemp.DeliveryChallanNo = fgSalesDelivery.DeliveryChallanNo;

                fgSalesDeliveryVMList.Add(fgSalesDeliveryTemp);
            }
            return Json(fgSalesDeliveryVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGSalesDeliveryListByInvoice(string invoiceno)
        {
            var fgSalesDeliveryListObj = this.fgSalesDeliveryService.GetAllFGSalesDelivery().Where(a => a.InvoiceNo == invoiceno && !a.IsDelete).OrderByDescending(b => b.DeliveryDate);
            List<FGSalesDeliveryViewModel> fgSalesDeliveryVMList = new List<FGSalesDeliveryViewModel>();

            foreach (var fgSalesDelivery in fgSalesDeliveryListObj)
            {
                FGSalesDeliveryViewModel fgSalesDeliveryTemp = new FGSalesDeliveryViewModel();
                fgSalesDeliveryTemp.DeliveryChallanNo = fgSalesDelivery.DeliveryChallanNo;

                fgSalesDeliveryVMList.Add(fgSalesDeliveryTemp);
            }
            return Json(fgSalesDeliveryVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGSalesDelivery(string challanNo)
        {
            var fgSalesDelivery = this.fgSalesDeliveryService.GetFGSalesDelivery(challanNo);

            FGSalesDeliveryViewModel fgSalesDeliveryTemp = new FGSalesDeliveryViewModel();

            fgSalesDeliveryTemp.DeliveryChallanNo = fgSalesDelivery.DeliveryChallanNo;
            fgSalesDeliveryTemp.InvoiceNo = fgSalesDelivery.InvoiceNo;

            var fgSales = this.fgSaleService.GetFGSale(fgSalesDelivery.InvoiceNo);
            fgSalesDeliveryTemp.InvoiceDate = fgSales.InvoiceDate;
            if (fgSales.InvoiceDate != null)
            {
                fgSalesDeliveryTemp.InvoiceDateString = fgSales.InvoiceDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                fgSalesDeliveryTemp.InvoiceDateStringForFireFox = fgSales.InvoiceDate.Value.AddMinutes(timeZoneOffset).ToString("s");
            }

            if (fgSalesDelivery.DeliveryDate != null)
            {
                fgSalesDeliveryTemp.DeliveryDate = fgSalesDelivery.DeliveryDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                fgSalesDeliveryTemp.DeliveryDateStringForFireFox = fgSalesDelivery.DeliveryDate.Value.AddMinutes(timeZoneOffset).ToString("s");
            }
            fgSalesDeliveryTemp.DealerId = fgSalesDelivery.DealerId;
            fgSalesDeliveryTemp.CustomerType = fgSalesDelivery.CustomerType;
            fgSalesDeliveryTemp.Name = fgSalesDelivery.Name;
            fgSalesDeliveryTemp.ContactPersonName = fgSalesDelivery.ContactPersonName;
            fgSalesDeliveryTemp.ContactPhone = fgSalesDelivery.ContactPhone;
            fgSalesDeliveryTemp.Address = fgSalesDelivery.Address;
            fgSalesDeliveryTemp.DeliverySite = fgSalesDelivery.DeliverySite;
            fgSalesDeliveryTemp.DeliverZoneId = fgSalesDelivery.DeliverZoneId;
            fgSalesDeliveryTemp.DriverName = fgSalesDelivery.DriverName;
            fgSalesDeliveryTemp.DriverPhone = fgSalesDelivery.DriverPhone;
            fgSalesDeliveryTemp.TruckNo = fgSalesDelivery.TruckNo;
            fgSalesDeliveryTemp.VATChallanNo = fgSalesDelivery.VATChallanNo;
            fgSalesDeliveryTemp.DeliveryOption = fgSalesDelivery.DeliveryOption;
            fgSalesDeliveryTemp.WeightDelivered = fgSalesDelivery.WeightDelivered;
            fgSalesDeliveryTemp.Status = fgSalesDelivery.Status;
            fgSalesDeliveryTemp.StatusText = ((CommonEnum)fgSalesDelivery.Status).ToString();
            fgSalesDeliveryTemp.Remarks = fgSalesDelivery.Remarks;

            List<FGSalesDeliveryDetailViewModel> fgSalesDeliveryDetailVMList = new List<FGSalesDeliveryDetailViewModel>();

            //fgSalesDeliveryTemp.HardApprove = false;
            //if (fgSalesDeliveryTemp.Status == (int)CommonEnum.Approved)
            //{
            var statusReview = (int)WorkFlowActionEnum.HardApprove;
            var user = UserSession.GetUserFromSession().EmployeeId;
            WorkflowactionSetting workflowactionSettingObj =
                WorkflowactionSettingObj(user, url, statusReview);
            fgSalesDeliveryTemp.HardApprove = workflowactionSettingObj != null;
            //}

            var grpByDetails = fgSalesDelivery.FGSalesDeliveryDetails.GroupBy(a => new { a.FGItemId, a.FGGradeId, a.Lot });
            foreach (var aitem in grpByDetails)
            {
                var FGSalesDeliveryDetai = fgSalesDelivery.FGSalesDeliveryDetails.FirstOrDefault(a => a.FGItemId == aitem.Key.FGItemId && a.FGGradeId == aitem.Key.FGGradeId && a.Lot == aitem.Key.Lot);
                FGSalesDeliveryDetailViewModel fgSalesDeliveryDetailTtemp = new FGSalesDeliveryDetailViewModel();
                fgSalesDeliveryDetailTtemp.FGItemId = FGSalesDeliveryDetai.FGItemId;
                fgSalesDeliveryDetailTtemp.SalesQuantity = this.fgSalesDetailService.GetAllFGSalesDetail().Where(a => a.FGItemId == FGSalesDeliveryDetai.FGItemId && a.FGGradeId == FGSalesDeliveryDetai.FGGradeId && a.Lot == FGSalesDeliveryDetai.Lot && a.FGSalesInvoiceNo == fgSalesDelivery.InvoiceNo && !a.FGSale.IsDelete).Sum(b => b.PackQuantity);
                if (FGSalesDeliveryDetai.FGItem != null)
                {
                    fgSalesDeliveryDetailTtemp.FGItemCode = FGSalesDeliveryDetai.FGItem.Code;
                    fgSalesDeliveryDetailTtemp.TypeId = FGSalesDeliveryDetai.FGItem.TypeId;
                    fgSalesDeliveryDetailTtemp.TypeName = FGSalesDeliveryDetai.FGItem.FGType != null ? FGSalesDeliveryDetai.FGItem.FGType.TypeName : "";
                    fgSalesDeliveryDetailTtemp.WeightPerCartoon = FGSalesDeliveryDetai.FGItem.WeightPerCartoon;
                    if (FGSalesDeliveryDetai.DeliveryFGUnitId == 1)
                    {
                        fgSalesDeliveryDetailTtemp.UnitPerCartoon = 1.0;
                    }
                    else if (FGSalesDeliveryDetai.DeliveryFGUnitId == 2)
                    {
                        fgSalesDeliveryDetailTtemp.UnitPerCartoon = FGSalesDeliveryDetai.FGItem.PcsPerCartoon;
                    }
                    else if (FGSalesDeliveryDetai.DeliveryFGUnitId == 3)
                    {
                        fgSalesDeliveryDetailTtemp.UnitPerCartoon = FGSalesDeliveryDetai.FGItem.PcsPerCartoon * FGSalesDeliveryDetai.FGItem.SftPerPiece;
                    }
                    else if (FGSalesDeliveryDetai.DeliveryFGUnitId == 4)
                    {
                        fgSalesDeliveryDetailTtemp.UnitPerCartoon = FGSalesDeliveryDetai.FGItem.PcsPerCartoon * FGSalesDeliveryDetai.FGItem.SmtPerPiece;
                    }
                }

                fgSalesDeliveryDetailTtemp.FGSizeId = FGSalesDeliveryDetai.FGSizeId;
                fgSalesDeliveryDetailTtemp.FGGradeId = FGSalesDeliveryDetai.FGGradeId;
                fgSalesDeliveryDetailTtemp.FGSizeName = FGSalesDeliveryDetai.FGSize != null ? FGSalesDeliveryDetai.FGSize.Size : "";
                fgSalesDeliveryDetailTtemp.FGGradeName = FGSalesDeliveryDetai.FGGrade != null ? FGSalesDeliveryDetai.FGGrade.Grade : "";
                fgSalesDeliveryDetailTtemp.Lot = FGSalesDeliveryDetai.Lot;

                List<FGItemInventoryViewModel> fgItemInventorys = new List<FGItemInventoryViewModel>();
                var getInventory = fgItemInventoryService.GetAllFGItemInventory().Where(a => a.FGItemId == FGSalesDeliveryDetai.FGItemId && a.FGGradeId == FGSalesDeliveryDetai.FGGradeId && a.Lot == FGSalesDeliveryDetai.Lot);
                double totaldeliveryqty = 0.0;
                foreach (var inv in getInventory)
                {
                    FGItemInventoryViewModel inventory = new FGItemInventoryViewModel();
                    inventory.BinCardId = inv.BinCardId;
                    if (inv.BinCard != null)
                        inventory.BinCardName = inv.BinCard.Warehouse.Name + "-" + inv.BinCard.CardNo;
                    if (FGSalesDeliveryDetai.DeliveryFGUnitId == 1)
                    {
                        inventory.Quantity = inv.QuantityInCTN;
                    }
                    else if (FGSalesDeliveryDetai.DeliveryFGUnitId == 2)
                    {
                        inventory.Quantity = inv.QuantityInPcs;
                    }
                    else if (FGSalesDeliveryDetai.DeliveryFGUnitId == 3)
                    {
                        inventory.Quantity = inv.QuantityInSFT;
                    }
                    else if (FGSalesDeliveryDetai.DeliveryFGUnitId == 4)
                    {
                        inventory.Quantity = inv.QuantityInSMT;
                    }

                    var deliveryDetList = fgSalesDelivery.FGSalesDeliveryDetails.Where(a => a.FGItemId == aitem.Key.FGItemId && a.FGGradeId == aitem.Key.FGGradeId && a.Lot == aitem.Key.Lot);
                    foreach (var deliveryDet in deliveryDetList)
                    {
                        if (inv.BinCardId == deliveryDet.BinCardId)
                        {
                            inventory.DeliveryQuantity = deliveryDet.DeliveryQuantity.Value;
                            break;
                        }
                    }
                    inventory.Quantity += inventory.DeliveryQuantity;
                    totaldeliveryqty += inventory.DeliveryQuantity;
                    fgItemInventorys.Add(inventory);
                }

                fgSalesDeliveryDetailTtemp.FGItemInventoryViewModels = fgItemInventorys;
                fgSalesDeliveryDetailTtemp.TotalStockQuantity = fgSalesDeliveryDetailTtemp.FGItemInventoryViewModels.Sum(b => b.Quantity);

                fgSalesDeliveryDetailTtemp.DeliveryFGUnitId = FGSalesDeliveryDetai.DeliveryFGUnitId;
                if (FGSalesDeliveryDetai.FGUOM != null)
                    fgSalesDeliveryDetailTtemp.UnitName = FGSalesDeliveryDetai.FGUOM.UnitName;

                fgSalesDeliveryDetailTtemp.PrevDeliveryQuantity = this.fgSalesDeliveryDetailService.GetAllFGSalesDeliveryDetail().Where(a => a.FGItemId == FGSalesDeliveryDetai.FGItemId && a.FGGradeId == FGSalesDeliveryDetai.FGGradeId && a.Lot == FGSalesDeliveryDetai.Lot && a.FGSalesDelivery.InvoiceNo == FGSalesDeliveryDetai.FGSalesDelivery.InvoiceNo && !a.FGSalesDelivery.IsDelete).Sum(b => b.DeliveryQuantity) - totaldeliveryqty;
                fgSalesDeliveryDetailTtemp.DeliveryQuantity = totaldeliveryqty;

                fgSalesDeliveryDetailTtemp.ProductWeight = aitem.Sum(a => a.ProductWeight);
                if (fgSalesDeliveryDetailTtemp.DeliveryQuantity > 0)
                    fgSalesDeliveryDetailVMList.Add(fgSalesDeliveryDetailTtemp);
            }
            fgSalesDeliveryTemp.FGSalesDeliveryDetails = fgSalesDeliveryDetailVMList;

            return Json(fgSalesDeliveryTemp, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFGSalesForDelivery(string invoiceNo)
        {
            var fgSales = this.fgSaleService.GetFGSale(invoiceNo);

            FGSalesDeliveryViewModel fgSalesDeliveryTemp = new FGSalesDeliveryViewModel();

            if (fgSales != null)
            {
                int deliverycount = this.fgSalesDeliveryService.GetAllFGSalesDelivery().Count(a => a.InvoiceNo == invoiceNo);
                fgSalesDeliveryTemp.InvoiceNo = fgSales.InvoiceNo;
                fgSalesDeliveryTemp.InvoiceDate = fgSales.InvoiceDate;
                if (fgSales.InvoiceDate != null)
                {
                    fgSalesDeliveryTemp.InvoiceDateString = fgSales.InvoiceDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                    fgSalesDeliveryTemp.InvoiceDateStringForFireFox = fgSales.InvoiceDate.Value.AddMinutes(timeZoneOffset).ToString("s");
                }
                fgSalesDeliveryTemp.DeliveryChallanNo = fgSales.InvoiceNo + "/" + (deliverycount + 1).ToString();
                fgSalesDeliveryTemp.DealerId = fgSales.DealerId;
                fgSalesDeliveryTemp.CustomerType = fgSales.CustomerType;
                if (fgSales.DeliveredBy == 1)
                    fgSalesDeliveryTemp.DeliveryOption = Resources.ResourceFGSale.Company;
                else if (fgSales.DeliveredBy == 2)
                    fgSalesDeliveryTemp.DeliveryOption = Resources.ResourceFGSale.Customer;
                fgSalesDeliveryTemp.Name = fgSales.Name;
                fgSalesDeliveryTemp.ContactPersonName = fgSales.ContactPersonName;
                fgSalesDeliveryTemp.ContactPhone = fgSales.ContactPhone;
                fgSalesDeliveryTemp.Address = fgSales.Address;
                fgSalesDeliveryTemp.DeliverySite = fgSales.DefaultDeliverySite;
                fgSalesDeliveryTemp.DeliverZoneId = fgSales.DeliverZoneId;
                List<FGSalesDeliveryDetailViewModel> fgSalesDetailVMList = new List<FGSalesDeliveryDetailViewModel>();
                if (fgSales.FGSalesDetails.Any())
                {
                    foreach (var FGSalesDetai in fgSales.FGSalesDetails.OrderBy(a => a.SlNo))
                    {
                        FGSalesDeliveryDetailViewModel fgSalesDeliveryDetailTtemp = new FGSalesDeliveryDetailViewModel();
                        fgSalesDeliveryDetailTtemp.FGItemId = FGSalesDetai.FGItemId;
                        fgSalesDeliveryDetailTtemp.SalesQuantity = FGSalesDetai.PackQuantity;
                        if (FGSalesDetai.FGItem != null)
                        {
                            fgSalesDeliveryDetailTtemp.FGItemCode = FGSalesDetai.FGItem.Code;
                            fgSalesDeliveryDetailTtemp.TypeId = FGSalesDetai.FGItem.TypeId;
                            fgSalesDeliveryDetailTtemp.TypeName = FGSalesDetai.FGItem.FGType != null ? FGSalesDetai.FGItem.FGType.TypeName : "";
                            fgSalesDeliveryDetailTtemp.WeightPerCartoon = FGSalesDetai.FGItem.WeightPerCartoon;
                            if (FGSalesDetai.PackFGUnitId == 1)
                            {
                                fgSalesDeliveryDetailTtemp.UnitPerCartoon = 1.0;
                            }
                            else if (FGSalesDetai.PackFGUnitId == 2)
                            {
                                fgSalesDeliveryDetailTtemp.UnitPerCartoon = FGSalesDetai.FGItem.PcsPerCartoon;
                            }
                            else if (FGSalesDetai.PackFGUnitId == 3)
                            {
                                fgSalesDeliveryDetailTtemp.UnitPerCartoon = FGSalesDetai.FGItem.PcsPerCartoon * FGSalesDetai.FGItem.SftPerPiece;
                            }
                            else if (FGSalesDetai.PackFGUnitId == 4)
                            {
                                fgSalesDeliveryDetailTtemp.UnitPerCartoon = FGSalesDetai.FGItem.PcsPerCartoon * FGSalesDetai.FGItem.SmtPerPiece;
                            }
                        }

                        fgSalesDeliveryDetailTtemp.FGSizeId = FGSalesDetai.FGSizeId;
                        fgSalesDeliveryDetailTtemp.FGGradeId = FGSalesDetai.FGGradeId;
                        fgSalesDeliveryDetailTtemp.FGSizeName = FGSalesDetai.FGSize != null ? FGSalesDetai.FGSize.Size : "";
                        fgSalesDeliveryDetailTtemp.FGGradeName = FGSalesDetai.FGGrade != null ? FGSalesDetai.FGGrade.Grade : "";
                        fgSalesDeliveryDetailTtemp.Lot = FGSalesDetai.Lot;

                        fgSalesDeliveryDetailTtemp.DeliveryFGUnitId = FGSalesDetai.PackFGUnitId;
                        if (FGSalesDetai.FGUOM != null)
                            fgSalesDeliveryDetailTtemp.UnitName = FGSalesDetai.FGUOM.UnitName;

                        fgSalesDeliveryDetailTtemp.PrevDeliveryQuantity = this.fgSalesDeliveryDetailService.GetAllFGSalesDeliveryDetail().Where(a => a.FGItemId == FGSalesDetai.FGItemId && a.FGGradeId == FGSalesDetai.FGGradeId && a.Lot == FGSalesDetai.Lot && a.FGSalesDelivery.InvoiceNo == invoiceNo && !a.FGSalesDelivery.IsDelete).Sum(b => b.DeliveryQuantity);

                        if (FGSalesDetai.FGItem != null && FGSalesDetai.FGSize != null && FGSalesDetai.FGGrade != null && FGSalesDetai.Lot != null)
                        {
                            List<FGItemInventoryViewModel> fgItemInventorys = new List<FGItemInventoryViewModel>();
                            var getInventory = fgItemInventoryService.GetAllFGItemInventory().Where(a => a.FGItemId == FGSalesDetai.FGItemId && a.FGGradeId == FGSalesDetai.FGGradeId && a.Lot == FGSalesDetai.Lot);

                            double RemainingQty = fgSalesDeliveryDetailTtemp.SalesQuantity.Value - fgSalesDeliveryDetailTtemp.PrevDeliveryQuantity.Value;
                            if (RemainingQty > 0)
                            {
                                foreach (var inv in getInventory)
                                {
                                    FGItemInventoryViewModel inventory = new FGItemInventoryViewModel();
                                    inventory.BinCardId = inv.BinCardId;
                                    if (inv.BinCard != null)
                                        inventory.BinCardName = inv.BinCard.Warehouse.Name + "-" + inv.BinCard.CardNo;
                                    if (FGSalesDetai.PackFGUnitId == 1)
                                    {
                                        inventory.Quantity = inv.QuantityInCTN;
                                    }
                                    else if (FGSalesDetai.PackFGUnitId == 2)
                                    {
                                        inventory.Quantity = inv.QuantityInPcs;
                                    }
                                    else if (FGSalesDetai.PackFGUnitId == 3)
                                    {
                                        inventory.Quantity = inv.QuantityInSFT;
                                    }
                                    else if (FGSalesDetai.PackFGUnitId == 4)
                                    {
                                        inventory.Quantity = inv.QuantityInSMT;
                                    }
                                    if (RemainingQty <= inventory.Quantity)
                                    {
                                        inventory.DeliveryQuantity = RemainingQty;
                                        RemainingQty = 0;
                                    }
                                    else
                                    {
                                        inventory.DeliveryQuantity = inventory.Quantity;
                                        RemainingQty -= inventory.Quantity;
                                    }
                                    fgItemInventorys.Add(inventory);
                                }
                                fgSalesDeliveryDetailTtemp.FGItemInventoryViewModels = fgItemInventorys;
                                fgSalesDeliveryDetailTtemp.TotalStockQuantity =
                                    fgSalesDeliveryDetailTtemp.FGItemInventoryViewModels.Sum(b => b.Quantity);
                                fgSalesDetailVMList.Add(fgSalesDeliveryDetailTtemp);
                            }
                        }

                    }
                }
                fgSalesDeliveryTemp.FGSalesDeliveryDetails = fgSalesDetailVMList;
            }
            return Json(fgSalesDeliveryTemp, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetFGSaleList()
        {
            var fgSaleListObj = this.fgSaleService.GetAllFGSale().Where(a => a.Status == (int)CommonEnum.Approved && !a.IsDelete);
            List<FGSaleViewModel> fgSaleVMList = new List<FGSaleViewModel>();

            foreach (var fgSale in fgSaleListObj)
            {
                FGSaleViewModel fgSaleTemp = new FGSaleViewModel();
                fgSaleTemp.InvoiceNo = fgSale.InvoiceNo;
                fgSaleTemp.Name = fgSale.Name;

                fgSaleVMList.Add(fgSaleTemp);
            }
            return Json(fgSaleVMList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetIncompleteFGSale()
        {
            var fgSaleListObj = this.fgSaleService.GetAllFGSale().Where(a => a.Status == (int)CommonEnum.Approved && !a.IsDelete && a.isDelivered == false && a.Reason != -1);
            List<FGSaleViewModel> fgSaleVMList = new List<FGSaleViewModel>();

            foreach (var fgSale in fgSaleListObj)
            {
                FGSaleViewModel fgSaleTemp = new FGSaleViewModel();
                fgSaleTemp.InvoiceNo = fgSale.InvoiceNo;
                fgSaleTemp.Name = fgSale.Name;

                fgSaleVMList.Add(fgSaleTemp);
            }
            return Json(fgSaleVMList, JsonRequestBehavior.AllowGet);
        }
    }

    public class FGSalesDeliveryViewModel
    {
        public FGSalesDeliveryViewModel()
        {
            this.FGSalesDeliveryDetails = new List<FGSalesDeliveryDetailViewModel>();
        }

        public string DeliveryChallanNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string InvoiceDateString { get; set; }
        public string InvoiceDateStringForFireFox { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryDateStringForFireFox { get; set; }
        public Nullable<int> DealerId { get; set; }
        public Nullable<int> CustomerType { get; set; }
        public string Name { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public string DeliverySite { get; set; }
        public Nullable<int> DeliverZoneId { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
        public string TruckNo { get; set; }
        public string VATChallanNo { get; set; }
        public string DeliveryOption { get; set; }
        public string WeightDelivered { get; set; }
        public bool IsDelete { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Remarks { get; set; }
        public bool HardApprove { set; get; }
        public virtual FGDealer FGDealer { get; set; }
        public virtual FGDealerZone FGDealerZone { get; set; }
        public virtual FGSale FGSale { get; set; }
        public virtual ICollection<FGSalesDeliveryDetailViewModel> FGSalesDeliveryDetails { get; set; }
    }

    public class FGSalesDeliveryDetailViewModel
    {
        public FGSalesDeliveryDetailViewModel()
        {
            this.FGItemInventoryViewModels = new List<FGItemInventoryViewModel>();
        }
        public System.Guid Id { get; set; }
        public string DeliveryChallanNo { get; set; }
        public Nullable<int> TypeId { get; set; }
        public string TypeName { get; set; }
        public Nullable<int> FGItemId { get; set; }
        public string FGItemCode { get; set; }
        public Nullable<double> UnitPerCartoon { get; set; }
        public Nullable<double> WeightPerCartoon { get; set; }
        public Nullable<int> FGSizeId { get; set; }
        public Nullable<int> FGGradeId { get; set; }
        public string FGSizeName { get; set; }
        public string FGGradeName { get; set; }
        public string Lot { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public Nullable<double> SalesQuantity { get; set; }
        public Nullable<double> TotalStockQuantity { set; get; }
        public Nullable<double> PrevDeliveryQuantity { get; set; }
        public Nullable<double> DeliveryQuantity { get; set; }
        public Nullable<int> DeliveryFGUnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<double> ProductWeight { get; set; }
        public virtual BinCard BinCard { get; set; }
        public virtual FGGrade FGGrade { get; set; }
        public virtual FGItem FGItem { get; set; }
        public virtual FGSalesDelivery FGSalesDelivery { get; set; }
        public virtual FGSize FGSize { get; set; }
        public virtual FGUOM FGUOM { get; set; }
        public Nullable<System.Guid> FGSalesDetailId { get; set; }
        public List<FGItemInventoryViewModel> FGItemInventoryViewModels { get; set; }
    }
}