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
    public class CurrencyController : Controller
    {
        public readonly ICurrencyService currencyService;
        public readonly ICompanyService companyService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public CurrencyController(ICurrencyService currencyService,ICompanyService companyService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.companyService = companyService;
            this.currencyService = currencyService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:currency" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        
        // GET: /Currency/
        public ActionResult Index()
        {
            const string url = "/Currency/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId) ;

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Currency");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            
            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult Currency()
        {
            return View();
        }

        public string GetBaseCurrency()
        {
            try
            {
                string result = companyService.GetAllCompany().ToList().FirstOrDefault().Currency.Name;
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult CreateCurrency(Currency currency)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = currency.Id == 0 ? true : false;
            const string url = "/Currency/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {

                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(currency))
                    {
                        if (this.currencyService.CreateCurrency(currency))
                        {
                            isSuccess = true;
                            message = "Currency saved successfully!";
                        }
                        else
                        {
                            message = "Currency could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same currency name found!";
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
                    if (this.currencyService.UpdateCurrency(currency))
                    {
                        isSuccess = true;
                        message = "Currency updated successfully!";
                    }
                    else
                    {
                        message = "Currency could not updated!";
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
        private bool CheckIsExist(Model.Models.Currency currency)
        {
            return this.currencyService.CheckIsExist(currency);
        }
        [HttpPost]
        public JsonResult DeleteCurrency(Currency currency)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Currency/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permission.DeleteOperation == true)
            {
                isSuccess = this.currencyService.DeleteCurrency(currency.Id);
                if (isSuccess)
                {
                    message = "Currency deleted successfully!";

                }
                else
                {
                    message = "Currency can't be deleted!";
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

        public JsonResult GetCurrencyList()
        {
            var currencyListObj = this.currencyService.GetAllCurrency();
            List<CurrencyViewModel> currencyVMList = new List<CurrencyViewModel>();

            foreach (var currency in currencyListObj)
            {
                CurrencyViewModel currencyTemp = new CurrencyViewModel();
                currencyTemp.Id = currency.Id;
                currencyTemp.Name = currency.Name;
                currencyTemp.Symbol = currency.Symbol;
                currencyVMList.Add(currencyTemp);
            }
            return Json(currencyVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCurrency(int id)
        {
            var currency = this.currencyService.GetCurrency(id);
            return Json(currency);
        }
    }
    
    public class CurrencyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}