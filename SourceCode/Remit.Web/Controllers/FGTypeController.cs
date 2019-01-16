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
    public class FGTypeController : Controller
    {
        public readonly IFGTypeService FGTypeService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public FGTypeController(IFGTypeService FGTypeService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.FGTypeService = FGTypeService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:FGType" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /FGType/
        public ActionResult Index()
        {
            const string url = "/FGType/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGType");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult FGType()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateFGType(FGType FGType)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = FGTypeService.GetFGType(FGType.Id);
            const string url = "/FGType/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(FGType))
                    {
                        if (this.FGTypeService.CreateFGType(FGType))
                        {
                            isSuccess = true;
                            message = "Product Type saved successfully!";
                        }
                        else
                        {
                            message = "Product Type could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same Product Type found!";
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
                    isNew.TypeName = FGType.TypeName;

                    if (this.FGTypeService.UpdateFGType(isNew))
                    {
                        isSuccess = true;
                        message = "Product Type updated successfully!";
                    }
                    else
                    {
                        message = "Product Type could not updated!";
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
        private bool CheckIsExist(Model.Models.FGType FGType)
        {
            return this.FGTypeService.CheckIsExist(FGType);
        }
        [HttpPost]
        public JsonResult DeleteFGType(FGType FGType)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/FGType/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.FGTypeService.DeleteFGType(FGType.Id);
                if (isSuccess)
                {
                    message = "Product Type deleted successfully!";

                }
                else
                {
                    message = "Product Type can't be deleted!";
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

        public JsonResult GetFGTypeList()
        {
            var FGTypeListObj = this.FGTypeService.GetAllFGType();
            List<FGTypeViewModel> FGTypeVMList = new List<FGTypeViewModel>();

            foreach (var FGType in FGTypeListObj)
            {
                FGTypeViewModel FGTypeTemp = new FGTypeViewModel();
                FGTypeTemp.Id = FGType.Id;
                FGTypeTemp.TypeName = FGType.TypeName;

                FGTypeVMList.Add(FGTypeTemp);
            }
            return Json(FGTypeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGType(int id)
        {
            var FGType = this.FGTypeService.GetFGType(id);
            FGTypeViewModel FGTypeTemp = new FGTypeViewModel();
            FGTypeTemp.Id = FGType.Id;
            FGTypeTemp.TypeName = FGType.TypeName;
            return Json(FGTypeTemp, JsonRequestBehavior.AllowGet);
        }





    }

    public class FGTypeViewModel
    {
        public int Id { get; set; }
        public string TypeName { get; set; }


    }
}