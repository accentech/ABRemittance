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
    public class NotificationSettingController : Controller
    {
        public readonly INotificationSettingService notificationSettingService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public NotificationSettingController(INotificationSettingService notificationSettingService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.notificationSettingService = notificationSettingService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:notificationSetting" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /NotificationSetting/
        public ActionResult Index()
        {
            const string url = "/NotificationSetting/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("NotificationSetting");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateNotificationSettingList(List<NotificationSetting> notificationSettingList, int subModuleItemId, int workflowactionId)
        {
            var isSuccess = false;
            var message = string.Empty;
            const string url = "/NotificationSetting/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.CreateOperation == true)
            {
                if (this.notificationSettingService.DeleteAllNotificationSettingBySubModuleId(subModuleItemId, workflowactionId))
                {
                    if (notificationSettingList != null)
                    {
                        foreach (var notificationSetting in notificationSettingList)
                        {
                            notificationSetting.Id = Guid.NewGuid();
                            if (this.notificationSettingService.CreateNotificationSetting(notificationSetting))
                            {
                                isSuccess = true;
                            }
                        }
                    }
                    message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceNotificationSetting.LblNotificationSetting);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceNotificationSetting.LblNotificationSetting);
                }
            }
            else
            {//TODO
                message = "";//Resources.ResourceCommon.MgsNoPermissionToCreate;
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult DeleteNotificationSetting(NotificationSetting notificationSetting)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/NotificationSetting/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.notificationSettingService.DeleteNotificationSetting(notificationSetting.Id);
                if (isSuccess)
                {
                    message = "NotificationSetting deleted successfully!";

                }
                else
                {
                    message = "NotificationSetting can't be deleted!";
                }
            }
            else
            {//TODO
                message = "";// Resources.ResourceCommon.MgsNoPermissionToDelete;
            }


            return Json(new
            {
                isSuccess = isSuccess,
                message = message
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNotificationSettingList()
        {
            var notificationSettingListObj = this.notificationSettingService.GetAllNotificationSetting();
            List<NotificationSettingViewModel> notificationSettingVMList = new List<NotificationSettingViewModel>();

            foreach (var notificationSetting in notificationSettingListObj)
            {
                NotificationSettingViewModel notificationSettingTemp = new NotificationSettingViewModel();
                notificationSettingTemp.Id = notificationSetting.Id;
                notificationSettingTemp.SubModuleItemId = notificationSetting.SubModuleItemId;
                notificationSettingTemp.NotifiedEmployeeId = notificationSetting.NotifiedEmployeeId;

                if (notificationSetting.NotifiedEmployeeId != null)
                    notificationSettingTemp.NotifiedEmployeeName = notificationSetting.Employee.FullName;

                if (notificationSetting.SubModuleItemId != null)
                    notificationSettingTemp.SubModuleItemName = notificationSetting.SubModuleItem.Name;

                notificationSettingVMList.Add(notificationSettingTemp);
            }
            return Json(notificationSettingVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNotificationSetting(Guid id)
        {
            var notificationSetting = this.notificationSettingService.GetNotificationSetting(id);
            return Json(notificationSetting);
        }
    }

    public class NotificationSettingViewModel
    {
        public System.Guid Id { get; set; }
        public Nullable<int> SubModuleItemId { get; set; }
        public Nullable<int> NotifiedEmployeeId { get; set; }
        public string NotifiedEmployeeName { get; set; }
        public string SubModuleItemName { get; set; }

    }
}