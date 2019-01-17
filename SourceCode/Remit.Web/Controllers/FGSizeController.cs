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
    public class FGSizeController : Controller
    {
        public readonly IFGTypeService FGTypeService;
        public readonly IFGSizeService FGSizeService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public FGSizeController(IFGTypeService FGTypeService,IFGSizeService FGSizeService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.FGTypeService = FGTypeService;
            this.FGSizeService = FGSizeService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:FGSize" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /FGSize/
        public ActionResult Index()
        {
            const string url = "/FGSize/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGSize");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult FGSize()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateFGSize(FGSize FGSize)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = FGSizeService.GetFGSize(FGSize.Id);
            const string url = "/FGSize/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(FGSize))
                    {
                        if (this.FGSizeService.CreateFGSize(FGSize))
                        {
                            isSuccess = true;
                            message = "Product Size saved successfully!";
                        }
                        else
                        {
                            message = "Product Size could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same Product Size name found!";
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
                    isNew.TypeId = FGSize.TypeId;
                    isNew.Size = FGSize.Size;

                    if (this.FGSizeService.UpdateFGSize(isNew))
                    {
                        isSuccess = true;
                        message = "Product Size updated successfully!";
                    }
                    else
                    {
                        message = "Prpduct Size could not updated!";
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
        private bool CheckIsExist(Model.Models.FGSize FGSize)
        {
            return this.FGSizeService.CheckIsExist(FGSize);
        }
        [HttpPost]
        public JsonResult DeleteFGSize(FGSize FGSize)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/FGSize/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.FGSizeService.DeleteFGSize(FGSize.Id);
                if (isSuccess)
                {
                    message = "Prpduct Size deleted successfully!";

                }
                else
                {
                    message = "Prpduct Size can't be deleted!";
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



        public JsonResult GetFGSizeListByTypeId(int Id)
        {
            var FGSizeListObj = this.FGSizeService.GetAllFGSize().Where(p=>p.TypeId==Id);
            List<FGSizeViewModel> FGSizeVMList = new List<FGSizeViewModel>();

            foreach (var FGSize in FGSizeListObj)
            {
                FGSizeViewModel FGSizeTemp = new FGSizeViewModel();
                FGSizeTemp.Id = FGSize.Id;
                
                FGSizeTemp.Size = FGSize.Size;

                FGSizeVMList.Add(FGSizeTemp);
            }
            return Json(FGSizeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGSizeList()
        {
            var FGSizeListObj = this.FGSizeService.GetAllFGSize();
            List<FGSizeViewModel> FGSizeVMList = new List<FGSizeViewModel>();

            foreach (var FGSize in FGSizeListObj)
            {
                FGSizeViewModel FGSizeTemp = new FGSizeViewModel();
                FGSizeTemp.Id = FGSize.Id;
                FGSizeTemp.TypeId = FGSize.TypeId;
                FGSizeTemp.TypeName = FGSize.FGType.TypeName;
                FGSizeTemp.Size = FGSize.Size;

                FGSizeVMList.Add(FGSizeTemp);
            }
            return Json(FGSizeVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGSize(int id)
        {
            var FGSize = this.FGSizeService.GetFGSize(id);
            FGSizeViewModel FGSizeTemp = new FGSizeViewModel();
            FGSizeTemp.Id = FGSize.Id;
            FGSizeTemp.TypeId = FGSize.TypeId;
            FGSizeTemp.TypeName = FGSize.FGType.TypeName;
            FGSizeTemp.Size = FGSize.Size;
            return Json(FGSizeTemp, JsonRequestBehavior.AllowGet);
        }





    }

    public class FGSizeViewModel
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string Size { get; set; }

        public virtual FGType FGType { get; set; }



    }
}