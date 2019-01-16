using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using iTextSharp.text.pdf;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Service.Enums;
using Remit.Service.Utilities;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class FGItemPriceController : Controller
    {
        #region fields

        public readonly ISubModuleItemService subModuleItemService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly IFGItemService fgItemService;
        public readonly IFGItemPriceService fgItemPriceService;
        public readonly IFGItemPriceDetailService fgItemPriceDetailService;

        public readonly ISupplierService supplierService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        #endregion

        #region Constructor

        public FGItemPriceController(ISupplierService supplierService, ISubModuleItemService subModuleItemService,
            IRoleSubModuleItemService roleSubModuleItemService,
            IWorkflowactionSettingService workflowactionSettingService,
            IItemService itemService, IFGItemPriceService fgItemPriceService,
            IFGItemPriceDetailService fgItemPriceDetailService, IFGItemService fgItemService)
        {
            this.supplierService = supplierService;

            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.itemService = itemService;
            this.fgItemPriceService = fgItemPriceService;
            this.fgItemPriceDetailService = fgItemPriceDetailService;
            this.fgItemService = fgItemService;
        }

        #endregion


        #region Global Fields

        string cacheKey = "permission:FGItemPrice" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string timeFormat = WebConfigurationManager.AppSettings["TimeFormat"];
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        const string url = "/FGItemPrice/FGItemPrice";

        #endregion



        #region Methods

        /// <summary>
        /// <returns> View FGItemPrice.cshtml</returns>
        /// </summary>

        public ActionResult FGItemPrice()
        {

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission =
                    roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                        Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View();
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");



        }



        /// <summary>
        /// Load Pricing Dates by Current Month and Year
        /// </summary>
        /// <returns> PriceDate List </returns>
        public JsonResult GetPricingDateListByMonthYear(int month = 0, int year = 0)
        {
            // always use Current month to load Dates
            //var today = DateTime.Today.ToLocalTime();
            //if (month ==0 && year== 0)
            //{
            //    month = today.Month;
            //    year = today.Year;
            //}

            var dt1 = new DateTime(year, month, 1).ToUniversalTime();
            var dt2 = new DateTime(year, month, 1).ToUniversalTime(); 
            if ((month + 1) == 13)
            {
                dt2 = new DateTime(year + 1, 1, 1).ToUniversalTime();
            }
            else
            {
                dt2 = new DateTime(year, month + 1, 1).ToUniversalTime();
            }

           

            try
            {
                var fgItemPriceList = this.fgItemPriceService.GetAllFGItemPrice().Where(a => a.PricingDate != DateTime.MinValue 
                    && (a.PricingDate >= dt1 && a.PricingDate < dt2));
                
                //var fgItemPriceList = this.fgItemPriceService.GetAllFGItemPrice()
                //    .Where(a => a.PricingDate.Month == month && a.PricingDate.Year == year).ToList();

                List<FgItemPriceViewModelForList> itemPricelist = new List<FgItemPriceViewModelForList>();

                if (fgItemPriceList.Any())
                {
                    foreach (var fgItemPrice in fgItemPriceList)
                    {
                        FgItemPriceViewModelForList ip = new FgItemPriceViewModelForList();

                        ip.Id = fgItemPrice.Id;
                        ip.PricingDate = fgItemPrice.PricingDate.AddMinutes(timeZoneOffset).ToString(dateFormat); ;
                        itemPricelist.Add(ip);
                    }
                }

                return Json(itemPricelist, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return null;
            }
            //var fgItemPriceList = this.fgItemPriceService.GetAllFGItemPrice()
            //    .Where(a => a.PricingDate.Month == date.Month && a.PricingDate.Year == date.Year).ToList();


        }


        /// <summary>
        /// Save Item Price
        /// </summary>
        [HttpPost]
        public JsonResult CreateFGItemPrice(FGItemPrice itemPrice, string insertType)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            bool isNew = itemPrice.Id == 0;

            if (isNew)
            {
                var chkIsExist = this.fgItemPriceService.CheckIsExist(itemPrice);
                if (chkIsExist)
                {
                    message = Resources.ResourceCommon.AlreadyExistSameDay;
                }
                else
                {
                    if (permission.CreateOperation == true)
                    {
                        if (itemPrice.ApproveDocumentPath != null)
                        {
                            itemPrice.ApproveDocumentPath = itemPrice.ApproveDocumentPath.Replace(" ", "_").Trim();
                        }



                        foreach (var fgItemPriceDetail in itemPrice.FGItemPriceDetails)
                        {
                            fgItemPriceDetail.Id = Guid.NewGuid();
                            //fgItemPriceDetail.EffectiveDate = itemPrice.EffectiveDate; // discard, as problem found that immediate after it shows itemprice=0
                            //10 Dec 2018 
                            fgItemPriceDetail.EffectiveDate = itemPrice.EffectiveDate.ToUniversalTime();

                        }

                        itemPrice.EffectiveDate = itemPrice.EffectiveDate.ToUniversalTime();
                        itemPrice.PricingDate = itemPrice.PricingDate.ToUniversalTime();

                        if (this.fgItemPriceService.CreateFGItemPrice(itemPrice))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save,
                                Resources.ResourceFGItemPrice.MsgItemPrice);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave,
                                Resources.ResourceFGItemPrice.MsgItemPrice);
                        }

                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToCreate;
                    }
                }
               
            }
            else
            {
                if (permission.UpdateOperation == true)
                {
                    var fgItemPriceObj = this.fgItemPriceService.GetFGItemPrice(itemPrice.Id);
                    if (insertType == "approve")
                    {
                        var statusReview = (int)WorkFlowActionEnum.Approve;
                        var user = UserSession.GetUserFromSession().EmployeeId;
                        WorkflowactionSetting workflowactionSettingObj =
                            WorkflowactionSettingObj(user, url, statusReview);
                        if (workflowactionSettingObj == null)
                        {
                            message = Resources.ResourceCommon.MsgNoPermissionToApprove;
                            return Json(new {message = message, isSuccess = isSuccess}, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            fgItemPriceObj.ApprovedBy = UserSession.GetUserFromSession().EmployeeId;
                            fgItemPriceObj.ApprovedOn = DateTime.UtcNow;
                            fgItemPriceObj.IsApproved = true;
                        }
                    }
                    
                   
                    fgItemPriceObj.EffectiveDate = itemPrice.EffectiveDate.ToUniversalTime();
                    fgItemPriceObj.PricingDate = itemPrice.PricingDate.ToUniversalTime();
                    if (fgItemPriceObj.FGItemPriceDetails.Any())
                    {
                        foreach (var fgItemPriceDetail in fgItemPriceObj.FGItemPriceDetails.ToList())
                        {
                            this.fgItemPriceDetailService.DeleteFGItemPriceDetail(fgItemPriceDetail.Id);
                        }
                    }
                    
                    foreach (var fgItemPriceDetail in itemPrice.FGItemPriceDetails)
                    {
                        fgItemPriceDetail.Id = Guid.NewGuid();
                        //fgItemPriceDetail.EffectiveDate = itemPrice.EffectiveDate; // discard, as problem found that immediate after it shows itemprice=0 in FGSale
                        //10 Dec 2018 
                        fgItemPriceDetail.EffectiveDate = itemPrice.EffectiveDate.ToUniversalTime();
                    }

                    fgItemPriceObj.FGItemPriceDetails = itemPrice.FGItemPriceDetails;

                    if (this.fgItemPriceService.UpdateFGItemPrice(fgItemPriceObj))
                    {
                        isSuccess = true;
                        if (insertType == "approve")
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_approve,
                                Resources.ResourceFGItemPrice.MsgItemPrice);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_update,
                                Resources.ResourceFGItemPrice.MsgItemPrice);
                        }
                    }
                    else
                    {
                        if (insertType == "approve")
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notapprove,
                                Resources.ResourceFGItemPrice.MsgItemPrice);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate,
                                Resources.ResourceFGItemPrice.MsgItemPrice);
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



        public WorkflowactionSetting WorkflowactionSettingObj(int? employeeId, string url, int workFlowStatusEnum)
        {
            return this.workflowactionSettingService
                .GetAllWorkflowactionSetting().Where(app => app.EmployeeId == employeeId &&
                                                            app.SubModuleItem.UrlPath == url &&
                                                            app.WorkflowactionId == workFlowStatusEnum).FirstOrDefault();
        }

        /// <summary>
        /// Delete Item Price
        /// </summary>
        [HttpPost]
        public JsonResult DeleteFGItemPrice(FGItemPrice itemPrice)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {

                var itemPriceObj = this.fgItemPriceService.GetFGItemPrice(itemPrice.Id);

                if (itemPriceObj != null)
                {
                    if (itemPriceObj.FGItemPriceDetails.Any())
                    {
                        foreach (var fgItemPriceDetail in itemPriceObj.FGItemPriceDetails.ToList())
                        {
                            try
                            {
                                this.fgItemPriceDetailService.DeleteFGItemPriceDetail(fgItemPriceDetail.Id);
                            }
                            catch (Exception e)
                            {

                            }

                        }
                    }
                }


                isSuccess = this.fgItemPriceService.DeleteFGItemPrice(itemPrice.Id);
                if (isSuccess)
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_delete,
                        Resources.ResourceFGItemPrice.MsgItemPrice);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete,
                        Resources.ResourceFGItemPrice.MsgItemPrice);
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


        /// <summary>
        /// Upload Approved File
        /// </summary>
        public void UpoladFile(HttpPostedFileBase file)
        {
            if (file != null)
            {
                string str = file.FileName;
                string ext = str.Substring(0, str.LastIndexOf(".") + 1).TrimEnd('.');
                if (ext != String.Empty)
                {
                    ext = ext.Replace(" ", "_");
                }

                if (System.IO.File.Exists(
                    Server.MapPath("~/Files/FGItemPrice/" + ext + Path.GetExtension(file.FileName))))
                {
                    System.IO.File.Delete(
                        Server.MapPath("~/Files/FGItemPrice/" + ext + Path.GetExtension(file.FileName)));
                }

                var saveToFileLoc = Server.MapPath("~/Files/FGItemPrice/" + ext + Path.GetExtension(file.FileName));
                try
                {
                    file.SaveAs(saveToFileLoc);
                }
                catch (Exception e)
                {
                    Console.WriteLine("File Save Error: " + e);
                }
            }


        }


        public JsonResult GetFGItemPriceById(int itemPriceId)
        {
            var fgItemPrice = this.fgItemPriceService.GetFGItemPrice(itemPriceId);
            FgItemPriceViewModel fgItemPriceTemp = null;


            if (fgItemPrice != null)
            {
                fgItemPriceTemp = PrepareFGItemPrice(fgItemPrice);
            }

            return Json(fgItemPriceTemp, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetFGItemPriceList()
        {
            var fgItemPriceListObj = this.fgItemPriceService.GetAllFGItemPrice();
            List<FgItemPriceViewModel> fgItemPriceVMList = new List<FgItemPriceViewModel>();

            foreach (var fgItemPrice in fgItemPriceListObj)
            {
                var fgItemTemp = PrepareFGItemPrice(fgItemPrice);
                fgItemPriceVMList.Add(fgItemTemp);
            }

            return Json(fgItemPriceVMList, JsonRequestBehavior.AllowGet);
        }


        private FgItemPriceViewModel PrepareFGItemPrice(FGItemPrice fgItemPrice )
        {
            List<FgItemPriceDetailViewModel> ipdvmList = new List<FgItemPriceDetailViewModel>();
            FgItemPriceViewModel fgItemPriceTemp = new FgItemPriceViewModel();
            
            fgItemPriceTemp.Id = fgItemPrice.Id;
            fgItemPriceTemp.EffectiveDate = fgItemPrice.EffectiveDate.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
            fgItemPriceTemp.PricingDate = fgItemPrice.PricingDate.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
            fgItemPriceTemp.DocName = fgItemPrice.ApproveDocumentPath;
            fgItemPriceTemp.ApproveDocumentPath = "/files/FGItemPrice/" + fgItemPrice.ApproveDocumentPath;

            fgItemPriceTemp.IsApproved = fgItemPrice.IsApproved;
            
            if (!System.IO.File.Exists(Server.MapPath(fgItemPriceTemp.ApproveDocumentPath)))
            {
                fgItemPriceTemp.DocName = null;
            }
            fgItemPriceTemp.PricingBy = fgItemPrice.Employee.Id;
            fgItemPriceTemp.PricingByName = fgItemPrice.Employee.FullName;

            if (fgItemPrice.FGItemPriceDetails.Any())
            {
                foreach (var fgItemPriceDetail in fgItemPrice.FGItemPriceDetails)
                {
                    FgItemPriceDetailViewModel fgItemDetailVm = new FgItemPriceDetailViewModel();

                    fgItemDetailVm.Id = fgItemPriceDetail.Id;
                    fgItemDetailVm.FGGradeId = fgItemPriceDetail.FGGradeId;
                    fgItemDetailVm.FGGradeName = fgItemPriceDetail.FGGrade.Grade;
                    fgItemDetailVm.FGItemPriceId = fgItemPriceDetail.FGItemPriceId;
                    fgItemDetailVm.FGItemId = fgItemPriceDetail.FGItemId;
                    fgItemDetailVm.TypeId = this.fgItemService.GetFGItem(fgItemPriceDetail.FGItemId).TypeId;
                    fgItemDetailVm.FGItemName = fgItemPriceDetail.FGItem.Name;
                    if (fgItemPriceDetail.SalesUnitId != null)
                        fgItemDetailVm.SalesUnitId = (int)fgItemPriceDetail.SalesUnitId;
                    fgItemDetailVm.SalesUnitName = fgItemPriceDetail.FGUOM.UnitName;
                    fgItemDetailVm.SalesUnitPrice = fgItemPriceDetail.SalesUnitPrice;
                    fgItemDetailVm.SpecialDiscount = fgItemPriceDetail.SpecialDiscount;

                    //fgItemDetailVm.EffectiveDate = fgItemPriceDetail.EffectiveDate.ToString(dateTimeFormat);
                                                  
                    fgItemDetailVm.EffectiveDate = fgItemPriceDetail.EffectiveDate.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);

                    var itemPriceDetailObj = fgItemPriceDetailService.GetFGItemPriceDetailByItemId(fgItemPriceDetail
                        .FGItemId);
                    if (itemPriceDetailObj != null)
                    {
                        fgItemDetailVm.PreviousEffectiveDate = itemPriceDetailObj.EffectiveDate.AddMinutes(timeZoneOffset).ToString(dateFormat);
                    }

                    ipdvmList.Add(fgItemDetailVm);
                }
            }

            fgItemPriceTemp.FgItemPriceDetails = ipdvmList;

            return fgItemPriceTemp;
        }

        #endregion

    }

    #region ViewModels





    public class FgItemPriceViewModel
    {
        public FgItemPriceViewModel()
        {
            this.FgItemPriceDetails = new List<FgItemPriceDetailViewModel>();
        }
        public int Id { get; set; }
        public string EffectiveDate { get; set; }
        public int PricingBy { get; set; }
        public string PricingByName { get; set; }
        public string PricingDate { get; set; }
        public string DocName { get; set; }
        public bool? IsApproved { set; get; }
        public string ApproveDocumentPath { get; set; }
        public virtual ICollection<FgItemPriceDetailViewModel> FgItemPriceDetails { get; set; }
    }

    public partial class FgItemPriceDetailViewModel
    {
        public System.Guid Id { get; set; }
        public int TypeId { get; set; }
        public int FGItemPriceId { get; set; }
        public int FGItemId { get; set; }
        public string FGItemName { get; set; }
        public int FGGradeId { get; set; }
        public string FGGradeName { get; set; }
        public int SalesUnitId { get; set; }
        public string SalesUnitName { get; set; }
        public double SalesUnitPrice { get; set; }
        public double? SpecialDiscount { get; set; }
        public string EffectiveDate { get; set; }
        public string PreviousEffectiveDate { get; set; }

    }

    public class FgItemPriceViewModelForList
    {
        public int Id { get; set; }
        public string PricingDate { get; set; }
    }


    #endregion
}