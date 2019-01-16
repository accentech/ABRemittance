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
using Remit.Web.Helpers;
using Remit.Web.Models;
using Remit.Service.Enums;
using Remit.Service.Utilities;
using System.Linq.Dynamic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace Remit.Web.Controllers
{
    public class FGItemOpeningController : Controller
    {
        public readonly IFGItemOpeningService fgItemOpeningService;
        public readonly IFGItemService fgItemService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly INotificationSettingService notificationSettingService;
        public readonly IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService;
        public readonly IFGItemInventoryService fgItemInventoryService;
        public readonly IFGItemInventoryHistoryService fgItemFGInventoryHistoryService;

        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public FGItemOpeningController(
            IFGItemOpeningService fgItemOpeningService,
            ISubModuleItemService subModuleItemService,
            IRoleSubModuleItemService roleSubModuleItemService,
            IFGItemInventoryService fgItemInventoryService,
            IFGItemInventoryHistoryService fgItemFGInventoryHistoryService,
            IWorkflowactionSettingService workflowactionSettingService,IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService,
            INotificationSettingService notificationSettingService, IFGItemService fgItemService)
        {
            this.fgItemOpeningService = fgItemOpeningService;
            this.subModuleItemService = subModuleItemService;
            this.fgItemInventoryService = fgItemInventoryService;
            this.fgItemFGInventoryHistoryService = fgItemFGInventoryHistoryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.notificationSettingService = notificationSettingService;
            this.fgItemService = fgItemService;
            this.fgItemInventoryWithoutBinService = fgItemInventoryWithoutBinService;
        }

        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string cacheKey = "permission:fgItemOpening" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        const string url = "/FGItemOpening/Index";

        public ActionResult Index()
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGItemOpening");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ActionResult GetRemainingFGItemList()
        {
            var success = false;
            var message = string.Empty;
            var fgItemOpeningIdList = fgItemOpeningService.GetAllFGItemOpening().GroupBy(a=>a.FGItemId).Select(c => c.Key).ToList();
            var fgItemOpeningList = new List<FGItemOpeningViewModel>();

            var fgItemList = fgItemService.GetAllFGItem().Where(a => !fgItemOpeningIdList.Contains(a.Id)).OrderBy(b => b.FGType.TypeName).ThenBy(b => b.Code);

            var fgItemObjList = fgItemList.ToList();
            if (fgItemObjList.Any())
            {
                foreach (var fgItemObj in fgItemObjList)
                {
                    var fgItemOpening = new FGItemOpeningViewModel();
                    fgItemOpening.FGItemId = fgItemObj.Id;
                    fgItemOpening.FGItemName = fgItemObj.Name;
                    fgItemOpening.TypeId = fgItemObj.TypeId;
                    fgItemOpening.TypeName = fgItemObj.FGType != null ? fgItemObj.FGType.TypeName : "";

                    fgItemOpening.PackageToSalesRatio = fgItemObj.PackageToSalesRatio;
                    fgItemOpening.UnitId = fgItemObj.PackUnitId;
                    fgItemOpening.UnitName = fgItemObj.FGUOM != null ? fgItemObj.FGUOM.UnitName : "";
                    fgItemOpening.SalesUnit = fgItemObj.FGUOM1 != null ? fgItemObj.FGUOM1.UnitName : "";

                    fgItemOpeningList.Add(fgItemOpening);
                }

                success = true;
            }

            return Json(new
            {
                isSuccess = success,
                List = fgItemOpeningList
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFGItemOpening(string openingdate)
        {
            var success = false;
            var message = string.Empty;
            var fgItemOpeningObjList = fgItemOpeningService.GetAllFGItemOpening().Where(a => a.OpeningDate.AddMinutes(timeZoneOffset).Date == DateTime.Parse(openingdate)).OrderBy(b => b.FGItem.FGType.TypeName).ThenBy(b => b.FGItem.Code).ThenBy(b => b.FGGrade.Grade).ThenBy(b => b.Lot);
            var fgItemOpeningList = new List<FGItemOpeningViewModel>();

            if (fgItemOpeningObjList.Any())
            {
                foreach (var fgItemOpeningObj in fgItemOpeningObjList.ToList())
                {
                    var fgItemOpening = new FGItemOpeningViewModel();
                    fgItemOpening.Id = fgItemOpeningObj.Id;
                    fgItemOpening.FGItemId = fgItemOpeningObj.FGItemId;
                    fgItemOpening.FGGradeId = fgItemOpeningObj.FGGradeId;
                    fgItemOpening.Lot = fgItemOpeningObj.Lot;
                    if (fgItemOpeningObj.FGItem != null)
                    {
                        fgItemOpening.FGItemName = fgItemOpeningObj.FGItem.Name;
                        fgItemOpening.TypeId = fgItemOpeningObj.FGItem.TypeId;
                        fgItemOpening.SalesUnit = fgItemOpeningObj.FGItem.FGUOM1 != null ? fgItemOpeningObj.FGItem.FGUOM1.UnitName : "";
                        fgItemOpening.PackageToSalesRatio = fgItemOpeningObj.FGItem.PackageToSalesRatio;
                        fgItemOpening.TypeName = fgItemOpeningObj.FGItem.FGType != null ? fgItemOpeningObj.FGItem.FGType.TypeName : "";
                    }
                    fgItemOpening.OpeningDate = fgItemOpeningObj.OpeningDate.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                    fgItemOpening.UnitId = fgItemOpeningObj.UnitId;
                    fgItemOpening.UnitName = fgItemOpeningObj.FGUOM != null ? fgItemOpeningObj.FGUOM.UnitName : "";
                    fgItemOpening.BinCardId = fgItemOpeningObj.BinCardId;
                    fgItemOpening.Quantity = fgItemOpeningObj.Quantity;
                    fgItemOpening.Status = fgItemOpeningObj.Status;

                    fgItemOpeningList.Add(fgItemOpening);
                }

                success = true;
            }

            return Json(new
            {
                isSuccess = success,
                List = fgItemOpeningList
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpeningDateListByYear(int year)
        {
            var fgItemOpeningDates = this.fgItemOpeningService.GetAllFGItemOpening().Where(a => a.OpeningDate.AddMinutes(timeZoneOffset).Year == year).GroupBy(b => b.OpeningDate.AddMinutes(timeZoneOffset).Date).Select(c => c.Key).ToList();

            List<string> openingDatelist = new List<string>();

            foreach (var fgItemOpeningDate in fgItemOpeningDates)
            {
                string openingDate = fgItemOpeningDate.ToString(dateFormat);
                openingDatelist.Add(openingDate);
            }
            return Json(openingDatelist, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateFGItemOpeningList(List<FGItemOpening> fgItemOpeningList)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemFGInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            var isSuccess = false;
            var message = string.Empty;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                    Helpers.UserSession.GetUserFromSession().RoleId);
            foreach (var fgItemOpening in fgItemOpeningList)
            {
                var isNew = fgItemOpening.Id == Guid.Empty ? true : false;
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {
                        fgItemOpening.Id = Guid.NewGuid();
                        fgItemOpening.OpeningDate = fgItemOpening.OpeningDate != null ? fgItemOpening.OpeningDate.ToUniversalTime():DateTime.UtcNow;
                        fgItemOpening.CreatedBy = UserSession.GetUserFromSession().EmployeeId;
                        fgItemOpening.CreatedOn = DateTime.UtcNow;

                        FGItem fgItem = this.fgItemService.GetFGItem(fgItemOpening.FGItemId);
                        FGItemQuanty fgiq = fgInventoryUtility.GetConvertedQuantity(fgItem, fgItemOpening.Quantity, fgItemOpening.UnitId);
                        if (fgiq != null)
                        {
                            fgItemOpening.QuantityInPcs = fgiq.QuantityInPcs;
                            fgItemOpening.QuantityInCTN = fgiq.QuantityInCTN;
                            fgItemOpening.QuantityInSFT = fgiq.QuantityInSFT;
                            fgItemOpening.QuantityInSMT = fgiq.QuantityInSMT;
                        }

                        if (this.fgItemOpeningService.CreateFGItemOpening(fgItemOpening))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceFGItemOpening.LblFGItemOpening);
                        }
                        else
                        {
                            isSuccess = false;
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceFGItemOpening.LblFGItemOpening);
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
                        var fgItemOpeningObj = this.fgItemOpeningService.GetFGItemOpening(fgItemOpening.Id);
                        if(fgItemOpeningObj.Status != (int)CommonEnum.Approved){
                            fgItemOpeningObj.OpeningDate = fgItemOpening.OpeningDate.ToUniversalTime();
                            fgItemOpeningObj.Quantity = fgItemOpening.Quantity;
                            
                            FGItemQuanty fgiq = fgInventoryUtility.GetConvertedQuantity(fgItemOpeningObj.FGItem, fgItemOpening.Quantity, fgItemOpening.UnitId);
                            if (fgiq != null)
                            {
                                fgItemOpeningObj.QuantityInPcs = fgiq.QuantityInPcs;
                                fgItemOpeningObj.QuantityInCTN = fgiq.QuantityInCTN;
                                fgItemOpeningObj.QuantityInSFT = fgiq.QuantityInSFT;
                                fgItemOpeningObj.QuantityInSMT = fgiq.QuantityInSMT;
                            }

                            if (this.fgItemOpeningService.UpdateFGItemOpening(fgItemOpeningObj))
                            {
                                isSuccess = true;
                                message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGItemOpening.LblFGItemOpening);
                            }
                            else
                            {
                                isSuccess = false;
                                message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGItemOpening.LblFGItemOpening);
                            }
                        }
                        else
                        {
                            message = Resources.ResourceCommon.MsgNotAllowedToUpdate;
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
                    }
                }
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ApproveFGItemOpeningList(List<FGItemOpening> fgItemOpeningList)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemFGInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            var isSuccess = false;
            var message = string.Empty;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                    Helpers.UserSession.GetUserFromSession().RoleId);
            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId &&a.SubModuleItem.UrlPath == url &&a.WorkflowactionId == (int)WorkFlowActionEnum.Approve).FirstOrDefault();
                
            foreach (var fgItemOpening in fgItemOpeningList)
            {
                var isNew = fgItemOpening.Id == Guid.Empty ? true : false;
                if (!isNew)
                {
                    if (permission.UpdateOperation == true && workflowactionSettingObj != null)
                    {
                        var fgItemOpeningObj = this.fgItemOpeningService.GetFGItemOpening(fgItemOpening.Id);
                        if (fgItemOpeningObj.Status != (int)CommonEnum.Approved)
                        {
                            fgItemOpeningObj.Status = (int)CommonEnum.Approved;
                            fgItemOpeningObj.OpeningDate = fgItemOpening.OpeningDate.ToUniversalTime();
                            fgItemOpeningObj.Quantity = fgItemOpening.Quantity;
                            
                            FGItemQuanty fgiq = fgInventoryUtility.GetConvertedQuantity(fgItemOpeningObj.FGItem, fgItemOpening.Quantity, fgItemOpening.UnitId);
                            if (fgiq != null)
                            {
                                fgItemOpeningObj.QuantityInPcs = fgiq.QuantityInPcs;
                                fgItemOpeningObj.QuantityInCTN = fgiq.QuantityInCTN;
                                fgItemOpeningObj.QuantityInSFT = fgiq.QuantityInSFT;
                                fgItemOpeningObj.QuantityInSMT = fgiq.QuantityInSMT;
                            }
                            fgItemOpeningObj.ApprovedBy = UserSession.GetUserFromSession().EmployeeId;
                            fgItemOpeningObj.ApprovedOn = DateTime.UtcNow;

                            try
                            {
                                var referenceId = fgItemOpeningObj.Id.ToString();
                                var type = "FGItemOpening"; bool isCreate = true; bool isPlusBalance = true; var action = actionEnum.Create;

                                var check = fgInventoryUtility.MainFunction(fgItemOpeningObj.FGItemId, fgItemOpeningObj.FGGradeId,
                                    fgItemOpeningObj.Lot, fgItemOpeningObj.BinCardId, fgItemOpeningObj.Quantity, fgItemOpeningObj.QuantityInSFT,
                                    fgItemOpeningObj.QuantityInSMT, fgItemOpeningObj.QuantityInCTN, fgItemOpeningObj.QuantityInPcs,
                                    fgItemOpeningObj.UnitId, referenceId, type, UserSession.GetUserFromSession().EmployeeId, isPlusBalance, (int)action, isCreate);
                            }
                            catch { }

                            if (this.fgItemOpeningService.UpdateFGItemOpening(fgItemOpeningObj))
                            {
                                isSuccess = true;
                                message = string.Format(Resources.ResourceCommon.CMsg_approve, Resources.ResourceFGItemOpening.LblFGItemOpening);
                            }
                            else
                            {
                                isSuccess = false;
                                message = string.Format(Resources.ResourceCommon.CMsg_notapprove, Resources.ResourceFGItemOpening.LblFGItemOpening);
                            }
                        }
                        else
                        {
                            message = Resources.ResourceCommon.MsgNotAllowedToUpdate;
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToApprove;
                    }
                }
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }
    }
    public class FGItemInventoryViewModel
    {
        public System.Guid Id { get; set; }
        public int FGItemId { get; set; }
        public int FGGradeId { get; set; }
        public string Lot { get; set; }
        public double Quantity { get; set; }
        public double DeliveryQuantity { get; set; }
        public double BookQuantity { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public string BinCardName { set; get; }
        public virtual BinCard BinCard { get; set; }
        public virtual FGGrade FGGrade { get; set; }
        public virtual FGItem FGItem { get; set; }
    }

    public class FGItemOpeningViewModel
    {
        public System.Guid Id { get; set; }
        public string OpeningDate { get; set; }
        public int FGItemId { get; set; }
        public string FGItemName { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int FGGradeId { get; set; }
        public string Lot { get; set; }
        public double Quantity { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string SalesUnit { get; set; }
        public double PackageToSalesRatio { get; set; }
        public double QuantityInSFT { get; set; }
        public double QuantityInSMT { get; set; }
        public double QuantityInCTN { get; set; }
        public int QuantityInPcs { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public string BinCardNo { get; set; }
        public Nullable<int> Status { get; set; }
        public virtual BinCard BinCard { get; set; }
        public virtual FGGrade FGGrade { get; set; }
        public virtual FGItem FGItem { get; set; }
        public virtual FGUOM FGUOM { get; set; }
    }
}