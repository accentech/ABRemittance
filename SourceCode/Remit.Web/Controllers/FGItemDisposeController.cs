using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class FGItemDisposeController : Controller
    {
        public readonly IFGItemDisposeService fgitemDisposeService;
        private readonly IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService;
        public readonly IFGItemService fgItemService;
        public readonly IFGItemDisposeDetailService fgitemDisposeDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly IFGItemInventoryService fgItemInventoryService;
        public readonly IFGItemInventoryHistoryService fgItemInventoryHistoryService;

        public readonly IFGItemInventoryService itemInventoryService;
        public readonly IFGItemInventoryHistoryService itemInventoryHistoryService;

        public readonly ISupplierService supplierService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();
        //public readonly IComparer com;
        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public FGItemDisposeController(ISupplierService supplierService,IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService, IFGItemDisposeService fgitemDisposeService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService, IFGItemDisposeDetailService fgitemDisposeDetailService,
            IWorkflowactionSettingService workflowactionSettingService, IFGItemInventoryService fgItemInventoryService, IFGItemInventoryHistoryService fgItemInventoryHistoryService, IItemService itemService
        , IFGItemService fgItemService, IFGItemInventoryService itemInventoryService, IFGItemInventoryHistoryService itemInventoryHistoryService)//, FGInventoryUtility fgInventoryUtility)
        {
            this.supplierService = supplierService;
            this.fgItemInventoryHistoryService = fgItemInventoryHistoryService;
            this.fgitemDisposeService = fgitemDisposeService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.itemInventoryHistoryService = itemInventoryHistoryService;
            this.itemInventoryService = itemInventoryService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.itemService = itemService;
            this.fgitemDisposeDetailService = fgitemDisposeDetailService;
            this.fgItemInventoryService = fgItemInventoryService;
            this.fgItemInventoryHistoryService = fgItemInventoryHistoryService;
            this.fgItemInventoryWithoutBinService = fgItemInventoryWithoutBinService;
            this.fgItemService = fgItemService;
            //this.com = com;
        }

        string cacheKey = "permission:fgitemDispose" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        const string url = "/FGItemDispose/Index";

        // GET: /FGItemDispose/
        public ActionResult Index()
        {
            return CommonItemIssueList(url, cacheKey, "FGItemDispose");
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
        public JsonResult CreateFGItemDispose(FGItemDispose itemDispose)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                var isNew = itemDispose.Id == 0 ? true : false;
                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                             roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                 Helpers.UserSession.GetUserFromSession().RoleId);
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {
                        FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
          
                        foreach (var itemDisposeDetail in itemDispose.FGItemDisposeDetails)
                        {
                            itemDisposeDetail.Id = Guid.NewGuid();
                            var fgItemObj = fgItemService.GetFGItem(itemDisposeDetail.FGItemId);
                            var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemObj, itemDisposeDetail.DisposeQuantity, (int)itemDisposeDetail.SalesUnitId);
                            if (fgQty != null)
                            {
                                itemDisposeDetail.QuantityInSFT = fgQty.QuantityInSFT;
                                itemDisposeDetail.QuantityInCTN = fgQty.QuantityInCTN;
                                itemDisposeDetail.QuantityInPCs = fgQty.QuantityInPcs;
                                itemDisposeDetail.QuantityInSMT = fgQty.QuantityInSMT;
                            }
                        }
                        if (itemDispose.Date != null) itemDispose.Date = itemDispose.Date.Value.ToUniversalTime();
                        if (this.fgitemDisposeService.CreateFGItemDispose(itemDispose, UserSession.GetUserFromSession().EmployeeId))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceFGItemDispose.LblFGItemDispose);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceFGItemDispose.LblFGItemDispose);
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

        public JsonResult UpdateFGItemDispose(FGItemDispose itemDispose, IList<FGItemDisposeDetail> fgitemDisposeDetails)
        {
            var isSuccess = false;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);
            if (permission.UpdateOperation == true)
            {
                FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
          
                foreach (var fgitemDisposeDetail in fgitemDisposeDetails)
                {
                    fgitemDisposeDetail.Id =Guid.NewGuid();
                    fgitemDisposeDetail.FGItemDisposeId = itemDispose.Id;
                }

                var itemDisposeObj = this.fgitemDisposeService.GetFGItemDispose(itemDispose.Id);
                if (itemDisposeObj != null)
                {
                    if (itemDisposeObj.FGItemDisposeDetails != null)
                    {
                        foreach (var itemDisposeDetail in itemDisposeObj.FGItemDisposeDetails.ToList())
                        {
                            FGInventoryUtility fgInventoryUtility2 = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
          
                            var fgQty = fgInventoryUtility2.GetConvertedQuantity(fgItemService.GetFGItem(itemDisposeDetail.FGItemId), itemDisposeDetail.DisposeQuantity, (int)itemDisposeDetail.SalesUnitId);
                            if (fgQty != null)
                            {
                                itemDisposeDetail.QuantityInSFT = fgQty.QuantityInSFT;
                                itemDisposeDetail.QuantityInCTN = fgQty.QuantityInCTN;
                                itemDisposeDetail.QuantityInPCs = fgQty.QuantityInPcs;
                                itemDisposeDetail.QuantityInSMT = fgQty.QuantityInSMT;
                            }
                            var referenceId = itemDisposeDetail.Id.ToString();
                            var type = "FGItemDisposeDetail"; bool isCreate = false; bool isPlusBalance = true; var action = actionEnum.Create;
                            var transactionBy = UserSession.GetUserFromSession().EmployeeId;
                            fgInventoryUtility2.MainFunction(itemDisposeDetail.FGItemId, itemDisposeDetail.FGGradeId,
                            itemDisposeDetail.Lot, itemDisposeDetail.BinCardId, itemDisposeDetail.DisposeQuantity, itemDisposeDetail.QuantityInSFT,
                            itemDisposeDetail.QuantityInSMT, itemDisposeDetail.QuantityInCTN, itemDisposeDetail.QuantityInPCs,
                            itemDisposeDetail.SalesUnitId, referenceId, type, transactionBy, isPlusBalance, (int)action, isCreate);
                            
                            if (this.fgitemDisposeDetailService.DeleteFGItemDisposeDetail(itemDisposeDetail.Id,
                                UserSession.GetUserFromSession().EmployeeId))
                            {
                            }
                        }
                    }

                    ItemDispose itd = new ItemDispose();
                    if (itemDispose.Date != null)
                    {
                        itemDisposeObj.Date = itemDispose.Date.Value.ToUniversalTime();   
                    }

                    if (this.fgitemDisposeService.UpdateFGItemDispose(itemDisposeObj))
                    {
                         this.fgitemDisposeDetailService.CreateFGItemDisposeDetail(fgitemDisposeDetails, UserSession.GetUserFromSession().EmployeeId);
                         isSuccess = true;
                         message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGItemDispose.LblFGItemDispose);        
                    }
                    else
                    {
                        message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGItemDispose.LblFGItemDispose);
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


        //Approve........
        public JsonResult ApproveDispose(FGItemDispose itemDispose, IList<FGItemDisposeDetail> fgitemDisposeDetails)
        {
            var isSuccess = false;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);
            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId && a.SubModuleItem.UrlPath == url).FirstOrDefault();

            if (permission.UpdateOperation == true && workflowactionSettingObj != null)
            {

                FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);

                foreach (var fgitemDisposeDetail in fgitemDisposeDetails)
                {
                    fgitemDisposeDetail.Id = Guid.NewGuid();
                    fgitemDisposeDetail.FGItemDisposeId = itemDispose.Id;
                }

                var itemDisposeObj = this.fgitemDisposeService.GetFGItemDispose(itemDispose.Id);
                if (itemDisposeObj != null)
                {
                    if (itemDisposeObj.FGItemDisposeDetails != null)
                    {
                        foreach (var itemDisposeDetail in itemDisposeObj.FGItemDisposeDetails.ToList())
                        {
                            FGInventoryUtility fgInventoryUtility2 = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);

                            var fgQty = fgInventoryUtility2.GetConvertedQuantity(fgItemService.GetFGItem(itemDisposeDetail.FGItemId), itemDisposeDetail.DisposeQuantity, (int)itemDisposeDetail.SalesUnitId);
                            if (fgQty != null)
                            {
                                itemDisposeDetail.QuantityInSFT = fgQty.QuantityInSFT;
                                itemDisposeDetail.QuantityInCTN = fgQty.QuantityInCTN;
                                itemDisposeDetail.QuantityInPCs = fgQty.QuantityInPcs;
                                itemDisposeDetail.QuantityInSMT = fgQty.QuantityInSMT;
                            }
                            var referenceId = itemDisposeDetail.Id.ToString();
                            var type = "FGItemDisposeDetail"; bool isCreate = false; bool isPlusBalance = true; var action = actionEnum.Create;
                            var transactionBy = UserSession.GetUserFromSession().EmployeeId;
                            fgInventoryUtility2.MainFunction(itemDisposeDetail.FGItemId, itemDisposeDetail.FGGradeId,
                            itemDisposeDetail.Lot, itemDisposeDetail.BinCardId, itemDisposeDetail.DisposeQuantity, itemDisposeDetail.QuantityInSFT,
                            itemDisposeDetail.QuantityInSMT, itemDisposeDetail.QuantityInCTN, itemDisposeDetail.QuantityInPCs,
                            itemDisposeDetail.SalesUnitId, referenceId, type, transactionBy, isPlusBalance, (int)action, isCreate);

                            if (this.fgitemDisposeDetailService.DeleteFGItemDisposeDetail(itemDisposeDetail.Id,
                                UserSession.GetUserFromSession().EmployeeId))
                            {
                            }
                        }
                    }

                    ItemDispose itd = new ItemDispose();
                    if (itemDispose.Date != null)
                    {
                        itemDisposeObj.Date = itemDispose.Date.Value.ToUniversalTime();
                    }

                    itemDisposeObj.AuthorisedBy = UserSession.GetUserFromSession().EmployeeId;
                    itemDisposeObj.AuthorisedDate = DateTime.UtcNow;

                    if (this.fgitemDisposeService.UpdateFGItemDispose(itemDisposeObj))
                    {
                        this.fgitemDisposeDetailService.CreateFGItemDisposeDetail(fgitemDisposeDetails, UserSession.GetUserFromSession().EmployeeId);
                        isSuccess = true;
                        message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceFGItemDispose.LblFGItemDispose);
                    }
                    else
                    {
                        message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceFGItemDispose.LblFGItemDispose);
                    }
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
        public JsonResult DeleteFGItemDispose(int id)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                var fgitemDispose = this.fgitemDisposeService.GetFGItemDispose(id);//.Where(a => a.FGItemDisposeId == id);

                fgitemDispose.DeleteFlag = true;
                if (this.fgitemDisposeService.UpdateFGItemDispose(fgitemDispose))
                {
                    FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
          
                    var fgItemDisposeDetails = fgitemDispose.FGItemDisposeDetails.ToList();
                    foreach (var itemInDet in fgItemDisposeDetails)
                    {

                        var fgQty = fgInventoryUtility.GetConvertedQuantity(fgItemService.GetFGItem(itemInDet.FGItemId), itemInDet.DisposeQuantity, (int)itemInDet.SalesUnitId);
                        if (fgQty != null)
                        {
                            itemInDet.QuantityInSFT = fgQty.QuantityInSFT;
                            itemInDet.QuantityInCTN = fgQty.QuantityInCTN;
                            itemInDet.QuantityInPCs = fgQty.QuantityInPcs;
                            itemInDet.QuantityInSMT = fgQty.QuantityInSMT;
                        }
                        var referenceId = itemInDet.Id.ToString();
                        var type = "FGItemDispose"; bool isCreate = false; bool isPlusBalance = true; var action = actionEnum.Create;
                        var transactionBy = UserSession.GetUserFromSession().EmployeeId;

                        fgInventoryUtility.MainFunction(itemInDet.FGItemId, itemInDet.FGGradeId,
                        itemInDet.Lot, itemInDet.BinCardId, itemInDet.DisposeQuantity, itemInDet.QuantityInSFT,
                        itemInDet.QuantityInSMT, itemInDet.QuantityInCTN, itemInDet.QuantityInPCs,
                        itemInDet.SalesUnitId, referenceId, type, transactionBy, isPlusBalance, (int)action, isCreate);

                       }
                    message = string.Format(Resources.ResourceCommon.CMsg_delete,
                        Resources.ResourceFGItemDispose.LblFGItemDispose);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete,
                        Resources.ResourceFGItemDispose.LblFGItemDispose);
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

        public JsonResult GetFGItemDisposeList()
        {
            var itemDisposeListObj = this.fgitemDisposeService.GetAllFGItemDispose().Where(fg => fg.DeleteFlag != true);
            List<FGItemDisposeViewModel> itemDisposeVMList = new List<FGItemDisposeViewModel>();
            foreach (var itemDispose in itemDisposeListObj)
            {
                FGItemDisposeViewModel itemDisposeTemp = new FGItemDisposeViewModel();
                itemDisposeTemp.Id = itemDispose.Id;
                itemDisposeTemp.Date = itemDispose.Date.Value.AddMinutes(timeZoneOffset);
                itemDisposeTemp.DateString = itemDispose.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                itemDisposeTemp.AuthorisedBy = (int)itemDispose.AuthorisedBy;
                if (itemDispose.Employee != null)
                {
                    itemDisposeTemp.AuthorisedByName = itemDispose.Employee.FullName;
                }

                if (itemDispose.AuthorisedDate != null)
                {
                    itemDisposeTemp.AuthorisedDateString = itemDispose.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                }
                if (itemDispose.AuthorisedDate != null)
                    itemDisposeTemp.AuthorisedDate = itemDispose.AuthorisedDate.Value.AddMinutes(timeZoneOffset);

                itemDisposeVMList.Add(itemDisposeTemp);
            }
            return Json(itemDisposeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDisposeListByYear(int year, int month)
        {
            var dt1=new DateTime(year,month,1).ToUniversalTime();
            //var dt2 = new DateTime(year, month+1, 1).ToUniversalTime();
            var dt2 = new DateTime(year, month, 1).ToUniversalTime();
            if ((month + 1) == 13)  // month can't be more than 
            {
                dt2 = new DateTime(year + 1, 1, 1).ToUniversalTime();
            }
            else
            {
                dt2 = new DateTime(year, month + 1, 1).ToUniversalTime();
            }
            var itemDisposeListObj = this.fgitemDisposeService.GetAllFGItemDispose().Where(a => a.Date != null && (a.DeleteFlag != true && a.Date.Value >= dt1 && a.Date.Value < dt2));
            List<FGItemDisposeViewModel> itemDisposeVMList = new List<FGItemDisposeViewModel>();
            foreach (var itemDispose in itemDisposeListObj)
            {
                FGItemDisposeViewModel itemDisposeTemp = new FGItemDisposeViewModel();
                itemDisposeTemp = AfgitemDispose(itemDispose);
                itemDisposeVMList.Add(itemDisposeTemp);
            }
            return Json(itemDisposeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetQtyFromFgInventory(int itemId, int gradeId, int bincardId, string lot,string salesUnit)
        {
           // FGItemDisposeViewModel itemDisposeTemp = null;
            var binCardQty = this.fgItemInventoryService.GetAllFGItemInventory().FirstOrDefault(a => a.FGItemId==itemId && a.FGGradeId==gradeId && a.BinCardId==bincardId && a.Lot==lot);
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

        public JsonResult GetLotFromFgInventory(int itemId, int gradeId)
        {
            var lotByItemGrade = this.fgItemInventoryService.GetAllFGItemInventory().Where(a => a.FGItemId == itemId && a.FGGradeId == gradeId && a.QuantityInCTN > 0);
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


        public JsonResult GetbinListByItemGradeLot(int itemId, int gradeId, string lot)
        {
            var binList = this.fgItemInventoryService.GetAllFGItemInventory().Where(a => a.FGItemId == itemId && a.FGGradeId == gradeId 
                && a.Lot == lot && a.BinCardId != null);
            List<BinCardViewModel> allBin = new List<BinCardViewModel>();
            foreach (var aBin in binList)
            {
                BinCardViewModel binCardTemp = new BinCardViewModel();
                binCardTemp.Id = aBin.BinCard.Id;
                binCardTemp.CardNo = aBin.BinCard.CardNo;
                if (aBin.BinCard.Warehouse != null)
                {
                    if (aBin.BinCard.WarehouseId != null) binCardTemp.WarehouseId = (int)aBin.BinCard.WarehouseId;
                    binCardTemp.WarhouseName = aBin.BinCard.Warehouse.Name;
                }
                allBin.Add(binCardTemp);
            }
            //return Json(binCardVMList, JsonRequestBehavior.AllowGet);
            //var allBin1 = allBin.OrderBy(x => x, new NaturalSort()).ToArray();
            return Json(allBin, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetFGItemDispose(int id)
        {
            var itemDispose = this.fgitemDisposeService.GetFGItemDispose(id);
            FGItemDisposeViewModel itemDisposeTemp = null;
            itemDisposeTemp = AfgitemDispose(itemDispose);

            return Json(itemDisposeTemp, JsonRequestBehavior.AllowGet);
        }

        public FGItemDisposeViewModel AfgitemDispose(FGItemDispose itemDispose)
        {
            FGItemDisposeViewModel itemDisposeTemp = null;
            var BinCardQty = new FGItemInventory();
            if (itemDispose != null)
            {
                itemDisposeTemp = new FGItemDisposeViewModel();
                itemDisposeTemp.Id = itemDispose.Id;
                if (itemDispose.Date != null)
                {
                    itemDisposeTemp.DateString = itemDispose.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                }

                if (itemDispose.Date != null)
                {
                    itemDisposeTemp.Date = (DateTime)itemDispose.Date.Value.AddMinutes(timeZoneOffset);
                }
               

                if (itemDispose.AuthorisedBy != null) itemDisposeTemp.AuthorisedBy = (int)itemDispose.AuthorisedBy;
               
                if (itemDispose.AuthorisedDate!=null)
                {
                    itemDisposeTemp.AuthorisedDateString = itemDispose.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                    itemDisposeTemp.AuthorisedDate = itemDispose.AuthorisedDate.Value.AddMinutes(timeZoneOffset);  
                }               

                if (itemDispose.FGItemDisposeDetails.Count() > 0)
                {
                    List<FGItemDisposeDetailViewModel> itemDisposeDetailVMList = new List<FGItemDisposeDetailViewModel>();
                    foreach (var itemDisposeDetai in itemDispose.FGItemDisposeDetails)
                    {
                        FGItemDisposeDetailViewModel itemDisposeDetailTtemp = new FGItemDisposeDetailViewModel();
                        itemDisposeDetailTtemp.Id = itemDisposeDetai.Id;
                        itemDisposeDetailTtemp.FGItemDisposeId = itemDisposeDetai.FGItemDisposeId;
                        itemDisposeDetailTtemp.FGItemId = itemDisposeDetai.FGItemId;
                        if (itemDisposeDetai.FGItem != null)
                        {
                            itemDisposeDetailTtemp.FGItemName = itemDisposeDetai.FGItem.Name;
                            itemDisposeDetailTtemp.FGTypeId = itemDisposeDetai.FGItem.FGType.Id;
                        }

                        itemDisposeDetailTtemp.SalesUnitId = itemDisposeDetai.SalesUnitId;
                        itemDisposeDetailTtemp.SalesUnit = itemDisposeDetai.FGUOM.UnitName;
                        itemDisposeDetailTtemp.DisposeReason = itemDisposeDetai.DisposeReason;
                       

                        itemDisposeDetailTtemp.FGGradeId = itemDisposeDetai.FGGradeId;
                        if (itemDisposeDetai.FGGrade != null)
                            itemDisposeDetailTtemp.FGGradeName = itemDisposeDetai.FGGrade.Grade;

                        itemDisposeDetailTtemp.Lot = itemDisposeDetai.Lot;

                        itemDisposeDetailTtemp.DisposeQuantity = itemDisposeDetai.DisposeQuantity;
                        itemDisposeDetailTtemp.HandDisposeQuantity = itemDisposeDetai.DisposeQuantity;
                        itemDisposeDetailTtemp.QuantityInSFT = itemDisposeDetai.QuantityInSFT;
                        itemDisposeDetailTtemp.QuantityInSMT = itemDisposeDetai.QuantityInSMT;
                        itemDisposeDetailTtemp.QuantityInCTN = itemDisposeDetai.QuantityInCTN;
                        itemDisposeDetailTtemp.QuantityInPCs = itemDisposeDetai.QuantityInPCs;

                        itemDisposeDetailTtemp.BinCardId = itemDisposeDetai.BinCardId;
                        if (itemDisposeDetai.BinCard != null)
                        {
                            itemDisposeDetailTtemp.BinCardName = itemDisposeDetai.BinCard.CardNo;
                        }

                        BinCardQty = this.fgItemInventoryService.GetFGItemInventoryByFGItemIdAndBinCardId(itemDisposeDetai.FGItemId, (int)itemDisposeDetai.BinCardId, itemDisposeDetai.FGGradeId, itemDisposeDetai.Lot);
                        if (BinCardQty != null)
                        {
                            if (itemDisposeDetai.SalesUnitId == 1)
                            {
                                itemDisposeDetailTtemp.BinCardQty = BinCardQty.QuantityInCTN;
                            }
                            else if (itemDisposeDetai.SalesUnitId == 2)
                            {
                                itemDisposeDetailTtemp.BinCardQty = BinCardQty.QuantityInPcs;
                            }
                            else if (itemDisposeDetai.SalesUnitId == 3)
                            {
                                itemDisposeDetailTtemp.BinCardQty = BinCardQty.QuantityInSFT;
                            }
                            else if (itemDisposeDetai.SalesUnitId == 4)
                            {
                                itemDisposeDetailTtemp.BinCardQty = BinCardQty.QuantityInSMT;
                            }
                        }

                        itemDisposeDetailVMList.Add(itemDisposeDetailTtemp);
                    }
                    itemDisposeTemp.FGItemDisposeDetails = itemDisposeDetailVMList;
                }
            }
            return itemDisposeTemp;
        }
    }

    public class FGItemDisposeViewModel
    {
        public FGItemDisposeViewModel()
        {
            this.FGItemDisposeDetails = new List<FGItemDisposeDetailViewModel>();
        }
        public int Id { get; set; }
        public System.DateTime Date { get; set; }
        public string DateString { get; set; }
        public string AuthorisedByName { set; get; }
        public int AuthorisedBy { get; set; }

        public System.DateTime AuthorisedDate { get; set; }
        public string AuthorisedDateString { get; set; }
        public Nullable<bool> DeleteFlag { get; set; }
        public List<FGItemDisposeDetailViewModel> FGItemDisposeDetails { get; set; }
    }

    public class FGItemDisposeDetailViewModel
    {
        public System.Guid Id { get; set; }
        public int FGItemDisposeId { get; set; }
        public int FGItemId { get; set; }
        public int FGTypeId { get; set; }
        public int FGGradeId { get; set; }
        public string Lot { get; set; }
        public double DisposeQuantity { get; set; }
        public double HandDisposeQuantity { get; set; }
        public Nullable<int> SalesUnitId { get; set; }
        public double QuantityInSFT { get; set; }
        public double QuantityInSMT { get; set; }
        public double QuantityInCTN { get; set; }
        public int QuantityInPCs { get; set; }

        public string BinCardName { set; get; }
        public Nullable<int> BinCardId { get; set; }

        public double BinCardQty { get; set; }
        public string FGItemName { set; get; }
        public string FGUnitName { set; get; }
        public string SalesUnit { set; get; }

        public string FGGradeName { set; get; }
        public string DisposeReason { set; get; }
    }

    
}