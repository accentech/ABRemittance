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
    public class RoleController : Controller
    {
        public readonly IRoleService roleService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:role" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /Role/
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
                    cacheProvider.Set("permission:role" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("Role");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public RoleController(IRoleService roleService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.roleService = roleService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateRole(Role role)
        {
            const string url = "/Role/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = role.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(role))
                    {

                        if (this.roleService.CreateRole(role))
                        {
                            isSuccess = true;
                            message = "Role saved successfully!";
                        }
                        else
                        {
                            message = "Role could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same role name found!";
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
                    if (!CheckIsExist(role))
                    {
                        if (this.roleService.UpdateRole(role))
                        {
                            isSuccess = true;
                            message = "Role updated successfully!";
                        }
                        else
                        {
                            message = "Role could not updated!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same role name found!";
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
        private bool CheckIsExist(Model.Models.Role role)
        {
            return this.roleService.CheckIsExist(role);
        }

        [HttpPost]
        public JsonResult DeleteRole(Role role)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Role/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.roleService.DeleteRole(role.Id);
                if (isSuccess)
                {
                    message = "Role deleted successfully!";

                }
                else
                {
                    message = "Role can't be deleted!";
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

        public JsonResult GetRoleList()
        {
            var roleListObj = this.roleService.GetAllRole();
            List<RoleViewModel> roleVMList = new List<RoleViewModel>();

            foreach (var role in roleListObj)
            {
                RoleViewModel roleTemp = new RoleViewModel();
                roleTemp.Id = role.Id;
                roleTemp.Name = role.Name;

                if (roleTemp.Id != 1)
                    roleVMList.Add(roleTemp);
                else if (UserSession.IsAdmin())
                    roleVMList.Add(roleTemp);
            }
            return Json(roleVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRole(int id)
        {
            var role = this.roleService.GetRole(id);
            return Json(role);
        }
    }

    public class RoleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}