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
    public class InstrumentLocationController : Controller
    {
        public readonly IDepartmentService departmentService;
        public readonly IInstrumentLocationService instrumentLocationService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:instrumentLocation" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/InstrumentLocation/Index";   

        // GET: /InstrumentLocation/
        public ActionResult Index()
        {
            var url1 = Request.RawUrl;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url1, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:instrumentLocation" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("InstrumentLocation");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public InstrumentLocationController(IInstrumentLocationService instrumentLocationService, IDepartmentService departmentService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.instrumentLocationService = instrumentLocationService;
            this.departmentService = departmentService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateInstrumentLocation(InstrumentLocation instrumentLocation)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = instrumentLocation.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(instrumentLocation))
                    {
                        if (this.instrumentLocationService.CreateInstrumentLocation(instrumentLocation))
                        {
                            isSuccess = true;
                            message = "MachineLocation saved successfully!";
                        }
                        else
                        {
                            message = "MachineLocation could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same MachineLocation name found!";
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
                    if (this.instrumentLocationService.UpdateInstrumentLocation(instrumentLocation))
                    {
                        isSuccess = true;
                        message = "MachineLocation updated successfully!";
                    }
                    else
                    {
                        message = "MachineLocation could not updated!";
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
        private bool CheckIsExist(Model.Models.InstrumentLocation instrumentLocation)
        {
            return this.instrumentLocationService.CheckIsExist(instrumentLocation);
        }

        [HttpPost]
        public JsonResult DeleteInstrumentLocation(InstrumentLocation instrumentLocation)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.instrumentLocationService.DeleteInstrumentLocation(instrumentLocation.Id);
                if (isSuccess)
                {
                    message = "MachineLocation deleted successfully!";
                }
                else
                {
                    message = "MachineLocation can't be deleted!";
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

        public JsonResult GetInstrumentLocationList()
        {
            var instrumentLocationListObj = this.instrumentLocationService.GetAllInstrumentLocation();
            List<InstrumentLocationViewModel> instrumentLocationVMList = new List<InstrumentLocationViewModel>();

            foreach (var instrumentLocation in instrumentLocationListObj)
            {
                InstrumentLocationViewModel instrumentLocationTemp = new InstrumentLocationViewModel();
                instrumentLocationTemp.Id = instrumentLocation.Id;
                instrumentLocationTemp.Location = instrumentLocation.Location;
                 
                instrumentLocationVMList.Add(instrumentLocationTemp);
            }
            return Json(instrumentLocationVMList, JsonRequestBehavior.AllowGet);
        }

   
        public JsonResult GetInstrumentLocation(int id)
        {
            var instrumentLocation = this.instrumentLocationService.GetInstrumentLocation(id);
            return Json(instrumentLocation);
        }
    }

    public class InstrumentLocationViewModel
    {
        public int Id { get; set; }
        public string Location { get; set; }        

    }

}