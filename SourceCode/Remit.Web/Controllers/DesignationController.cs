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
using Remit.ClientModel;

namespace Remit.Web.Controllers
{
    public class DesignationController : Controller
    {
        public readonly IDepartmentService departmentService;
        public readonly IDesignationService designationService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:designation" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /Designation/
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
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Designation");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public DesignationController(IDesignationService designationService, IDepartmentService departmentService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.designationService = designationService;
            this.departmentService = departmentService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateDesignation(Designation designation)
        {
            const string url = "/Designation/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = designation.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(designation))
                    {
                        if (this.designationService.CreateDesignation(designation))
                        {
                            isSuccess = true;
                            message = "Designation saved successfully!";
                        }
                        else
                        {
                            message = "Designation could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same designation name found!";
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
                    if (this.designationService.UpdateDesignation(designation))
                    {
                        isSuccess = true;
                        message = "Designation updated successfully!";
                    }
                    else
                    {
                        message = "Designation could not updated!";
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
        private bool CheckIsExist(Model.Models.Designation designation)
        {
            return this.designationService.CheckIsExist(designation);
        }

        [HttpPost]
        public JsonResult DeleteDesignation(Designation designation)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.designationService.DeleteDesignation(designation.Id);
                if (isSuccess)
                {
                    message = "Designation deleted successfully!";
                }
                else
                {
                    message = "Designation can't be deleted!";
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

        public JsonResult GetDesignationList()
        {
            var designationListObj = this.designationService.GetAllDesignation();
            List<DesignationModel> designationVMList = new List<DesignationModel>();

            foreach (var designation in designationListObj)
            {
                DesignationModel designationTemp = new DesignationModel();
                designationTemp.Id = designation.Id;
                designationTemp.Name = designation.Name;
                designationTemp.DepartmentName = departmentService.GetDepartment(Convert.ToInt32(designation.DepartmentId)).Name;
                designationTemp.DepartmentId = designation.DepartmentId;

                designationVMList.Add(designationTemp);
            }
            return Json(designationVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDesignationListByConfig()
        {
            var designationListObj = this.designationService.GetAllDesignationByConfig();
            List<DesignationModel> designationVMList = new List<DesignationModel>();

            foreach (var designation in designationListObj)
            {
                DesignationModel designationTemp = new DesignationModel();
                designationTemp.Id = designation.Id;
                designationTemp.Name = designation.Name;
                designationTemp.DepartmentName = departmentService.GetDepartment(Convert.ToInt32(designation.DepartmentId)).Name;
                designationTemp.DepartmentId = designation.DepartmentId;

                designationVMList.Add(designationTemp);
            }
            return Json(designationVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDesignationListByDepartmentId(int id) //id is departmentId
        {
            var designationListObj = this.designationService.GetAllDesignationByDepartmentId(id);
            List<DesignationModel> designationVMList = null;

            if (designationListObj != null)
            {
                designationVMList = new List<DesignationModel>();
                foreach (var designation in designationListObj)
                {
                    DesignationModel designationTemp = new DesignationModel();
                    designationTemp.Id = designation.Id;
                    designationTemp.Name = designation.Name;
                    designationTemp.DepartmentId = designation.DepartmentId;
                    if (designation.Department != null)
                        designationTemp.DepartmentName = designation.Department.Name;

                    designationVMList.Add(designationTemp);
                }
            }
            return Json(designationVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDesignation(int id)
        {
            var designation = this.designationService.GetDesignation(id);
            return Json(designation);
        }
    }

   

}