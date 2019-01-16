using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.CachingService;
using Remit.Web.Helpers;
using System.Configuration;

namespace Remit.Web.Controllers
{
    public class InstrumentMaintenanceController : Controller
    {
        public readonly IInstrumentMaintenanceService instrumentMaintenanceService;
        public readonly IInstrumentService instrumentService;
        public readonly IInstrumentStatusService instrumentStatusService;
        public readonly IInstrumentScheduleService instrumentScheduleService;
        public readonly IServiceProviderService serviceProviderService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:instrumentMaintenance" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/InstrumentMaintenance/Index";
            
        // GET: /InstrumentMaintenance/
        public ActionResult Index()
        {
            return CommonActionResult("InstrumentMaintenanceList");     
        }

        public ActionResult CreateView()
        {
            ViewBag.InstrumentId = "";
            ViewBag.LastServiceDate = "";
            ViewBag.MaintenanceId = "";
            ViewBag.RedirectPage = "";
            return CommonActionResult("InstrumentMaintenance");
        }

        public ActionResult CreateMaintenanceView(string id)
        {
            ViewBag.MaintenanceId = "";
            ViewBag.RedirectPage = "";
            ViewBag.InstrumentId = id;
            ViewBag.ServiceInterval = instrumentService.GetInstrument(int.Parse(id)).ServiceInterval.ToString();
            ViewBag.LastServiceDate = GetLastServiceDate(id);
            return CommonActionResult("InstrumentMaintenance");
        }

        public ActionResult EditView(string id, string redirectPage)
        {
            ViewBag.InstrumentId = "";
            ViewBag.LastServiceDate = "";
            ViewBag.MaintenanceId = id;
            ViewBag.RedirectPage = redirectPage;

            return CommonActionResult("InstrumentMaintenance");
        }

        public ActionResult MaintenanceHistory()
        {
            return CommonActionResult("MaintenanceHistory");
        }
        public ActionResult MaintenanceStatus()
        {
            GetMaintenanceInfo();
            return CommonActionResult("MaintenanceStatus");
        }

        private ActionResult CommonActionResult(string viewName)
        { 
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                cacheProvider.Set("permission:instrumentMaintenance" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                if (permission.ReadOperation == true)
                {
                    return View(viewName);
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public InstrumentMaintenanceController(IInstrumentMaintenanceService instrumentMaintenanceService, IInstrumentScheduleService instrumentScheduleService, IInstrumentService instrumentService, IInstrumentStatusService instrumentStatusService, IServiceProviderService serviceProviderService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.instrumentMaintenanceService = instrumentMaintenanceService;
            this.instrumentService = instrumentService;
            this.instrumentStatusService = instrumentStatusService;
            this.instrumentScheduleService = instrumentScheduleService;
            this.serviceProviderService = serviceProviderService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateInstrumentMaintenance(InstrumentMaintenance instrumentMaintenance)
        {
            BusinessUser curUser = Helpers.UserSession.GetUserFromSession();
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, curUser.RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = instrumentMaintenance.Id == Guid.Empty ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(instrumentMaintenance))
                    {
                        instrumentMaintenance.Id = Guid.NewGuid();
                        if (this.instrumentMaintenanceService.CreateInstrumentMaintenance(instrumentMaintenance))
                        {
                            InstrumentSchedule instrumentSchedule = new InstrumentSchedule() { Id = Guid.NewGuid(), InstrumentId = instrumentMaintenance.InstrumentId, InstrumentStatusId = instrumentMaintenance.InstrumentStatusId.Value, ServiceDate= instrumentMaintenance.ServiceDate, NextServiceDate = instrumentMaintenance.NextServiceDate, IsActive = true, IsInitial = false, CreateOn = DateTime.Now, CreatedBy = curUser.EmployeeId };
                            this.instrumentScheduleService.CreateInstrumentSchedule(instrumentSchedule);

                            var maintenanceSchedules = this.instrumentScheduleService.GetInstrumentsScheduleByInstrumentId(instrumentSchedule.InstrumentId.Value);
                            foreach (var maintenanceSchedule in maintenanceSchedules)
                            {
                                if (maintenanceSchedule.Id != instrumentSchedule.Id && maintenanceSchedule.NextServiceDate <= instrumentSchedule.NextServiceDate)
                                {
                                    maintenanceSchedule.IsActive = false;
                                    maintenanceSchedule.Comment = "New schedule created after maintenance on dated " + instrumentSchedule.NextServiceDate;
                                    maintenanceSchedule.CommentBy = curUser.EmployeeId;
                                    maintenanceSchedule.CommentOn = DateTime.Now;

                                    this.instrumentScheduleService.UpdateInstrumentSchedule(maintenanceSchedule);
                                }
                                else if (maintenanceSchedule.Id != instrumentSchedule.Id && maintenanceSchedule.NextServiceDate > instrumentSchedule.NextServiceDate && instrumentSchedule.InstrumentStatusId == int.Parse(ConfigurationManager.AppSettings["NonServiceableStatus"]))
                                {
                                    maintenanceSchedule.IsActive = false;
                                    maintenanceSchedule.Comment = "This is no more servicable as declared on " + instrumentSchedule.ServiceDate;
                                    maintenanceSchedule.CommentBy = curUser.EmployeeId;
                                    maintenanceSchedule.CommentOn = DateTime.Now;

                                    this.instrumentScheduleService.UpdateInstrumentSchedule(maintenanceSchedule);
                                }
                            }

                            var instrument = this.instrumentService.GetInstrument(instrumentMaintenance.InstrumentId.Value);
                            instrument.InstrumentStatusId = instrumentMaintenance.InstrumentStatusId.Value;
                            this.instrumentService.UpdateInstrument(instrument);

                            isSuccess = true;
                            message = "MachineMaintenance saved successfully!";
                        }
                        else
                        {
                            message = "MachineMaintenance could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same instrumentMaintenance name found!";
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
                    if (this.instrumentMaintenanceService.UpdateInstrumentMaintenance(instrumentMaintenance))
                    {
                        var maintenanceSchedules = this.instrumentScheduleService.GetInstrumentsScheduleByInstrumentId(instrumentMaintenance.InstrumentId.Value);
                        var maintenanceSchedule = maintenanceSchedules.Where(ms => ms.IsInitial.Value).First();
                        if (maintenanceSchedule != null && maintenanceSchedule.NextServiceDate != instrumentMaintenance.NextServiceDate)
                        {
                            maintenanceSchedule.NextServiceDate = instrumentMaintenance.NextServiceDate;
                            maintenanceSchedule.InstrumentStatusId = instrumentMaintenance.InstrumentStatusId.Value;
                            maintenanceSchedule.Comment = "Machine Maintenance info updated!";
                            maintenanceSchedule.CommentBy = curUser.EmployeeId;
                            maintenanceSchedule.CommentOn = DateTime.Now;

                            this.instrumentScheduleService.UpdateInstrumentSchedule(maintenanceSchedule);
                        }

                        var instrument = this.instrumentService.GetInstrument(instrumentMaintenance.InstrumentId.Value);
                        instrument.InstrumentStatusId = instrumentMaintenance.InstrumentStatusId.Value;
                        this.instrumentService.UpdateInstrument(instrument);

                        isSuccess = true;
                        message = "MachineMaintenance updated successfully!";
                    }
                    else
                    {
                        message = "MachineMaintenance could not updated!";
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

        private bool CheckIsExist(Model.Models.InstrumentMaintenance instrumentMaintenance)
        {
            return this.instrumentMaintenanceService.CheckIsExist(instrumentMaintenance);
        }

        [HttpPost]
        public JsonResult DeleteInstrumentMaintenance(InstrumentMaintenance instrumentMaintenance)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.instrumentMaintenanceService.DeleteInstrumentMaintenance(instrumentMaintenance.Id);
                if (isSuccess)
                {
                    message = "MachineMaintenance deleted successfully!";
                }
                else
                {
                    message = "MachineMaintenance can't be deleted!";
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

        public JsonResult GetInstrumentMaintenanceList()
        {
            var instrumentMaintenanceListObj = this.instrumentMaintenanceService.GetAllInstrumentMaintenance();
            List<InstrumentMaintenanceViewModel> instrumentMaintenanceVMList = new List<InstrumentMaintenanceViewModel>();

            foreach (var instrumentMaintenance in instrumentMaintenanceListObj)
            {
                InstrumentMaintenanceViewModel instrumentMaintenanceTemp = PrepareViewObject(instrumentMaintenance);
                instrumentMaintenanceVMList.Add(instrumentMaintenanceTemp);
            }
            return Json(instrumentMaintenanceVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetInstrumentMaintenanceListByInstrumentId(string id)
        {
            var instrumentMaintenanceListObj = this.instrumentMaintenanceService.GetAllInstrumentMaintenanceByInstrument(int.Parse(id));
            List<InstrumentMaintenanceViewModel> instrumentMaintenanceVMList = new List<InstrumentMaintenanceViewModel>();

            foreach (var instrumentMaintenance in instrumentMaintenanceListObj)
            {
                InstrumentMaintenanceViewModel instrumentMaintenanceTemp = PrepareViewObject(instrumentMaintenance);
                instrumentMaintenanceVMList.Add(instrumentMaintenanceTemp);
            }
            return Json(instrumentMaintenanceVMList, JsonRequestBehavior.AllowGet);
        }

        private InstrumentMaintenanceViewModel PrepareViewObject(InstrumentMaintenance instrumentMaintenance)
        {
            InstrumentMaintenanceViewModel instrumentMaintenanceTemp = new InstrumentMaintenanceViewModel();
            instrumentMaintenanceTemp.Id = instrumentMaintenance.Id;
            instrumentMaintenanceTemp.Problem = instrumentMaintenance.Problem;
            instrumentMaintenanceTemp.WorkDone = instrumentMaintenance.WorkDone;
            instrumentMaintenanceTemp.InstrumentId = instrumentMaintenance.InstrumentId.Value;
            if (instrumentMaintenanceTemp.InstrumentId > 0)
            {
                var instrumentTemp = instrumentService.GetInstrument(instrumentMaintenanceTemp.InstrumentId);
                instrumentMaintenanceTemp.InstrumentName = instrumentTemp.USTCCode + " - " + instrumentTemp.InstrumentMaster.Name;
                instrumentMaintenanceTemp.ServiceInterval = instrumentTemp.ServiceInterval.Value;
            }


            instrumentMaintenanceTemp.InstrumentStatusId = instrumentMaintenance.InstrumentStatusId.Value;
            if (instrumentMaintenanceTemp.InstrumentStatusId > 0)
            {
                instrumentMaintenanceTemp.InstrumentStatusName = instrumentStatusService.GetInstrumentStatus(instrumentMaintenanceTemp.InstrumentStatusId).Name;
            }

            instrumentMaintenanceTemp.ServiceProviderId = instrumentMaintenance.ServiceProviderId.Value;
            if (instrumentMaintenanceTemp.ServiceProviderId > 0)
                instrumentMaintenanceTemp.ServiceProviderName = instrumentMaintenance.ServiceProvider.Name;

            instrumentMaintenanceTemp.ServicePerson = instrumentMaintenance.ServicePerson;
            instrumentMaintenanceTemp.ServiceDate = instrumentMaintenance.ServiceDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            if (instrumentMaintenance.NextServiceDate!=null)
                instrumentMaintenanceTemp.NextServiceDate = instrumentMaintenance.NextServiceDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            instrumentMaintenanceTemp.LastServiceDate = GetLastServiceDate(instrumentMaintenanceTemp.InstrumentId.ToString());

            return instrumentMaintenanceTemp;
        }

        public JsonResult GetInstrumentMaintenance(Guid id)
        {
            var instrumentMaintenance = this.instrumentMaintenanceService.GetInstrumentMaintenance(id);
            return Json(PrepareViewObject(instrumentMaintenance), JsonRequestBehavior.AllowGet);
        }

        public string GetLastServiceDate(string id)
        {
            var instrumentMaintenance = this.instrumentMaintenanceService.GetAllInstrumentMaintenanceByInstrument(int.Parse(id));
            var maxDate = instrumentMaintenance.Max(a => a.ServiceDate);
            if (maxDate != null)
                return maxDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            else
                return string.Empty;
        }


        protected void GetMaintenanceInfo()
        {
            List<InstrumentSummaryReportViewModel> tmpInstrumentSummaryReportViewModelList = new List<InstrumentSummaryReportViewModel>();
            var instrumentList = this.instrumentService.GetAllInstrument().Where(a => a.IsActive);
            foreach (Instrument instrument in instrumentList)
            {
                DateTime currentDate = DateTime.Today;
                var instrumentMaintenanceList = this.instrumentMaintenanceService.GetAllInstrumentMaintenanceByInstrument(instrument.Id);
                if (instrumentMaintenanceList.Count() > 0)
                {
                    InstrumentSummaryReportViewModel tmpInstrumentSummaryReportViewModel = new InstrumentSummaryReportViewModel();
                    tmpInstrumentSummaryReportViewModel.InstrumentName = instrument.USTCCode + " - " + instrument.InstrumentMaster.Name;

                    tmpInstrumentSummaryReportViewModel.CountMonth01 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth02 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth03 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth04 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth05 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth06 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth07 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth08 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth09 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth10 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth11 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();
                    currentDate = currentDate.AddMonths(-1);
                    tmpInstrumentSummaryReportViewModel.CountMonth12 = instrumentMaintenanceList.Where(a => a.ServiceDate != null && a.ServiceDate.Value.Year == currentDate.Year && a.ServiceDate.Value.Month == currentDate.Month).Count();

                    tmpInstrumentSummaryReportViewModelList.Add(tmpInstrumentSummaryReportViewModel);
                }
            }

            ViewBag.InstrumentSummaryReportViewModelList = tmpInstrumentSummaryReportViewModelList.OrderByDescending(a => a.CountMonth01);
            ViewBag.MonthNames = GetMonthNames();
        }

        private MonthTitleViewModel GetMonthNames()
        {
            string dateFormat = "MMM'' yy";
            MonthTitleViewModel tmpMonthTitleViewModel = new MonthTitleViewModel();
            DateTime currentDate = DateTime.Today;
            tmpMonthTitleViewModel.MonthName01 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName02 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName03 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName04 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName05 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName06 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName07 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName08 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName09 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName10 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName11 = currentDate.ToString(dateFormat);
            currentDate = currentDate.AddMonths(-1);
            tmpMonthTitleViewModel.MonthName12 = currentDate.ToString(dateFormat);

            return tmpMonthTitleViewModel;
        }
    }

    public class InstrumentMaintenanceViewModel
    {
        public System.Guid Id { get; set; }
        public int InstrumentId { get; set; }
        public string InstrumentName { get; set; }
        public string Problem { get; set; }
        public string WorkDone { get; set; }
        public int ServiceProviderId { get; set; }
        public string ServiceProviderName { get; set; }
        public string ServicePerson { get; set; }
        public string ServiceDate { get; set; }
        public string NextServiceDate { get; set; }
        public string LastServiceDate { get; set; }
        public int ServiceInterval { get; set; }
        public int InstrumentStatusId { get; set; }
        public string InstrumentStatusName { get; set; }
    }


    public class InstrumentSummaryReportViewModel
    {
        public string InstrumentName { get; set; }
        public int CountMonth01 { get; set; }
        public int CountMonth02 { get; set; }
        public int CountMonth03 { get; set; }
        public int CountMonth04 { get; set; }
        public int CountMonth05 { get; set; }
        public int CountMonth06 { get; set; }
        public int CountMonth07 { get; set; }
        public int CountMonth08 { get; set; }
        public int CountMonth09 { get; set; }
        public int CountMonth10 { get; set; }
        public int CountMonth11 { get; set; }
        public int CountMonth12 { get; set; }
    }

    public class MonthTitleViewModel
    {
        public string MonthName01 { get; set; }
        public string MonthName02 { get; set; }
        public string MonthName03 { get; set; }
        public string MonthName04 { get; set; }
        public string MonthName05 { get; set; }
        public string MonthName06 { get; set; }
        public string MonthName07 { get; set; }
        public string MonthName08 { get; set; }
        public string MonthName09 { get; set; }
        public string MonthName10 { get; set; }
        public string MonthName11 { get; set; }
        public string MonthName12 { get; set; }
    }
}