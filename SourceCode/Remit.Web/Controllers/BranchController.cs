
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
    public class BranchController : Controller
    {
        public readonly ICountryService countryService;
        public readonly IBranchService branchService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:branch" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /Branch/
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
                    return View("Branch");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public BranchController(IBranchService branchService, ICountryService countryService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.branchService = branchService;
            this.countryService = countryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateBranch(Branch branch)
        {
            const string url = "/Branch/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = branch.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(branch))
                    {
                        if (this.branchService.CreateBranch(branch))
                        {
                            isSuccess = true;
                            message = "Branch saved successfully!";
                        }
                        else
                        {
                            message = "Branch could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same branch name found!";
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
                    if (this.branchService.UpdateBranch(branch))
                    {
                        isSuccess = true;
                        message = "Branch updated successfully!";
                    }
                    else
                    {
                        message = "Branch could not updated!";
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
        private bool CheckIsExist(Model.Models.Branch branch)
        {
            return this.branchService.CheckIsExist(branch);
        }


        [HttpPost]
        public JsonResult DeleteBranch(Branch branch)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.branchService.DeleteBranch(branch.Id);
                if (isSuccess)
                {
                    message = "Branch deleted successfully!";
                }
                else
                {
                    message = "Branch can't be deleted!";
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



        public JsonResult GetBranchListWithCondition(int bankId, string countryId)
        {
            var branchListObj = this.branchService.GetAllBranch().Where(c => c.BankId == bankId && c.CountryId == countryId);
            List<BranchViewModel> branchVMList = new List<BranchViewModel>();

            foreach (var branch in branchListObj)
            {
                BranchViewModel branchTemp = new BranchViewModel();
                branchTemp.Id = branch.Id;
                branchTemp.Name = branch.Name;

                //if (branch.Bank != null)
                //{
                //    branchTemp.BankName = branch.Bank.Name;
                //    branchTemp.BankId = branch.BankId;
                //}

                if (branch.Country != null)
                {
                    branchTemp.CountryName = branch.Country.Name;
                    branchTemp.CountryId = branch.CountryId;
                }

                branchVMList.Add(branchTemp);
            }

            return Json(branchVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBranchList()
        {
            var branchListObj = this.branchService.GetAllBranch();
            List<BranchViewModel> branchVMList = new List<BranchViewModel>();

            foreach (var branch in branchListObj)
            {
                BranchViewModel branchTemp = new BranchViewModel();
                branchTemp.Id = branch.Id;
                branchTemp.Name = branch.Name;
                branchTemp.Address = branch.Address;
                branchTemp.Email = branch.Email;
                branchTemp.Phone = branch.Phone;
                
                branchTemp.BankId =branch.BankId;
                //branchTemp.BankName = branch.Bank.Name;
                if (branch.Country != null)
                {
                    branchTemp.CountryName = branch.Country.Name;
                    branchTemp.CountryId = branch.CountryId;
                }


                branchVMList.Add(branchTemp);
            }
            return Json(branchVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBranchListByBankId(int id)
        {
            var branchListObj = this.branchService.GetAllBranch().Where(c => c.BankId == id);
            List<BranchViewModel> branchVMList = new List<BranchViewModel>();

            foreach (var branch in branchListObj)
            {
                BranchViewModel branchTemp = new BranchViewModel();
                branchTemp.Id = branch.Id;
                branchTemp.Name = branch.Name;

                //if (branch.Bank != null)
                //{
                //    branchTemp.BankName = branch.Bank.Name;
                //    branchTemp.BankId = branch.BankId;
                //}

                branchVMList.Add(branchTemp);
            }
            return Json(branchVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBranch(int id)
        {
            var branch = this.branchService.GetBranch(id);
            return Json(branch);
        }
    }

    public class BranchViewModel
    {
        public int Id { get; set; }
        public string CountryId { get; set; }
        public int? BankId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CountryName { get; set; }
        public string BankName { get; set; }
    }
}