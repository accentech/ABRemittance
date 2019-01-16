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

namespace Remit.Web.Controllers
{
    public class InstrumentStatusController : Controller
    {
        public readonly IInstrumentStatusService instrumentStatusService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:instrumentStatus" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/instrumentStatus/Index";
            
        // GET: /instrumentStatus/
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
                    cacheProvider.Set("permission:instrumentStatus" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("instrumentStatus");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml"); 
        }

        public InstrumentStatusController(IInstrumentStatusService instrumentStatusService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.instrumentStatusService = instrumentStatusService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateInstrumentStatus(InstrumentStatu instrumentStatus)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = instrumentStatus.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(instrumentStatus))
                    {

                        if (this.instrumentStatusService.CreateInstrumentStatus(instrumentStatus))
                        {
                            isSuccess = true;
                            message = "MachineStatus saved successfully!";
                        }
                        else
                        {
                            message = "MachineStatus could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same MachineStatus name found!";
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
                    if (this.instrumentStatusService.UpdateInstrumentStatus(instrumentStatus))
                    {
                        isSuccess = true;
                        message = "MachineStatus updated successfully!";
                    }
                    else
                    {
                        message = "MachineStatus could not updated!";
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
        private bool CheckIsExist(Model.Models.InstrumentStatu instrumentStatus)
        {
            return this.instrumentStatusService.CheckIsExist(instrumentStatus);
        }

        [HttpPost]
        public JsonResult DeleteInstrumentStatus(InstrumentStatu instrumentStatus)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.instrumentStatusService.DeleteInstrumentStatus(instrumentStatus.Id);
                if (isSuccess)
                {
                    message = "MachineStatus deleted successfully!";

                }
                else
                {
                    message = "MachineStatus can't be deleted!";
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

        public JsonResult GetInstrumentStatusList()
        {
            var instrumentStatusListObj = this.instrumentStatusService.GetAllInstrumentStatus();
            List<InstrumentStatusViewModel> instrumentStatusVMList = new List<InstrumentStatusViewModel>();

            foreach (var instrumentStatus in instrumentStatusListObj)
            {
                InstrumentStatusViewModel instrumentStatusTemp = PrepareInstrumentStatusViewModel(instrumentStatus);
                               
                instrumentStatusVMList.Add(instrumentStatusTemp);
            }
            return Json(instrumentStatusVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveInstrumentStatusList()
        {
            var instrumentStatusListObj = this.instrumentStatusService.GetAllInstrumentStatus().Where(a=> a.IsActive);
            List<InstrumentStatusViewModel> instrumentStatusVMList = new List<InstrumentStatusViewModel>();

            foreach (var instrumentStatus in instrumentStatusListObj)
            {
                InstrumentStatusViewModel instrumentStatusTemp = PrepareInstrumentStatusViewModel(instrumentStatus);

                instrumentStatusVMList.Add(instrumentStatusTemp);
            }
            return Json(instrumentStatusVMList, JsonRequestBehavior.AllowGet);
        }

        private static InstrumentStatusViewModel PrepareInstrumentStatusViewModel(InstrumentStatu instrumentStatus)
        {
            InstrumentStatusViewModel instrumentStatusTemp = new InstrumentStatusViewModel();
            instrumentStatusTemp.Id = instrumentStatus.Id;
            instrumentStatusTemp.Name = instrumentStatus.Name;
            instrumentStatusTemp.IsActive = instrumentStatus.IsActive;
            return instrumentStatusTemp;
        }

        public JsonResult GetInstrumentStatus(int id)
        {
            var instrumentStatus = this.instrumentStatusService.GetInstrumentStatus(id);
            return Json(instrumentStatus);
        }
    }
    
    public class InstrumentStatusViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}