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
    public class ZoneCommisionController : Controller
    {
        public readonly IZoneCommisionService zoneCommisionService;
        public readonly IZoneCommisionDetailService zoneCommisionDetailService;
        public readonly IDealerCommisionDetailService dealerCommisionDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IFGDealerService fgDealerService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public ZoneCommisionController(IFGDealerService fgDealerService, IDealerCommisionDetailService dealerCommisionDetailService, IZoneCommisionService zoneCommisionService, IZoneCommisionDetailService zoneCommisionDetailService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.zoneCommisionService = zoneCommisionService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.zoneCommisionDetailService = zoneCommisionDetailService;
            this.fgDealerService = fgDealerService;
            this.dealerCommisionDetailService = dealerCommisionDetailService;
        }

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string timeFormat = WebConfigurationManager.AppSettings["TimeFormat"];
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:zoneCommision" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /ZoneCommision/
        public ActionResult Index()
        {
            const string url = "/ZoneCommision/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("ZoneCommision");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }
        public ActionResult ZoneCommision()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateZoneCommision(ZoneCommision zoneCommision)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = zoneCommisionService.GetZoneCommision(zoneCommision.Id);
            const string url = "/ZoneCommision/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (zoneCommision.ZoneCommisionDetails != null)
                    {
                        foreach (var zcd in zoneCommision.ZoneCommisionDetails)
                        {
                            zcd.Id = Guid.NewGuid();
                        }
                    }

                    if (zoneCommision.EffectiveDate != null)
                        zoneCommision.EffectiveDate = zoneCommision.EffectiveDate.Value.ToUniversalTime();
                    //if (!CheckIsExist(zoneCommision))
                    //{
                    if (this.zoneCommisionService.CreateZoneCommision(zoneCommision))
                    {
                        var getDealerList = fgDealerService.GetAllFGDealer().Where(de=> de.IsActive == true && de.DealersZoneId == zoneCommision.ZoneId);
                        foreach (var aDealer in getDealerList)
                        {
                            if (zoneCommision.ZoneCommisionDetails != null)
                                foreach (var zcd in zoneCommision.ZoneCommisionDetails)
                                {
                                    DealerCommisionDetail dcd = new DealerCommisionDetail();
                                    dcd.Id = Guid.NewGuid();
                                    dcd.DealerId = aDealer.Id;
                                    dcd.ZoneCommisionId = zcd.ZoneCommisionId;
                                    dcd.FGUOMId = zcd.FGUOMId;
                                    dcd.MonthlyTarget = zcd.MonthlyTarget;
                                    dcd.MonthlyCommission = zcd.MonthlyCommission;
                                    dcd.QuarterlyCommission = zcd.QuarterlyCommission;
                                    dcd.HalfYearlyCommission = zcd.HalfYearlyCommission;
                                    dcd.YearlyCommission = zcd.YearlyCommission;
                                    dcd.IsOverride = false;
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
                        message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceZoneCommision.ZoneCommission);
                    }
                    else
                    {
                        isSuccess = false;
                        message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceZoneCommision.ZoneCommission);
                    }
                    //}
                    //else
                    //{
                    //    isSuccess = false;
                    //    message = "Can't save. Same zoneCommision name found!";
                    //}
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
                    if (isNew.ZoneCommisionDetails != null)
                    {
                        foreach (var zcd in isNew.ZoneCommisionDetails.ToList())
                        {
                            var getZone = this.zoneCommisionDetailService.GetZoneCommisionDetail(zcd.Id);
                            if (getZone != null)
                            {
                                try
                                {
                                    this.zoneCommisionDetailService.DeleteZoneCommisionDetail(getZone.Id);
                                }
                                catch { }
                            }
                        }
                    }


                    if (zoneCommision.ZoneCommisionDetails != null)
                    {
                        foreach (var zcvm in zoneCommision.ZoneCommisionDetails)
                        {
                            zcvm.Id = Guid.NewGuid();
                            zcvm.ZoneCommisionId = zoneCommision.Id;
                            try
                            {
                                this.zoneCommisionDetailService.CreateZoneCommisionDetail(zcvm);
                            }
                            catch { }
                        }
                    }

                    if (zoneCommision.EffectiveDate != null)
                        isNew.EffectiveDate = zoneCommision.EffectiveDate.Value.ToUniversalTime();
                    if (this.zoneCommisionService.UpdateZoneCommision(isNew))
                    {
                        var getdealerCommission = dealerCommisionDetailService.GetAllDealerCommisionDetail()
                            .Where(del => del.ZoneCommisionId == isNew.Id).ToList();
                        if (getdealerCommission.Any())
                        {
                            var dealerList = getdealerCommission.GroupBy(de => de.DealerId).ToList();

                            foreach (var dc in getdealerCommission)
                            {
                                //dc.IsOverride = false;
                                this.dealerCommisionDetailService.DeleteDealerCommisionDetail(dc.Id);
                            }

                            foreach (var aDealer in dealerList)
                            {
                                if (zoneCommision.ZoneCommisionDetails != null)
                                    foreach (var zcd in zoneCommision.ZoneCommisionDetails)
                                    {
                                        DealerCommisionDetail aDealerCommission = new DealerCommisionDetail();
                                        aDealerCommission.Id = Guid.NewGuid();
                                        aDealerCommission.DealerId = Convert.ToInt32(aDealer.Key);
                                        aDealerCommission.ZoneCommisionId = zcd.ZoneCommisionId;
                                        aDealerCommission.FGUOMId = zcd.FGUOMId;
                                        aDealerCommission.MonthlyTarget = zcd.MonthlyTarget;
                                        aDealerCommission.MonthlyCommission = zcd.MonthlyCommission;
                                        aDealerCommission.QuarterlyCommission = zcd.QuarterlyCommission;
                                        aDealerCommission.HalfYearlyCommission = zcd.HalfYearlyCommission;
                                        aDealerCommission.YearlyCommission = zcd.YearlyCommission;
                                        aDealerCommission.IsOverride = false;
                                        try
                                        {
                                            this.dealerCommisionDetailService.CreateDealerCommisionDetail(aDealerCommission);
                                        }
                                        catch
                                        {
                                        }
                                    }
                            }
                        }

                        isSuccess = true;
                        message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceZoneCommision.ZoneCommission);
                    }
                    else
                    {
                        isSuccess = false;
                        message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceZoneCommision.ZoneCommission);
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
        private bool CheckIsExist(Model.Models.ZoneCommision zoneCommision)
        {
            return this.zoneCommisionService.CheckIsExist(zoneCommision);
        }
        [HttpPost]
        public JsonResult DeleteZoneCommision(ZoneCommision zoneCommision)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/ZoneCommision/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.zoneCommisionService.DeleteZoneCommision(zoneCommision.Id);
                if (isSuccess)
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_delete, Resources.ResourceZoneCommision.ZoneCommission);

                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceZoneCommision.ZoneCommission);
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

        public JsonResult GetDateAndDealer(int zoneId)
        {
            var zoneCommision = this.zoneCommisionService.GetAllZoneCommision()
                .Where(zn => zn.ZoneId == zoneId);// && DateTime.UtcNow < zn.EffectiveDate);
            List<ZoneCommisionViewModel> dates = new List<ZoneCommisionViewModel>();

            if (zoneCommision.Any())
            {
                foreach (var zz in zoneCommision)
                {
                    ZoneCommisionViewModel zone = new ZoneCommisionViewModel();
                    zone.Id = zz.Id;
                    if (zz.EffectiveDate != null)
                        zone.EffectiveDateString = zz.EffectiveDate.Value.AddMinutes(timeZoneOffset)
                            .ToString(dateTimeFormat);
                    dates.Add(zone);
                }
                dates = dates.OrderByDescending(dt => dt.EffectiveDate).ToList();
            }
            
            var dealers = this.fgDealerService.GetAllFGDealer()
                .Where(dealer => dealer.DealersZoneId == zoneId && dealer.IsActive == true);
            List<FGDealer> Dealers = new List<FGDealer>();
            if (dealers.Any())
            {
                foreach (var dd in dealers)
                {
                    FGDealer dealer = new FGDealer();
                    dealer.Id = dd.Id;
                    dealer.Name = dd.Name;
                    Dealers.Add(dealer);
                }
                Dealers = Dealers.OrderBy(de => de.Name).ToList();
            }
            return Json(new { dates = dates, Dealers = Dealers },
                JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult GetEffectiveDate(int zoneId)
        {
            var effectiveDates = this.zoneCommisionService.GetAllZoneCommision().Where(dd => dd.ZoneId == zoneId);
            List<ZoneCommisionViewModel> dates = new List<ZoneCommisionViewModel>();
            if (effectiveDates.Any())
            {
                foreach (var zz in effectiveDates)
                {
                    ZoneCommisionViewModel zone = new ZoneCommisionViewModel();
                    zone.Id = zz.Id;
                    if (zz.EffectiveDate != null)
                        zone.EffectiveDateString = zz.EffectiveDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                    dates.Add(zone);
                }
                dates = dates.OrderByDescending(dt => dt.EffectiveDate).ToList();
            }
            return Json(dates = dates, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetZoneCommisionByZoneAndDate(int id)//int zoneId, DateTime effectiveDate)
        {
            //var dt = getAllData.EffectiveDate.Value.ToUniversalTime();
            var zoneCommision = this.zoneCommisionService.GetZoneCommision(id);
            var zoneCommisionTemp = new ZoneCommisionViewModel();
            if (zoneCommision != null)
            {
                zoneCommisionTemp = AZoneCommision(zoneCommision);
                //zoneCommisionVMList.Add(zoneCommisionTemp);
            }

            return Json(zoneCommisionTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetZoneCommisionList()
        {
            var zoneCommisionListObj = this.zoneCommisionService.GetAllZoneCommision();
            List<ZoneCommisionViewModel> zoneCommisionVMList = new List<ZoneCommisionViewModel>();

            foreach (var zoneCommision in zoneCommisionListObj)
            {
                var zoneCommisionTemp = AZoneCommision(zoneCommision);

                zoneCommisionVMList.Add(zoneCommisionTemp);
            }
            return Json(zoneCommisionVMList, JsonRequestBehavior.AllowGet);
        }

        public ZoneCommisionViewModel AZoneCommision(ZoneCommision zoneCommision)
        {
            List<ZoneCommisionDetailsViewModel> zoneDetailsCTN = new List<ZoneCommisionDetailsViewModel>();
            List<ZoneCommisionDetailsViewModel> zoneDetailsPCS = new List<ZoneCommisionDetailsViewModel>();
            List<ZoneCommisionDetailsViewModel> zoneDetailsSFT = new List<ZoneCommisionDetailsViewModel>();
            List<ZoneCommisionDetailsViewModel> zoneDetailsSMT = new List<ZoneCommisionDetailsViewModel>();

            ZoneCommisionViewModel zoneCommisionTemp = new ZoneCommisionViewModel();
            zoneCommisionTemp.Id = zoneCommision.Id;
            zoneCommisionTemp.ZoneId = zoneCommision.ZoneId;
            if (zoneCommision.EffectiveDate != null)
                zoneCommisionTemp.EffectiveDateString = zoneCommision.EffectiveDate.Value.AddMinutes(timeZoneOffset)
                    .ToString(dateTimeFormat);

            if (zoneCommision.ZoneCommisionDetails.Any())
            {
                var getZoneCommisionDetailsCtn = zoneCommision.ZoneCommisionDetails.Where(zz => zz.FGUOMId == 1);
                if (getZoneCommisionDetailsCtn.Any())
                {
                    foreach (var zCtn in getZoneCommisionDetailsCtn)
                    {
                        var tempCtn = AZoneCommisionDetail(zCtn);
                        zoneDetailsCTN.Add(tempCtn);
                    }

                }

                var getZoneCommisionDetailsPcs = zoneCommision.ZoneCommisionDetails.Where(zz => zz.FGUOMId == 2);
                if (getZoneCommisionDetailsPcs.Any())
                {
                    foreach (var zPcs in getZoneCommisionDetailsPcs)
                    {
                        var tempPcs = AZoneCommisionDetail(zPcs);
                        zoneDetailsPCS.Add(tempPcs);
                    }

                }

                var getZoneCommisionDetailsSft = zoneCommision.ZoneCommisionDetails.Where(zz => zz.FGUOMId == 3);
                if (getZoneCommisionDetailsSft.Any())
                {
                    foreach (var zSft in getZoneCommisionDetailsSft)
                    {
                        var tempSft = AZoneCommisionDetail(zSft);
                        zoneDetailsSFT.Add(tempSft);
                    }

                }

                var getZoneCommisionDetailsSmt = zoneCommision.ZoneCommisionDetails.Where(zz => zz.FGUOMId == 4);
                if (getZoneCommisionDetailsSmt.Any())
                {
                    foreach (var zSmt in getZoneCommisionDetailsSmt)
                    {
                        var tempSmt = AZoneCommisionDetail(zSmt);
                        zoneDetailsSMT.Add(tempSmt);
                    }

                }

            }
            zoneCommisionTemp.zoneDetailsCTN = zoneDetailsCTN.OrderBy(cc=> cc.MonthlyTarget).ToList();
            zoneCommisionTemp.zoneDetailsPCS = zoneDetailsPCS.OrderBy(cc => cc.MonthlyTarget).ToList();
            zoneCommisionTemp.zoneDetailsSFT = zoneDetailsSFT.OrderBy(cc => cc.MonthlyTarget).ToList();
            zoneCommisionTemp.zoneDetailsSMT = zoneDetailsSMT.OrderBy(cc => cc.MonthlyTarget).ToList();

            return zoneCommisionTemp;
        }
        
        public ZoneCommisionDetailsViewModel AZoneCommisionDetail(ZoneCommisionDetail zcd)
        {
            ZoneCommisionDetailsViewModel zoneCommisionDetailsTemp = new ZoneCommisionDetailsViewModel();
            zoneCommisionDetailsTemp.FGUOMId = zcd.FGUOMId;
            zoneCommisionDetailsTemp.MonthlyCommission = zcd.MonthlyCommission;
            zoneCommisionDetailsTemp.MonthlyTarget = zcd.MonthlyTarget;
            zoneCommisionDetailsTemp.QuarterlyCommission = zcd.QuarterlyCommission;
            zoneCommisionDetailsTemp.HalfYearlyCommission = zcd.HalfYearlyCommission;
            zoneCommisionDetailsTemp.YearlyCommission = zcd.YearlyCommission;
            return zoneCommisionDetailsTemp;
        }


        public JsonResult GetZoneCommision(int id)
        {
            var zoneCommision = this.zoneCommisionService.GetZoneCommision(id);
            return Json(zoneCommision);
        }
    }

    public class ZoneCommisionViewModel
    {
        public ZoneCommisionViewModel()
        {
            this.ZoneCommisionDetails = new List<ZoneCommisionDetail>();
            this.zoneDetailsCTN = new List<ZoneCommisionDetailsViewModel>();
            this.zoneDetailsPCS = new List<ZoneCommisionDetailsViewModel>();
            this.zoneDetailsSFT = new List<ZoneCommisionDetailsViewModel>();
            this.zoneDetailsSMT = new List<ZoneCommisionDetailsViewModel>();
        }

        public int Id { get; set; }
        public Nullable<int> ZoneId { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public string EffectiveDateString { get; set; }
        public List<ZoneCommisionDetail> ZoneCommisionDetails { get; set; }
        public List<ZoneCommisionDetailsViewModel> zoneDetailsCTN { get; set; }
        public List<ZoneCommisionDetailsViewModel> zoneDetailsPCS { get; set; }
        public List<ZoneCommisionDetailsViewModel> zoneDetailsSFT { get; set; }
        public List<ZoneCommisionDetailsViewModel> zoneDetailsSMT { get; set; }
    }


    public class ZoneCommisionDetailsViewModel
    {
        public System.Guid Id { get; set; }
        public Nullable<int> ZoneCommisionId { get; set; }
        public Nullable<int> FGUOMId { get; set; }
        public Nullable<double> MonthlyTarget { get; set; }
        public Nullable<double> MonthlyCommission { get; set; }
        public Nullable<double> QuarterlyCommission { get; set; }
        public Nullable<double> HalfYearlyCommission { get; set; }
        public Nullable<double> YearlyCommission { get; set; }
    }
}