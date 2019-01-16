using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class DealerCommisionDetailController : Controller
    {
        public readonly IDealerCommisionDetailService dealerCommisionDetailService;
        public readonly IZoneCommisionService zoneCommisionService;

        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public DealerCommisionDetailController(IZoneCommisionService zoneCommisionService, IDealerCommisionDetailService dealerCommisionDetailService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.dealerCommisionDetailService = dealerCommisionDetailService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.zoneCommisionService = zoneCommisionService;
        }

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string timeFormat = WebConfigurationManager.AppSettings["TimeFormat"];
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:dealerCommisionDetail" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /DealerCommisionDetail/
        public ActionResult Index()
        {
            const string url = "/DealerCommisionDetail/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("DealerCommisionDetail");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }
        public ActionResult DealerCommisionDetail()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateDealerCommisionDetail(List<DealerCommisionDetail> dealerZoneCommisionDetailsList)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = dealerZoneCommisionDetailsList != null && (dealerZoneCommisionDetailsList[0].Id == Guid.Empty ? true : false);
            const string url = "/DealerCommisionDetail/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (dealerZoneCommisionDetailsList != null)
                    {
                        foreach (var zcd in dealerZoneCommisionDetailsList)
                        {
                            DealerCommisionDetail dcd = new DealerCommisionDetail();
                            dcd.Id = Guid.NewGuid();
                            dcd.DealerId = zcd.DealerId;
                            dcd.ZoneCommisionId = zcd.ZoneCommisionId;
                            dcd.FGUOMId = zcd.FGUOMId;
                            dcd.MonthlyTarget = zcd.MonthlyTarget;
                            dcd.MonthlyCommission = zcd.MonthlyCommission;
                            dcd.QuarterlyCommission = zcd.QuarterlyCommission;
                            dcd.HalfYearlyCommission = zcd.HalfYearlyCommission;
                            dcd.YearlyCommission = zcd.YearlyCommission;
                            dcd.IsOverride = true;
                            try
                            {
                                this.dealerCommisionDetailService.CreateDealerCommisionDetail(dcd);
                            }
                            catch
                            {
                            }
                        }
                    }

                    isSuccess = true;
                    message = string.Format(Resources.ResourceCommon.CMsg_save,
                        Resources.ResourceDealerCommisionDetail.Title);

                    //isSuccess = false;
                    //message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceDealerCommisionDetail.ZoneCommission);

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
                    foreach (var zcd in dealerZoneCommisionDetailsList)
                    {
                        var getdcd = this.dealerCommisionDetailService.GetDealerCommisionDetail(zcd.Id);
                        if (getdcd != null)
                        {
                            getdcd.MonthlyTarget = zcd.MonthlyTarget;
                            getdcd.MonthlyCommission = zcd.MonthlyCommission;
                            getdcd.QuarterlyCommission = zcd.QuarterlyCommission;
                            getdcd.HalfYearlyCommission = zcd.HalfYearlyCommission;
                            getdcd.YearlyCommission = zcd.YearlyCommission;
                            //getdcd.IsOverride = true;
                            try
                            {
                                this.dealerCommisionDetailService.UpdateDealerCommisionDetail(getdcd);
                            }
                            catch { }
                        }
                    }
                    isSuccess = true;
                    message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceDealerCommisionDetail.Title);
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



        public JsonResult UpdateDealerCommisionDetail(List<DealerCommisionDetail> dealerZoneCommisionDetailsList)
        {
            var isSuccess = false;
            var message = string.Empty;
            const string url = "/DealerCommisionDetail/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.UpdateOperation == true)
            {
                foreach (var zcd in dealerZoneCommisionDetailsList.ToList())
                {
                    //var getdcd = this.dealerCommisionDetailService.GetAllDealerCommisionDetail()
                    //    .Where(dea => dea.DealerId == zcd.DealerId && dea.ZoneCommisionId == zcd.ZoneCommisionId);
                    var getdcd = this.dealerCommisionDetailService.GetDealerCommisionDetail(zcd.Id);
                    if (getdcd != null)
                    {
                        getdcd.MonthlyTarget = zcd.MonthlyTarget;
                        getdcd.MonthlyCommission = zcd.MonthlyCommission;
                        getdcd.QuarterlyCommission = zcd.QuarterlyCommission;
                        getdcd.HalfYearlyCommission = zcd.HalfYearlyCommission;
                        getdcd.YearlyCommission = zcd.YearlyCommission;
                        getdcd.IsOverride = true;
                        try
                        {
                            this.dealerCommisionDetailService.UpdateDealerCommisionDetail(getdcd);
                        }
                        catch { }
                    }
                    else
                    {
                        DealerCommisionDetail dcd = new DealerCommisionDetail();
                        dcd.Id = Guid.NewGuid();
                        dcd.DealerId = zcd.DealerId;
                        dcd.ZoneCommisionId = zcd.ZoneCommisionId;
                        dcd.FGUOMId = zcd.FGUOMId;
                        dcd.MonthlyTarget = zcd.MonthlyTarget;
                        dcd.MonthlyCommission = zcd.MonthlyCommission;
                        dcd.QuarterlyCommission = zcd.QuarterlyCommission;
                        dcd.HalfYearlyCommission = zcd.HalfYearlyCommission;
                        dcd.YearlyCommission = zcd.YearlyCommission;
                        dcd.IsOverride = true;
                        try
                        {
                            this.dealerCommisionDetailService.CreateDealerCommisionDetail(dcd);
                        }
                        catch
                        {
                        }
                    }
                }
                isSuccess = true;
                message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceDealerCommisionDetail.Title);
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

        private bool CheckIsExist(Model.Models.DealerCommisionDetail dealerCommisionDetail)
        {
            return this.dealerCommisionDetailService.CheckIsExist(dealerCommisionDetail);
        }
        [HttpPost]
        public JsonResult DeleteDealerCommisionDetail(DealerCommisionDetail dealerCommisionDetail)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/DealerCommisionDetail/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.dealerCommisionDetailService.DeleteDealerCommisionDetail(dealerCommisionDetail.Id);
                if (isSuccess)
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_delete, Resources.ResourceDealerCommisionDetail.Title);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceDealerCommisionDetail.Title);
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

        public JsonResult GetEffectiveDate(int zoneId)
        {
            var effectiveDates = this.dealerCommisionDetailService.GetAllDealerCommisionDetail().Where(dd => dd.ZoneCommision.ZoneId == zoneId && dd.IsOverride == true);

            List<ZoneCommisionViewModel> dates = new List<ZoneCommisionViewModel>();
            if (effectiveDates.Any())
            {
                var grpDates = effectiveDates.GroupBy(ddd => new { ddd.ZoneCommision });
                foreach (var zz in grpDates)
                {
                    ZoneCommisionViewModel zone = new ZoneCommisionViewModel();
                    zone.Id = zz.Key.ZoneCommision.Id;
                    if (zz.Key.ZoneCommision.EffectiveDate != null)
                        zone.EffectiveDateString = zz.Key.ZoneCommision.EffectiveDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                    dates.Add(zone);
                }
                dates = dates.OrderByDescending(dt => dt.EffectiveDate).ToList();
            }
            return Json(dates, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDealerZoneAndDateWhenUpdate(int zoneId, int zoneCommissionId)
        {
            var dealers = this.dealerCommisionDetailService.GetAllDealerCommisionDetail()
                .Where(dd => dd.ZoneCommision.Id == zoneCommissionId);
            //&& dd.IsOverride == false && dd.ZoneCommision.EffectiveDate == dat);

            List<FGDealer> Dealers = new List<FGDealer>();
            var dealerCommisionDetails = dealers as DealerCommisionDetail[] ?? dealers.ToArray();
            if (dealerCommisionDetails.Any())
            {
                var grpDealer = dealerCommisionDetails.GroupBy(ddd => new { ddd.DealerId, ddd.FGDealer.Name });
                foreach (var dd in grpDealer)
                {
                    FGDealer dealer = new FGDealer();
                    if (dd.Key.DealerId != null) dealer.Id = (int)dd.Key.DealerId;
                    dealer.Name = dd.Key.Name;
                    Dealers.Add(dealer);
                }
                Dealers = Dealers.OrderBy(de => de.Name).ToList();
            }
            return Json(Dealers, JsonRequestBehavior.AllowGet);
        }


        //public JsonResult GetDealerCommisionDetailList()
        //{
        //    var dealerCommisionDetailListObj = this.dealerCommisionDetailService.GetAllDealerCommisionDetail();
        //    List<DealerCommisionDetailViewModelMain> dealerCommisionDetailVMList = new List<DealerCommisionDetailViewModelMain>();

        //    foreach (var dealerCommisionDetail in dealerCommisionDetailListObj)
        //    {
        //        var dealerCommisionDetailTemp = ADealerCommisionDetail(dealerCommisionDetail);

        //        dealerCommisionDetailVMList.Add(dealerCommisionDetailTemp);
        //    }
        //    return Json(dealerCommisionDetailVMList, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetDealerCommisionByZoneDateDealer(int zoneCommissionId, int dealerId)
        {
            var dealerCommisionDetailListObj = this.dealerCommisionDetailService.GetAllDealerCommisionDetail().Where(de => de.ZoneCommisionId == zoneCommissionId &&
                 de.DealerId == dealerId).ToList();

            string dealerName = string.Empty;
            //List<DealerCommisionDetailViewModelMain> dealerCommisionDetailVMList = new List<DealerCommisionDetailViewModelMain>();
            foreach (var dealer in dealerCommisionDetailListObj)
            {
                dealerName = dealer.FGDealer.Name;
            }

            List<DealerCommisionDetailViewModelPartial> zoneDetailsCTN = new List<DealerCommisionDetailViewModelPartial>();
            List<DealerCommisionDetailViewModelPartial> zoneDetailsPCS = new List<DealerCommisionDetailViewModelPartial>();
            List<DealerCommisionDetailViewModelPartial> zoneDetailsSFT = new List<DealerCommisionDetailViewModelPartial>();
            List<DealerCommisionDetailViewModelPartial> zoneDetailsSMT = new List<DealerCommisionDetailViewModelPartial>();

            DealerCommisionDetailViewModelMain dealerCommisionDetailTemp = new DealerCommisionDetailViewModelMain();

            var getdata = this.zoneCommisionService.GetZoneCommision(zoneCommissionId);
            dealerCommisionDetailTemp.ZoneCommisionId = zoneCommissionId;
            if (getdata != null)
            {
                dealerCommisionDetailTemp.ZoneId = getdata.ZoneId;
                if (getdata.EffectiveDate != null)
                    dealerCommisionDetailTemp.EffectiveDateString = getdata.EffectiveDate.Value
                        .AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
            }


            dealerCommisionDetailTemp.DealerId = dealerId;
            dealerCommisionDetailTemp.DealerName = dealerName;
            //if(dealerCommisionDetailTemp)

            var commisionDetailListObj = dealerCommisionDetailListObj;//as IList<DealerCommisionDetail> ?? dealerCommisionDetailListObj.ToList();
            var getDealerCommisionDetailDetailsCtn = commisionDetailListObj.Where(zz => zz.FGUOMId == 1).ToList();
            if (getDealerCommisionDetailDetailsCtn.Any())
            {
                foreach (var zCtn in getDealerCommisionDetailDetailsCtn)
                {
                    var tempCtn = ADealerCommisionDetail(zCtn);
                    zoneDetailsCTN.Add(tempCtn);
                }

            }

            var getDealerCommisionDetailDetailsPcs = commisionDetailListObj.Where(zz => zz.FGUOMId == 2).ToList();
            if (getDealerCommisionDetailDetailsPcs.Any())
            {
                foreach (var zPcs in getDealerCommisionDetailDetailsPcs)
                {
                    var tempPcs = ADealerCommisionDetail(zPcs);
                    zoneDetailsPCS.Add(tempPcs);
                }

            }

            var getDealerCommisionDetailDetailsSft = commisionDetailListObj.Where(zz => zz.FGUOMId == 3).ToList();
            if (getDealerCommisionDetailDetailsSft.Any())
            {
                foreach (var zSft in getDealerCommisionDetailDetailsSft)
                {
                    var tempSft = ADealerCommisionDetail(zSft);
                    zoneDetailsSFT.Add(tempSft);
                }

            }

            var getDealerCommisionDetailDetailsSmt = commisionDetailListObj.Where(zz => zz.FGUOMId == 4).ToList();
            if (getDealerCommisionDetailDetailsSmt.Any())
            {
                foreach (var zSmt in getDealerCommisionDetailDetailsSmt)
                {
                    var tempSmt = ADealerCommisionDetail(zSmt);
                    zoneDetailsSMT.Add(tempSmt);
                }

            }

            dealerCommisionDetailTemp.zoneDetailsCTN = zoneDetailsCTN.OrderBy(cc => cc.MonthlyTarget).ToList(); ;
            dealerCommisionDetailTemp.zoneDetailsPCS = zoneDetailsPCS.OrderBy(cc => cc.MonthlyTarget).ToList(); ;
            dealerCommisionDetailTemp.zoneDetailsSFT = zoneDetailsSFT.OrderBy(cc => cc.MonthlyTarget).ToList(); ;
            dealerCommisionDetailTemp.zoneDetailsSMT = zoneDetailsSMT.OrderBy(cc => cc.MonthlyTarget).ToList(); ;

            return Json(dealerCommisionDetailTemp, JsonRequestBehavior.AllowGet);
            //return dealerCommisionDetailTemp;
        }


        public DealerCommisionDetailViewModelPartial ADealerCommisionDetail(DealerCommisionDetail zcd)
        {
            DealerCommisionDetailViewModelPartial dealerCommisionDetailDetailsTemp = new DealerCommisionDetailViewModelPartial();
            dealerCommisionDetailDetailsTemp.Id = zcd.Id;
            dealerCommisionDetailDetailsTemp.FGUOMId = zcd.FGUOMId;
            dealerCommisionDetailDetailsTemp.MonthlyCommission = zcd.MonthlyCommission;
            dealerCommisionDetailDetailsTemp.MonthlyTarget = zcd.MonthlyTarget;
            dealerCommisionDetailDetailsTemp.QuarterlyCommission = zcd.QuarterlyCommission;
            dealerCommisionDetailDetailsTemp.HalfYearlyCommission = zcd.HalfYearlyCommission;
            dealerCommisionDetailDetailsTemp.YearlyCommission = zcd.YearlyCommission;
            return dealerCommisionDetailDetailsTemp;
        }

        public JsonResult GetDealerCommisionDetail(int id)
        {
            //var dealerCommisionDetail = this.dealerCommisionDetailService.GetDealerCommisionDetail(id);
            //return Json(dealerCommisionDetail);
            return null;
        }
    }

    public class DealerCommisionDetailViewModelMain
    {
        public DealerCommisionDetailViewModelMain()
        {
            //this.DealerCommisionDetailDetails = new List<DealerCommisionDetailDetail>();
            this.zoneDetailsCTN = new List<DealerCommisionDetailViewModelPartial>();
            this.zoneDetailsPCS = new List<DealerCommisionDetailViewModelPartial>();
            this.zoneDetailsSFT = new List<DealerCommisionDetailViewModelPartial>();
            this.zoneDetailsSMT = new List<DealerCommisionDetailViewModelPartial>();
        }

        public int Id { get; set; }
        public Nullable<int> ZoneCommisionId { get; set; }
        public int? ZoneId { set; get; }
        public string DealerName { set; get; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public string EffectiveDateString { get; set; }
        public int? DealerId { set; get; }
        public List<DealerCommisionDetail> DealerCommisionDetails { get; set; }
        public List<DealerCommisionDetailViewModelPartial> zoneDetailsCTN { get; set; }
        public List<DealerCommisionDetailViewModelPartial> zoneDetailsPCS { get; set; }
        public List<DealerCommisionDetailViewModelPartial> zoneDetailsSFT { get; set; }
        public List<DealerCommisionDetailViewModelPartial> zoneDetailsSMT { get; set; }
    }


    public class DealerCommisionDetailViewModelPartial
    {
        public System.Guid Id { get; set; }
        public Nullable<int> FGUOMId { get; set; }
        public Nullable<double> MonthlyTarget { get; set; }
        public Nullable<double> MonthlyCommission { get; set; }
        public Nullable<double> QuarterlyCommission { get; set; }
        public Nullable<double> HalfYearlyCommission { get; set; }
        public Nullable<double> YearlyCommission { get; set; }
    }

}