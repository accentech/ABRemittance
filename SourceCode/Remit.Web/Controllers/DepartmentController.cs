using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.ClientModel;

namespace Remit.Web.Controllers
{
    public class DepartmentController : Controller
    {
        public readonly IDepartmentService departmentService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public DepartmentController(IDepartmentService departmentService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.departmentService = departmentService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:department" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        
        // GET: /Department/
        public ActionResult Index()
        {
            const string url = "/Department/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId) ;

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Department");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            
            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult Department()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateDepartment(Department department)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = department.Id == 0 ? true : false;
            const string url = "/Department/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {

                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(department))
                    {

                        if (this.departmentService.CreateDepartment(department))
                        {
                            isSuccess = true;
                            message = "Department saved successfully!";
                        }
                        else
                        {
                            message = "Department could not saved!";
                        }


                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same department name found!";
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
                    if (this.departmentService.UpdateDepartment(department))
                    {
                        isSuccess = true;
                        message = "Department updated successfully!";
                    }
                    else
                    {
                        message = "Department could not updated!";
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
        private bool CheckIsExist(Model.Models.Department department)
        {
            return this.departmentService.CheckIsExist(department);
        }
        [HttpPost]
        public JsonResult DeleteDepartment(Department department)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Department/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permission.DeleteOperation == true)
            {
                isSuccess = this.departmentService.DeleteDepartment(department.Id);
                if (isSuccess)
                {
                    message = "Department deleted successfully!";

                }
                else
                {
                    message = "Department can't be deleted!";
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

        public JsonResult GetDepartmentList()
        {
            var departmentListObj = this.departmentService.GetAllDepartment();
            List<DepartmentModel> departmentVMList = new List<DepartmentModel>();

            foreach (var department in departmentListObj)
            {
                DepartmentModel departmentTemp = new DepartmentModel();
                departmentTemp.Id = department.Id;
                departmentTemp.Name = department.Name;
                departmentTemp.Code = department.Code;
                departmentTemp.Information = department.Information;
                departmentTemp.CodeName = department.Code + " (" + department.Name + ")";

                departmentVMList.Add(departmentTemp);
            }
            return Json(departmentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDepartmentListOperation()
        {
            var departmentListObj = this.departmentService.GetAllDepartmentByConfig();
            List<DepartmentModel> departmentVMList = new List<DepartmentModel>();

            foreach (var department in departmentListObj)
            {
                DepartmentModel departmentTemp = new DepartmentModel();
                departmentTemp.Id = department.Id;
                departmentTemp.Name = department.Name;
                departmentTemp.Code = department.Code;
                departmentTemp.Information = department.Information;
                departmentTemp.CodeName = department.Code + " (" + department.Name + ")";

                departmentVMList.Add(departmentTemp);
            }
            return Json(departmentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDepartment(int id)
        {
            var department = this.departmentService.GetDepartment(id);
            return Json(department);
        }
    }

    
}