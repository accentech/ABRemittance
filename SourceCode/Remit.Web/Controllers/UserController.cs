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
    public class UserController : Controller
    {
        public readonly IUserService userService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;

        public readonly IRoleService roleService;
        public readonly ISecurityService securityService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:user" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /User/
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
                    return View("User");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public UserController(IUserService userService, IRoleService roleService, ISecurityService securityService, IRoleSubModuleItemService roleSubModuleItemService, IWorkflowactionSettingService workflowactionSettingService)
        {
            this.userService = userService;
            this.roleService = roleService;
            this.securityService = securityService;
            this.workflowactionSettingService = workflowactionSettingService;     
             
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateUser(BusinessUser user)
        {
            const string url = "/User/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = user.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    user.Password = this.securityService.GenerateHashWithSalt(user.Password, user.LoginName);
                    if (this.userService.CreateUser(user))
                    {
                        isSuccess = true;
                        message = "User saved successfully!";
                    }
                    else
                    {
                        message = "User could not saved!";
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
                    if (user.Password != null && user.Password != "")
                        user.Password = this.securityService.GenerateHashWithSalt(user.Password, user.LoginName);
                    if (this.userService.UpdateUser(user))
                    {
                        isSuccess = true;
                        message = "User updated successfully!";
                    }
                    else
                    {
                        message = "User could not updated!";
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

        [HttpPost]
        public JsonResult DeleteUser(BusinessUser user)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/User/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.userService.DeleteUser(user.Id);
                if (isSuccess)
                {
                    message = "User deleted successfully!";
                }
                else
                {
                    message = "User can't be deleted!";
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

        public JsonResult GetUserList()
        {
            var userListObj = this.userService.GetAllUser();
            List<UserModel> userVMList = new List<UserModel>();

            foreach (var user in userListObj)
            {
                UserModel userTemp = new UserModel();
                userTemp.Id = user.Id;
                
                userTemp.LoginName = user.LoginName;
                //userTemp.FullName = user.FullName;
                //userTemp.Email = user.Email;
                userTemp.RoleId = user.RoleId;
                if (user.RoleId != null)
                    userTemp.RoleName = roleService.GetRole(Convert.ToInt32(user.RoleId)).Name;
                if (user.IsActive == true)
                    userTemp.IsActive = true;
                else
                    userTemp.IsActive = false;

                if (user.EmployeeId != null)
                {
                    userTemp.EmployeeId = user.Employee.Id;
                    userTemp.FullName = user.Employee.FullName;
                    userTemp.EmployeeCode = user.Employee.Code;
                }

                if (userTemp.RoleId != 1)
                    userVMList.Add(userTemp);
                else if (UserSession.IsAdmin())
                    userVMList.Add(userTemp);
            }
            return Json(userVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUser(int id)
        {
            var user = this.userService.GetUser(id);
            
            UserModel userTemp = new UserModel();
            userTemp.Id = user.Id;

            userTemp.LoginName = user.LoginName;
            //userTemp.FullName = user.FullName;
            //userTemp.Email = user.Email;
            userTemp.RoleId = user.RoleId;
            if (user.RoleId != null)
                userTemp.RoleName = roleService.GetRole(Convert.ToInt32(user.RoleId)).Name;
            if (user.IsActive == true)
                userTemp.IsActive = true;
            else
                userTemp.IsActive = false;

            if (user.EmployeeId != null)
            {
                userTemp.EmployeeId = user.Employee.Id;
                userTemp.FullName = user.Employee.FullName;
                userTemp.EmployeeCode = user.Employee.Code;
            }
            return Json(userTemp, JsonRequestBehavior.AllowGet);
        }
    }

    class UserModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; }
        public string FullName { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public Nullable<int> RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }

    }
}