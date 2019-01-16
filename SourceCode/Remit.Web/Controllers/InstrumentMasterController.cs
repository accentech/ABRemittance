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
    public class InstrumentMasterController : Controller
    {
        public readonly IInstrumentMasterService instrumentMasterService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:instrumentMaster" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/instrumentMaster/Index";
            
        // GET: /instrumentMaster/
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
                    cacheProvider.Set("permission:instrumentMaster" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("instrumentMaster");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml"); 
        }

        public InstrumentMasterController(IInstrumentMasterService instrumentMasterService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.instrumentMasterService = instrumentMasterService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateInstrumentMaster(InstrumentMaster instrumentMaster)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = instrumentMaster.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(instrumentMaster))
                    {

                        if (this.instrumentMasterService.CreateInstrumentMaster(instrumentMaster))
                        {
                            isSuccess = true;
                            message = "MachineFamily saved successfully!";
                        }
                        else
                        {
                            message = "MachineFamily could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same MachineFamily name found!";
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
                    if (this.instrumentMasterService.UpdateInstrumentMaster(instrumentMaster))
                    {
                        isSuccess = true;
                        message = "MachineFamily updated successfully!";
                    }
                    else
                    {
                        message = "MachineFamily could not updated!";
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
        private bool CheckIsExist(Model.Models.InstrumentMaster instrumentMaster)
        {
            return this.instrumentMasterService.CheckIsExist(instrumentMaster);
        }

        [HttpPost]
        public JsonResult DeleteInstrumentMaster(InstrumentMaster instrumentMaster)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.instrumentMasterService.DeleteInstrumentMaster(instrumentMaster.Id);
                if (isSuccess)
                {
                    message = "MachineFamily deleted successfully!";

                }
                else
                {
                    message = "instrumentMaster can't be deleted!";
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

        public JsonResult GetInstrumentMasterList()
        {
            var instrumentMasterListObj = this.instrumentMasterService.GetAllInstrumentMaster();
            List<InstrumentMasterViewModel> instrumentMasterVMList = new List<InstrumentMasterViewModel>();

            foreach (var instrumentMaster in instrumentMasterListObj)
            {
                InstrumentMasterViewModel instrumentMasterTemp = PrepareInstrumentMasterViewModel(instrumentMaster);
                               
                instrumentMasterVMList.Add(instrumentMasterTemp);
            }
            return Json(instrumentMasterVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveInstrumentMasterList()
        {
            var instrumentMasterListObj = this.instrumentMasterService.GetAllInstrumentMaster().Where(a=> a.IsActive);
            List<InstrumentMasterViewModel> instrumentMasterVMList = new List<InstrumentMasterViewModel>();

            foreach (var instrumentMaster in instrumentMasterListObj)
            {
                InstrumentMasterViewModel instrumentMasterTemp = PrepareInstrumentMasterViewModel(instrumentMaster);

                instrumentMasterVMList.Add(instrumentMasterTemp);
            }
            return Json(instrumentMasterVMList, JsonRequestBehavior.AllowGet);
        }

        private static InstrumentMasterViewModel PrepareInstrumentMasterViewModel(InstrumentMaster instrumentMaster)
        {
            InstrumentMasterViewModel instrumentMasterTemp = new InstrumentMasterViewModel();
            instrumentMasterTemp.Id = instrumentMaster.Id;
            instrumentMasterTemp.Name = instrumentMaster.Name;
            instrumentMasterTemp.IsActive = instrumentMaster.IsActive;
            return instrumentMasterTemp;
        }

        public JsonResult GetInstrumentMaster(int id)
        {
            var instrumentMaster = this.instrumentMasterService.GetInstrumentMaster(id);
            return Json(instrumentMaster);
        }
    }
    
    public class InstrumentMasterViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}