using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Service.Enums;
using Remit.Service.Utilities;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class FGItemInController : Controller
    {
        public readonly IFGItemInService fgItemInService;
        public readonly IFGItemService fgItemService;
        public readonly IFGItemInDetailService fgItemInDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService;
        public readonly IFGItemInventoryService fgItemInventoryService;
        public readonly IFGItemInventoryHistoryService fgItemInventoryHistoryService;

        public readonly IFGItemInventoryService itemInventoryService;
        public readonly IFGItemInventoryHistoryService itemInventoryHistoryService;

        public readonly ISupplierService supplierService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public FGItemInController(ISupplierService supplierService, IFGItemInService fgItemInService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService, IFGItemInDetailService fgItemInDetailService,
            IWorkflowactionSettingService workflowactionSettingService, IFGItemInventoryService itemInventoryService, IFGItemInventoryHistoryService itemInventoryHistoryService, IItemService itemService
        , IFGItemService fgItemService, IFGItemInventoryService fgItemInventoryService, IFGItemInventoryHistoryService fgItemInventoryHistoryService, IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService)//, FGInventoryUtility fgInventoryUtility)
        {
            this.supplierService = supplierService;
            this.fgItemInService = fgItemInService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.itemInventoryHistoryService = itemInventoryHistoryService;
            this.itemInventoryService = itemInventoryService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.itemService = itemService;
            this.fgItemInDetailService = fgItemInDetailService;
            this.fgItemInventoryService = fgItemInventoryService;
            this.fgItemInventoryHistoryService = fgItemInventoryHistoryService;
            this.fgItemService = fgItemService;
            this.fgItemInventoryWithoutBinService = fgItemInventoryWithoutBinService;
        }

        string cacheKey = "permission:fgItemIn" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string timeFormat = WebConfigurationManager.AppSettings["TimeFormat"];
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        const string url = "/FGItemIn/Index";

        // GET: /FGItemIn/
        public ActionResult Index()
        {
            return CommonItemIssueList(url, cacheKey, "FGItemIn");
        }

        private ActionResult CommonItemIssueList(string url, string cacheKey, string viewName)
        {
            RoleSubModuleItem permissionItemIssue = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                                    roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(
                                                        url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permissionItemIssue != null)
            {
                if (permissionItemIssue.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permissionItemIssue, 240);
                    return View(viewName);
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateFGItemIn(FGItemIn itemIn)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                var isNew = itemIn.Id == 0 ? true : false;
                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                             roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                 Helpers.UserSession.GetUserFromSession().RoleId);
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {
                        FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
                        foreach (var itemInDetail in itemIn.FGItemInDetails)
                        {
                            itemInDetail.Id = Guid.NewGuid();
                            var fgItemObj = fgItemService.GetFGItem(itemInDetail.FGItemId);
                            // Here goes Qty Convertion Code............
                            var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, itemInDetail.InQuantity, (int)itemInDetail.PackUnitId);
                            if (fgQty != null)
                            {
                                itemInDetail.QuantityInSFT = fgQty.QuantityInSFT;
                                itemInDetail.QuantityInCTN = fgQty.QuantityInCTN;
                                itemInDetail.QuantityInPCs = fgQty.QuantityInPcs;
                                itemInDetail.QuantityInSMT = fgQty.QuantityInSMT;
                            }
                        }

                        itemIn.ReceivedDate = itemIn.ReceivedDate.ToUniversalTime();
                        if (this.fgItemInService.CreateFGItemIn(itemIn, UserSession.GetUserFromSession().EmployeeId))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceFGItemIn.LblFGItemIn);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceFGItemIn.LblFGItemIn);
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToCreate;
                    }
                }
                else
                { }
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


        public JsonResult UpdateFGItemIn(FGItemIn itemIn, List<FGItemInDetailViewModel> fgItemInDetails)
        {
            var isSuccess = false;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);
            if (permission.UpdateOperation == true)
            {
                FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
                var itemInObj = this.fgItemInService.GetFGItemIn(itemIn.Id);
                if (itemInObj != null)
                {
                    if (itemInObj.FGItemInDetails != null)
                    {
                        foreach (var itemInDetail in itemInObj.FGItemInDetails.ToList())
                        {
                            if (this.fgItemInDetailService.DeleteFGItemInDetail(itemInDetail.Id, UserSession.GetUserFromSession().EmployeeId))
                            { }
                            else
                            {
                            }
                        }
                    }

                    if (itemIn.FGItemInDetails != null)
                    {
                        foreach (var itemInDetail in itemIn.FGItemInDetails)
                        {
                            itemInDetail.Id = Guid.NewGuid();
                            itemInDetail.FGItemInId = itemIn.Id;
                            // Here goes Qty Convertion Code............
                            var fgItemObj = fgItemService.GetFGItem(itemInDetail.FGItemId);
                            var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, itemInDetail.InQuantity, (int)itemInDetail.PackUnitId);
                            if (fgQty != null)
                            {
                                itemInDetail.QuantityInSFT = fgQty.QuantityInSFT;
                                itemInDetail.QuantityInCTN = fgQty.QuantityInCTN;
                                itemInDetail.QuantityInPCs = fgQty.QuantityInPcs;
                                itemInDetail.QuantityInSMT = fgQty.QuantityInSMT;
                            }

                            try
                            {
                                this.fgItemInDetailService.CreateFGItemInDetail(itemInDetail,
                                    UserSession.GetUserFromSession().EmployeeId);
                            }
                            catch
                            {
                                //message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGItemIn.LblFGItemIn);
                                //return Json(new
                                //{
                                //    isSuccess = isSuccess,
                                //    message = message,
                                //}, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    
                    itemInObj.DeleteFlag = itemIn.DeleteFlag;
                    itemInObj.ReceivedBy = itemIn.ReceivedBy;
                    itemInObj.ReceivedDate = itemIn.ReceivedDate.ToUniversalTime();

                    itemInObj.UpdatedOn = DateTime.UtcNow;
                    itemInObj.UpdatedBy = UserSession.GetUserFromSession().EmployeeId;
                    
                    if (this.fgItemInService.UpdateFGItemIn(itemInObj))
                    {
                        isSuccess = true;
                        message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGItemIn.LblFGItemIn);
                    }
                    else
                    {
                        message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGItemIn.LblFGItemIn);
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




        //[HttpPost]
        public JsonResult DeleteFGItemIn(int id)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                var fgItemIn = this.fgItemInService.GetFGItemIn(id);//.Where(a => a.FGItemInId == id);

                foreach (var itemInDetail in fgItemIn.FGItemInDetails)
                {
                    if (this.fgItemInDetailService.MinusHistoryandBalance(itemInDetail, UserSession.GetUserFromSession().EmployeeId))
                    { }
                }

                fgItemIn.DeleteFlag = true;
                fgItemIn.DeletedOn = DateTime.UtcNow;
                fgItemIn.DeletedBy = UserSession.GetUserFromSession().EmployeeId;
                if (this.fgItemInService.UpdateFGItemIn(fgItemIn))
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_delete,
                        Resources.ResourceFGItemIn.LblFGItemIn);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete,
                        Resources.ResourceFGItemIn.LblFGItemIn);
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

        public JsonResult GetFGItemInList()
        {
            var itemreceiveListObj = this.fgItemInService.GetAllFGItemIn().Where(fg=> fg.DeleteFlag != true);
            List<FGItemInViewModel> itemreceiveVMList = new List<FGItemInViewModel>();
            foreach (var itemreceive in itemreceiveListObj)
            {
                FGItemInViewModel itemreceiveTemp = new FGItemInViewModel();
                itemreceiveTemp.Id = itemreceive.Id;
                itemreceiveTemp.ReceivedDate = itemreceive.ReceivedDate;
                itemreceiveTemp.ReceivedDateString = itemreceive.ReceivedDate.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
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
            var dt1 = new DateTime(year, month, 1).ToUniversalTime();
            var dt2 = new DateTime(year, month, 1).ToUniversalTime(); 
            if ((month + 1) == 13)
            {
                 dt2 = new DateTime(year+1, 1, 1).ToUniversalTime();
            }
            else
            {
                dt2 = new DateTime(year, month + 1, 1).ToUniversalTime();
            }
           
            //var itemDisposeListObj = this.fgitemDisposeService.GetAllFGItemDispose().Where(a => a.Date.Value.Year == year && a.Date.Value.Month == month && a.DeleteFlag != true);
            var itemreceiveListObj = this.fgItemInService.GetAllFGItemIn().Where(a => a.ReceivedDate != DateTime.MinValue && (a.DeleteFlag != true && a.ReceivedDate >= dt1 && a.ReceivedDate < dt2));
            
            //var itemreceiveListObj = this.fgItemInService.GetAllFGItemIn().Where(a => a.ReceivedDate.Year == year && a.ReceivedDate.Month == month && a.DeleteFlag != true);

            List<FGItemInViewModel> itemreceiveVMList = new List<FGItemInViewModel>();
            foreach (var itemreceive in itemreceiveListObj)
            {
                FGItemInViewModel itemreceiveTemp = new FGItemInViewModel();
                itemreceiveTemp = AfgItemIn(itemreceive);
                itemreceiveVMList.Add(itemreceiveTemp);
            }
            return Json(itemreceiveVMList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetFGItemIn(int id)
        {
            var itemreceive = this.fgItemInService.GetFGItemIn(id);
            FGItemInViewModel itemreceiveTemp = null;
            itemreceiveTemp = AfgItemIn(itemreceive);

            return Json(itemreceiveTemp, JsonRequestBehavior.AllowGet);
        }

        public FGItemInViewModel AfgItemIn(FGItemIn itemreceive)
        {
            FGItemInViewModel itemreceiveTemp = null;
            if (itemreceive != null)
            {
                itemreceiveTemp = new FGItemInViewModel();
                itemreceiveTemp.Id = itemreceive.Id;
                
                itemreceiveTemp.ReceivedDateString = itemreceive.ReceivedDate.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                itemreceiveTemp.ReceivedDateStringShowed = itemreceive.ReceivedDate.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                itemreceiveTemp.ReceivedDate = itemreceive.ReceivedDate;// != null ? itemreceive.ReceivedDate.Value.ToString(dateTimeFormat) : "";
                itemreceiveTemp.ReceivedBy = itemreceive.ReceivedBy;
                if (itemreceive.FGItemInDetails.Count() > 0)
                {
                    List<FGItemInDetailViewModel> itemInDetailVMList = new List<FGItemInDetailViewModel>();
                    foreach (var itemInDetai in itemreceive.FGItemInDetails)
                    {
                        FGItemInDetailViewModel itemInDetailTtemp = new FGItemInDetailViewModel();
                        itemInDetailTtemp.Id = itemInDetai.Id;
                        itemInDetailTtemp.FGItemInId = itemInDetai.FGItemInId;
                        itemInDetailTtemp.FGItemId = itemInDetai.FGItemId;
                        if (itemInDetai.FGItem != null)
                        {
                            itemInDetailTtemp.FGItemName = itemInDetai.FGItem.Name;
                            itemInDetailTtemp.FGTypeId = itemInDetai.FGItem.FGType.Id;
                        }

                        itemInDetailTtemp.PackUnitId = itemInDetai.PackUnitId;
                        
                        itemInDetailTtemp.PackUnitId = itemInDetai.PackUnitId;
                        if (itemInDetai.FGUOM != null)
                        {
                            itemInDetailTtemp.FGUnitName = itemInDetai.FGUOM.UnitName;
                            itemInDetailTtemp.PackUnit = itemInDetai.FGUOM.UnitName;
                        }
                            
                        itemInDetailTtemp.FGGradeId = itemInDetai.FGGradeId;
                        if (itemInDetai.FGGrade != null)
                            itemInDetailTtemp.FGGradeName = itemInDetai.FGGrade.Grade;

                        itemInDetailTtemp.Lot = itemInDetai.Lot;
                        itemInDetailTtemp.InQuantity = itemInDetai.InQuantity;
                        itemInDetailTtemp.QuantityInSFT = itemInDetai.QuantityInSFT;
                        itemInDetailTtemp.QuantityInSMT = itemInDetai.QuantityInSMT;
                        itemInDetailTtemp.QuantityInCTN = itemInDetai.QuantityInCTN;
                        itemInDetailTtemp.QuantityInPCs = itemInDetai.QuantityInPCs;

                        itemInDetailTtemp.BinCardId = itemInDetai.BinCardId;
                        if (itemInDetai.BinCard != null)
                            itemInDetailTtemp.BinCardName = itemInDetai.BinCard.CardNo;

                        itemInDetailVMList.Add(itemInDetailTtemp);
                    }
                    itemreceiveTemp.FGItemInDetails = itemInDetailVMList;
                }

            }
            return itemreceiveTemp;
        }


    }

    public class FGItemInViewModel
    {
        public FGItemInViewModel()
        {
            this.FGItemInDetails = new List<FGItemInDetailViewModel>();
        }
        public int Id { get; set; }
        public System.DateTime ReceivedDate { get; set; }
        public string ReceivedDateString { get; set; }
        public string ReceivedDateStringShowed { get; set; }
        public string ReceivedByName { set; get; }
        public int ReceivedBy { get; set; }
        public Nullable<bool> DeleteFlag { get; set; }
        public List<FGItemInDetailViewModel> FGItemInDetails { get; set; }
    }

    public class FGItemInDetailViewModel
    {
        public System.Guid Id { get; set; }
        public int FGItemInId { get; set; }
        public int FGItemId { get; set; }
        public int FGTypeId { get; set; }
        public int FGGradeId { get; set; }
        public string Lot { get; set; }
        public double InQuantity { get; set; }
        public Nullable<int> PackUnitId { get; set; }
        public double QuantityInSFT { get; set; }
        public double QuantityInSMT { get; set; }
        public double QuantityInCTN { get; set; }
        public int QuantityInPCs { get; set; }
        public string FGItemName { set; get; }
        public string FGUnitName { set; get; }
        public string PackUnit { set; get; }
        public string FGGradeName { set; get; }
        public string BinCardName { set; get; }
        public Nullable<int> BinCardId { get; set; }

    }
}