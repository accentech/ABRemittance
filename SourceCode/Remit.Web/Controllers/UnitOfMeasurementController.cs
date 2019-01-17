using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;


namespace Remit.Web.Controllers
{
    public class UnitOfMeasurementController : Controller
    {
        public readonly IUnitOfMeasurementService unitOfMeasurementService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public UnitOfMeasurementController(IUnitOfMeasurementService unitOfMeasurementService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.unitOfMeasurementService = unitOfMeasurementService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:unitOfMeasurement" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /UnitOfMeasurement/
        public ActionResult Index()
        {
            const string url = "/UnitOfMeasurement/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:unitOfMeasurement" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("UnitOfMeasurement");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult UnitOfMeasurement()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateUnitOfMeasurement(UnitOfMeasurement unitOfMeasurement)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = unitOfMeasurement.Id == 0 ? true : false;
            const string url = "/UnitOfMeasurement/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(unitOfMeasurement))
                    {
                        if (this.unitOfMeasurementService.CreateUnitOfMeasurement(unitOfMeasurement))
                        {
                            isSuccess = true;
                            message = "UnitOfMeasurement saved successfully!";
                        }
                        else
                        {
                            message = "UnitOfMeasurement could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same unitOfMeasurement name found!";
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
                    var UnitOfMeasurementObj = this.unitOfMeasurementService.GetUnitOfMeasurement(unitOfMeasurement.Id);

                    UnitOfMeasurementObj.Name = unitOfMeasurement.Name;

                    if (this.unitOfMeasurementService.UpdateUnitOfMeasurement(UnitOfMeasurementObj))
                    {
                        isSuccess = true;
                        message = "UnitOfMeasurement updated successfully!";
                    }
                    else
                    {
                        message = "UnitOfMeasurement could not updated!";
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
        private bool CheckIsExist(Model.Models.UnitOfMeasurement unitOfMeasurement)
        {
            return this.unitOfMeasurementService.CheckIsExist(unitOfMeasurement);
        }
        [HttpPost]
        public JsonResult DeleteUnitOfMeasurement(UnitOfMeasurement unitOfMeasurement)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/UnitOfMeasurement/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.unitOfMeasurementService.DeleteUnitOfMeasurement(unitOfMeasurement.Id);
                if (isSuccess)
                {
                    message = "UnitOfMeasurement deleted successfully!";

                }
                else
                {
                    message = "UnitOfMeasurement can't be deleted!";
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

        public JsonResult GetUnitOfMeasurementList()
        {
            var unitOfMeasurementListObj = this.unitOfMeasurementService.GetAllUnitOfMeasurement();
            List<UnitOfMeasurementViewModel> unitOfMeasurementVMList = new List<UnitOfMeasurementViewModel>();

            foreach (var unitOfMeasurement in unitOfMeasurementListObj)
            {
                UnitOfMeasurementViewModel unitOfMeasurementTemp = new UnitOfMeasurementViewModel();
                unitOfMeasurementTemp.Id = unitOfMeasurement.Id;
                unitOfMeasurementTemp.Name = unitOfMeasurement.Name;

                unitOfMeasurementVMList.Add(unitOfMeasurementTemp);
            }
            return Json(unitOfMeasurementVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnitOfMeasurement(int id)
        {
            var unitOfMeasurement = this.unitOfMeasurementService.GetUnitOfMeasurement(id);
            return Json(unitOfMeasurement);
        }
    }

    public class UnitOfMeasurementViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}