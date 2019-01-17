using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Service.Enums;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    //public class CommissionCalculatorController : Controller
    //{
    //    //
    //    // GET: /CommissionCalculator/
    //    public ActionResult Index()
    //    {
    //        return View();
    //    }
    //}

    public class CommissionCalculatorController : Controller
    {
        public readonly ICommissionCalculatorService commissionCalculatorService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;

        public readonly IFGSaleService fgSaleService;
        public readonly IFGSalesDetailService fgSalesDetailService;
        public readonly IFGDealerService fgDealerService;
        public readonly IDealerCommisionDetailService dealerCommisionDetailService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();
        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();
        public CommissionCalculatorController(ICommissionCalculatorService commissionCalculatorService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService,
            IFGSaleService fgSaleService, IFGSalesDetailService fgSalesDetailService, IFGDealerService fgDealerService, IDealerCommisionDetailService dealerCommisionDetailService)
        {
            this.commissionCalculatorService = commissionCalculatorService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.fgSaleService = fgSaleService;
            this.fgSalesDetailService = fgSalesDetailService;
            this.fgDealerService = fgDealerService;
            this.dealerCommisionDetailService = dealerCommisionDetailService;
        }

        string cacheKey = "permission:commissionCalculator" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /CommissionCalculator/
        public ActionResult Index()
        {
            const string url = "/CommissionCalculator/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("CommissionCalculator");//
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult CommissionCalculator()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateCommissionCalculator(List<CommissionCalculator> commissionCalculatorList)
        {
            var isSuccess = false;
            var message = string.Empty;
            //var isNew = commissionCalculatorService.GetCommissionCalculator(commissionCalculator.Id);
            const string url = "/CommissionCalculator/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);
            var sn = 1;
            foreach (var commissionCalculator in commissionCalculatorList)
            {
                var isNew = this.commissionCalculatorService.GetAllCommissionCalculator()
                    .FirstOrDefault(cc => cc.PeriodNumber == commissionCalculator.PeriodNumber);
                if (isNew == null) 
                {
                    if (permission.CreateOperation == true)
                    {
                        commissionCalculator.Id = Guid.NewGuid();
                        commissionCalculator.CreatedBy = UserSession.GetUserFromSession().EmployeeId;
                        commissionCalculator.CreatedOn = DateTime.UtcNow;
                        commissionCalculator.SN = sn;

                        if (this.commissionCalculatorService.CreateCommissionCalculator(commissionCalculator))
                        {
                            sn++;
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceCommissionCalculator.CommissionCalculation);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceCommissionCalculator.CommissionCalculation);
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
                        //commissionCalculator.Id = Guid.NewGuid();
                        isNew.UpdatedBy = UserSession.GetUserFromSession().EmployeeId;
                        isNew.UpdatedOn = DateTime.UtcNow;
                        
                        isNew.DealerId = commissionCalculator.DealerId;
                        isNew.ZoneId = commissionCalculator.ZoneId;
                        //isNew.FGGradeId = commissionCalculator.FGGradeId;
                        isNew.PeriodType = commissionCalculator.PeriodType;

                        isNew.Year = commissionCalculator.Year;
                        isNew.SFTSale = commissionCalculator.SFTSale;
                        isNew.PCSSale = commissionCalculator.PCSSale;
                        isNew.CTNSale = commissionCalculator.CTNSale;
                        isNew.SMTSale = commissionCalculator.SMTSale;

                        //aCalculator.PeriodNumber = aCalculator.Year + aCalculator.DealerId + gradeId + Convert.ToString(type) + periodTo.ToString("D2");

                        isNew.SFTRate = commissionCalculator.SFTRate; 
                        isNew.PCSRate = commissionCalculator.PCSRate; 
                        isNew.CTNRate = commissionCalculator.CTNRate; 
                        isNew.SMTRate = commissionCalculator.SMTRate; 
                        //isNew.PaidAmount = commissionCalculator.PaidAmount;
                        isNew.Total = commissionCalculator.Total;
                        isNew.MonthlyTarget = commissionCalculator.MonthlyTarget;

                        if (this.commissionCalculatorService.UpdateCommissionCalculator(isNew))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceCommissionCalculator.CommissionCalculation);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceCommissionCalculator.CommissionCalculation);
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
        private bool CheckIsExist(Model.Models.CommissionCalculator commissionCalculator)
        {
            return this.commissionCalculatorService.CheckIsExist(commissionCalculator);
        }
        [HttpPost]
        public JsonResult DeleteCommissionCalculator(CommissionCalculator commissionCalculator)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/CommissionCalculator/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.commissionCalculatorService.DeleteCommissionCalculator(commissionCalculator.Id);
                if (isSuccess)
                {
                    message = "CommissionCalculator deleted successfully!";

                }
                else
                {
                    message = "CommissionCalculator can't be deleted!";
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

        public JsonResult GetCommissionCalculatorList()
        {
            var commissionCalculatorListObj = this.commissionCalculatorService.GetAllCommissionCalculator();
            List<CommissionCalculatorViewModel> commissionCalculatorVMList = new List<CommissionCalculatorViewModel>();

            foreach (var commissionCalculator in commissionCalculatorListObj)
            {
                CommissionCalculatorViewModel commissionCalculatorTemp = new CommissionCalculatorViewModel();
                commissionCalculatorTemp.Id = commissionCalculator.Id;
                //commissionCalculatorTemp.Name = commissionCalculator.Name;

                commissionCalculatorVMList.Add(commissionCalculatorTemp);
            }
            return Json(commissionCalculatorVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCommissionCalculator(Guid id)
        {
            var commissionCalculator = this.commissionCalculatorService.GetCommissionCalculator(id);
            return Json(commissionCalculator);
        }


        public JsonResult GetAllData(int year, int type, int gradeId, int periodFrom, int periodTo)
        {
            List<CommissionCalculatorViewModel> commissionCalculatorList = new List<CommissionCalculatorViewModel>();
            var PeriodName = string.Empty;
            if (type == 1)
            {
                PeriodName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(periodTo);
            }
            else if (type == 2)
            {
                if (periodTo == 3)
                    PeriodName = Resources.ResourceCommissionCalculator.JanToMar;
                if (periodTo == 6)
                    PeriodName = Resources.ResourceCommissionCalculator.AprToJun;
                if (periodTo == 9)
                    PeriodName = Resources.ResourceCommissionCalculator.JulToSep;
                if (periodTo == 12)
                    PeriodName = Resources.ResourceCommissionCalculator.OctToDec;
            }
            else if (type == 3)
            {
                if(periodTo == 6)
                    PeriodName = Resources.ResourceCommissionCalculator.JanToJun;
                if(periodTo == 12)
                    PeriodName = Resources.ResourceCommissionCalculator.JulyToDec;
            }
            else if (type == 4) { PeriodName = Resources.ResourceCommissionCalculator.JanToDec; }

            var isExist = false;
            var chkIsExist = this.commissionCalculatorService.GetAllCommissionCalculator().FirstOrDefault(
                cc => cc.Year == year.ToString() && cc.PeriodType == type
                      && cc.FGGradeId == gradeId && cc.PeriodName == PeriodName);
            if (chkIsExist != null)
            {
                isExist = true;
            }
            var fgDealer = this.fgDealerService.GetAllFGDealer().Where(de => de.IsActive == true);
            foreach (var aDealer in fgDealer)
            {
                var aCalculator = GetCC(aDealer, PeriodName, year, type, gradeId, periodFrom, periodTo);
                commissionCalculatorList.Add(aCalculator);
            }

            return Json(new{ccList = commissionCalculatorList.OrderBy(ss => ss.DivisionName).ThenBy(ss => ss.ZoneName), isExist = isExist}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllDataForNewDealer(int year, int type, int gradeId, int periodFrom, int periodTo)
        {
            // If a dealer added newly (after calculated commission)..to commission calculate for these dealer..
            List<CommissionCalculatorViewModel> commissionCalculatorList = new List<CommissionCalculatorViewModel>();
            var PeriodName = string.Empty;
            if (type == 1)
            {
                PeriodName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(periodTo);
            }
            else if (type == 2)
            {
                if (periodTo == 3)
                    PeriodName = Resources.ResourceCommissionCalculator.JanToMar;
                if (periodTo == 6)
                    PeriodName = Resources.ResourceCommissionCalculator.AprToJun;
                if (periodTo == 9)
                    PeriodName = Resources.ResourceCommissionCalculator.JulToSep;
                if (periodTo == 12)
                    PeriodName = Resources.ResourceCommissionCalculator.OctToDec;
            }
            else if (type == 3)
            {
                if (periodTo == 6)
                    PeriodName = Resources.ResourceCommissionCalculator.JanToJun;
                if (periodTo == 12)
                    PeriodName = Resources.ResourceCommissionCalculator.JulyToDec;
            }
            else if (type == 4) { PeriodName = Resources.ResourceCommissionCalculator.JanToDec; }

            var isExist = false;
           
            var fgDealer = this.fgDealerService.GetAllFGDealer().Where(de => de.IsActive == true);
            foreach (var aDealer in fgDealer)
            {
                var periodNumForChk = year.ToString() +aDealer.Id + gradeId + Convert.ToString(type) + periodTo.ToString("D2");
                var chkNewDealer = this.commissionCalculatorService.GetAllCommissionCalculator().FirstOrDefault(
                    cc => cc.PeriodNumber == periodNumForChk);
                if (chkNewDealer == null)
                {
                    var aCalculator = GetCC(aDealer, PeriodName, year, type, gradeId, periodFrom, periodTo);
                    commissionCalculatorList.Add(aCalculator);
                }
            }

            return Json(new { ccList = commissionCalculatorList.OrderBy(ss => ss.DivisionName).ThenBy(ss => ss.ZoneName), isExist = isExist }, JsonRequestBehavior.AllowGet);
        }


        public CommissionCalculatorViewModel GetCC(FGDealer aDealer, string PeriodName, int year, int type, int gradeId, int periodFrom, int periodTo)
        {
            double sftRate = 0.00;
            double pcsRate = 0.00;
            double ctnRate = 0.00;
            double smtRate = 0.00;
            var sftLatestCommission = new DealerCommisionDetail();
            var pcsLatestCommission = new DealerCommisionDetail();
            var ctnLatestCommission = new DealerCommisionDetail();
            var smtLatestCommission = new DealerCommisionDetail();

            var fgsales = this.fgSaleService.GetAllFGSale().Where(deal =>
            {
                return deal.InvoiceDate != null && (deal.DealerId == aDealer.Id && deal.IsDelete != true
                                                    && deal.CustomerType == 1 && deal.Reason == 1
                                                    && deal.InvoiceDate.Value.AddMinutes(timeZoneOffset).Year == year);
            });

            if (type == 1)
            {
                fgsales = fgsales.Where(sa => sa.InvoiceDate.Value.AddMinutes(timeZoneOffset).Month == periodFrom);
            }
            else if (type == 2 || type == 3)
            {
                fgsales = fgsales.Where(sa => sa.InvoiceDate.Value.AddMinutes(timeZoneOffset).Month >= periodFrom && sa.InvoiceDate.Value.AddMinutes(timeZoneOffset).Month <= periodTo);
            }
            //else if (type == 4){}
            //else{}

            var totalSFT = fgsales.Sum(a => a.FGSalesDetails.Where(b => b.SalesFGUnitId == 3 && b.FGGradeId == gradeId).Sum(zz => zz.SalesQuantity));
            var totalPCS = fgsales.Sum(a => a.FGSalesDetails.Where(b => b.SalesFGUnitId == 1 && b.FGGradeId == gradeId).Sum(zz => zz.SalesQuantity));
            var totalCTN = fgsales.Sum(a => a.FGSalesDetails.Where(b => b.SalesFGUnitId == 2 && b.FGGradeId == gradeId).Sum(zz => zz.SalesQuantity));
            var totalSMT = fgsales.Sum(a => a.FGSalesDetails.Where(b => b.SalesFGUnitId == 4 && b.FGGradeId == gradeId).Sum(zz => zz.SalesQuantity));

            if (totalSFT != 0.00)
            {
                sftLatestCommission = dealerCommisionDetailService.GetAllDealerCommisionDetail()
                    .Where(ss => ss.DealerId == aDealer.Id && ss.FGUOMId == 3 &&
                                 ss.ZoneCommision.EffectiveDate <= DateTime.UtcNow && ss.MonthlyTarget <= totalSFT)
                    .OrderByDescending(ss => ss.ZoneCommision.EffectiveDate)
                    .ThenByDescending(ss => ss.MonthlyTarget).FirstOrDefault();
            }
            if (totalPCS != 0.00)
            {
                pcsLatestCommission = dealerCommisionDetailService.GetAllDealerCommisionDetail()
                    .Where(ss => ss.DealerId == aDealer.Id && ss.FGUOMId == 1 &&
                                 ss.ZoneCommision.EffectiveDate <= DateTime.UtcNow && ss.MonthlyTarget <= totalPCS)
                    .OrderByDescending(ss => ss.ZoneCommision.EffectiveDate)
                    .ThenByDescending(ss => ss.MonthlyTarget).FirstOrDefault();
            }
            if (totalCTN != 0.00)
            {
                ctnLatestCommission = dealerCommisionDetailService.GetAllDealerCommisionDetail()
                    .Where(ss => ss.DealerId == aDealer.Id && ss.FGUOMId == 2 &&
                                 ss.ZoneCommision.EffectiveDate <= DateTime.UtcNow && ss.MonthlyTarget <= totalCTN)
                    .OrderByDescending(ss => ss.ZoneCommision.EffectiveDate)
                    .ThenByDescending(ss => ss.MonthlyTarget).FirstOrDefault();
            }
            if (totalSMT != 0.00)
            {
                smtLatestCommission = dealerCommisionDetailService.GetAllDealerCommisionDetail()
                    .Where(ss => ss.DealerId == aDealer.Id && ss.FGUOMId == 4 &&
                                 ss.ZoneCommision.EffectiveDate <= DateTime.UtcNow && ss.MonthlyTarget <= totalSMT)
                    .OrderByDescending(ss => ss.ZoneCommision.EffectiveDate)
                    .ThenByDescending(ss => ss.MonthlyTarget).FirstOrDefault();
            }

            if (type == 1)
            {
                if (sftLatestCommission != null) sftRate = sftLatestCommission.MonthlyCommission ?? 0.00;
                if (pcsLatestCommission != null) pcsRate = pcsLatestCommission.MonthlyCommission ?? 0.00;
                if (ctnLatestCommission != null) ctnRate = ctnLatestCommission.MonthlyCommission ?? 0.00;
                if (smtLatestCommission != null) smtRate = smtLatestCommission.MonthlyCommission ?? 0.00;
            }
            else if (type == 2)
            {
                if (sftLatestCommission != null) sftRate = sftLatestCommission.QuarterlyCommission ?? 0.00;
                if (pcsLatestCommission != null) pcsRate = pcsLatestCommission.QuarterlyCommission ?? 0.00;
                if (ctnLatestCommission != null) ctnRate = ctnLatestCommission.QuarterlyCommission ?? 0.00;
                if (smtLatestCommission != null) smtRate = smtLatestCommission.QuarterlyCommission ?? 0.00;
            }
            else if (type == 3)
            {
                if (sftLatestCommission != null) sftRate = sftLatestCommission.HalfYearlyCommission ?? 0.00;
                if (pcsLatestCommission != null) pcsRate = pcsLatestCommission.HalfYearlyCommission ?? 0.00;
                if (ctnLatestCommission != null) ctnRate = ctnLatestCommission.HalfYearlyCommission ?? 0.00;
                if (smtLatestCommission != null) smtRate = smtLatestCommission.HalfYearlyCommission ?? 0.00;
            }
            else
            {
                if (sftLatestCommission != null) sftRate = sftLatestCommission.YearlyCommission ?? 0.00;
                if (pcsLatestCommission != null) pcsRate = pcsLatestCommission.YearlyCommission ?? 0.00;
                if (ctnLatestCommission != null) ctnRate = ctnLatestCommission.YearlyCommission ?? 0.00;
                if (smtLatestCommission != null) smtRate = smtLatestCommission.YearlyCommission ?? 0.00;
            }

            CommissionCalculatorViewModel aCalculator = new CommissionCalculatorViewModel();
            aCalculator.Id = Guid.Empty;
            aCalculator.DealerId = aDealer.Id;
            aCalculator.DealerName = aDealer.Name;
            aCalculator.ZoneId = aDealer.DealersZoneId;
            aCalculator.ZoneName = aDealer.FGDealerZone.ZoneName;
            aCalculator.DivisionName = string.Empty;
            if (aDealer.FGDealerZone.Division != null)
                aCalculator.DivisionName = aDealer.FGDealerZone.Division.Name;
            aCalculator.FGGradeId = gradeId;
            aCalculator.PeriodType = type;
            aCalculator.PeriodName = PeriodName;

            aCalculator.Year = year.ToString();
            aCalculator.SFTSale = totalSFT != null ? Math.Round((double)totalSFT, 2) : 0.00;
            aCalculator.SFTSaleCommaSeparate = ((double)aCalculator.SFTSale).ToString("N");
            aCalculator.PCSSale = totalPCS != null ? Math.Round((double)totalPCS, 2) : 0.00;
            aCalculator.CTNSale = totalCTN != null ? Math.Round((double)totalCTN, 2) : 0.00;
            aCalculator.SMTSale = totalSMT != null ? Math.Round((double)totalSMT, 2) : 0.00;

            aCalculator.PeriodNumber = aCalculator.Year + aCalculator.DealerId + gradeId + Convert.ToString(type) + periodTo.ToString("D2");
            //if (aCalculator.DealerId == 71 || aCalculator.DealerId == 73)
            //{
            //    var a = "Hello";
            //}
            if (sftLatestCommission != null)
            {
                if (sftLatestCommission.MonthlyTarget == null)
                {
                    var sftNearestCommission = dealerCommisionDetailService.GetAllDealerCommisionDetail()
                        .Where(ss => ss.DealerId == aDealer.Id && ss.FGUOMId == 3 &&
                                     ss.ZoneCommision.EffectiveDate <= DateTime.UtcNow)
                        .OrderByDescending(ss => ss.ZoneCommision.EffectiveDate).ThenBy(ss => ss.MonthlyTarget)
                        .FirstOrDefault();
                    if (sftNearestCommission != null)
                    {
                        aCalculator.MonthlyTarget = sftNearestCommission.MonthlyTarget ?? 0.00;
                        aCalculator.MonthlyTargetCommaSeparate = ((double) aCalculator.MonthlyTarget).ToString("N");
                    }
                    else
                    {
                        aCalculator.MonthlyTarget = 0.00;
                        aCalculator.MonthlyTargetCommaSeparate = ((double) aCalculator.MonthlyTarget).ToString("N");
                    }
                }
                else
                {
                    aCalculator.MonthlyTarget = sftLatestCommission.MonthlyTarget ?? 0.00;
                    aCalculator.MonthlyTargetCommaSeparate = ((double)aCalculator.MonthlyTarget).ToString("N");
                }
            }
            else
            {
                var sftNearestCommission = dealerCommisionDetailService.GetAllDealerCommisionDetail()
                    .Where(ss => ss.DealerId == aDealer.Id && ss.FGUOMId == 3 && ss.ZoneCommision.EffectiveDate <= DateTime.UtcNow)
                    .OrderByDescending(ss => ss.ZoneCommision.EffectiveDate).ThenBy(ss => ss.MonthlyTarget).FirstOrDefault();
                if (sftNearestCommission != null)
                {
                    aCalculator.MonthlyTarget = sftNearestCommission.MonthlyTarget ?? 0.00;
                    aCalculator.MonthlyTargetCommaSeparate = ((double)aCalculator.MonthlyTarget).ToString("N");
                }
                else
                {
                    aCalculator.MonthlyTarget = 0.00;
                    aCalculator.MonthlyTargetCommaSeparate = ((double)aCalculator.MonthlyTarget).ToString("N");
                }                
            }
            aCalculator.SFTRate = sftRate;
            aCalculator.PCSRate = pcsRate;
            aCalculator.CTNRate = ctnRate;
            aCalculator.SMTRate = smtRate;
            aCalculator.Total = Math.Round((double)(aCalculator.SFTSale * sftRate + aCalculator.PCSSale * pcsRate + aCalculator.CTNSale * ctnRate + aCalculator.SMTSale * smtRate));
            aCalculator.TotalCommaSeparate = ((double)aCalculator.Total).ToString("N");
            aCalculator.PaidValue = "";

            return aCalculator;
        }


        public JsonResult GetPeriodNameList(string year, int gradeId, int type)
        {
            var getListData = this.commissionCalculatorService.GetAllCommissionCalculator()
                .Where(cc => cc.Year == year && cc.FGGradeId == gradeId && cc.PeriodType == type).ToList();

            var periodNameList = new List<String>();
            if (getListData.Any())
            {
                periodNameList = getListData.GroupBy(cc => cc.PeriodName).Select(p => p.Key).ToList();
            }
            return Json(periodNameList , JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPeriodAllList(string year, int gradeId, int type, string periodName)
        {
            var getListData = this.commissionCalculatorService.GetAllCommissionCalculator()
                .Where(cc => cc.Year == year && cc.FGGradeId == gradeId && cc.PeriodType == type && cc.PeriodName == periodName).ToList();

            List<CommissionCalculatorViewModel> calList = new List<CommissionCalculatorViewModel>();
            foreach (var aData in getListData)
            {
                CommissionCalculatorViewModel aCalculator = new CommissionCalculatorViewModel();
                //aCalculator.Id = Guid.Empty;
                aCalculator.DealerId = aData.DealerId;
                aCalculator.DealerName = aData.FGDealer.Name;
                aCalculator.ZoneId = aData.ZoneId;
                aCalculator.ZoneName = aData.FGDealerZone.ZoneName;
                if (aData.FGDealerZone.Division != null)
                    aCalculator.DivisionName = aData.FGDealerZone.Division.Name;
                aCalculator.FGGradeId = aData.FGGradeId;
                aCalculator.PeriodType = aData.PeriodType;
                aCalculator.PeriodNumber = aData.PeriodNumber;
                var PeriodNameType = aData.PeriodNumber.Substring(aData.PeriodNumber.Length - 2);
                aCalculator.PeriodNameType = Convert.ToInt32(PeriodNameType);

                aCalculator.Year = aData.Year;
                aCalculator.SFTSale = aData.SFTSale;
                if (aCalculator.SFTSale != null)
                    aCalculator.SFTSaleCommaSeparate = ((double) aCalculator.SFTSale).ToString("N");
                aCalculator.PCSSale = aData.PCSSale;
                aCalculator.CTNSale = aData.CTNSale;
                aCalculator.SMTSale = aData.SMTSale;

                aCalculator.SFTRate = aData.SFTRate;
                aCalculator.PCSRate = aData.PCSRate;
                aCalculator.CTNRate = aData.CTNRate;
                aCalculator.SMTRate = aData.SMTRate;
                aCalculator.Total = Convert.ToDouble(aData.Total);
                aCalculator.Total = Math.Round((double) aCalculator.Total);
                aCalculator.TotalCommaSeparate = ((double)aCalculator.Total).ToString("N");
                aCalculator.SN = aData.SN;
                if (aData.MonthlyTarget != null)
                {
                    aCalculator.MonthlyTarget = aData.MonthlyTarget ?? 0.00;
                    aCalculator.MonthlyTargetCommaSeparate = ((double)aData.MonthlyTarget).ToString("N");
                }
                if (aData.PaidAmount != null && aData.PaidAmount != 0)
                { aCalculator.PaidValue = "Paid"; }
                else { aCalculator.PaidValue = ""; }
                calList.Add(aCalculator);
            }

            //return null;
            return Json(calList.OrderBy(ss => ss.DivisionName).ThenBy(ss => ss.ZoneName), JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetCommissionByDealerAndType(int dealerId, int type)
        {
            var list = this.commissionCalculatorService.GetAllCommissionCalculator()
                .Where(cc => cc.DealerId == dealerId && cc.PeriodType == type && Convert.ToInt32(cc.PaidAmount) != Convert.ToInt32(cc.Total) && cc.Total != 0).ToList();

            List<CustomCommissionCalculatorViewModel> customList = new List<CustomCommissionCalculatorViewModel>();
            if (list.Any())
            {
                foreach (var aData in list)
                {
                    CustomCommissionCalculatorViewModel aValue =
                        new CustomCommissionCalculatorViewModel
                        {
                            Id = aData.Id,
                            Total = aData.Total,
                            showValueName = aData.Year + "_" + aData.FGGrade.Grade + "_" + aData.PeriodName
                        };
                    customList.Add(aValue);
                }
            }

            return Json(customList, JsonRequestBehavior.AllowGet);
        }
        
    }

    public class CommissionCalculatorViewModel
    {
        public System.Guid Id { get; set; }
        public Nullable<int> ZoneId { get; set; }
        public string ZoneName { get; set; }
        public string DivisionName { get; set; }
        public string SFTSaleCommaSeparate { get; set; }
        public string MonthlyTargetCommaSeparate { get; set; }
        public string TotalCommaSeparate { get; set; }
        
        public Nullable<int> DealerId { get; set; }
        public string DealerName { get; set; }
        public Nullable<int> FGGradeId { get; set; }
        public Nullable<int> PeriodType { get; set; }
        public string PeriodNumber { get; set; }
        public string PeriodName { get; set; }
        public int? PeriodNameType { set; get; }
        public string Year { get; set; }
        public Nullable<int> SN { get; set; }
        public Nullable<double> SFTSale { get; set; }
        public Nullable<double> SFTRate { get; set; }
        public Nullable<double> PCSSale { get; set; }
        public Nullable<double> PCSRate { get; set; }
        public Nullable<double> SMTSale { get; set; }
        public Nullable<double> SMTRate { get; set; }
        public Nullable<double> CTNSale { get; set; }
        public Nullable<double> CTNRate { get; set; }
        public Nullable<double> PaidAmount { get; set; }
        public Nullable<double> Total { get; set; }
        public Nullable<double> MonthlyTarget { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string PaidValue { set; get; }
    }

    public class CustomCommissionCalculatorViewModel
    {
        public System.Guid Id { get; set; }
        public Nullable<double> Total { get; set; }
        public string showValueName { get; set; }
        
    }

}