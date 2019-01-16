using System.Web.SessionState;
using Remit.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Remit.Service;

namespace Remit.Web.Helpers
{
    public static class UserSession
    {
        public static BusinessUser GetUserFromSession()
        {
            return (BusinessUser)HttpContext.Current.Session["LoggedInUser"];
        }

        public static void SetUserFromSession(BusinessUser user)
        {
            HttpContext.Current.Session["LoggedInUser"] = user;
        }

        public static void SetUserFullNameInSession(string name)
        {
            HttpContext.Current.Session["LoggedInUserFullName"] = name;
        }

        public static string GetUserFullNameFromSession()
        {
            return (string)HttpContext.Current.Session["LoggedInUserFullName"];
        }

        public static void DestroySessionAfterUserLogout()
        {
            HttpContext.Current.Session.Clear();
        }

        public static bool IsAdmin()
        {
            var isAdminRole = false;
            if (UserSession.GetUserFromSession() != null)
            {
                var defaultRoleId = UserSession.GetUserFromSession().RoleId;
                if (defaultRoleId == 1)
                    isAdminRole = true;
            }
            return isAdminRole;
        }

        public static void SetSession(string id)
        {
            HttpContext.Current.Session["LoggedInUser"] = id;
        }

        public static void SetTimeZoneOffset(long offset)
        {
            HttpContext.Current.Session["TimezoneOffset"] = offset;
        }

        public static long GetTimeZoneOffset()
        {
            return (long)HttpContext.Current.Session["TimezoneOffset"];
        }

        public static void SetModuleClicked(string id)
        {
            HttpContext.Current.Session["ModuleId"] = id;
        }

        public static string GetModuleId()
        {
            return (string)HttpContext.Current.Session["ModuleId"];
        }

        public static void SetCurrentUICulture(string value)
        {
            if (value == "zh")
            {
                HttpContext.Current.Session["CurrentUICulture"] = "zh-CN";
            }
            else
            {
                HttpContext.Current.Session["CurrentUICulture"] = "en-US";
            }
        }
        public static string GetCurrentUICulture()
        {
            return (string)HttpContext.Current.Session["CurrentUICulture"];

        }
    }
}