using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace Remit.Web.Controllers
{
    public class DivisionController : Controller
    {
        public readonly IDivisionService divisionService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public DivisionController(IDivisionService divisionService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.divisionService = divisionService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:division" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /Division/
        public ActionResult Index()
        {
            const string url = "/Division/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Division");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ActionResult Division()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateDivision(Division division)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = divisionService.GetDivision(division.Id);
            const string url = "/Division/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(division))
                    {
                        if (this.divisionService.CreateDivision(division))
                        {
                            isSuccess = true;
                            message = "Division saved successfully!";
                        }
                        else
                        {
                            message = "Division could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same division name found!";
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
                    isNew.Name = division.Name;
                   
                    if (this.divisionService.UpdateDivision(isNew))
                    {
                        isSuccess = true;
                        message = "Division updated successfully!";
                    }
                    else
                    {
                        message = "Division could not updated!";
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
        private bool CheckIsExist(Model.Models.Division division)
        {
            return this.divisionService.CheckIsExist(division);
        }
        [HttpPost]
        public JsonResult DeleteDivision(Division division)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Division/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.divisionService.DeleteDivision(division.Id);
                if (isSuccess)
                {
                    message = "Division deleted successfully!";

                }
                else
                {
                    message = "Division can't be deleted!";
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

        public JsonResult GetDivisionList()
        {
            var divisionListObj = this.divisionService.GetAllDivision();
            List<DivisionViewModel> divisionVMList = new List<DivisionViewModel>();

            foreach (var division in divisionListObj)
            {
                DivisionViewModel divisionTemp = new DivisionViewModel();
                divisionTemp.Id = division.Id;
                divisionTemp.Name = division.Name;
                
                divisionVMList.Add(divisionTemp);
            }
            return Json(divisionVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDivision(int id)
        {
            var division = this.divisionService.GetDivision(id);
            return Json(division);
        }
    }

    public class DivisionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}