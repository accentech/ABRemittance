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
    public class CountryController : Controller
    {
        public readonly ICountryService countryService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public CountryController(ICountryService countryService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.countryService = countryService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:country" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        
        // GET: /Country/
        public ActionResult Index()
        {
            const string url = "/Country/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId) ;

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Country");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            
            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult Country()
        {
            return View();
        }

        public ActionResult TestAngCountryIndex()
        {
            return View("TestAngCountryIndex");
        }


        [HttpPost]
        public JsonResult CreateCountry(Country country)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = countryService.GetCountry(country.Id);
            const string url = "/Country/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(country))
                    {
                        if (this.countryService.CreateCountry(country))
                        {
                            isSuccess = true;
                            message = "Country saved successfully!";
                        }
                        else
                        {
                            message = "Country could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same country name found!";
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
                    isNew.Name = country.Name;
                    isNew.Code = country.Code;
                    if (this.countryService.UpdateCountry(isNew))
                    {
                        isSuccess = true;
                        message = "Country updated successfully!";
                    }
                    else
                    {
                        message = "Country could not updated!";
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
        private bool CheckIsExist(Model.Models.Country country)
        {
            return this.countryService.CheckIsExist(country);
        }
        [HttpPost]
        public JsonResult DeleteCountry(Country country)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Country/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permission.DeleteOperation == true)
            {
                isSuccess = this.countryService.DeleteCountry(country.Id);
                if (isSuccess)
                {
                    message = "Country deleted successfully!";

                }
                else
                {
                    message = "Country can't be deleted!";
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

        public JsonResult GetCountryList()
        {
            var countryListObj = this.countryService.GetAllCountry();
            List<CountryViewModel> countryVMList = new List<CountryViewModel>();

            foreach (var country in countryListObj)
            {
                CountryViewModel countryTemp = new CountryViewModel();
                countryTemp.Id = country.Id;
                countryTemp.Name = country.Name;
                countryTemp.Code = country.Code;
                countryVMList.Add(countryTemp);
            }
            return Json(countryVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCountry(string id)
        {
            var country = this.countryService.GetCountry(id);
            return Json(country);
        }
    }

    public class CountryViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }

    }
}