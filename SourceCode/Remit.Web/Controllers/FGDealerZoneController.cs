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
    public class FGDealerZoneController : Controller
    {
        public readonly IFGDealerZoneService FGDealerZoneService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public FGDealerZoneController(IFGDealerZoneService FGDealerZoneService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.FGDealerZoneService = FGDealerZoneService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:FGDealerZone" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /FGDealerZone/
        public ActionResult Index()
        {
            const string url = "/FGDealerZone/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGDealerZone");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult FGDealerZone()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateFGDealerZone(FGDealerZone FGDealerZone)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = FGDealerZoneService.GetFGDealerZone(FGDealerZone.Id);
            const string url = "/FGDealerZone/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(FGDealerZone))
                    {
                        if (this.FGDealerZoneService.CreateFGDealerZone(FGDealerZone))
                        {
                            isSuccess = true;
                            message = "District saved successfully!";
                        }
                        else
                        {
                            message = "District could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same District name found!";
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
                    isNew.ZoneName = FGDealerZone.ZoneName;
                    isNew.DivisionId = FGDealerZone.DivisionId;

                    if (this.FGDealerZoneService.UpdateFGDealerZone(isNew))
                    {
                        isSuccess = true;
                        message = "District updated successfully!";
                    }
                    else
                    {
                        message = "District could not updated!";
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
        private bool CheckIsExist(Model.Models.FGDealerZone FGDealerZone)
        {
            return this.FGDealerZoneService.CheckIsExist(FGDealerZone);
        }
        [HttpPost]
        public JsonResult DeleteFGDealerZone(FGDealerZone FGDealerZone)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/FGDealerZone/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.FGDealerZoneService.DeleteFGDealerZone(FGDealerZone.Id);
                if (isSuccess)
                {
                    message = "District deleted successfully!";

                }
                else
                {
                    message = "District can't be deleted!";
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

        public JsonResult GetFGDealerZoneListWithoutOther()
        {
            var FGDealerZoneListObj = this.FGDealerZoneService.GetAllFGDealerZone().Where(deal => deal.Id != -1);
            List<FGDealerZoneViewModel> FGDealerZoneVMList = new List<FGDealerZoneViewModel>();

            foreach (var FGDealerZone in FGDealerZoneListObj)
            {
                FGDealerZoneViewModel FGDealerZoneTemp = new FGDealerZoneViewModel();
                FGDealerZoneTemp.Id = FGDealerZone.Id;
                FGDealerZoneTemp.ZoneName = FGDealerZone.ZoneName;
                FGDealerZoneTemp.DivisionId = FGDealerZone.DivisionId;

                FGDealerZoneVMList.Add(FGDealerZoneTemp);
            }
            return Json(FGDealerZoneVMList, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetFGDealerZoneList()
        {
            var FGDealerZoneListObj = this.FGDealerZoneService.GetAllFGDealerZone();
            List<FGDealerZoneViewModel> FGDealerZoneVMList = new List<FGDealerZoneViewModel>();

            foreach (var FGDealerZone in FGDealerZoneListObj)
            {
                FGDealerZoneViewModel FGDealerZoneTemp = new FGDealerZoneViewModel();
                FGDealerZoneTemp.Id = FGDealerZone.Id;
                FGDealerZoneTemp.ZoneName = FGDealerZone.ZoneName;
                FGDealerZoneTemp.DivisionId = FGDealerZone.DivisionId;

                FGDealerZoneVMList.Add(FGDealerZoneTemp);
            }
            return Json(FGDealerZoneVMList.OrderBy(ss=> ss.ZoneName), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGDealerZone(int id)
        {
            var FGDealerZone = this.FGDealerZoneService.GetFGDealerZone(id);
            FGDealerZoneViewModel FGDealerZoneTemp = new FGDealerZoneViewModel();
            FGDealerZoneTemp.Id = FGDealerZone.Id;
            FGDealerZoneTemp.ZoneName = FGDealerZone.ZoneName;
            FGDealerZoneTemp.DivisionId = FGDealerZone.DivisionId;
            return Json(FGDealerZoneTemp, JsonRequestBehavior.AllowGet);
        }





    }

    public class FGDealerZoneViewModel
    {
        public int Id { get; set; }
        public string ZoneName { get; set; }
        public Nullable<int> DivisionId { get; set; }
        public virtual Division Division { get; set; }
    }
}