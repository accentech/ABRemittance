using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Media;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Remit.CachingService;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class SupplierController : Controller
    {
        public readonly ICountryService countryService;
        public readonly ICityService cityService;
        public readonly ISupplierService supplierService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:supplier" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /Supplier/
        public ActionResult Index()
        {
            var url = Request.RawUrl;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                    Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:supplier" + Helpers.UserSession.GetUserFromSession().RoleId,
                        permission, 240);
                    return View("Supplier");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public SupplierController(ISupplierService supplierService, ICountryService countryService,
            ICityService cityService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.supplierService = supplierService;
            this.countryService = countryService;
            this.cityService = cityService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateSupplier(Supplier supplier, List<ComponentViewModel> componentList)
        {
            const string url = "/Supplier/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                    Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = supplier.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(supplier))
                    {
                        if (this.supplierService.CreateSupplier(supplier))
                        {
                            isSuccess = true;
                            message = "Supplier saved successfully!";
                        }
                        else
                        {
                            message = "Supplier could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same supplier name found!";
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
                    if (this.supplierService.UpdateSupplier(supplier))
                    {
                        isSuccess = true;
                        message = "Supplier updated successfully!";
                    }
                    else
                    {
                        message = "Supplier could not updated!";
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

        private bool CheckIsExist(Model.Models.Supplier supplier)
        {
            return this.supplierService.CheckIsExist(supplier);
        }

        [HttpPost]
        public JsonResult DeleteSupplier(Supplier supplier)
        {
 
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.supplierService.DeleteSupplier(supplier.Id);
                if (isSuccess)
                {
                    message = "Supplier deleted successfully!";
                }
                else
                {
                    message = "Supplier can't be deleted!";
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

        public JsonResult GetSupplierList()
        {
            var supplierListObj = this.supplierService.GetAllSupplier();
            var supplierVmList = AllSupplierManufacturerList(supplierListObj);            
            return Json(supplierVmList, JsonRequestBehavior.AllowGet);
        }


       // Get Supplier list that is related with LC
        public JsonResult GetSupplierListByRelatedLc()
        {
            var supplierListObj = this.supplierService.GetAllSupplierRelatedWithLc();
            var supplierVmList = AllSupplierManufacturerList(supplierListObj);            
            return Json(supplierVmList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetSupplierAndManufacturerList()
        {
            var isSuccess = true;
            var supplierListObj = this.supplierService.GetAllSupplierAndManufacturer();

            var supplierVmList = AllSupplierManufacturerList(supplierListObj);


            if (supplierVmList.Count == 0)
            {
                isSuccess = false;
            }
            return Json(new
            {
                result = isSuccess,
                list = supplierVmList

            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetManufacturerList()
        {
            var supplierListObj = this.supplierService.GetAllManufacturer();

            var manufacturerVMList = AllSupplierManufacturerList(supplierListObj);


            return Json(manufacturerVMList, JsonRequestBehavior.AllowGet);
        }

        public List<SupplierViewModel> AllSupplierManufacturerList(IEnumerable<Supplier> supplierListObj)
        {
            List<SupplierViewModel> supplierVMList = new List<SupplierViewModel>();

            foreach (var supplier in supplierListObj)
            {
                SupplierViewModel supplierTemp = new SupplierViewModel();
                supplierTemp.Id = supplier.Id;
                supplierTemp.Name = supplier.Name;
                supplierTemp.AddressLine1 = supplier.AddressLine1;
                supplierTemp.AddressLine2 = supplier.AddressLine2;
                supplierTemp.AddressLine3 = supplier.AddressLine3;
                if (supplier.CityId != null && supplier.City != null)
                {
                    supplierTemp.CityName = supplier.City.Name;
                    supplierTemp.CityId = (int)supplier.CityId;
                }
                supplierTemp.CountryName = supplier.Country != null ? supplier.Country.Name : "";
                supplierTemp.CountryId = supplier.CountryId;
                supplierTemp.Email = supplier.Email;
                supplierTemp.EmergencyContact = supplier.EmergencyContact;
                supplierTemp.ItemLoadingPort = supplier.ItemLoadingPort;
                supplierTemp.OfficePhone = supplier.OfficePhone;
                supplierTemp.Skype = supplier.Skype;
                supplierTemp.Website = supplier.Website;

                supplierTemp.CurrencyId = supplier.CurrencyId;
                try
                {
                    supplierTemp.CurrencyName = supplier.Currency.Name;
                }
                catch { }

                if (supplier.IsLocalSupplier)
                    supplierTemp.IsLocalSupplier = true;
                else
                    supplierTemp.IsLocalSupplier = false;

                supplierTemp.IsActive = supplier.IsActive;

                supplierVMList.Add(supplierTemp);
            }

            return supplierVMList;
        }



        public JsonResult GetSupplierListonlyIdAndName()
        {
            var supplierListObj = this.supplierService.GetAllSupplier();

            List<SuppliersViewModel> supplierList = new List<SuppliersViewModel>();
            foreach (var supplier in supplierListObj)
            {
                SuppliersViewModel aSupplier = new SuppliersViewModel();
                aSupplier.Id = supplier.Id;
                aSupplier.Name = supplier.Name;
                supplierList.Add(aSupplier);
            }
            return Json(supplierList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSupplier(int id)
        {
            var supplier = this.supplierService.GetSupplier(id);
            return Json(supplier);
        }
    }

    public class SupplierViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string RegistrationDate { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string OfficePhone { get; set; }
        public string EmergencyContact { get; set; }
        public string Email { get; set; }
        public string Skype { get; set; }
        public string Website { get; set; }
        public string ClientList { get; set; }
        public string ItemsBuild { get; set; }
        public string CountryOrigin { get; set; }
        public Nullable<int>Type { get; set; }
        public string CountryOriginName { get; set; }
        public string ItemLoadingPort { get; set; }
        public virtual City City { get; set; }
        public virtual Country Country { get; set; }
        public virtual Country Country1 { get; set; }
        public bool IsLocalSupplier { get; set; }
        public bool IsActive { get; set; }
        public List<ComponentViewModel> ComponentViewModels { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
    }

    public class SuppliersViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class ComponentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class SupplierAircraftComponentViewModel
    {
        public int? Id { get; set; }
        public int AircraftComponentId { get; set; }
        public string Name { get; set; }

    }

}