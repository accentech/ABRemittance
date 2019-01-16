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
    public class InstrumentScheduleController : Controller
    {
        public readonly IInstrumentScheduleService instrumentScheduleService;
        public readonly IInstrumentService instrumentService;
        public readonly IInstrumentStatusService instrumentStatusService;
        public readonly IServiceProviderService serviceProviderService;
        public readonly IEmployeeService employeeService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:instrumentSchedule" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/InstrumentSchedule/Index";
            
        // GET: /InstrumentSchedule/
        public ActionResult Index()
        {
            var url = Request.RawUrl;

            return CommonActionResult("InstrumentScheduleList");
        }

        public ActionResult CreateView()
        {
            return CommonActionResult("InstrumentSchedule");
        }

        

        public ActionResult EditView(string id, string redirectPage)
        {
            ViewBag.ScheduleId = id;
            ViewBag.RedirectPage = redirectPage;

            return CommonActionResult("InstrumentSchedule");
        }

        private ActionResult CommonActionResult(string viewName)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                cacheProvider.Set("permission:instrumentSchedule" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
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

        public InstrumentScheduleController(IInstrumentScheduleService instrumentScheduleService, IInstrumentService instrumentService, IInstrumentStatusService instrumentStatusService, IServiceProviderService serviceProviderService, IEmployeeService employeeService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.instrumentScheduleService = instrumentScheduleService;
            this.instrumentService = instrumentService;
            this.instrumentStatusService = instrumentStatusService;
            this.serviceProviderService = serviceProviderService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.employeeService = employeeService;
        }

        [HttpPost]
        public JsonResult CreateInstrumentSchedule(InstrumentSchedule instrumentSchedule)
        {
            BusinessUser curUser = Helpers.UserSession.GetUserFromSession();
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, curUser.RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = instrumentSchedule.Id == Guid.Empty ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(instrumentSchedule))
                    {
                        instrumentSchedule.Id = Guid.NewGuid();
                        if (this.instrumentScheduleService.CreateInstrumentSchedule(instrumentSchedule))
                        {
                            if (instrumentSchedule.IsActive!=null) 
                            {
                                var maintenanceSchedules = this.instrumentScheduleService.GetInstrumentsScheduleByInstrumentId(instrumentSchedule.InstrumentId.Value);
                                foreach (var maintenanceSchedule in maintenanceSchedules)
                                {
                                    if (maintenanceSchedule.Id != instrumentSchedule.Id && maintenanceSchedule.NextServiceDate <= instrumentSchedule.NextServiceDate) 
                                    {
                                        maintenanceSchedule.IsActive = false;
                                        maintenanceSchedule.Comment = "New schedule created on dated " + instrumentSchedule.NextServiceDate;
                                        maintenanceSchedule.CommentBy = curUser.EmployeeId;
                                        maintenanceSchedule.CommentOn = DateTime.Now;

                                        this.instrumentScheduleService.UpdateInstrumentSchedule(maintenanceSchedule);
                                    }                                    
                                }                                                                                           
                            }
                        
                            isSuccess = true;
                            message = "MaintenanceSchedule saved successfully!";
                        }
                        else
                        {
                            message = "MaintenanceSchedule could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same MaintenanceSchedule name found!";
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
                    instrumentSchedule.CommentBy = curUser.EmployeeId;
                    instrumentSchedule.CommentOn = DateTime.Now;
                    if (this.instrumentScheduleService.UpdateInstrumentSchedule(instrumentSchedule))
                    {
                        isSuccess = true;
                        message = "MaintenanceSchedule updated successfully!";
                    }
                    else
                    {
                        message = "MaintenanceSchedule could not updated!";
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

        private bool CheckIsExist(Model.Models.InstrumentSchedule instrumentSchedule)
        {
            return this.instrumentScheduleService.CheckIsExist(instrumentSchedule);
        }

        [HttpPost]
        public JsonResult DeleteInstrumentSchedule(InstrumentSchedule instrumentSchedule)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.instrumentScheduleService.DeleteInstrumentSchedule(instrumentSchedule.Id);
                if (isSuccess)
                {
                    message = "MaintenanceSchedule deleted successfully!";
                }
                else
                {
                    message = "MaintenanceSchedule can't be deleted!";
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

        public JsonResult GetInstrumentScheduleList()
        {
            var instrumentScheduleListObj = this.instrumentScheduleService.GetAllInstrumentSchedule();
            List<InstrumentScheduleViewModel> instrumentScheduleVMList = new List<InstrumentScheduleViewModel>();

            foreach (var instrumentSchedule in instrumentScheduleListObj)
            {
                InstrumentScheduleViewModel instrumentScheduleTemp = PrepareViewObject(instrumentSchedule);

                instrumentScheduleVMList.Add(instrumentScheduleTemp);
            }
            return Json(instrumentScheduleVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUpcomingInstrumentScheduleList()
        {
            var instrumentScheduleListObj = this.instrumentScheduleService.GetAllInstrumentSchedule().Where(a => a.IsActive && a.Instrument.InstrumentStatusId!=int.Parse(ConfigurationManager.AppSettings["NonServiceableStatus"]) && a.NextServiceDate != null && a.NextServiceDate >= DateTime.Today).OrderByDescending(a => a.NextServiceDate).Take(int.Parse(ConfigurationManager.AppSettings["upcomingItemCount"]));
            List<InstrumentScheduleViewModel> instrumentScheduleVMList = new List<InstrumentScheduleViewModel>();

            foreach (var instrumentSchedule in instrumentScheduleListObj)
            {
                InstrumentScheduleViewModel instrumentScheduleTemp = PrepareViewObject(instrumentSchedule);

                instrumentScheduleVMList.Add(instrumentScheduleTemp);
            }
            return Json(instrumentScheduleVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDueInstrumentScheduleList()
        {
            var instrumentScheduleListObj = this.instrumentScheduleService.GetAllInstrumentSchedule().Where(a => a.IsActive && a.Instrument.InstrumentStatusId != int.Parse(ConfigurationManager.AppSettings["NonServiceableStatus"]) && a.NextServiceDate != null && a.NextServiceDate < DateTime.Today).OrderByDescending(a => a.NextServiceDate).Take(int.Parse(ConfigurationManager.AppSettings["DueItemCount"]));
            List<InstrumentScheduleViewModel> instrumentScheduleVMList = new List<InstrumentScheduleViewModel>();

            foreach (var instrumentSchedule in instrumentScheduleListObj)
            {
                InstrumentScheduleViewModel instrumentScheduleTemp = PrepareViewObject(instrumentSchedule);

                instrumentScheduleVMList.Add(instrumentScheduleTemp);
            }
            return Json(instrumentScheduleVMList, JsonRequestBehavior.AllowGet);
        }

        private InstrumentScheduleViewModel PrepareViewObject(InstrumentSchedule instrumentSchedule)
        {
            InstrumentScheduleViewModel instrumentScheduleTemp = new InstrumentScheduleViewModel();
            instrumentScheduleTemp.Id = instrumentSchedule.Id;
            instrumentScheduleTemp.IsActive = instrumentSchedule.IsActive;
            if (instrumentSchedule.IsInitial!=null)
                instrumentScheduleTemp.IsInitial = instrumentSchedule.IsInitial.Value;
            instrumentScheduleTemp.Comment = instrumentSchedule.Comment;
            if (instrumentSchedule.CommentOn != null)
                instrumentScheduleTemp.CommentOn = instrumentSchedule.CommentOn.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            instrumentScheduleTemp.CommentBy = instrumentSchedule.CommentBy;
            if (instrumentSchedule.CreateOn != null)
                instrumentScheduleTemp.CreateOn = instrumentSchedule.CreateOn.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            instrumentScheduleTemp.CreatedBy = instrumentSchedule.CreatedBy;
            if (instrumentScheduleTemp.CommentBy > 0)
            {
                var employee = this.employeeService.GetEmployee(instrumentScheduleTemp.CommentBy.Value);
                instrumentScheduleTemp.CommentByName = employee.FullName;
            }

            instrumentScheduleTemp.InstrumentId = instrumentSchedule.InstrumentId.Value;
            if (instrumentScheduleTemp.InstrumentId > 0)
            {
                var instrumentTemp = instrumentService.GetInstrument(instrumentScheduleTemp.InstrumentId.Value);
                instrumentScheduleTemp.InstrumentName = instrumentTemp.USTCCode + " - " + instrumentTemp.InstrumentMaster.Name;
            }

            instrumentScheduleTemp.InstrumentStatusId = instrumentSchedule.InstrumentStatusId;
            if (instrumentScheduleTemp.InstrumentStatusId > 0)
                instrumentScheduleTemp.InstrumentStatusName = instrumentSchedule.InstrumentStatu.Name;

            if (instrumentSchedule.ServiceDate != null)
                instrumentScheduleTemp.ServiceDate = instrumentSchedule.ServiceDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);
            if (instrumentSchedule.NextServiceDate != null)
                instrumentScheduleTemp.NextServiceDate = instrumentSchedule.NextServiceDate.Value.AddMinutes(timeZoneOffset).ToString(ConfigurationManager.AppSettings["dateFormat"]);

            if (instrumentScheduleTemp.IsActive)
            {
                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                               Helpers.UserSession.GetUserFromSession().RoleId);
                instrumentScheduleTemp.IsEditable = permission.UpdateOperation.Value;
            }
            else
            {
                instrumentScheduleTemp.IsEditable = false;
            }
            return instrumentScheduleTemp;
        }

        public JsonResult GetInstrumentSchedule(Guid id)
        {
            var instrumentSchedule = this.instrumentScheduleService.GetInstrumentSchedule(id);
            return Json(PrepareViewObject(instrumentSchedule), JsonRequestBehavior.AllowGet);
        }

    }

    public class InstrumentScheduleViewModel
    {
        public System.Guid Id { get; set; }
        public Nullable<int> InstrumentId { get; set; }
        public string InstrumentName { get; set; }
        public string ServiceDate { get; set; }
        public string NextServiceDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsInitial { get; set; }
        public int InstrumentStatusId { get; set; }
        public string InstrumentStatusName { get; set; }
        public bool IsEditable { get; set; }
        public string Comment { get; set; }
        public string CommentOn { get; set; }
        public Nullable<int> CommentBy { get; set; }
        public string CommentByName { get; set; }
        public string CreateOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
    }


}