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
    public class ServiceProviderController : Controller
    {
        public readonly ICountryService countryService;
        public readonly ICityService cityService;
        public readonly IServiceProviderService serviceProviderService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        static string cacheKey = "permission:serviceProvider" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = cacheProvider.Get(cacheKey) as RoleSubModuleItem;

        const string url = "/ServiceProvider/Index";
            
        // GET: /ServiceProvider/
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
                    cacheProvider.Set("permission:serviceProvider" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("ServiceProvider");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ServiceProviderController(IServiceProviderService serviceProviderService, ICountryService countryService, ICityService cityService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.serviceProviderService = serviceProviderService;
            this.countryService = countryService;
            this.cityService = cityService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateServiceProvider(ServiceProvider serviceProvider)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = serviceProvider.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(serviceProvider))
                    {
                        if (this.serviceProviderService.CreateServiceProvider(serviceProvider))
                        {
                            isSuccess = true;
                            message = "ServiceProvider saved successfully!";
                        }
                        else
                        {
                            message = "ServiceProvider could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same serviceProvider name found!";
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
                    if (this.serviceProviderService.UpdateServiceProvider(serviceProvider))
                    {
                        isSuccess = true;
                        message = "ServiceProvider updated successfully!";
                    }
                    else
                    {
                        message = "ServiceProvider could not updated!";
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

        private bool CheckIsExist(Model.Models.ServiceProvider serviceProvider)
        {
            return this.serviceProviderService.CheckIsExist(serviceProvider);
        }

        [HttpPost]
        public JsonResult DeleteServiceProvider(ServiceProvider serviceProvider)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.serviceProviderService.DeleteServiceProvider(serviceProvider.Id);
                if (isSuccess)
                {
                    message = "ServiceProvider deleted successfully!";
                }
                else
                {
                    message = "ServiceProvider can't be deleted!";
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

        public JsonResult GetServiceProviderList()
        {
            var serviceProviderListObj = this.serviceProviderService.GetAllServiceProvider();
            List<ServiceProviderViewModel> serviceProviderVMList = new List<ServiceProviderViewModel>();

            foreach (var serviceProvider in serviceProviderListObj)
            {
                ServiceProviderViewModel serviceProviderTemp = new ServiceProviderViewModel();
                serviceProviderTemp.Id = serviceProvider.Id;
                serviceProviderTemp.Name = serviceProvider.Name;
                serviceProviderTemp.Address = serviceProvider.Address;
                serviceProviderTemp.CityName = cityService.GetCity(Convert.ToInt32(serviceProvider.CityId)).Name;
                serviceProviderTemp.CityId = serviceProvider.CityId;
                serviceProviderTemp.CountryName = countryService.GetCountry(serviceProvider.CountryId).Name;
                serviceProviderTemp.CountryId = serviceProvider.CountryId;
                
                serviceProviderTemp.Email = serviceProvider.Email;
                serviceProviderTemp.EmergencyContact = serviceProvider.EmergencyContact;
                serviceProviderTemp.OfficePhone = serviceProvider.OfficePhone;
                serviceProviderTemp.Skype = serviceProvider.Skype;
                serviceProviderTemp.Website = serviceProvider.Website;

                serviceProviderVMList.Add(serviceProviderTemp);
            }
            return Json(serviceProviderVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetServiceProvider(int id)
        {
            var serviceProvider = this.serviceProviderService.GetServiceProvider(id);
            return Json(serviceProvider);
        }
    }

    public class ServiceProviderViewModel
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
        public virtual City City { get; set; }
        public virtual Country Country { get; set; }
    }

}