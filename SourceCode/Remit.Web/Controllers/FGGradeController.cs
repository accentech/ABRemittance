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
    public class FGGradeController : Controller
    {
        public readonly IFGGradeService FGGradeService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public FGGradeController(IFGGradeService FGGradeService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.FGGradeService = FGGradeService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:FGGrade" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /FGGrade/
        public ActionResult Index()
        {
            const string url = "/FGGrade/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGGrade");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult FGGrade()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateFGGrade(FGGrade FGGrade)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = FGGradeService.GetFGGrade(FGGrade.Id);
            const string url = "/FGGrade/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(FGGrade))
                    {
                        if (this.FGGradeService.CreateFGGrade(FGGrade))
                        {
                            isSuccess = true;
                            message = "Grade saved successfully!";
                        }
                        else
                        {
                            message = "Grade could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same Grade name found!";
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
                    isNew.Grade = FGGrade.Grade;

                    if (this.FGGradeService.UpdateFGGrade(isNew))
                    {
                        isSuccess = true;
                        message = "Grade updated successfully!";
                    }
                    else
                    {
                        message = "Grade could not updated!";
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
        private bool CheckIsExist(Model.Models.FGGrade FGGrade)
        {
            return this.FGGradeService.CheckIsExist(FGGrade);
        }
        [HttpPost]
        public JsonResult DeleteFGGrade(FGGrade FGGrade)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/FGGrade/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.FGGradeService.DeleteFGGrade(FGGrade.Id);
                if (isSuccess)
                {
                    message = "Grade deleted successfully!";

                }
                else
                {
                    message = "Grade can't be deleted!";
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

        public JsonResult GetFGGradeList()
        {
            var FGGradeListObj = this.FGGradeService.GetAllFGGrade();
            List<FGGradeViewModel> FGGradeVMList = new List<FGGradeViewModel>();

            foreach (var FGGrade in FGGradeListObj)
            {
                FGGradeViewModel FGGradeTemp = new FGGradeViewModel();
                FGGradeTemp.Id = FGGrade.Id;
                FGGradeTemp.Grade = FGGrade.Grade;

                FGGradeVMList.Add(FGGradeTemp);
            }
            return Json(FGGradeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGGrade(int id)
        {
            var FGGrade = this.FGGradeService.GetFGGrade(id);
            FGGradeViewModel FGGradeTemp = new FGGradeViewModel();
            FGGradeTemp.Id = FGGrade.Id;
            FGGradeTemp.Grade = FGGrade.Grade;
            return Json(FGGradeTemp, JsonRequestBehavior.AllowGet);
        }





    }

    public class FGGradeViewModel
    {
        public int Id { get; set; }
        public string Grade { get; set; }


    }
}