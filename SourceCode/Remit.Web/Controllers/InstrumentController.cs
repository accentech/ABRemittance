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
    public class InstrumentController : Controller
    {
        public readonly ICountryService countryService;
        public readonly IManufacturerService manufacturerService;
        public readonly IInstrumentService instrumentService;
        public readonly IInstrumentMasterService instrumentMasterService;
        public readonly IInstrumentStatusService instrumentStatusService;
        public readonly IInstrumentScheduleService instrumentScheduleService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:instrument" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/Instrument/Index";
            
        // GET: /Instrument/
        public ActionResult Index()
        {
            var url = Request.RawUrl;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:instrument" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("Instrument");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public InstrumentController(IInstrumentService instrumentService, IInstrumentScheduleService instrumentScheduleService,IInstrumentMasterService instrumentMasterService, IInstrumentStatusService instrumentStatusService, ICountryService countryService, IManufacturerService manufacturerService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.instrumentService = instrumentService;
            this.instrumentMasterService = instrumentMasterService;
            this.instrumentStatusService = instrumentStatusService;
            this.countryService = countryService;
            this.manufacturerService = manufacturerService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.instrumentScheduleService = instrumentScheduleService;
        }

        [HttpPost]
        public JsonResult CreateInstrument(Instrument instrument)
        {
            BusinessUser curUser = Helpers.UserSession.GetUserFromSession();
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, curUser.RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = instrument.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(instrument))
                    {
                        if (this.instrumentService.CreateInstrument(instrument))
                        {
                            DateTime? serviceDate = null;
                            if (instrument.ServiceDate != null)
                            {
                                serviceDate = instrument.ServiceDate.Value;
                                if (serviceDate == DateTime.MinValue)
                                    serviceDate = instrument.InstallDate.Value.AddDays(instrument.ServiceInterval.Value);
                            }
                            else
                            {
                                serviceDate = instrument.InstallDate.Value.AddDays(instrument.ServiceInterval.Value);
                            }
                             
                            InstrumentSchedule instSchedule = new InstrumentSchedule() { Id = Guid.NewGuid(), InstrumentId = instrument.Id, InstrumentStatusId = instrument.InstrumentStatusId, NextServiceDate = serviceDate, IsActive = true, IsInitial = true, CreateOn=DateTime.Now, CreatedBy=curUser.EmployeeId };
                            this.instrumentScheduleService.CreateInstrumentSchedule(instSchedule);

                            isSuccess = true;
                            message = "Machine saved successfully!";
                        }
                        else
                        {
                            message = "Machine could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same instrument name found!";
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
                    if (this.instrumentService.UpdateInstrument(instrument))
                    {
                        var maintenanceSchedules = this.instrumentScheduleService.GetInstrumentsScheduleByInstrumentId(instrument.Id);
                        if (maintenanceSchedules.Count() > 0)
                        {
                            var maintenanceSchedule = maintenanceSchedules.Where(ms => ms.IsInitial != null && ms.IsInitial.Value).First();
                            if (maintenanceSchedule != null && maintenanceSchedule.NextServiceDate != instrument.ServiceDate)
                            {
                                DateTime serviceDate = instrument.ServiceDate.Value;
                                if (serviceDate == DateTime.MinValue)
                                    serviceDate = instrument.InstallDate.Value.AddDays(instrument.ServiceInterval.Value);

                                maintenanceSchedule.NextServiceDate = serviceDate;
                                maintenanceSchedule.InstrumentStatusId = instrument.InstrumentStatusId;
                                maintenanceSchedule.Comment = "Machine info updated!";
                                maintenanceSchedule.CommentBy = curUser.EmployeeId;
                                maintenanceSchedule.CommentOn = DateTime.Now;

                                this.instrumentScheduleService.UpdateInstrumentSchedule(maintenanceSchedule);
                            }
                        }                        

                        isSuccess = true;
                        message = "Machine updated successfully!";
                    }
                    else
                    {
                        message = "Machine could not updated!";
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

        private bool CheckIsExist(Model.Models.Instrument instrument)
        {
            return this.instrumentService.CheckIsExist(instrument);
        }

        [HttpPost]
        public JsonResult DeleteInstrument(Instrument instrument)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.instrumentService.DeleteInstrument(instrument.Id);
                if (isSuccess)
                {
                    message = "Machine deleted successfully!";
                }
                else
                {
                    message = "Machine can't be deleted!";
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

        public JsonResult GetInstrumentList()
        {
            var instrumentListObj = this.instrumentService.GetAllInstrument();
            List<InstrumentViewModel> instrumentVMList = PrepareViewModel(instrumentListObj);
            return Json(instrumentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveInstrumentList()
        {
            var instrumentListObj = this.instrumentService.GetAllInstrument().Where(i=> i.IsActive);
            List<InstrumentViewModel> instrumentVMList = PrepareViewModel(instrumentListObj);
            return Json(instrumentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetServiceableInstrumentList()
        {
            var instrumentListObj = this.instrumentService.GetAllInstrument().Where(i => i.IsActive && i.InstrumentStatusId != int.Parse(ConfigurationManager.AppSettings["NonServiceableStatus"]));
            List<InstrumentViewModel> instrumentVMList = PrepareViewModel(instrumentListObj);
            return Json(instrumentVMList, JsonRequestBehavior.AllowGet);
        }

        private List<InstrumentViewModel> PrepareViewModel(IEnumerable<Instrument> instrumentListObj)
        {
            List<InstrumentViewModel> instrumentVMList = new List<InstrumentViewModel>();

            foreach (var instrument in instrumentListObj)
            {
                InstrumentViewModel instrumentTemp = PrepareViewObject(instrument);

                instrumentVMList.Add(instrumentTemp);
            }
            return instrumentVMList;
        }

        private InstrumentViewModel PrepareViewObject(Instrument instrument)
        {
            InstrumentViewModel instrumentTemp = new InstrumentViewModel();
            instrumentTemp.Id = instrument.Id;
            instrumentTemp.Brand = instrument.Brand;
            instrumentTemp.ModelNo = instrument.ModelNo;
            instrumentTemp.InstrumentStatusId = instrument.InstrumentStatusId;
            if (instrumentTemp.InstrumentStatusId > 0)
                instrumentTemp.InstrumentStatusName = instrumentStatusService.GetInstrumentStatus(Convert.ToInt32(instrument.InstrumentStatusId)).Name;

            instrumentTemp.InstrumentMasterId = instrument.InstrumentMasterId;
            if (instrumentTemp.InstrumentMasterId > 0)
                instrumentTemp.InstrumentMasterName = instrumentMasterService.GetInstrumentMaster(Convert.ToInt32(instrument.InstrumentMasterId)).Name;

            instrumentTemp.ManufacturerId = instrument.ManufacturerId.Value;
            if (instrumentTemp.ManufacturerId > 0)
                instrumentTemp.ManufacturerName = manufacturerService.GetManufacturer(Convert.ToInt32(instrument.ManufacturerId)).Name;

            instrumentTemp.CountryOriginId = instrument.CountryOriginId;
            if (instrumentTemp.CountryOriginId !="")
                instrumentTemp.CountryName = countryService.GetCountry(instrument.CountryOriginId).Name;

            instrumentTemp.USTCCode = instrument.USTCCode;
            if (instrument.PurchaseDate!=null)
                instrumentTemp.PurchaseDate = instrument.PurchaseDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            if (instrument.InstallDate!=null)
                instrumentTemp.InstallDate = instrument.InstallDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            instrumentTemp.LifeInUse = instrument.LifeInUse;
            instrumentTemp.IsActive = instrument.IsActive;
            if (instrument.ServiceInterval!=null)
                instrumentTemp.ServiceInterval = instrument.ServiceInterval.Value;

            var maintenanceSchedules = this.instrumentScheduleService.GetInstrumentsScheduleByInstrumentId(instrumentTemp.Id);
            if (maintenanceSchedules.Count() > 0)
            {
                var maintenanceSchedule = maintenanceSchedules.Where(ms => ms.IsInitial.HasValue && ms.IsInitial.Value).FirstOrDefault();
                if (maintenanceSchedule != null && maintenanceSchedule.NextServiceDate.HasValue && maintenanceSchedule.NextServiceDate.Value != DateTime.MinValue)
                    instrumentTemp.ServiceDate = maintenanceSchedule.NextServiceDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            }

            if (instrument.InstrumentLocationId != null)
            {
                instrumentTemp.InstrumentLocationId = instrument.InstrumentLocationId.Value;
                if (instrument.InstrumentLocation != null)
                {
                    instrumentTemp.InstrumentLocationName = instrument.InstrumentLocation.Location;
                }
            }
           
            return instrumentTemp;
        }

        public JsonResult GetInstrument(int id)
        {
            var instrument = this.instrumentService.GetInstrument(id);
            return Json(PrepareViewObject(instrument), JsonRequestBehavior.AllowGet);
        }
    }

    public class InstrumentViewModel
    {
        public int Id { get; set; }
        public int InstrumentMasterId { get; set; }
        public string InstrumentMasterName { get; set; }
        public string ModelNo { get; set; }
        public string Brand { get; set; }
        public string USTCCode { get; set; }
        public string CountryOriginId { get; set; }
        public string CountryName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int InstrumentStatusId { get; set; }
        public string InstrumentStatusName { get; set; }
        public string PurchaseDate { get; set; }
        public string InstallDate { get; set; }
        public string ServiceDate { get; set; }
        public int LifeInUse { get; set; }
        public int ServiceInterval { get; set; }
        public bool IsActive { get; set; }
        public int? InstrumentLocationId { get; set; }
        public string InstrumentLocationName { get; set; }
    }

}