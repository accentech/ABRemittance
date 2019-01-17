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
    public class FGAdjustmentSetupController : Controller
    {
        public readonly IFGTypeService FGTypeService;
        public readonly IFGAdjustmentSetupService FGAdjustmentSetupService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public FGAdjustmentSetupController(IFGTypeService FGTypeService, IFGAdjustmentSetupService FGAdjustmentSetupService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.FGTypeService = FGTypeService;
            this.FGAdjustmentSetupService = FGAdjustmentSetupService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:FGAdjustmentSetup" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /FGAdjustmentSetup/
        public ActionResult Index()
        {
            const string url = "/FGAdjustmentSetup/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGAdjustmentSetup");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult FGAdjustmentSetup()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateFGAdjustmentSetup(FGAdjustmentSetup FGAdjustmentSetup)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = FGAdjustmentSetupService.GetFGAdjustmentSetup(FGAdjustmentSetup.Id);
            const string url = "/FGAdjustmentSetup/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(FGAdjustmentSetup))
                    {
                        if (this.FGAdjustmentSetupService.CreateFGAdjustmentSetup(FGAdjustmentSetup))
                        {
                            isSuccess = true;
                            message = "FG Adjustment Setup saved successfully!";
                        }
                        else
                        {
                            message = "FG Adjustment Setup could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same FG Adjustment Setup name found!";
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
                    isNew.AdjustmnetName = FGAdjustmentSetup.AdjustmnetName;
                    isNew.ApplyMode = FGAdjustmentSetup.ApplyMode;
                    isNew.CalculationOn = FGAdjustmentSetup.CalculationOn;
                    isNew.DefaultValue = FGAdjustmentSetup.DefaultValue;
                    isNew.UOM = FGAdjustmentSetup.UOM;


                    if (this.FGAdjustmentSetupService.UpdateFGAdjustmentSetup(isNew))
                    {
                        isSuccess = true;
                        message = "FG Adjustment Setup updated successfully!";
                    }
                    else
                    {
                        message = "FG Adjustment Setup could not updated!";
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
        private bool CheckIsExist(Model.Models.FGAdjustmentSetup FGAdjustmentSetup)
        {
            return this.FGAdjustmentSetupService.CheckIsExist(FGAdjustmentSetup);
        }



        [HttpPost]
        public JsonResult DeleteFGAdjustmentSetup(FGAdjustmentSetup FGAdjustmentSetup)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/FGAdjustmentSetup/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.FGAdjustmentSetupService.DeleteFGAdjustmentSetup(FGAdjustmentSetup.Id);
                if (isSuccess)
                {
                    message = "FG Adjustment Setup deleted successfully!";

                }
                else
                {
                    message = "FG Adjustment Setup can't be deleted!";
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



        //public JsonResult GetFGAdjustmentSetupListByTypeId(int Id)
        //{
        //    var FGAdjustmentSetupListObj = this.FGAdjustmentSetupService.GetAllFGAdjustmentSetup().Where(p => p.TypeId == Id);
        //    List<FGAdjustmentSetupViewModel> FGAdjustmentSetupVMList = new List<FGAdjustmentSetupViewModel>();

        //    foreach (var FGAdjustmentSetup in FGAdjustmentSetupListObj)
        //    {
        //        FGAdjustmentSetupViewModel FGAdjustmentSetupTemp = new FGAdjustmentSetupViewModel();
        //        FGAdjustmentSetupTemp.Id = FGAdjustmentSetup.Id;

        //        FGAdjustmentSetupTemp.Size = FGAdjustmentSetup.Size;

        //        FGAdjustmentSetupVMList.Add(FGAdjustmentSetupTemp);
        //    }
        //    return Json(FGAdjustmentSetupVMList, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetFGAdjustmentSetupList()
        {
            var FGAdjustmentSetupListObj = this.FGAdjustmentSetupService.GetAllFGAdjustmentSetup();
            List<FGAdjustmentSetupViewModel> FGAdjustmentSetupVMList = new List<FGAdjustmentSetupViewModel>();

            foreach (var FGAdjustmentSetup in FGAdjustmentSetupListObj)
            {
                FGAdjustmentSetupViewModel FGAdjustmentSetupTemp = new FGAdjustmentSetupViewModel();
                FGAdjustmentSetupTemp.Id = FGAdjustmentSetup.Id;

                FGAdjustmentSetupTemp.AdjustmnetName = FGAdjustmentSetup.AdjustmnetName;
                FGAdjustmentSetupTemp.ApplyMode = FGAdjustmentSetup.ApplyMode;
                FGAdjustmentSetupTemp.CalculationOn = FGAdjustmentSetup.CalculationOn;
                FGAdjustmentSetupTemp.DefaultValue = FGAdjustmentSetup.DefaultValue;
                FGAdjustmentSetupTemp.UOM = FGAdjustmentSetup.UOM;

                FGAdjustmentSetupVMList.Add(FGAdjustmentSetupTemp);
            }
            return Json(FGAdjustmentSetupVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGAdjustmentSetup(int id)
        {
            var FGAdjustmentSetup = this.FGAdjustmentSetupService.GetFGAdjustmentSetup(id);
            FGAdjustmentSetupViewModel FGAdjustmentSetupTemp = new FGAdjustmentSetupViewModel();
            FGAdjustmentSetupTemp.Id = FGAdjustmentSetup.Id;

            FGAdjustmentSetupTemp.AdjustmnetName = FGAdjustmentSetup.AdjustmnetName;
            FGAdjustmentSetupTemp.ApplyMode = FGAdjustmentSetup.ApplyMode;
            FGAdjustmentSetupTemp.CalculationOn = FGAdjustmentSetup.CalculationOn;
            FGAdjustmentSetupTemp.DefaultValue = FGAdjustmentSetup.DefaultValue;
            FGAdjustmentSetupTemp.UOM = FGAdjustmentSetup.UOM;

            return Json(FGAdjustmentSetupTemp, JsonRequestBehavior.AllowGet);
        }
    }

    public class FGAdjustmentSetupViewModel
    {
        public int Id { get; set; }
        public string AdjustmnetName { get; set; }
        public string ApplyMode { get; set; }
        public string CalculationOn { get; set; }
        public double? DefaultValue { get; set; }
        public int? UOM { get; set; }
        public string UnitName { get; set; }
        public virtual FGUOM FGUOM { get; set; }


    }
}