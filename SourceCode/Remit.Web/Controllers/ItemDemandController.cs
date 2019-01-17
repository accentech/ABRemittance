﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Service.Utilities;
using Helpers;
using Remit.Web.Helpers;
using Remit.Service.Enums;
using System.Linq.Dynamic;
using System.Web.Configuration;
using iTextSharp.text.pdf;

namespace Remit.Web.Controllers
{
    public class ItemDemandController : Controller
    {
        public readonly IItemDemandService itemDemandService;
        public readonly IItemInventoryService itemInventoryService;
        public readonly IItemService itemService;
        public readonly IItemIssueService itemIssueService;
        public readonly IEmployeeService employeeService;
        public readonly ICompanyService companyService;
        public readonly IDepartmentService departmentService;
        public readonly IItemDemandDetailService itemDemandDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly IAutoGeneratedNoService autoGeneratedNoService;

        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public ItemDemandController(IItemDemandService itemDemandService, IItemIssueService itemIssueService, IAutoGeneratedNoService autoGeneratedNoService,
            IDepartmentService departmentService, ICompanyService companyService, ISubModuleItemService subModuleItemService,IItemService itemService,
            IRoleSubModuleItemService roleSubModuleItemService, IItemDemandDetailService itemDemandDetailService, IItemInventoryService itemInventoryService,
            IWorkflowactionSettingService workflowactionSettingService, INotificationSettingService notificationSettingService, IEmployeeService employeeService
            )
        {
            this.itemIssueService = itemIssueService;
            this.autoGeneratedNoService = autoGeneratedNoService;
            this.companyService = companyService;
            this.departmentService = departmentService;
            this.itemDemandService = itemDemandService;
            this.subModuleItemService = subModuleItemService;
            this.itemDemandDetailService = itemDemandDetailService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.employeeService = employeeService;
            this.itemService = itemService;
            this.itemInventoryService = itemInventoryService;
        }

        string cacheKeyDemand = "permission:itemDemand" + Helpers.UserSession.GetUserFromSession().RoleId;
        //string cacheKeyDemandSpareParts = "permission:itemDemandSpareParts" + Helpers.UserSession.GetUserFromSession().RoleId;
        //const string urlDemand = "/ItemDemand/Index";
        RoleSubModuleItem permissionDemand = null;

        
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        public ActionResult Index()
        {
            const string urlDemand = "/ItemDemand/Index";
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            Session["GroupT"] = "raw";
            cacheKeyDemand += WebConfigurationManager.AppSettings["GroupType"];
            return CommonItemDemandList(urlDemand, cacheKeyDemand, "ItemDemand");
        }

        public ActionResult SparePartsAndOtherIndex()
        {
            ViewBag.GroupType = "";
            Session["GroupT"] = "spare";
            const string urlDemand = "/ItemDemand/SparePartsAndOtherIndex"; 
            return CommonItemDemandList(urlDemand, cacheKeyDemand, "ItemDemand");
        }


        private ActionResult CommonItemDemandList(string url, string cacheKey, string viewName)
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

        public ActionResult ApprovedInternalDemandList()
        {
            const string urlPreparedDemand = "/ItemDemand/Index";
            return CommonItemDemandList(urlPreparedDemand, cacheKeyDemand, "ApprovedInternalDemandList");
        }

        public ActionResult ShowAllItemDemandList()
        {
            const string urlApprovedDemand = "/ItemDemand/Index";
            return CommonItemDemandList(urlApprovedDemand, cacheKeyDemand, "ShowAllItemDemandList");
        }

        public ActionResult ShowPendingItemDemandList()
        {
            const string urlPreparedDemand = "/ItemDemand/Index";
            return CommonItemDemandList(urlPreparedDemand, cacheKeyDemand, "PreparedInternalDemandList");
        }

        public ActionResult ShowDemandReport()
        {
            const string urlPreparedDemand = "/ItemDemand/Index";
            return CommonItemDemandList(urlPreparedDemand, cacheKeyDemand, "DemandReport");
        }


        public ActionResult GoToItemIssue(string id, string redirectPage)  //edit fields
        {
            ViewBag.ItemDemandId = id;
            ViewBag.ForCreateItemIssue = 100;  //for return approve demand list
            ViewBag.RedirectPage = redirectPage;
            return View("~/Views/ItemIssue/ItemIssue.cshtml");
        }

        public ActionResult EditItemDemand(string id, string redirectPage, int status)// int status)  //edit fields
        {
            ViewBag.ItemDemandId = id;
            ViewBag.ForEditOrApproveItemDemand = 77;  //for edit

            if (status == 2)
            {
                ViewBag.ForEditOrApproveItemDemand = (int)CommonEnum.Approved; //for edit open status
            }

            ViewBag.RedirectPage = redirectPage;
            return View("ItemDemand");
        }

        public ActionResult EditApprovedInternalDemandList(string id, string redirectPage)  //approved demand
        {
            ViewBag.ItemDemandId = id;
            ViewBag.ForEditOrApproveItemDemand = (int)CommonEnum.Approved;  //status is approve
            ViewBag.RedirectPage = redirectPage;
            return View("ItemDemand");
        }

        public WorkflowactionSetting WorkflowactionSettingObj(int? employeeId, string url, int workFlowStatusEnum)
        {
            return this.workflowactionSettingService
                .GetAllWorkflowactionSetting().Where(app => app.EmployeeId == employeeId &&
                                                            app.SubModuleItem.UrlPath == url &&
                                                            app.WorkflowactionId == workFlowStatusEnum).FirstOrDefault();
        }
        //Approve or Reject
        [HttpPost]
        public JsonResult UpdateItemDemandStatus(ItemDemand itemDemand, string name)
        {
            string message = "";
            bool isSuccess = false;
            string urlDemand = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                urlDemand = "/ItemDemand/Index";
                cacheKeyDemand += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                urlDemand = "/ItemDemand/SparePartsAndOtherIndex";
            }


            permissionDemand = (RoleSubModuleItem)cacheProvider.Get(cacheKeyDemand) ??
                          roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(urlDemand, Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permissionDemand.UpdateOperation == true)
            {
                if (name == "approve")
                {
                    var statusReview = (int)WorkFlowActionEnum.Approve;
                    var user = UserSession.GetUserFromSession().EmployeeId;
                    WorkflowactionSetting workflowactionSettingObj = WorkflowactionSettingObj(user, urlDemand, statusReview);
                    if (workflowactionSettingObj == null)
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToApprove;
                        return Json(new { message = message, isSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    var statusReview = (int)WorkFlowActionEnum.ApproveReject;
                    var user = UserSession.GetUserFromSession().EmployeeId;
                    WorkflowactionSetting workflowactionSettingObj = WorkflowactionSettingObj(user, urlDemand, statusReview);
                    if (workflowactionSettingObj == null)
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToReject;
                        return Json(new { message = message, isSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
                    }
                }


                var itemDemandObj = this.itemDemandService.GetItemDemand(itemDemand.Id);
                if (itemDemandObj != null)
                {
                    if (itemDemandObj.ItemDemandDetails != null)
                    {
                        foreach (var itemDemDet in itemDemandObj.ItemDemandDetails.ToList())
                        {
                            this.itemDemandDetailService.DeleteItemDemandDetail(itemDemDet.Id);
                        }
                    }
                }
                if (itemDemand.ItemDemandDetails != null)
                {
                    foreach (var itemDemDet in itemDemand.ItemDemandDetails)
                    {
                        itemDemDet.Id = Guid.NewGuid();
                        itemDemDet.ItemDemandId = itemDemand.Id;
                        this.itemDemandDetailService.CreateItemDemandDetail(itemDemDet);
                    }
                }
                var itemDemandObjAttach = this.itemDemandService.GetItemDemand(itemDemand.Id);
                if (itemDemandObjAttach != null)
                {
                    itemDemandObjAttach.DemandNo = itemDemand.DemandNo;
                    itemDemandObjAttach.DemandOn = itemDemand.DemandOn != null ? itemDemand.DemandOn.Value.ToUniversalTime() : DateTime.UtcNow;
                    itemDemandObjAttach.DemandedBy = itemDemand.DemandedBy;
                    itemDemandObjAttach.DemandUsage = itemDemand.DemandUsage;
                    itemDemandObjAttach.ApprovedBy = UserSession.GetUserFromSession().EmployeeId;
                    itemDemandObjAttach.ApprovedOn = itemDemand.ApprovedOn != null ? itemDemand.ApprovedOn.Value.ToUniversalTime() : DateTime.UtcNow;
                    itemDemandObjAttach.ApproveRemarks = itemDemand.ApproveRemarks;
                    if (name == "approve")
                    {
                        itemDemandObjAttach.Status = (int)CommonEnum.Approved;
                    }
                    else
                    {
                        itemDemandObjAttach.Status = (int)CommonEnum.Rejected;
                    }
                    if (this.itemDemandService.UpdateItemDemand(itemDemandObjAttach))
                    {
                        isSuccess = true;
                        if (name == "approve")
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_approve, Resources.ResourceItemDemand.LblItemDemand);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_reject, Resources.ResourceItemDemand.LblItemDemand);
                        }
                        
                    }
                    else
                    {
                        if (name == "approve")
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notapprove, Resources.ResourceItemDemand.LblItemDemand);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notreject, Resources.ResourceItemDemand.LblItemDemand);
                        }
                    }
                }
                else
                {
                    message = message = string.Format(Resources.ResourceCommon.CMsg_notapprove, Resources.ResourceItemDemand.LblItemDemand);
                }
            }
            else
                message = Resources.ResourceCommon.MsgNoPermissionToUpdate;

            return Json(new { message = message, isSuccess = isSuccess }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult CreateItemDemand(ItemDemand itemDemand)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = itemDemand.Id == Guid.Empty ? true : false;

            string urlDemand = string.Empty;
            if ((string) Session["GroupT"] == "raw")
            {
                urlDemand = "/ItemDemand/Index";
                cacheKeyDemand += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                urlDemand = "/ItemDemand/SparePartsAndOtherIndex";
            }
            
            permissionDemand = (RoleSubModuleItem)cacheProvider.Get(cacheKeyDemand) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(urlDemand, Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (isNew)
            {
                if (permissionDemand.CreateOperation == true)
                {
                    if (!CheckIsExist(itemDemand))
                    {
                        var isDemReq = false;
                        itemDemand.Id = Guid.NewGuid();
                        itemDemand.DemandOn = itemDemand.DemandOn != null ? itemDemand.DemandOn.Value.ToUniversalTime():DateTime.UtcNow;
                        if (itemDemand.ItemDemandDetails != null)
                        {
                            
                            foreach (var itemDemandDet in itemDemand.ItemDemandDetails)
                            {
                                itemDemandDet.Id = Guid.NewGuid();
                                itemDemandDet.ItemDemandId = itemDemand.Id;
                                if (!isDemReq)
                                {
                                    var chkReq = itemService.GetItem(itemDemandDet.ItemId);
                                    if (chkReq != null)
                                    {
                                        if (chkReq.IsApprovalRequiredForDemand == true)
                                        {
                                            isDemReq = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (isDemReq)
                        {
                            itemDemand.Status = (int)CommonEnum.Pending;
                        }
                        else
                        {
                            itemDemand.Status = (int)CommonEnum.Approved;
                            itemDemand.ApprovedBy = UserSession.GetUserFromSession().EmployeeId;
                            itemDemand.ApprovedOn = itemDemand.DemandOn;
                        }
                        
                        if (this.itemDemandService.CreateItemDemand(itemDemand))
                        {
                            if (autoGeneratedNoService.UpdateNo("Demand")) { }

                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceItemDemand.LblItemDemand);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceItemDemand.LblItemDemand);
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = string.Format(Resources.ResourceCommon.CMsg_duplicate, Resources.ResourceItemDemand.LblDemandNo);
                    }
                }
                else
                {
                    message = Resources.ResourceCommon.MsgNoPermissionToCreate;
                }
            }
            else
            {
                if (permissionDemand.UpdateOperation == true)
                {
                    var isDemReq = false;
                    var itemDemandObj = this.itemDemandService.GetItemDemand(itemDemand.Id);
                    if (itemDemandObj != null)
                    {
                        if (itemDemandObj.ItemDemandDetails != null)
                        {
                            foreach (var itemDemDet in itemDemandObj.ItemDemandDetails.ToList())
                            {
                                this.itemDemandDetailService.DeleteItemDemandDetail(itemDemDet.Id);
                            }
                        }
                    }
                    if (itemDemand.ItemDemandDetails != null)
                    {
                        foreach (var itemDemDet in itemDemand.ItemDemandDetails)
                        {
                            itemDemDet.Id = Guid.NewGuid();
                            itemDemDet.ItemDemandId = itemDemand.Id;
                            if (!isDemReq)
                            {
                                var chkReq = itemService.GetItem(itemDemDet.ItemId);
                                if (chkReq != null)
                                {
                                    if (chkReq.IsApprovalRequiredForDemand == true)
                                    {
                                        isDemReq = true;
                                    }
                                }
                            }
                            this.itemDemandDetailService.CreateItemDemandDetail(itemDemDet);
                        }
                    }
                    var itemDemandObjAttach = this.itemDemandService.GetItemDemand(itemDemand.Id);
                    if (itemDemandObjAttach != null)
                    {
                        itemDemandObjAttach.DemandNo = itemDemand.DemandNo;
                        if (itemDemand.DemandOn != null)
                            itemDemandObjAttach.DemandOn = itemDemand.DemandOn.Value.ToUniversalTime();
                        itemDemandObjAttach.DemandedBy = itemDemand.DemandedBy;
                        itemDemandObjAttach.DemandUsage = itemDemand.DemandUsage;

                        if (this.itemDemandService.UpdateItemDemand(itemDemandObjAttach))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceItemDemand.LblItemDemand);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceItemDemand.LblItemDemand);
                        }
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

        private bool CheckIsExist(Model.Models.ItemDemand itemDemand)
        {
            return this.itemDemandService.CheckIsExist(itemDemand);
        }

        [HttpPost]
        public JsonResult DeleteItemDemand(ItemDemand itemDemand)
        {
            var isSuccess = false;
            var message = string.Empty;

            string urlDemand = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                urlDemand = "/ItemDemand/Index";
                cacheKeyDemand += WebConfigurationManager.AppSettings["GroupType"];
            }
            else
            {
                urlDemand = "/ItemDemand/SparePartsAndOtherIndex";
            }

            permissionDemand = (RoleSubModuleItem)cacheProvider.Get(cacheKeyDemand) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(urlDemand,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permissionDemand.DeleteOperation == true)
            {
                var getDemand = this.itemDemandService.GetItemDemand(itemDemand.Id);
                if (getDemand != null)
                {
                    if (getDemand.IsIssued != true)
                    {
                        //if (getDemand.ItemDemandDetails != null)
                        //{
                        //    foreach (var itemDemDet in getDemand.ItemDemandDetails.ToList())
                        //    {
                        //        this.itemDemandDetailService.DeleteItemDemandDetail(itemDemDet.Id);
                        //    }
                        //}
                        getDemand.IsDeleted = true;
                        getDemand.DeletedBy = UserSession.GetUserFromSession().EmployeeId;
                        getDemand.DeletedOn = DateTime.UtcNow;
                        isSuccess = this.itemDemandService.UpdateItemDemand(getDemand);
                        message = string.Format(isSuccess ? Resources.ResourceCommon.CMsg_delete : Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceItemDemand.LblItemDemand);

                    }
                    else
                    {
                        message = Resources.ResourceItemDemand.AlreadyIssued;
                    }
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceItemDemand.LblItemDemand);
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

        public JsonResult GetDemandListByYear(int year, int month)
        {
            var demandList = new List<ItemDemand>();
            var grpType = WebConfigurationManager.AppSettings["GroupType"];
            if ((string) Session["GroupT"] == "raw")
            {
                 demandList = this.itemDemandService.GetAllItemDemand().Where(dem =>
                    {
                        var itemDemandDetail = dem.ItemDemandDetails.FirstOrDefault();
                        return itemDemandDetail != null && (dem.DemandOn != null && dem.IsDeleted != true &&
                                                            dem.DemandOn.Value.AddMinutes(timeZoneOffset).Year ==
                                                            year &&
                                                            dem.DemandOn.Value.AddMinutes(timeZoneOffset).Month ==
                                                            month &&
                                                            itemDemandDetail.Item.ItemCategory.ItemGroup.TypeId ==
                                                            Convert.ToInt32(grpType));
                    })
                    .OrderBy(a => a.DemandOn).ToList();
            }
            else
            {
                demandList = this.itemDemandService.GetAllItemDemand().Where(dem =>
                    {
                        var itemDemandDetail = dem.ItemDemandDetails.FirstOrDefault();
                        return itemDemandDetail != null && (dem.DemandOn != null && dem.IsDeleted != true &&
                                                            dem.DemandOn.Value.AddMinutes(timeZoneOffset).Year ==
                                                            year &&
                                                            dem.DemandOn.Value.AddMinutes(timeZoneOffset).Month ==
                                                            month &&
                                                            itemDemandDetail.Item.ItemCategory.ItemGroup.TypeId !=
                                                            Convert.ToInt32(grpType));
                    })
                    .OrderBy(a => a.DemandOn).ToList();
            }

            
            List<ItemDemandViewModel> itemDemands = new List<ItemDemandViewModel>();
            if (demandList.Any())
            {
                foreach (var aDemand in demandList)
                {
                    ItemDemandViewModel itemDemand = new ItemDemandViewModel();
                    itemDemand.Id = aDemand.Id;
                    itemDemand.DemandNo = aDemand.DemandNo;
                    if (aDemand.DemandOn != null) itemDemand.DemandOnDate = (DateTime)aDemand.DemandOn.Value.AddMinutes(timeZoneOffset);
                    itemDemand.Status = aDemand.Status;
                    itemDemand.IsThisDemandAlreadyIssued = aDemand.IsIssued;

                    itemDemands.Add(itemDemand);
                }
            }
            return Json(itemDemands, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemDemand(Guid id)
        {
            var itemDemand = this.itemDemandService.GetItemDemand(id);
            ItemDemandViewModel itemDemandTemp = null;
            if (itemDemand != null)
            {
                itemDemandTemp = new ItemDemandViewModel();
                itemDemandTemp.Id = itemDemand.Id;
                itemDemandTemp.DemandNo = itemDemand.DemandNo;
                itemDemandTemp.IsThisDemandAlreadyIssued = itemDemand.IsIssued;
                itemDemandTemp.DemandUsage = itemDemand.DemandUsage;
                if (itemDemand.DemandOn != null)
                    itemDemandTemp.DemandOn = itemDemand.DemandOn.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);

                itemDemandTemp.DemandedBy = itemDemand.DemandedBy;
                if (itemDemand.Employee != null)
                    itemDemandTemp.DemandedByName = itemDemand.Employee.FullName;

                itemDemandTemp.ApprovedBy = itemDemand.ApprovedBy;
                if (itemDemand.Employee1 != null)
                    itemDemandTemp.ApprovedByName = itemDemand.Employee1.FullName;

                if (itemDemand.ApprovedOn != null)
                    itemDemandTemp.ApprovedOn = itemDemand.ApprovedOn.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);

                itemDemandTemp.Status = itemDemand.Status;
                if (itemDemand.Status != null)
                {
                    try
                    {
                        itemDemandTemp.StatusString = ((CommonEnum)itemDemand.Status).ToString();
                    }
                    catch (Exception e) { }
                }

                if (itemDemand.ItemDemandDetails != null)
                {
                    List<ItemDemandDetailViewModel> ItemDemandDetailVMList = new List<ItemDemandDetailViewModel>();
                    foreach (var itemDemandDet in itemDemand.ItemDemandDetails)
                    {
                        ItemDemandDetailViewModel itemDemandDetTemp = new ItemDemandDetailViewModel();
                        itemDemandDetTemp.Id = itemDemandDet.Id;
                        itemDemandDetTemp.Quantity = itemDemandDet.Quantity;
                        itemDemandDetTemp.ItemId = itemDemandDet.ItemId;
                        itemDemandDetTemp.UnitId = itemDemandDet.UnitId;
                        if (itemDemandDet.UnitOfMeasurement != null)
                            itemDemandDetTemp.UnitIdName = itemDemandDet.UnitOfMeasurement.Name;
                        if (itemDemandDet.Item != null)
                            itemDemandDetTemp.ItemCategoryId = itemDemandDet.Item.ItemCategoryId;

                        ItemDemandDetailVMList.Add(itemDemandDetTemp);
                    }
                    itemDemandTemp.ItemDemandDetails = ItemDemandDetailVMList;
                }
            }
            return Json(itemDemandTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemDemandAndItemInventoryForIssue(Guid id)
        {
            var isSuccess = false;
            var mes = string.Empty;
            var itemDemand = this.itemDemandService.GetItemDemand(id);
            ItemDemandViewModel itemDemandTemp = null;
            if (itemDemand != null)
            {
                itemDemandTemp = new ItemDemandViewModel();
                itemDemandTemp.Id = itemDemand.Id;
                itemDemandTemp.DemandedByName = itemDemand.Employee.FullName;
                itemDemandTemp.DemandUsage = itemDemand.DemandUsage;
                
                if (itemDemand.ItemDemandDetails != null)
                {
                    List<ItemDemandDetailViewModel> ItemDemandDetailVMList = new List<ItemDemandDetailViewModel>();
                    foreach (var itemDemandDet in itemDemand.ItemDemandDetails)
                    {
                        ItemDemandDetailViewModel itemDemandDetTemp = new ItemDemandDetailViewModel();
                        itemDemandDetTemp.Id = itemDemandDet.Id;
                        itemDemandDetTemp.Quantity = Math.Round((double) itemDemandDet.Quantity, 3);
                        itemDemandDetTemp.ItemId = itemDemandDet.ItemId;
                        itemDemandDetTemp.UnitId = itemDemandDet.UnitId;
                       if (itemDemandDet.UnitOfMeasurement != null)
                            itemDemandDetTemp.UnitIdName = itemDemandDet.UnitOfMeasurement.Name;
                        if (itemDemandDet.Item != null)
                        {
                            itemDemandDetTemp.ItemCategoryId = itemDemandDet.Item.ItemCategoryId;
                            List<ItemInventoryViewModel> itemInventorys = new List<ItemInventoryViewModel>();
                            var getInventory = itemInventoryService.GetAllItemInventory()
                                .Where(itm => itm.ItemId == itemDemandDet.ItemId);
                            foreach (var inv in getInventory)
                            {
                                ItemInventoryViewModel inventory = new ItemInventoryViewModel();
                                inventory.BinCardId = inv.BinCardId;
                                if (inv.BinCard != null)
                                inventory.BinCardName = inv.BinCard.CardNo;
                                inventory.Quantity = Math.Round((double) inv.Quantity, 3);
                                itemInventorys.Add(inventory);
                            }
                            itemDemandDetTemp.ItemInventoryViewModels = itemInventorys;
                            itemDemandDetTemp.TotalStockBalance = itemDemandDetTemp.ItemInventoryViewModels.Sum(l => l.Quantity);
                        }
                        ItemDemandDetailVMList.Add(itemDemandDetTemp);
                    }
                    itemDemandTemp.ItemDemandDetails = ItemDemandDetailVMList;

                    return Json(new { isSuccess = true, mes = "Demand Ok", demDetail = itemDemandTemp }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isSuccess = isSuccess, mes = "No Demand Detail Found" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new {isSuccess = isSuccess, mes = "No Demand Found"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemDemandListForAutoCompleteSerchKey(string id) //id is serachText
        {
            List<string> orderList = new List<string>();
            if (id != null)
            {
                orderList = this.itemDemandService.GetItemDemandListBySearchText(id);
            }
            return Json(orderList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemDemandListFromDropDown() //id is serachText
        {
            var getDemands = new List<ItemDemand>();
            List<ItemDemand> orderList = new List<ItemDemand>();
            var grpType = WebConfigurationManager.AppSettings["GroupType"];
            if ((string) Session["GroupT"] == "raw")
            {
                getDemands = this.itemDemandService.GetItemDemandListByDropDown().Where(dem =>
                    {
                        var itemDemandDetail = dem.ItemDemandDetails.FirstOrDefault();
                        return itemDemandDetail != null && (itemDemandDetail.Item.ItemCategory.ItemGroup.TypeId ==
                                                            Convert.ToInt32(grpType));
                    }).ToList();
            }
            else
            {
                getDemands = this.itemDemandService.GetItemDemandListByDropDown().Where(dem =>
                {
                    var itemDemandDetail = dem.ItemDemandDetails.FirstOrDefault();
                    return itemDemandDetail != null && (itemDemandDetail.Item.ItemCategory.ItemGroup.TypeId !=
                                                        Convert.ToInt32(grpType));
                }).ToList();
            }
            
            foreach (var dem in getDemands)
            {
                ItemDemand aItemDemand = new ItemDemand();
                aItemDemand.Id = dem.Id;
                aItemDemand.DemandNo = dem.DemandNo;

                orderList.Add(aItemDemand);
            }
            
            return Json(orderList, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemDemandViewModel
    {
        public ItemDemandViewModel()
        {
            this.ItemDemandDetails = new List<ItemDemandDetailViewModel>();
        }

        public Guid Id { set; get; }
        public string DemandNo { get; set; }
        public bool? IsThisDemandAlreadyIssued { get; set; }
        public string DemandOn { get; set; }
        public Nullable<int> DemandedBy { get; set; }
        public string DemandedByName { get; set; }
        public string DemandFor { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string ApprovedOn { get; set; }
        public int? Status { get; set; }
        public string StatusString { get; set; }
        public string ApproveRemarks { get; set; }
        public string DemandUsage { get; set; }
        public DateTime DemandOnDate { get; set; }
        public List<ItemDemandDetailViewModel> ItemDemandDetails { get; set; }
    }

    public class ItemDemandDetailViewModel
    {
        public ItemDemandDetailViewModel()
        {
            this.ItemInventoryViewModels = new List<ItemInventoryViewModel>();
        }
        public System.Guid Id { get; set; }
        public string DemandNo { get; set; }
        public Guid? ItemId { get; set; }
        public double Quantity { get; set; }
        public Nullable<int> UnitId { get; set; }
        public double? TotalStockBalance { set; get; }
        public Nullable<int> ItemCategoryId { get; set; }
        public string UnitIdName { get; set; }
        public string UnitofMeasurementName { get; set; }
        public List<ItemInventoryViewModel> ItemInventoryViewModels { get; set; }
    }

    public class ItemInventoryViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid ItemId { get; set; }
        public double Quantity { get; set; }
        public double QuantityIssued { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<double> HandQtyForIssueUpdate { get; set; }
        public string BinUnitIdName { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public string BinCardName { set; get; }
        public virtual BinCard BinCard { get; set; }
    }
}