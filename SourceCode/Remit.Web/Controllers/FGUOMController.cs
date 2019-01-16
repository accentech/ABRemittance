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
    public class FGUOMController : Controller
    {
        public readonly IFGUOMService FGUOMService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public FGUOMController(IFGUOMService FGUOMService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.FGUOMService = FGUOMService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:FGUOM" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /FGUOM/
        public ActionResult Index()
        {
            const string url = "/FGUOM/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGUOM");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult FGUOM()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateFGUOM(FGUOM FGUOM)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = FGUOMService.GetFGUOM(FGUOM.Id);
            const string url = "/FGUOM/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(FGUOM))
                    {
                        if (this.FGUOMService.CreateFGUOM(FGUOM))
                        {
                            isSuccess = true;
                            message = "Unit of Measurement saved successfully!";
                        }
                        else
                        {
                            message = "Unit of Measurement could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same Unit of Measurement name found!";
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
                    isNew.UnitName = FGUOM.UnitName;

                    if (this.FGUOMService.UpdateFGUOM(isNew))
                    {
                        isSuccess = true;
                        message = "Unit of Measurement updated successfully!";
                    }
                    else
                    {
                        message = "Unit of Measurement could not updated!";
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
        private bool CheckIsExist(Model.Models.FGUOM FGUOM)
        {
            return this.FGUOMService.CheckIsExist(FGUOM);
        }
        [HttpPost]
        public JsonResult DeleteFGUOM(FGUOM FGUOM)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/FGUOM/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.FGUOMService.DeleteFGUOM(FGUOM.Id);
                if (isSuccess)
                {
                    message = "Unit of Measurement deleted successfully!";

                }
                else
                {
                    message = "Unit of Measurement can't be deleted!";
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

        public JsonResult GetFGUOMList()
        {
            var FGUOMListObj = this.FGUOMService.GetAllFGUOM();
            List<FGUOMViewModel> FGUOMVMList = new List<FGUOMViewModel>();

            foreach (var FGUOM in FGUOMListObj)
            {
                FGUOMViewModel FGUOMTemp = new FGUOMViewModel();
                FGUOMTemp.Id = FGUOM.Id;
                FGUOMTemp.UnitName = FGUOM.UnitName;

                FGUOMVMList.Add(FGUOMTemp);
            }
            return Json(FGUOMVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGUOM(int id)
        {
            var FGUOM = this.FGUOMService.GetFGUOM(id);
            FGUOMViewModel FGUOMTemp = new FGUOMViewModel();
            FGUOMTemp.Id = FGUOM.Id;
            FGUOMTemp.UnitName = FGUOM.UnitName;
            return Json(FGUOMTemp, JsonRequestBehavior.AllowGet);
        }

       



    }

    public class FGUOMViewModel
    {
        public int Id { get; set; }
        public string UnitName { get; set; }


    }
}