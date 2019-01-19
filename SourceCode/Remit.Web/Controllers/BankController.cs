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
using Remit.ClientModel;

namespace Remit.Web.Controllers
{
    public class BankController : Controller
    {
        public readonly IBankService bankService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public BankController(IBankService bankService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.bankService = bankService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:bank" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /Bank/
        public ActionResult Index()
        {
            const string url = "/Bank/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Bank");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult Bank()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateBank(Bank bank)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = bankService.GetBank(bank.Id);
            const string url = "/Bank/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(bank))
                    {
                        if (this.bankService.CreateBank(bank))
                        {
                            isSuccess = true;
                            message = "Bank saved successfully!";
                        }
                        else
                        {
                            message = "Bank could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same bank name found!";
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
                    isNew.Name = bank.Name;
                   
                    if (this.bankService.UpdateBank(isNew))
                    {
                        isSuccess = true;
                        message = "Bank updated successfully!";
                    }
                    else
                    {
                        message = "Bank could not updated!";
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
        private bool CheckIsExist(Model.Models.Bank bank)
        {
            return this.bankService.CheckIsExist(bank);
        }
        [HttpPost]
        public JsonResult DeleteBank(Bank bank)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Bank/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.bankService.DeleteBank(bank.Id);
                if (isSuccess)
                {
                    message = "Bank deleted successfully!";

                }
                else
                {
                    message = "Bank can't be deleted!";
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

        public JsonResult GetBankList()
        {
            var bankListObj = this.bankService.GetAllBank();
            List<BankModel> bankVMList = new List<BankModel>();

            foreach (var bank in bankListObj)
            {
                BankModel bankTemp = new BankModel();
                bankTemp.Id = bank.Id;
                bankTemp.Name = bank.Name;
                
                bankVMList.Add(bankTemp);
            }
            return Json(bankVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBank(int id)
        {
            var bank = this.bankService.GetBank(id);
            return Json(bank);
        }
    }

    
}