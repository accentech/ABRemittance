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
    public class WarehouseController : Controller
    {
        public readonly IWarehouseService warehouseService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public WarehouseController(IWarehouseService warehouseService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.warehouseService = warehouseService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:warehouse" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /Warehouse/
        public ActionResult Index()
        {
            const string url = "/Warehouse/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Warehouse");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult Warehouse()
        {
            return View();
        }



        [HttpPost]
        
        public JsonResult CreateWarehouse(Warehouse warehouse)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = warehouseService.GetWarehouse(warehouse.Id);
            const string url = "/Warehouse/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(warehouse))
                    {
                        if (this.warehouseService.CreateWarehouse(warehouse))
                        {
                            isSuccess = true;
                            message = "Warehouse saved successfully!";
                        }
                        else
                        {
                            message = "Warehouse could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same warehouse name found!";
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
                    isNew.Name = warehouse.Name;
                    isNew.Code = warehouse.Code;
                    isNew.Address = warehouse.Address;

                    if (this.warehouseService.UpdateWarehouse(isNew))
                    {
                        isSuccess = true;
                        message = "Warehouse updated successfully!";
                    }
                    else
                    {
                        message = "Warehouse could not updated!";
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
     
        private bool CheckIsExist(Model.Models.Warehouse warehouse)
        {
            return this.warehouseService.CheckIsExist(warehouse);
        }
        [HttpPost]
        public JsonResult DeleteWarehouse(Warehouse warehouse)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Warehouse/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.warehouseService.DeleteWarehouse(warehouse.Id);
                if (isSuccess)
                {
                    message = "Warehouse deleted successfully!";

                }
                else
                {
                    message = "Warehouse can't be deleted!";
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

        public JsonResult GetWarehouseList()
        {
            var warehouseListObj = this.warehouseService.GetAllWarehouse();
            List<WarehouseViewModel> warehouseVMList = new List<WarehouseViewModel>();
            foreach (var warehouse in warehouseListObj)
            {
                WarehouseViewModel warehouseTemp = new WarehouseViewModel();
                warehouseTemp.Id = warehouse.Id;
                warehouseTemp.Name = warehouse.Name;
                warehouseTemp.Code = warehouse.Code;
                warehouseTemp.Address = warehouse.Address;

             
                warehouseVMList.Add(warehouseTemp);
            }
            return Json(warehouseVMList, JsonRequestBehavior.AllowGet);
        }


        //public JsonResult GetWarehouseListForShipment()
        //{
        //    var warehouseListObj = this.warehouseService.GetAllWarehouse();
        //    List<WarehouseViewModel> warehouseVMList = new List<WarehouseViewModel>();
        //    foreach (var warehouse in warehouseListObj)
        //    {
        //        if (warehouse.Warehouses.Any())
        //        {
        //            WarehouseViewModel warehouseTemp = new WarehouseViewModel();
        //            warehouseTemp.Id = warehouse.Id;
        //            warehouseTemp.HeadCategory = warehouse.HeadCategory;

        //            List<WarehouseViewModel> warehouseList = new List<WarehouseViewModel>();
        //            if (warehouse.Warehouses.Any())
        //            {
        //                foreach (var aExpenseHead in warehouse.Warehouses)
        //                {
        //                    WarehouseViewModel lcshipmentexpenseheadTemp = new WarehouseViewModel();
        //                    lcshipmentexpenseheadTemp.Id = aExpenseHead.Id;
        //                    lcshipmentexpenseheadTemp.Head = aExpenseHead.Head;
        //                    lcshipmentexpenseheadTemp.AmountInLocal = 0;
        //                    warehouseList.Add(lcshipmentexpenseheadTemp);
        //                }
        //            }
        //            warehouseTemp.Warehouses = warehouseList;
        //            warehouseVMList.Add(warehouseTemp);
        //        }
        //    }
        //    return Json(warehouseVMList, JsonRequestBehavior.AllowGet);
        //}



        public JsonResult GetWarehouse(int id)
        {
            var warehouse = this.warehouseService.GetWarehouse(id);
            return Json(warehouse);
        }
    }

    public class WarehouseViewModel
    {
        public WarehouseViewModel()
        {
            this.Warehouses = new List<WarehouseViewModel>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }

        public ICollection<WarehouseViewModel> Warehouses { set; get; }

    }
}