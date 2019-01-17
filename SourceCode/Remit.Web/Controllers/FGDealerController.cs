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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Remit.Web.Helpers;


namespace Remit.Web.Controllers
{
    public class FGDealerController : Controller
    {
        public readonly IFGDealerZoneService FGDealerZoneService;
        public readonly IEmploymentHistoryService EmploymentHistoryService;
        public readonly IEmployeeService EmployeeService;
        public readonly IFGDealerService FGDealerService;
        public readonly IFGSaleService fgSaleService;
        public readonly IZoneCommisionService zoneCommisionService;
        public readonly IZoneCommisionDetailService zoneCommisionDetailService;
        public readonly IDealerCommisionDetailService dealerCommisionDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();
        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string timeFormat = WebConfigurationManager.AppSettings["TimeFormat"];
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        public FGDealerController(IFGDealerZoneService FGDealerZoneService,IEmploymentHistoryService EmploymentHistoryService, IFGSaleService fgSaleService, IEmployeeService EmployeeService, IFGDealerService FGDealerService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService,
            IZoneCommisionService zoneCommisionService, IZoneCommisionDetailService zoneCommisionDetailService, IDealerCommisionDetailService dealerCommisionDetailService)
        {
            this.FGDealerZoneService = FGDealerZoneService;
            this.EmploymentHistoryService = EmploymentHistoryService;
            this.EmployeeService = EmployeeService;
            this.FGDealerService = FGDealerService;
            this.fgSaleService = fgSaleService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.zoneCommisionService = zoneCommisionService;
            this.zoneCommisionDetailService = zoneCommisionDetailService;
            this.dealerCommisionDetailService = dealerCommisionDetailService;
        }

        string cacheKey = "permission:FGDealer" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /FGDealer/
        public ActionResult Index()
        {
            const string url = "/FGDealer/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGDealer");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult FGDealer()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateFGDealer(FGDealer FGDealer)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = FGDealerService.GetFGDealer(FGDealer.Id);
            const string url = "/FGDealer/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(FGDealer))
                    {
                        if (this.FGDealerService.CreateFGDealer(FGDealer))
                        {
                            if (FGDealer.IsActive == true)
                            {
                                var getZone = zoneCommisionService.GetAllZoneCommision()
                                    .Where(zn => zn.ZoneId == FGDealer.DealersZoneId).OrderByDescending(zn => zn.EffectiveDate).FirstOrDefault();

                                if (getZone != null)
                                {
                                    if (getZone.ZoneCommisionDetails.Any())
                                    {
                                        foreach (var znDet in getZone.ZoneCommisionDetails)
                                        {
                                            DealerCommisionDetail dc = new DealerCommisionDetail();
                                            dc.Id = Guid.NewGuid();
                                            dc.DealerId = FGDealer.Id;
                                            dc.ZoneCommisionId = getZone.Id;
                                            dc.MonthlyTarget = znDet.MonthlyTarget;
                                            dc.MonthlyCommission = znDet.MonthlyCommission;
                                            dc.QuarterlyCommission = znDet.QuarterlyCommission;
                                            dc.HalfYearlyCommission = znDet.HalfYearlyCommission;
                                            dc.YearlyCommission = znDet.YearlyCommission;
                                            dc.FGUOMId = znDet.FGUOM.Id;

                                            try
                                            {
                                                this.dealerCommisionDetailService.CreateDealerCommisionDetail(dc);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                            
                            isSuccess = true;
                            message = "Dealer saved successfully!";
                        }
                        else
                        {
                            message = "Dealer could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same Dealer found!";
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
                    isNew.Name = FGDealer.Name;
                    isNew.ContactPersonName = FGDealer.ContactPersonName;
                    isNew.ContactPersonDesignation = FGDealer.ContactPersonDesignation;
                    isNew.ContactPhone = FGDealer.ContactPhone;
                    isNew.ContactEmail = FGDealer.ContactEmail;
                    isNew.Address = FGDealer.Address;
                    isNew.OwnerName = FGDealer.OwnerName;
                    isNew.OwnerPhone = FGDealer.OwnerPhone;
                    isNew.DefaultDeliverySite = FGDealer.DefaultDeliverySite;
                    isNew.DealersZoneId = FGDealer.DealersZoneId;
                    isNew.RelatedEmployeeId = FGDealer.RelatedEmployeeId;
                    isNew.AllocatedCreditLimit = FGDealer.AllocatedCreditLimit;
                    isNew.AvailableCreditLimit = FGDealer.AvailableCreditLimit;
                    isNew.IsActive = FGDealer.IsActive;
                    
                    if (this.FGDealerService.UpdateFGDealer(isNew))
                    {
                        if (FGDealer.IsActive == true)
                        {
                            var getZone = zoneCommisionService.GetAllZoneCommision()
                                .Where(zn => zn.ZoneId == FGDealer.DealersZoneId).OrderByDescending(zn => zn.EffectiveDate).FirstOrDefault();
                            if (getZone != null)
                            {
                                var chkDealCommission = this.dealerCommisionDetailService.GetAllDealerCommisionDetail()
                                    .FirstOrDefault(dc => dc.DealerId == isNew.Id && dc.ZoneCommisionId == getZone.Id);
                                if (chkDealCommission == null)
                                {
                                    if (getZone.ZoneCommisionDetails.Any())
                                    {
                                        foreach (var znDet in getZone.ZoneCommisionDetails)
                                        {
                                            DealerCommisionDetail dc = new DealerCommisionDetail();
                                            dc.Id = Guid.NewGuid();
                                            dc.DealerId = FGDealer.Id;
                                            dc.ZoneCommisionId = getZone.Id;
                                            dc.MonthlyTarget = znDet.MonthlyTarget;
                                            dc.MonthlyCommission = znDet.MonthlyCommission;
                                            dc.QuarterlyCommission = znDet.QuarterlyCommission;
                                            dc.HalfYearlyCommission = znDet.HalfYearlyCommission;
                                            dc.YearlyCommission = znDet.YearlyCommission;
                                            dc.FGUOMId = znDet.FGUOM.Id;

                                            try
                                            {
                                                this.dealerCommisionDetailService.CreateDealerCommisionDetail(dc);
                                            }
                                            catch { }
                                        }
                                    }
                                }
                            }
                        }

                        isSuccess = true;
                        message = "Dealer updated successfully!";
                    }
                    else
                    {
                        message = "Dealer could not updated!";
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
        private bool CheckIsExist(Model.Models.FGDealer FGDealer)
        {
            return this.FGDealerService.CheckIsExist(FGDealer);
        }
        [HttpPost]
        public JsonResult DeleteFGDealer(FGDealer FGDealer)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/FGDealer/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.FGDealerService.DeleteFGDealer(FGDealer.Id);
                if (isSuccess)
                {
                    message = "Dealer deleted successfully!";

                }
                else
                {
                    message = "Dealer can't be deleted!";
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

        public JsonResult GetActiveFGDealerList()
        {
            var FGDealerListObj = this.FGDealerService.GetAllFGDealer().Where(deal=> deal.IsActive == true);
            List<FGDealerViewModel> FGDealerVMList = new List<FGDealerViewModel>();

            foreach (var FGDealer in FGDealerListObj)
            {
                var FGDealerTemp = AfgDealer(FGDealer);
                FGDealerVMList.Add(FGDealerTemp);
            }
            return Json(FGDealerVMList, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetFGDealerList()
        {
            var FGDealerListObj = this.FGDealerService.GetAllFGDealer();
            List<FGDealerViewModel> FGDealerVMList = new List<FGDealerViewModel>();

            foreach (var FGDealer in FGDealerListObj)
            {
                var FGDealerTemp = AfgDealer(FGDealer);
                FGDealerVMList.Add(FGDealerTemp);
            }
            return Json(FGDealerVMList.OrderBy(aa=> aa.Name == "Other").ThenBy(bb=> bb.Name), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGDealerListForShowData()
        {
            var FGDealerListObj = this.FGDealerService.GetAllFGDealer();
            List<FGDealerViewModel> FGDealerVMList = new List<FGDealerViewModel>();

            foreach (var FGDealer in FGDealerListObj)
            {
                FGDealerViewModel FGDealerTemp = new FGDealerViewModel();
                FGDealerTemp.Id = FGDealer.Id;
                FGDealerTemp.Name = FGDealer.Name;
                FGDealerTemp.DealersZoneId = FGDealer.DealersZoneId;
                if (FGDealer.FGDealerZone != null)
                {
                    FGDealerTemp.DivisionId = FGDealer.FGDealerZone.DivisionId;
                    FGDealerTemp.ZoneName = FGDealer.FGDealerZone.ZoneName;
                }
                FGDealerVMList.Add(FGDealerTemp);
            }
            return Json(FGDealerVMList.OrderBy(aa => aa.Name == "Other").ThenBy(bb => bb.Name), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGDealerListForAutoComplete(string id) //id is serachText
        {
            List<FGDealerViewModel> dealerList = new List<FGDealerViewModel>();
            if (id != null)
            {
               var  dealerObjList = this.FGDealerService.GetFGDealerListBySearchKey(id);
               foreach (var fgDealer in dealerObjList)
                {
                    var fgDealerTemp = AfgDealer(fgDealer);

                    dealerList.Add(fgDealerTemp);
                }
            }

            return Json(dealerList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGDealer(int id)
        {
            var FGDealer = this.FGDealerService.GetFGDealer(id);
            var FGDealerTemp = AfgDealer(FGDealer);
            return Json(FGDealerTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGDealerWithInvoice(int id)
        {
            var FGDealer = this.FGDealerService.GetFGDealer(id);
            var FGDealerTemp = AfgDealer(FGDealer);
            
            List<FGSaleViewModel> fgSaleList = new List<FGSaleViewModel>();
            var fgSaleInvoice = this.fgSaleService.GetFGSaleByDealerDueAdvancedInvoice(id);
            if (fgSaleInvoice.Any())
            {
                foreach (var fgsale in fgSaleInvoice)
                {
                    FGSaleViewModel aFgSale = new FGSaleViewModel();
                    aFgSale.InvoiceNo = fgsale.InvoiceNo;
                    aFgSale.InvoiceDate = fgsale.InvoiceDate;
                    if (fgsale.InvoiceDate != null)
                    {
                        aFgSale.InvoiceDateString = fgsale.InvoiceDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                        aFgSale.InvoiceDateStringForFireFox = fgsale.InvoiceDate.Value.AddMinutes(timeZoneOffset).ToString("s");
                    }
                       
                    aFgSale.DueAdvancedAmount = fgsale.TotalDueAdvancedAmount;
                    fgSaleList.Add(aFgSale);
                }
            }
            FGDealerTemp.FgSales = fgSaleList;

            return Json(FGDealerTemp, JsonRequestBehavior.AllowGet);
        }
        
        public FGDealerViewModel AfgDealer(FGDealer FGDealer)
        {
            FGDealerViewModel FGDealerTemp = new FGDealerViewModel();
            FGDealerTemp.Id = FGDealer.Id;
            FGDealerTemp.Name = FGDealer.Name;
            FGDealerTemp.ContactPersonName = FGDealer.ContactPersonName;
            FGDealerTemp.ContactPersonDesignation = FGDealer.ContactPersonDesignation;
            FGDealerTemp.ContactPhone = FGDealer.ContactPhone;
            FGDealerTemp.ContactEmail = FGDealer.ContactEmail;
            FGDealerTemp.Address = FGDealer.Address;
            FGDealerTemp.OwnerName = FGDealer.OwnerName;
            FGDealerTemp.OwnerPhone = FGDealer.OwnerPhone;
            FGDealerTemp.DefaultDeliverySite = FGDealer.DefaultDeliverySite;
            FGDealerTemp.DealersZoneId = FGDealer.DealersZoneId;
            if (FGDealer.FGDealerZone != null)
            {
                FGDealerTemp.DivisionId = FGDealer.FGDealerZone.DivisionId;
                FGDealerTemp.ZoneName = FGDealer.FGDealerZone.ZoneName;
            }
                
            FGDealerTemp.RelatedEmployeeId = FGDealer.RelatedEmployeeId;
            if (FGDealer.Employee != null)
            {
                FGDealerTemp.EmployeeName = FGDealer.Employee.FullName;
                if (FGDealer.Employee.EmploymentHistories.Any())
                {
                    var aaa = FGDealer.Employee.EmploymentHistories.OrderByDescending(m => m.DateFrom != null).FirstOrDefault();
                    if (aaa != null) { 
                        FGDealerTemp.DepartmentId = aaa.DepartmentId;
                        FGDealerTemp.DesignationId = aaa.DesignationId;
                    }
                }
            }

            //var empHistoryObj = EmploymentHistoryService.GetAllEmploymentHistory().Where(a => a.EmployeeId == FGDealerTemp.RelatedEmployeeId).FirstOrDefault();
            //if (empHistoryObj != null)
            //{
            //    FGDealerTemp.DepartmentId = empHistoryObj.DepartmentId;
            //    FGDealerTemp.DesignationId = empHistoryObj.DesignationId;

            //}
            FGDealerTemp.AllocatedCreditLimit = FGDealer.AllocatedCreditLimit;
            FGDealerTemp.AvailableCreditLimit = FGDealer.AvailableCreditLimit;
            FGDealerTemp.IsActive = FGDealer.IsActive;

            return FGDealerTemp;
        }
        
    }

    public class FGDealerViewModel
    {
        public FGDealerViewModel()
        {
            this.FgSales = new List<FGSaleViewModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonDesignation { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string Address { get; set; }
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
        public string DefaultDeliverySite { get; set; }

        public int? DivisionId { get; set; }

        public int DealersZoneId { get; set; }
        public string ZoneName { get; set; }
        public int? RelatedEmployeeId { get; set; }
        public string EmployeeName { get; set; }

        public int? DepartmentId { get; set; }

        public int? DesignationId { get; set; }
        public int? AllocatedCreditLimit { get; set; }
        public int? AvailableCreditLimit { get; set; }
        public bool? IsActive { get; set; }
        public List<FGSaleViewModel> FgSales { get; set; }
       
    }
}