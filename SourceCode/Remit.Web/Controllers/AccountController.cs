using Remit.Model.Models;
using Remit.Service;
using Remit.Web.Helpers;
using System.Linq;
using System.Web.Mvc;

namespace Remit.Web.Controllers
{
    public class AccountController : Controller
    {
        public readonly IUserService userService;
        public readonly ISecurityService securityService;
        public readonly IRoleService roleService;
        public readonly ISubModuleService subModuleService;
        public readonly ISubModuleItemService subModuleItemService;


        //have to remove after file writing test

        public AccountController(IUserService userService, ISecurityService securityService, ISubModuleItemService subModuleItemService,
            IRoleService roleService, ISubModuleService subModuleService)
        {
            this.userService = userService;
            this.securityService = securityService;
            this.roleService = roleService;
            this.subModuleService = subModuleService;
            this.subModuleItemService = subModuleItemService;
        }

        // GET: /Account/
        public ActionResult Index()
        {
            return View("Login");
        }

        public ActionResult ViewChangePassword()
        {
            return View("../Account/ChangePassword");
        }

        public ActionResult Login()
        {
            return View();
        }
        public ActionResult VeiwLoginAfterSessionExpire()
        {
            ViewBag.RedirectMessage = "Login redirected after session timeout";
            return View("Login");
        }

        public ActionResult Logout()
        {
            UserSession.DestroySessionAfterUserLogout();
            return View("Login");
        }
        public ActionResult RecoverPassword()
        {
            return View("PasswordRecovery");
        }

        public ActionResult SwitchRole()
        {
            return View("SwitchRole");
        }

        public void SetTimeZoneOffset(long timeZoneOffset)
        {
            UserSession.SetTimeZoneOffset(timeZoneOffset);
        }

        [HttpPost]
        /*public ActionResult CheckLogin(User user, long timeZoneOffset, string myIp)
        {
            string chk = string.Empty;
            user.Password = this.securityService.GenerateHashWithSalt(user.Password, user.LoginName);

            User aUser = new User();
            aUser = this.userService.AuthenticateUser(user);
            if (aUser != null)
            {
                var urlpath = string.Empty;

                if (aUser.RoleId != null)
                {
                    if (aUser.Role.RoleSubModuleItems.Count() != 0)
                    {

                    }
                    else
                    {
                        aUser.Role.RoleSubModuleItems = null;
                    }
                }
                else
                {
                    aUser.Role = null;
                }

                UserSession.SetUserFromSession(aUser);
                UserSession.SetTimeZoneOffset(timeZoneOffset);
                UserSession.SetUserFullNameInSession(aUser.Employee.FullName);

                return Json(new
                {
                    isSuccess = true,
                    Id = aUser.Id,
                    RoleId = aUser.RoleId,
                    //url = urlpath,
                    chk = chk
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                isSuccess = false,

            }, JsonRequestBehavior.AllowGet);

        }*/

        public ActionResult CheckLogin(BusinessUser user, long timeZoneOffset, string language)
        {
            string chk = string.Empty;
            user.Password = this.securityService.GenerateHashWithSalt(user.Password, user.LoginName);

            BusinessUser aUser = new BusinessUser();
            aUser = this.userService.AuthenticateUser(user);
            if (aUser != null)
            {
                var urlpath = string.Empty;

                if (aUser.RoleId != null)
                {
                    if (aUser.Role.RoleSubModuleItems.Count() != 0)
                    {

                    }
                    else
                    {
                        aUser.Role.RoleSubModuleItems = null;
                    }
                }
                else
                {
                    aUser.Role = null;
                }

                UserSession.SetUserFromSession(aUser);
                UserSession.SetTimeZoneOffset(timeZoneOffset);
                UserSession.SetUserFullNameInSession(aUser.FullName);
                UserSession.SetCurrentUICulture(language);

                return Json(new
                {
                    isSuccess = true,
                    Id = aUser.Id,
                    RoleId = aUser.RoleId,
                    //url = urlpath,
                    chk = chk
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                isSuccess = false,

            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult UpdatePassword(BusinessUser login)
        {
            BusinessUser aUser = new BusinessUser();
            aUser = userService.GetUser(login.Id);
            //Encrypt password
            aUser.Password = this.securityService.GenerateHashWithSalt(login.Password, login.LoginName);
            aUser.PwdTimeStamp = null;
            if (this.userService.UpdateUser(aUser))
            {
                UserSession.DestroySessionAfterUserLogout();
                return Json(new
                {
                    isSuccess = true,
                    UserId = aUser.Id,
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return null;
        }
    }
}