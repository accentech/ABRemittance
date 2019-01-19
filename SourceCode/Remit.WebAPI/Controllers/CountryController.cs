using Newtonsoft.Json;
using Remit.ClientModel;
using Remit.Model.Models;
using Remit.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Remit.WebAPI.Controllers
{
    public class CountryController : ApiController
    {
        public readonly ICountryService countryService;
        //public readonly ISubModuleItemService subModuleItemService;
        //public readonly IRoleSubModuleItemService roleSubModuleItemService;

        public CountryController(ICountryService countryService)
        {
            this.countryService = countryService;
            //this.subModuleItemService = subModuleItemService;
            //this.roleSubModuleItemService = roleSubModuleItemService;
        }

        public JsonResult<List<CountryModel>> GetCountryList()
        {
            var countryListObj = this.countryService.GetAllCountry();
            List<CountryModel> countryVMList = new List<CountryModel>();

            foreach (var country in countryListObj)
            {
                CountryModel countryTemp = new CountryModel();
                countryTemp.Id = country.Id;
                countryTemp.Name = country.Name;
                countryTemp.Code = country.Code;
                countryVMList.Add(countryTemp);
            }
            return Json(countryVMList);
        }


    }

}
