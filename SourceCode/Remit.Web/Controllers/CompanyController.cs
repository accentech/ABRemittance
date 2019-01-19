using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using System.IO;
using Remit.ClientModel;

namespace Remit.Web.Controllers
{
    public class CompanyController : Controller
    {
        public readonly ICompanyService companyService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public CompanyController(ICompanyService companyService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.companyService = companyService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:company" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission =null;


        // GET: /Company/
        public ActionResult Index()
        {
            const string url = "/Company/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("Company");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult Company()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateCompany(Company company)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = company.Id == 0 ? true : false;
            const string url = "/Company/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(company))
                    {
                        if (this.companyService.CreateCompany(company))
                        {
                            isSuccess = true;
                            message = "Company saved successfully!";
                        }
                        else
                        {
                            message = "Company could not saved!";
                        }
                    }
                    else
                    {
                        if (permission.UpdateOperation == true)
                        {

                            var companyObj = this.companyService.GetCompany(company.Id);
                            companyObj.Name = company.Name;
                            //companyObj.Address1 = company.Address1;
                            //companyObj.Address2 = company.Address2;
                            //companyObj.Address3 = company.Address3;
                            companyObj.Fax = company.Fax;
                            companyObj.Email = company.Email;
                            companyObj.Phone = company.Phone;
                            companyObj.ContactPerson = company.ContactPerson;
                            companyObj.LogoName = company.LogoName;
                          
                            companyObj.BaseCurrency = company.BaseCurrency;
                            companyObj.LocalCurrency = company.LocalCurrency;
                            companyObj.CompanyUrl = company.CompanyUrl;


                            if (this.companyService.UpdateCompany(companyObj))
                            {
                                isSuccess = true;
                                message = "Company updated successfully!";
                            }
                            else
                            {
                                message = "Company could not updated!";
                            }
                        }
                        else
                        {
                            message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
                        }
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

                    var companyObj = this.companyService.GetCompany(company.Id);
                   
                        companyObj.Name = company.Name;
                        //companyObj.Address1 = company.Address1;
                        //companyObj.Address2 = company.Address2;
                        //companyObj.Address3 = company.Address3;
                        companyObj.Fax = company.Fax;
                        companyObj.Email = company.Email;
                        companyObj.Phone = company.Phone;
                        companyObj.ContactPerson = company.ContactPerson;
                        companyObj.LogoName = company.LogoName;
                      ;
                        companyObj.BaseCurrency = company.BaseCurrency;
                        companyObj.LocalCurrency = company.LocalCurrency;
                        companyObj.CompanyUrl = company.CompanyUrl;
                  

                    if (this.companyService.UpdateCompany(companyObj))
                    {
                        isSuccess = true;
                        message = "Company updated successfully!";
                    }
                    else
                    {
                        message = "Company could not updated!";
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
        private bool CheckIsExist(Model.Models.Company company)
        {
            return this.companyService.CheckIsExist(company);
        }
        [HttpPost]
        public JsonResult DeleteCompany(Company company)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Company/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.companyService.DeleteCompany(company.Id);
                if (isSuccess)
                {
                    message = "Company deleted successfully!";

                }
                else
                {
                    message = "Company can't be deleted!";
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

        public JsonResult GetCompanyList()
        {
            var companyObj = this.companyService.GetAllCompany().FirstOrDefault();
            CompanyModel companyTemp = new CompanyModel();
            if (companyObj != null)
            {
                
                companyTemp.Id = companyObj.Id;
                companyTemp.Name = companyObj.Name;
                //companyTemp.Address1 = companyObj.Address1;
                //companyTemp.Address2 = companyObj.Address2;
                //companyTemp.Address3 = companyObj.Address3;
                companyTemp.Phone = companyObj.Phone;
                companyTemp.Fax = companyObj.Fax;
                companyTemp.Email = companyObj.Email;
                companyTemp.ContactPerson = companyObj.ContactPerson;
                companyTemp.LogoName = companyObj.LogoName;
            
                if (companyObj.BaseCurrency != null)
                    companyTemp.BaseCurrency = (int)companyObj.BaseCurrency;
                if (companyObj.Currency != null)
                    companyTemp.BaseCurrecySymbol = companyObj.Currency.Symbol;
                if (companyObj.LocalCurrency != null)
                    companyTemp.LocalCurrency = (int)companyObj.LocalCurrency;
                companyTemp.CompanyUrl = companyObj.CompanyUrl;
            }

            return Json(companyTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCompany(int id)
        {
            var company = this.companyService.GetCompany(id);
            return Json(company);
        }


        [HttpPost]
        public bool UploadSampleFile(HttpPostedFileBase file, string fileName)
        {

            if (file != null && file.ContentLength > 0)
            {
                var companyObj = this.companyService.GetCompanyByName(fileName);

                var imageName = fileName + Path.GetExtension(file.FileName);

                if (System.IO.File.Exists(Server.MapPath("~/CompanyLogo/" + imageName)))
                {
                    System.IO.File.Delete(Server.MapPath("~/CompanyLogo/" + imageName));
                }

                if (companyObj != null)
                {
                    companyObj.LogoName = fileName + Path.GetExtension(file.FileName);
                    try
                    {
                        file.SaveAs(Server.MapPath(Path.Combine("~/CompanyLogo/", fileName + Path.GetExtension(file.FileName))));
                        this.companyService.UpdateCompany(companyObj);
                    }
                    catch (Exception e)
                    {

                        //
                    }

                    return true;
                }
            }
            return false;
        }
    }

    
}