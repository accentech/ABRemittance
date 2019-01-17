using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class BreakageCalculatorController : Controller
    {
        public readonly IBreakageCalculatorService breakageCalculatorService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;

        public readonly IFGSaleService fgSaleService;
        public readonly IFGSalesDetailService fgSalesDetailService;
        public readonly IFGDealerService fgDealerService;
        public readonly IDealerCommisionDetailService dealerCommisionDetailService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();
        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();
        public BreakageCalculatorController(IBreakageCalculatorService breakageCalculatorService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService,
            IFGSaleService fgSaleService, IFGSalesDetailService fgSalesDetailService, IFGDealerService fgDealerService, IDealerCommisionDetailService dealerCommisionDetailService)
        {
            this.breakageCalculatorService = breakageCalculatorService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.fgSaleService = fgSaleService;
            this.fgSalesDetailService = fgSalesDetailService;
            this.fgDealerService = fgDealerService;
            this.dealerCommisionDetailService = dealerCommisionDetailService;
        }

        string cacheKey = "permission:breakageCalculator" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /BreakageCalculator/
        public ActionResult Index()
        {
            const string url = "/BreakageCalculator/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("BreakageCalculator");//
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }
        
        public ActionResult BreakageCalculator()
        {
            return View();
        }
        

        [HttpPost]
        public JsonResult CreateBreakageCalculator(List<BreakageCalculator> breakageCalculatorList)
        {
            var isSuccess = false;
            var message = string.Empty;
            const string url = "/BreakageCalculator/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);
            var sn = 1;
            foreach (var breakageCalculator in breakageCalculatorList)
            {
                var isNew = this.breakageCalculatorService.GetAllBreakageCalculator()
                    .FirstOrDefault(cc => cc.PeriodNumber == breakageCalculator.PeriodNumber);
                if (isNew == null)
                {
                    if (permission.CreateOperation == true)
                    {
                        breakageCalculator.Id = Guid.NewGuid();
                        breakageCalculator.CreatedBy = UserSession.GetUserFromSession().EmployeeId;
                        breakageCalculator.CreatedOn = DateTime.UtcNow;
                        breakageCalculator.SN = sn;

                        if (this.breakageCalculatorService.CreateBreakageCalculator(breakageCalculator))
                        {
                            sn++;
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceBreakageCalculator.Title);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceBreakageCalculator.Title);
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
                        isNew.UpdatedBy = UserSession.GetUserFromSession().EmployeeId;
                        isNew.UpdatedOn = DateTime.UtcNow;
                        isNew.DealerId = breakageCalculator.DealerId;
                        isNew.PeriodType = breakageCalculator.PeriodType;
                        isNew.PeriodName = breakageCalculator.PeriodName;
                        isNew.Year = breakageCalculator.Year;
                       
                        isNew.BreakageAmount = breakageCalculator.BreakageAmount;
                        isNew.BreakageRate = breakageCalculator.BreakageRate;
                        isNew.InvoiceAmount = breakageCalculator.InvoiceAmount;
                        //isNew.PaidAmount = breakageCalculator.PaidAmount;

                        if (this.breakageCalculatorService.UpdateBreakageCalculator(isNew))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceBreakageCalculator.Title);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceBreakageCalculator.Title);
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
        private bool CheckIsExist(Model.Models.BreakageCalculator breakageCalculator)
        {
            return this.breakageCalculatorService.CheckIsExist(breakageCalculator);
        }
        [HttpPost]
        public JsonResult DeleteBreakageCalculator(BreakageCalculator breakageCalculator)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/BreakageCalculator/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.breakageCalculatorService.DeleteBreakageCalculator(breakageCalculator.Id);
                if (isSuccess)
                {
                    message = "BreakageCalculator deleted successfully!";

                }
                else
                {
                    message = "BreakageCalculator can't be deleted!";
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

        
        public JsonResult GetAllData(int year, int type, int periodFrom, int periodTo)
        {
            List<BreakageCalculatorViewModel> breakageCalculatorList = new List<BreakageCalculatorViewModel>();
            var PeriodName = string.Empty;
            
                if (periodTo == 6)
                    PeriodName = Resources.ResourceCommissionCalculator.JanToJun;
                if (periodTo == 12)
                    PeriodName = Resources.ResourceCommissionCalculator.JulyToDec;
            
            var isExist = false;
            var chkIsExist = this.breakageCalculatorService.GetAllBreakageCalculator().FirstOrDefault(
                cc => cc.Year == year && cc.PeriodType == type && cc.PeriodName == PeriodName);
            if (chkIsExist != null)
            {
                isExist = true;
            }
            var fgDealer = this.fgDealerService.GetAllFGDealer().Where(de => de.IsActive == true);
            foreach (var aDealer in fgDealer)
            {
                //double sftRate = 0.00;
                //double pcsRate = 0.00;
                //double ctnRate = 0.00;
                //double smtRate = 0.00;
                
                var fgsales = this.fgSaleService.GetAllFGSale().Where(deal =>
                {
                    return deal.InvoiceDate != null && (deal.DealerId == aDealer.Id && deal.IsDelete != true
                                                        && deal.CustomerType == 1 && deal.Reason == 1
                                                        && deal.InvoiceDate.Value.AddMinutes(timeZoneOffset).Year == year
                                                        && deal.InvoiceDate.Value.AddMinutes(timeZoneOffset).Month >= periodFrom 
                                                        && deal.InvoiceDate.Value.AddMinutes(timeZoneOffset).Month <= periodTo);
                });

                var TotalInvoiceAmount = fgsales.Sum(sale=> sale.TotalAmount - sale.TotalAdjustment - sale.DiscountAmount);
                var BreakageAmount = Convert.ToDouble(TotalInvoiceAmount * (Convert.ToDouble(ConfigurationManager.AppSettings["BreakageRate"])/100)) ;
                
                BreakageCalculatorViewModel aBreakage = new BreakageCalculatorViewModel();
                aBreakage.Id = Guid.Empty;
                aBreakage.DealerId = aDealer.Id;
                aBreakage.DealerName = aDealer.Name;
                aBreakage.PeriodType = type;
                aBreakage.PeriodName = PeriodName;
                aBreakage.Year = year;
                
                aBreakage.PeriodNumber = Convert.ToString(aBreakage.Year) + Convert.ToString(aBreakage.DealerId) + Convert.ToString(type) + periodTo.ToString("D2");
                
                aBreakage.InvoiceAmount = TotalInvoiceAmount;
                //aBreakage.InvoiceAmount = Math.Round((double)aBreakage.InvoiceAmount);
                aBreakage.InvoiceAmountCommaSeparate = ((double)aBreakage.InvoiceAmount).ToString("N");
                aBreakage.BreakageRate = Convert.ToDouble(ConfigurationManager.AppSettings["BreakageRate"]);
                aBreakage.BreakageAmount = BreakageAmount;
                aBreakage.BreakageAmount = Math.Round((double)aBreakage.BreakageAmount);
                aBreakage.BreakageAmountCommaSeparate = ((double)aBreakage.BreakageAmount).ToString("N");
                aBreakage.PaidValue = "";
                breakageCalculatorList.Add(aBreakage);
            }

            return Json(new { ccList = breakageCalculatorList.OrderBy(bb=> bb.DealerName), isExist = isExist }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPeriodNameList(int year, int type)
        {
            var getListData = this.breakageCalculatorService.GetAllBreakageCalculator()
                .Where(cc => cc.Year == year && cc.PeriodType == type).ToList();

            var periodNameList = new List<String>();
            if (getListData.Any())
            {
                periodNameList = getListData.GroupBy(cc => cc.PeriodName).Select(p => p.Key).ToList();
            }
            return Json(periodNameList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPeriodAllList(int year, int type, string periodName)
        {
            var getListData = this.breakageCalculatorService.GetAllBreakageCalculator()
                .Where(cc => cc.Year == year && cc.PeriodType == type && cc.PeriodName == periodName).ToList();

            List<BreakageCalculatorViewModel> calList = new List<BreakageCalculatorViewModel>();
            foreach (var aData in getListData)
            {
                BreakageCalculatorViewModel aBreakage = new BreakageCalculatorViewModel();
                //aBreakage.Id = Guid.Empty;
                aBreakage.DealerId = aData.DealerId;
                aBreakage.DealerName = aData.FGDealer.Name;
                aBreakage.PeriodType = aData.PeriodType;
                aBreakage.PeriodNumber = aData.PeriodNumber;
                aBreakage.PeriodName = aData.PeriodName;
                var PeriodNameType = aData.PeriodNumber.Substring(aData.PeriodNumber.Length - 2);
                aBreakage.PeriodNameType = Convert.ToInt32(PeriodNameType);

                aBreakage.Year = aData.Year;
                aBreakage.BreakageRate = aData.BreakageRate;
                aBreakage.BreakageAmount = aData.BreakageAmount;
                aBreakage.BreakageAmountCommaSeparate = ((double)aBreakage.BreakageAmount).ToString("N");
                
                aBreakage.InvoiceAmount = aData.InvoiceAmount;
                aBreakage.InvoiceAmount = Math.Round((double)aBreakage.InvoiceAmount);
                aBreakage.InvoiceAmountCommaSeparate = ((double)aBreakage.InvoiceAmount).ToString("N");
                aBreakage.SN = aData.SN;
                aBreakage.PaidAmount = aData.PaidAmount;
                if (aBreakage.PaidAmount != null && aBreakage.PaidAmount != 0)
                {aBreakage.PaidValue = "Paid";}
                else{aBreakage.PaidValue = "";}
                calList.Add(aBreakage);
            }

            return Json(calList.OrderBy(ss => ss.DealerName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBreakageByDealerAndType(int dealerId, int type)
        {
            var list = this.breakageCalculatorService.GetAllBreakageCalculator()
                .Where(cc => cc.DealerId == dealerId && cc.PeriodType == type && Convert.ToInt32(cc.PaidAmount) != Convert.ToInt32(cc.BreakageAmount) && cc.BreakageAmount != 0).ToList();

            List<CustomBreakageCalculatorViewModel> customList = new List<CustomBreakageCalculatorViewModel>();
            if (list.Any())
            {
                foreach (var aData in list)
                {
                    CustomBreakageCalculatorViewModel aValue =
                        new CustomBreakageCalculatorViewModel
                        {
                            Id = aData.Id,
                            InvoiceAmount = aData.InvoiceAmount,
                            BreakageAmount = aData.BreakageAmount,
                            showValueName = aData.Year + "_" + aData.PeriodName
                        };
                    customList.Add(aValue);
                }
            }
            return Json(customList, JsonRequestBehavior.AllowGet);
        }
    }

    public class BreakageCalculatorViewModel
    {
        public System.Guid Id { get; set; }
        public int DealerId { get; set; }
        public string DealerName { set; get; }
        public Nullable<int> PeriodType { get; set; }
        public string PeriodNumber { get; set; }
        public string PeriodName { get; set; }
        public int PeriodNameType { set; get; }
        public Nullable<int> Year { get; set; }
        public Nullable<int> SN { get; set; }
        public Nullable<double> InvoiceAmount { get; set; }
        public Nullable<double> BreakageRate { get; set; }
        public Nullable<double> BreakageAmount { get; set; }
        public Nullable<double> PaidAmount { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string InvoiceAmountCommaSeparate { set; get; }
        public string BreakageAmountCommaSeparate { set; get; }
        public string PaidValue { set; get; }
    }

    public class CustomBreakageCalculatorViewModel
    {
        public System.Guid Id { get; set; }
        public Nullable<double> InvoiceAmount { get; set; }
        public Nullable<double> BreakageAmount { get; set; }
        public string showValueName { get; set; }

    }

}