using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Data.Repository;
using Remit.Model.Models;
using Remit.Service;
using Remit.Service.Enums;
using Remit.Service.Utilities;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class FGItemAdjustmentController : Controller
    {
        public readonly IFGItemAdjustmentService fgItemAdjustmentService;
        public readonly IFGItemAdjustmentDetailService fgItemAdjustmentDetailService;
        public readonly IFGItemService fgItemService;
        public readonly IFGItemInventoryService fgItemInventoryService;
        public readonly IFGItemInventoryHistoryService fgItemInventoryHistoryService;
        public readonly IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService;

        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public FGItemAdjustmentController(IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService, IFGItemAdjustmentService fgItemAdjustmentService, IRoleSubModuleItemService roleSubModuleItemService, IFGItemAdjustmentDetailService fgItemAdjustmentDetailService,
            IWorkflowactionSettingService workflowactionSettingService, IFGItemInventoryService fgItemInventoryService, IFGItemInventoryHistoryService fgItemInventoryHistoryService
        , IFGItemService fgItemService)
        {
            this.fgItemInventoryHistoryService = fgItemInventoryHistoryService;
            this.fgItemAdjustmentService = fgItemAdjustmentService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.fgItemAdjustmentDetailService = fgItemAdjustmentDetailService;
            this.fgItemInventoryService = fgItemInventoryService;
            this.fgItemInventoryWithoutBinService = fgItemInventoryWithoutBinService;
            this.fgItemService = fgItemService;
        }

        string cacheKey = "permission:fgitemAdjustment" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        const string url = "/FGItemAdjustment/Index";

        // GET: /FGItemAdjustment/
        public ActionResult Index()
        {
            RoleSubModuleItem permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                                    roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(
                                                        url, Helpers.UserSession.GetUserFromSession().RoleId);
            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGItemAdjustment");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateFGItemAdjustment(FGItemAdjustment fgItemAdjustment)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                             roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                 Helpers.UserSession.GetUserFromSession().RoleId);

                var isNew = fgItemAdjustment.Id == 0 ? true : false;
                
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {          
                        foreach (var fgItemAdjustmentDetail in fgItemAdjustment.FGItemAdjustmentDetails)
                        {
                            fgItemAdjustmentDetail.Id = Guid.NewGuid();
                            var fgItemObj = fgItemService.GetFGItem(fgItemAdjustmentDetail.FGItemId);
                            var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, fgItemAdjustmentDetail.InventoryQuantity - fgItemAdjustmentDetail.ActualQuantity, (int)fgItemAdjustmentDetail.SalesUnitId);
                            if (fgQty != null)
                            {
                                fgItemAdjustmentDetail.QuantityInSFT = fgQty.QuantityInSFT;
                                fgItemAdjustmentDetail.QuantityInCTN = fgQty.QuantityInCTN;
                                fgItemAdjustmentDetail.QuantityInPCs = fgQty.QuantityInPcs;
                                fgItemAdjustmentDetail.QuantityInSMT = fgQty.QuantityInSMT;
                            }
                        }

                        fgItemAdjustment.AdjustmentDate = fgItemAdjustment.AdjustmentDate != null ? fgItemAdjustment.AdjustmentDate.Value.ToUniversalTime() : DateTime.UtcNow;
                        
                        if (this.fgItemAdjustmentService.CreateFGItemAdjustment(fgItemAdjustment))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
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
                        var fgItemAdjustmentObj = this.fgItemAdjustmentService.GetFGItemAdjustment(fgItemAdjustment.Id);
                        if (fgItemAdjustmentObj != null)
                        {
                            if (fgItemAdjustmentObj.FGItemAdjustmentDetails != null)
                            {
                                foreach (var fgItemAdjustmentDet in fgItemAdjustmentObj.FGItemAdjustmentDetails.ToList())
                                {
                                    this.fgItemAdjustmentDetailService.DeleteFGItemAdjustmentDetail(fgItemAdjustmentDet.Id);
                                }
                            }
                        }

                        foreach (var fgItemAdjustmentDetail in fgItemAdjustment.FGItemAdjustmentDetails)
                        {
                            fgItemAdjustmentDetail.Id = Guid.NewGuid();
                            fgItemAdjustmentDetail.FGItemAdjustmentId = fgItemAdjustment.Id;

                            var fgItemObj = fgItemService.GetFGItem(fgItemAdjustmentDetail.FGItemId);
                            var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, fgItemAdjustmentDetail.InventoryQuantity - fgItemAdjustmentDetail.ActualQuantity, (int)fgItemAdjustmentDetail.SalesUnitId);
                            if (fgQty != null)
                            {
                                fgItemAdjustmentDetail.QuantityInSFT = fgQty.QuantityInSFT;
                                fgItemAdjustmentDetail.QuantityInCTN = fgQty.QuantityInCTN;
                                fgItemAdjustmentDetail.QuantityInPCs = fgQty.QuantityInPcs;
                                fgItemAdjustmentDetail.QuantityInSMT = fgQty.QuantityInSMT;
                            }

                            this.fgItemAdjustmentDetailService.CreateFGItemAdjustmentDetail(fgItemAdjustmentDetail);
                        }

                        if (fgItemAdjustment.AdjustmentDate != null)
                            fgItemAdjustmentObj.AdjustmentDate = fgItemAdjustment.AdjustmentDate.Value.ToUniversalTime();

                        if (this.fgItemAdjustmentService.UpdateFGItemAdjustment(fgItemAdjustmentObj))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
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

        //Approve........
        public JsonResult ApproveAdjustment(FGItemAdjustment fgItemAdjustment)
        {
            var isSuccess = false;
            var message = string.Empty;
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);
            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId && a.SubModuleItem.UrlPath == url).FirstOrDefault();

            if (permission.UpdateOperation == true && workflowactionSettingObj != null)
            {
                var fgItemAdjustmentObj = this.fgItemAdjustmentService.GetFGItemAdjustment(fgItemAdjustment.Id);
                if (fgItemAdjustmentObj != null)
                {
                    if (fgItemAdjustmentObj.FGItemAdjustmentDetails != null)
                    {
                        foreach (var fgItemAdjustmentDet in fgItemAdjustmentObj.FGItemAdjustmentDetails.ToList())
                        {
                            this.fgItemAdjustmentDetailService.DeleteFGItemAdjustmentDetail(fgItemAdjustmentDet.Id);
                        }
                    }
                }

                foreach (var fgItemAdjustmentDetail in fgItemAdjustment.FGItemAdjustmentDetails)
                {
                    fgItemAdjustmentDetail.Id = Guid.NewGuid();
                    fgItemAdjustmentDetail.FGItemAdjustmentId = fgItemAdjustment.Id;

                    var fgItemObj = fgItemService.GetFGItem(fgItemAdjustmentDetail.FGItemId);
                    var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, fgItemAdjustmentDetail.InventoryQuantity - fgItemAdjustmentDetail.ActualQuantity, (int)fgItemAdjustmentDetail.SalesUnitId);
                    if (fgQty != null)
                    {
                        fgItemAdjustmentDetail.QuantityInSFT = fgQty.QuantityInSFT;
                        fgItemAdjustmentDetail.QuantityInCTN = fgQty.QuantityInCTN;
                        fgItemAdjustmentDetail.QuantityInPCs = fgQty.QuantityInPcs;
                        fgItemAdjustmentDetail.QuantityInSMT = fgQty.QuantityInSMT;
                    }

                    this.fgItemAdjustmentDetailService.CreateFGItemAdjustmentDetail(fgItemAdjustmentDetail);
                }

                if (fgItemAdjustment.AdjustmentDate != null)
                    fgItemAdjustmentObj.AdjustmentDate = fgItemAdjustment.AdjustmentDate.Value.ToUniversalTime();
                fgItemAdjustmentObj.AuthorisedBy = UserSession.GetUserFromSession().EmployeeId;
                fgItemAdjustmentObj.AuthorisedDate = DateTime.UtcNow;

                if (this.fgItemAdjustmentService.UpdateFGItemAdjustment(fgItemAdjustmentObj))
                {
                    foreach (var fgItemAdjustmentDetail in fgItemAdjustment.FGItemAdjustmentDetails)
                    {
                        var referenceId = fgItemAdjustmentDetail.Id.ToString();
                        var type = "FGItemAdjustmentDetail"; bool isCreate = false; bool isPlusBalance = false; var action = actionEnum.Create;
                        var transactionBy = UserSession.GetUserFromSession().EmployeeId;
                        fgInventoryUtility.MainFunction(fgItemAdjustmentDetail.FGItemId, fgItemAdjustmentDetail.FGGradeId,
                        fgItemAdjustmentDetail.Lot, fgItemAdjustmentDetail.BinCardId, fgItemAdjustmentDetail.InventoryQuantity - fgItemAdjustmentDetail.ActualQuantity, fgItemAdjustmentDetail.QuantityInSFT,
                        fgItemAdjustmentDetail.QuantityInSMT, fgItemAdjustmentDetail.QuantityInCTN, fgItemAdjustmentDetail.QuantityInPCs,
                        fgItemAdjustmentDetail.SalesUnitId, referenceId, type, transactionBy, isPlusBalance, (int)action, isCreate);
                    }
                    
                    isSuccess = true;
                    message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
                }
            }
            else
            {
                message = Resources.ResourceCommon.MsgNoPermissionToApprove;
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteFGItemAdjustment(int id)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                var fgitemAdjustment = this.fgItemAdjustmentService.GetFGItemAdjustment(id);

                fgitemAdjustment.DeleteFlag = true;
                if (this.fgItemAdjustmentService.UpdateFGItemAdjustment(fgitemAdjustment))
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_delete,
                        Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete,
                        Resources.ResourceFGItemAdjustment.LblFGItemAdjustment);
                    isSuccess = false;
                }
            }
            else
            {
                isSuccess = false;
                message = Resources.ResourceCommon.MsgNoPermissionToDelete;
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGItemAdjustmentList()
        {
            var fgItemAdjustmentListObj = this.fgItemAdjustmentService.GetAllFGItemAdjustment().Where(fg => fg.DeleteFlag != true);
            List<FGItemAdjustmentViewModel> fgItemAdjustmentVMList = new List<FGItemAdjustmentViewModel>();
            foreach (var fgItemAdjustment in fgItemAdjustmentListObj)
            {
                FGItemAdjustmentViewModel fgItemAdjustmentTemp = new FGItemAdjustmentViewModel();
                fgItemAdjustmentTemp.Id = fgItemAdjustment.Id;
                fgItemAdjustmentTemp.AdjustmentDate = fgItemAdjustment.AdjustmentDate.Value.AddMinutes(timeZoneOffset);
                fgItemAdjustmentTemp.AdjustmentDateString = fgItemAdjustment.AdjustmentDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                fgItemAdjustmentTemp.AuthorisedBy = (int)fgItemAdjustment.AuthorisedBy;
                if (fgItemAdjustment.Employee != null)
                {
                    fgItemAdjustmentTemp.AuthorisedByName = fgItemAdjustment.Employee.FullName;
                }

                if (fgItemAdjustment.AuthorisedDate != null)
                {
                    fgItemAdjustmentTemp.AuthorisedDateString = fgItemAdjustment.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                }
                if (fgItemAdjustment.AuthorisedDate != null)
                    fgItemAdjustmentTemp.AuthorisedDate = fgItemAdjustment.AuthorisedDate.Value.AddMinutes(timeZoneOffset);

                fgItemAdjustmentVMList.Add(fgItemAdjustmentTemp);
            }
            return Json(fgItemAdjustmentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAdjustmentListByYear(int year, int month)
        {
            var dt1=new DateTime(year,month,1).ToUniversalTime();
            //var dt2 = new DateTime(year, month+1, 1).ToUniversalTime();
            var dt2 = new DateTime(year, month, 1).ToUniversalTime();
            if ((month + 1) == 13)
            {
                dt2 = new DateTime(year + 1, 1, 1).ToUniversalTime();
            }
            else
            {
                dt2 = new DateTime(year, month + 1, 1).ToUniversalTime();
            }
            var fgItemAdjustmentListObj = this.fgItemAdjustmentService.GetAllFGItemAdjustment().Where(a => a.AdjustmentDate != null && (a.DeleteFlag != true && a.AdjustmentDate.Value >= dt1 && a.AdjustmentDate.Value < dt2));
            List<FGItemAdjustmentViewModel> fgItemAdjustmentVMList = new List<FGItemAdjustmentViewModel>();
            foreach (var fgItemAdjustment in fgItemAdjustmentListObj)
            {
                FGItemAdjustmentViewModel fgItemAdjustmentTemp = new FGItemAdjustmentViewModel();
                fgItemAdjustmentTemp = AfgitemAdjustment(fgItemAdjustment);
                fgItemAdjustmentVMList.Add(fgItemAdjustmentTemp);
            }
            return Json(fgItemAdjustmentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQtyFromFgInventory(int fgItemId, int fgGradeId, int bincardId, string lot,string salesUnit)
        {
            var binCardQty = this.fgItemInventoryService.GetAllFGItemInventory().FirstOrDefault(a => a.FGItemId==fgItemId && a.FGGradeId==fgGradeId && a.BinCardId==bincardId && a.Lot==lot);
            double qty = 0.0;

            if (salesUnit == "CTN")
            {
                if (binCardQty != null)
                {
                      qty = binCardQty.QuantityInCTN;
                }
            }
            else if (salesUnit == "PCS")
            {
                if (binCardQty != null)
                {
                     qty = binCardQty.QuantityInPcs;
                }
            }
            else if (salesUnit == "SFT")
            {
                if (binCardQty != null)
                {
                     qty = binCardQty.QuantityInSFT;
                }
            }
            else
            {
                if (binCardQty != null)
                {
                     qty = binCardQty.QuantityInSMT;
                }
            }
            return Json(qty, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLotFromFgInventory(int fgItemId, int fgGradeId)
        {
            var lotByItemGrade = this.fgItemInventoryService.GetAllFGItemInventory().Where(a => a.FGItemId == fgItemId && a.FGGradeId == fgGradeId);
            var lot = "";
            List<string> allLot = new List<string>();
            foreach (var alot in lotByItemGrade)
            {
                lot = alot.Lot;
                allLot.Add(lot);
            }
            var allLot1 = allLot.OrderBy(x => x, new NaturalSort()).ToArray();
            return Json(allLot1, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGItemAdjustment(int id)
        {
            var fgItemAdjustment = this.fgItemAdjustmentService.GetFGItemAdjustment(id);
            FGItemAdjustmentViewModel fgItemAdjustmentTemp = null;
            fgItemAdjustmentTemp = AfgitemAdjustment(fgItemAdjustment);

            return Json(fgItemAdjustmentTemp, JsonRequestBehavior.AllowGet);
        }

        public FGItemAdjustmentViewModel AfgitemAdjustment(FGItemAdjustment fgItemAdjustment)
        {
            FGItemAdjustmentViewModel fgItemAdjustmentTemp = null;
            if (fgItemAdjustment != null)
            {
                fgItemAdjustmentTemp = new FGItemAdjustmentViewModel();
                fgItemAdjustmentTemp.Id = fgItemAdjustment.Id;
                if (fgItemAdjustment.AdjustmentDate != null)
                {
                    fgItemAdjustmentTemp.AdjustmentDateString = fgItemAdjustment.AdjustmentDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                }

                if (fgItemAdjustment.AdjustmentDate != null)
                {
                    fgItemAdjustmentTemp.AdjustmentDate = (DateTime)fgItemAdjustment.AdjustmentDate.Value.AddMinutes(timeZoneOffset);
                }
               

                if (fgItemAdjustment.AuthorisedBy != null) fgItemAdjustmentTemp.AuthorisedBy = (int)fgItemAdjustment.AuthorisedBy;
               
                if (fgItemAdjustment.AuthorisedDate!=null)
                {
                    fgItemAdjustmentTemp.AuthorisedDateString = fgItemAdjustment.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                    fgItemAdjustmentTemp.AuthorisedDate = fgItemAdjustment.AuthorisedDate.Value.AddMinutes(timeZoneOffset);  
                }               

                if (fgItemAdjustment.FGItemAdjustmentDetails.Count() > 0)
                {
                    List<FGItemAdjustmentDetailViewModel> itemAdjustmentDetailVMList = new List<FGItemAdjustmentDetailViewModel>();
                    foreach (var fgItemAdjustmentDetai in fgItemAdjustment.FGItemAdjustmentDetails)
                    {
                        FGItemAdjustmentDetailViewModel fgItemAdjustmentDetailTtemp = new FGItemAdjustmentDetailViewModel();
                        fgItemAdjustmentDetailTtemp.Id = fgItemAdjustmentDetai.Id;
                        fgItemAdjustmentDetailTtemp.FGItemAdjustmentId = fgItemAdjustmentDetai.FGItemAdjustmentId;
                        fgItemAdjustmentDetailTtemp.FGItemId = fgItemAdjustmentDetai.FGItemId;
                        if (fgItemAdjustmentDetai.FGItem != null)
                        {
                            fgItemAdjustmentDetailTtemp.FGItemName = fgItemAdjustmentDetai.FGItem.Name;
                            fgItemAdjustmentDetailTtemp.FGTypeId = fgItemAdjustmentDetai.FGItem.FGType.Id;
                        }

                        fgItemAdjustmentDetailTtemp.SalesUnitId = fgItemAdjustmentDetai.SalesUnitId;
                        fgItemAdjustmentDetailTtemp.SalesUnit = fgItemAdjustmentDetai.FGUOM.UnitName;
                        fgItemAdjustmentDetailTtemp.AdjustmentReason = fgItemAdjustmentDetai.AdjustmentReason;
                       

                        fgItemAdjustmentDetailTtemp.FGGradeId = fgItemAdjustmentDetai.FGGradeId;
                        if (fgItemAdjustmentDetai.FGGrade != null)
                            fgItemAdjustmentDetailTtemp.FGGradeName = fgItemAdjustmentDetai.FGGrade.Grade;

                        fgItemAdjustmentDetailTtemp.Lot = fgItemAdjustmentDetai.Lot;

                        fgItemAdjustmentDetailTtemp.InventoryQuantity = fgItemAdjustmentDetai.InventoryQuantity;
                        fgItemAdjustmentDetailTtemp.ActualQuantity = fgItemAdjustmentDetai.ActualQuantity;
                        fgItemAdjustmentDetailTtemp.QuantityInSFT = fgItemAdjustmentDetai.QuantityInSFT;
                        fgItemAdjustmentDetailTtemp.QuantityInSMT = fgItemAdjustmentDetai.QuantityInSMT;
                        fgItemAdjustmentDetailTtemp.QuantityInCTN = fgItemAdjustmentDetai.QuantityInCTN;
                        fgItemAdjustmentDetailTtemp.QuantityInPCs = fgItemAdjustmentDetai.QuantityInPCs;

                        fgItemAdjustmentDetailTtemp.BinCardId = fgItemAdjustmentDetai.BinCardId;
                        if (fgItemAdjustmentDetai.BinCard != null)
                        {
                            fgItemAdjustmentDetailTtemp.BinCardName = fgItemAdjustmentDetai.BinCard.CardNo;
                        }

                        itemAdjustmentDetailVMList.Add(fgItemAdjustmentDetailTtemp);
                    }
                    fgItemAdjustmentTemp.FGItemAdjustmentDetails = itemAdjustmentDetailVMList;
                }
            }
            return fgItemAdjustmentTemp;
        }
    }

    public class FGItemAdjustmentViewModel
    {
        public FGItemAdjustmentViewModel()
        {
            this.FGItemAdjustmentDetails = new List<FGItemAdjustmentDetailViewModel>();
        }
        public int Id { get; set; }
        public System.DateTime AdjustmentDate { get; set; }
        public string AdjustmentDateString { get; set; }
        public string AuthorisedByName { set; get; }
        public int AuthorisedBy { get; set; }
        public System.DateTime AuthorisedDate { get; set; }
        public string AuthorisedDateString { get; set; }
        public Nullable<bool> DeleteFlag { get; set; }
        public List<FGItemAdjustmentDetailViewModel> FGItemAdjustmentDetails { get; set; }
    }

    public class FGItemAdjustmentDetailViewModel
    {
        public System.Guid Id { get; set; }
        public int FGItemAdjustmentId { get; set; }
        public int FGItemId { get; set; }
        public int FGTypeId { get; set; }
        public int FGGradeId { get; set; }
        public string Lot { get; set; }
        public double InventoryQuantity { get; set; }
        public double ActualQuantity { get; set; }
        public Nullable<int> SalesUnitId { get; set; }
        public double QuantityInSFT { get; set; }
        public double QuantityInSMT { get; set; }
        public double QuantityInCTN { get; set; }
        public int QuantityInPCs { get; set; }
        public string BinCardName { set; get; }
        public Nullable<int> BinCardId { get; set; }
        public string FGItemName { set; get; }
        public string FGUnitName { set; get; }
        public string SalesUnit { set; get; }
        public string FGGradeName { set; get; }
        public string AdjustmentReason { set; get; }
    }
}