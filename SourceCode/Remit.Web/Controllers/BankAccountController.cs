
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
    public class BankAccountController : Controller
    {
        public readonly ICountryService countryService;
        public readonly IBankService bankService;
        public readonly IBranchService branchService;
        public readonly IBankAccountService bankAccountService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:bankAccount" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /BankAccount/Index
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
                    return View("BankAccount");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public BankAccountController(IBankAccountService bankAccountService, ICountryService countryService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.bankAccountService = bankAccountService;
            this.countryService = countryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateBankAccount(BankAccount bankAccount)
        {
            const string url = "/BankAccount/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = bankAccount.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(bankAccount))
                    {
                        if (this.bankAccountService.CreateBankAccount(bankAccount))
                        {
                            isSuccess = true;
                            message = "BankAccount saved successfully!";
                        }
                        else
                        {
                            message = "BankAccount could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same bankAccount name found!";
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
                    if (this.bankAccountService.UpdateBankAccount(bankAccount))
                    {
                        isSuccess = true;
                        message = "BankAccount updated successfully!";
                    }
                    else
                    {
                        message = "BankAccount could not updated!";
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
        private bool CheckIsExist(Model.Models.BankAccount bankAccount)
        {
            return this.bankAccountService.CheckIsExist(bankAccount);
        }


        [HttpPost]
        public JsonResult DeleteBankAccount(BankAccount bankAccount)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.bankAccountService.DeleteBankAccount(bankAccount.Id);
                if (isSuccess)
                {
                    message = "BankAccount deleted successfully!";
                }
                else
                {
                    message = "BankAccount can't be deleted!";
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

        public JsonResult GetBankAccountList()
        {
            var bankAccountListObj = this.bankAccountService.GetAllBankAccount();
            List<BankAccountViewModel> bankAccountVMList = new List<BankAccountViewModel>();

            foreach (var bankAccount in bankAccountListObj)
            {
                BankAccountViewModel bankAccountTemp = new BankAccountViewModel();
                bankAccountTemp.Id = bankAccount.Id;
                bankAccountTemp.AccountName = bankAccount.AccountName;
                bankAccountTemp.AccountNumber = bankAccount.AccountNumber;
                bankAccountTemp.BankId =bankAccount.BankId;
                bankAccountTemp.BankName = bankAccount.Bank.Name;
                bankAccountTemp.BranchId = bankAccount.BranchId;
                bankAccountTemp.BranchName = bankAccount.Branch.Name;

                bankAccountTemp.CountryId = bankAccount.Branch.Country.Id;
                bankAccountTemp.CountryName = bankAccount.Branch.Country.Name;
           


                bankAccountVMList.Add(bankAccountTemp);
            }
            return Json(bankAccountVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBankAccountListByBankId(int id)
        {
            var bankAccountListObj = this.bankAccountService.GetAllBankAccount().Where(c => c.BankId == id);
            List<BankAccountViewModel> bankAccountVMList = new List<BankAccountViewModel>();

            foreach (var bankAccount in bankAccountListObj)
            {
                BankAccountViewModel bankAccountTemp = new BankAccountViewModel();
                bankAccountTemp.Id = bankAccount.Id;
                bankAccountTemp.AccountNumber = bankAccount.AccountNumber;
                bankAccountTemp.AccountName = bankAccount.AccountName;

                if (bankAccount.Bank != null)
                {
                    bankAccountTemp.BankName = bankAccount.Bank.Name;
                    bankAccountTemp.BankId = bankAccount.BankId;
                }

                bankAccountVMList.Add(bankAccountTemp);
            }
            return Json(bankAccountVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBankAccount(int id)
        {
            var bankAccount = this.bankAccountService.GetBankAccount(id);
            return Json(bankAccount);
        }
    }

    public class BankAccountViewModel
    {
        public int Id { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
        public int BankId { get; set; }
        public string BankName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }

    }
}