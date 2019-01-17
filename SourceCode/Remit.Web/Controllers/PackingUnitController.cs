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
    public class PackingUnitController : Controller
    {
        public readonly IPackingUnitService packingUnitService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public PackingUnitController(IPackingUnitService packingUnitService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.packingUnitService = packingUnitService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:packingUnit" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /PackingUnit/
        public ActionResult Index()
        {
            const string url = "/PackingUnit/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:packingUnit" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("PackingUnit");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult PackingUnit()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreatePackingUnit(PackingUnit packingUnit)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = packingUnit.Id == 0 ? true : false;
            const string url = "/PackingUnit/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(packingUnit))
                    {
                        if (this.packingUnitService.CreatePackingUnit(packingUnit))
                        {
                            isSuccess = true;
                            message = "PackingUnit saved successfully!";
                        }
                        else
                        {
                            message = "PackingUnit could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same packingUnit name found!";
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
                    var PackingUnitObj = this.packingUnitService.GetPackingUnit(packingUnit.Id);

                    PackingUnitObj.Name = packingUnit.Name;

                    if (this.packingUnitService.UpdatePackingUnit(PackingUnitObj))
                    {
                        isSuccess = true;
                        message = "PackingUnit updated successfully!";
                    }
                    else
                    {
                        message = "PackingUnit could not updated!";
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
        private bool CheckIsExist(Model.Models.PackingUnit packingUnit)
        {
            return this.packingUnitService.CheckIsExist(packingUnit);
        }
        [HttpPost]
        public JsonResult DeletePackingUnit(PackingUnit packingUnit)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/PackingUnit/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.packingUnitService.DeletePackingUnit(packingUnit.Id);
                if (isSuccess)
                {
                    message = "PackingUnit deleted successfully!";

                }
                else
                {
                    message = "PackingUnit can't be deleted!";
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

        public JsonResult GetPackingUnitList()
        {
            var packingUnitListObj = this.packingUnitService.GetAllPackingUnit();
            List<PackingUnitViewModel> packingUnitVMList = new List<PackingUnitViewModel>();

            foreach (var packingUnit in packingUnitListObj)
            {
                PackingUnitViewModel packingUnitTemp = new PackingUnitViewModel();
                packingUnitTemp.Id = packingUnit.Id;
                packingUnitTemp.Name = packingUnit.Name;

                packingUnitVMList.Add(packingUnitTemp);
            }
            return Json(packingUnitVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPackingUnit(int id)
        {
            var packingUnit = this.packingUnitService.GetPackingUnit(id);
            return Json(packingUnit);
        }
    }

    public class PackingUnitViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }
}