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
    public class EmploymentHistoryController : Controller
    {
        public readonly IEmploymentHistoryService employmentHistoryService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public EmploymentHistoryController(IEmploymentHistoryService employmentHistoryService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.employmentHistoryService = employmentHistoryService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:employmentHistory" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        
        // GET: /EmploymentHistory/
        public ActionResult Index()
        {
            const string url = "/EmploymentHistory/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId) ;

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("EmploymentHistory");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            
            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult EmploymentHistory()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateEmploymentHistory(EmploymentHistory employmentHistory)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = employmentHistory.Id == 0 ? true : false;
            const string url = "/EmploymentHistory/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {

                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(employmentHistory))
                    {

                        if (this.employmentHistoryService.CreateEmploymentHistory(employmentHistory))
                        {
                            isSuccess = true;
                            message = "EmploymentHistory saved successfully!";
                        }
                        else
                        {
                            message = "EmploymentHistory could not saved!";
                        }


                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same employmentHistory name found!";
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
                    if (this.employmentHistoryService.UpdateEmploymentHistory(employmentHistory))
                    {
                        isSuccess = true;
                        message = "EmploymentHistory updated successfully!";
                    }
                    else
                    {
                        message = "EmploymentHistory could not updated!";
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
        private bool CheckIsExist(Model.Models.EmploymentHistory employmentHistory)
        {
            return this.employmentHistoryService.CheckIsExist(employmentHistory);
        }
        [HttpPost]
        public JsonResult DeleteEmploymentHistory(EmploymentHistory employmentHistory)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/EmploymentHistory/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permission.DeleteOperation == true)
            {
                isSuccess = this.employmentHistoryService.DeleteEmploymentHistory(employmentHistory.Id);
                if (isSuccess)
                {
                    message = "EmploymentHistory deleted successfully!";

                }
                else
                {
                    message = "EmploymentHistory can't be deleted!";
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

        public JsonResult GetEmploymentHistoryList()
        {
            var employmentHistoryListObj = this.employmentHistoryService.GetAllEmploymentHistory();
            List<EmploymentHistoryViewModel> employmentHistoryVMList = new List<EmploymentHistoryViewModel>();

            foreach (var employmentHistory in employmentHistoryListObj)
            {
                EmploymentHistoryViewModel employmentHistoryTemp = new EmploymentHistoryViewModel();
                employmentHistoryTemp.Id = employmentHistory.Id;
                //employmentHistoryTemp.Name = employmentHistory.Name;
                //employmentHistoryTemp.Code = employmentHistory.Code;

                employmentHistoryVMList.Add(employmentHistoryTemp);
            }
            return Json(employmentHistoryVMList, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetEmployeeListByDesignationId(int id) //id is designationId
        {
            var employeeListObj = this.employmentHistoryService.GetAllEmploymentHistory().Where(a=>a.DesignationId==id).ToList();
            List<EmploymentHistoryViewModel> employmentHistoryVMList = new List<EmploymentHistoryViewModel>();

            if (employeeListObj != null)
            {
               
                foreach (var employee in employeeListObj)
                {
                    EmploymentHistoryViewModel employmentHistoryTemp = new EmploymentHistoryViewModel();
                    employmentHistoryTemp.Id = employee.Id;
                    employmentHistoryTemp.EmployeeId = employee.EmployeeId;

                    if (employee.Employee != null)
                        employmentHistoryTemp.EmployeeName = employee.Employee.FullName;

                    employmentHistoryVMList.Add(employmentHistoryTemp);
                }
            }
            return Json(employmentHistoryVMList, JsonRequestBehavior.AllowGet);
        }





        public JsonResult GetEmploymentHistory(int id)
        {
            var employmentHistory = this.employmentHistoryService.GetEmploymentHistory(id);
            return Json(employmentHistory);
        }
    }

    public class EmploymentHistoryViewModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int? DepartmentId { get; set; }
        public int? DesignationId { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }

        public string DesignationName { get; set; }
    }
}