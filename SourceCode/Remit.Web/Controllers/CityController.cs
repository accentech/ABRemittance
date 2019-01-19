
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
using Remit.ClientModel;

namespace Remit.Web.Controllers
{
    public class CityController : Controller
    {
        public readonly ICountryService countryService;
        public readonly ICityService cityService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:city" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /City/
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
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("City");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public CityController(ICityService cityService, ICountryService countryService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.cityService = cityService;
            this.countryService = countryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }





        [HttpPost]
        public JsonResult CreateCity(City city)
        {
            const string url = "/City/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = city.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(city))
                    {
                        if (this.cityService.CreateCity(city))
                        {
                            isSuccess = true;
                            message = "City saved successfully!";
                        }
                        else
                        {
                            message = "City could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same city name found!";
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
                    if (this.cityService.UpdateCity(city))
                    {
                        isSuccess = true;
                        message = "City updated successfully!";
                    }
                    else
                    {
                        message = "City could not updated!";
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
        private bool CheckIsExist(Model.Models.City city)
        {
            return this.cityService.CheckIsExist(city);
        }


    [HttpPost]
        public JsonResult DeleteCity(City city)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.cityService.DeleteCity(city.Id);
                if (isSuccess)
                {
                    message = "City deleted successfully!";
                }
                else
                {
                    message = "City can't be deleted!";
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

        public JsonResult GetCityList()
        {
            var cityListObj = this.cityService.GetAllCity();
            List<CityModel> cityVMList = new List<CityModel>();

            foreach (var city in cityListObj)
            {
                CityModel cityTemp = new CityModel();
                cityTemp.Id = city.Id;
                cityTemp.Name = city.Name;
                if (city.Country != null)
                {
                    cityTemp.CountryName = city.Country.Name;
                    cityTemp.CountryId = city.CountryId;
                }
                

                cityVMList.Add(cityTemp);
            }
            return Json(cityVMList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCityByCountry(string id)
        {
            var cityListObj = this.cityService.GetAllCity().Where(c=> c.CountryId == id);
            List<CityModel> cityVMList = new List<CityModel>();

            foreach (var city in cityListObj)
            {
                CityModel cityTemp = new CityModel();
                cityTemp.Id = city.Id;
                cityTemp.Name = city.Name;

                if (city.Country != null)
                {
                    cityTemp.CountryName = city.Country.Name;
                    cityTemp.CountryId = city.CountryId;
                }

                cityVMList.Add(cityTemp);
            }
            return Json(cityVMList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetCity(int id)
        {
            var city = this.cityService.GetCity(id);
            return Json(city);
        }
    }

   

}