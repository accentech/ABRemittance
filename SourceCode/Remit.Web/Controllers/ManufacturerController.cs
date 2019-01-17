using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.CachingService;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class ManufacturerController : Controller
    {
        public readonly ICountryService countryService;
        public readonly ICityService cityService;
        public readonly IManufacturerService manufacturerService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:manufacturer" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/Manufacturer/Index";
            
        // GET: /Manufacturer/
        public ActionResult Index()
        {
            var url = Request.RawUrl;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set("permission:manufacturer" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("Manufacturer");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ManufacturerController(IManufacturerService manufacturerService, ICountryService countryService, ICityService cityService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.manufacturerService = manufacturerService;
            this.countryService = countryService;
            this.cityService = cityService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateManufacturer(Manufacturer manufacturer)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = manufacturer.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(manufacturer))
                    {
                        if (this.manufacturerService.CreateManufacturer(manufacturer))
                        {
                            isSuccess = true;
                            message = "Manufacturer saved successfully!";
                        }
                        else
                        {
                            message = "Manufacturer could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same manufacturer name found!";
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
                    if (this.manufacturerService.UpdateManufacturer(manufacturer))
                    {
                        isSuccess = true;
                        message = "Manufacturer updated successfully!";
                    }
                    else
                    {
                        message = "Manufacturer could not updated!";
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
        private bool CheckIsExist(Model.Models.Manufacturer manufacturer)
        {
            return this.manufacturerService.CheckIsExist(manufacturer);
        }

        [HttpPost]
        public JsonResult DeleteManufacturer(Manufacturer manufacturer)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.manufacturerService.DeleteManufacturer(manufacturer.Id);
                if (isSuccess)
                {
                    message = "Manufacturer deleted successfully!";
                }
                else
                {
                    message = "Manufacturer can't be deleted!";
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

        public JsonResult GetManufacturerList()
        {
            var manufacturerListObj = this.manufacturerService.GetAllManufacturer();
            List<ManufacturerViewModel> manufacturerVMList = new List<ManufacturerViewModel>();

            foreach (var manufacturer in manufacturerListObj)
            {
                ManufacturerViewModel manufacturerTemp = new ManufacturerViewModel();
                manufacturerTemp.Id = manufacturer.Id;
                manufacturerTemp.Name = manufacturer.Name;
                manufacturerTemp.Address = manufacturer.Address;
                manufacturerTemp.CityName = cityService.GetCity(Convert.ToInt32(manufacturer.CityId)).Name;
                manufacturerTemp.CityId = manufacturer.CityId;
                manufacturerTemp.CountryName = countryService.GetCountry(manufacturer.CountryId).Name;
                manufacturerTemp.CountryId = manufacturer.CountryId;
                if (manufacturer.CountryOrigin != null)
                {
                    manufacturerTemp.CountryOriginName = countryService.GetCountry(manufacturer.CountryOrigin).Name;
                    manufacturerTemp.CountryOrigin = manufacturer.CountryOrigin;
                }
                manufacturerTemp.Email = manufacturer.Email;
                manufacturerTemp.EmergencyContact = manufacturer.EmergencyContact;
                 manufacturerTemp.LoadingPort = manufacturer.LoadingPort;
                manufacturerTemp.OfficePhone = manufacturer.OfficePhone;
                manufacturerTemp.Skype = manufacturer.Skype;
                manufacturerTemp.Website = manufacturer.Website;

                manufacturerVMList.Add(manufacturerTemp);
            }
            return Json(manufacturerVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetManufacturer(int id)
        {
            var manufacturer = this.manufacturerService.GetManufacturer(id);
            return Json(manufacturer);
        }
    }

    public class ManufacturerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public string OfficePhone { get; set; }
        public string EmergencyContact { get; set; }
        public string Email { get; set; }
        public string Skype { get; set; }
        public string Website { get; set; }
        public string CountryOrigin { get; set; }
        public string CountryOriginName { get; set; }
        public string LoadingPort { get; set; }
        public virtual City City { get; set; }
        public virtual Country Country { get; set; }
        public virtual Country Country1 { get; set; }

    }

}