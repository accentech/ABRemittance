﻿using System;
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
    public class SubModuleController : Controller
    {
        public readonly IModuleService moduleService;
        public readonly ISubModuleService subModuleService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:subModule" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /SubModule/
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
                    cacheProvider.Set("permission:subModule" + Helpers.UserSession.GetUserFromSession().RoleId, permission, 240);
                    return View("SubModule");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public SubModuleController(ISubModuleService subModuleService, IModuleService moduleService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.subModuleService = subModuleService;
            this.moduleService = moduleService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateSubModule(SubModule subModule)
        {
            const string url = "/SubModule/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = subModule.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(subModule))
                    {
                        if (this.subModuleService.CreateSubModule(subModule))
                        {
                            isSuccess = true;
                            message = "SubModule saved successfully!";
                        }
                        else
                        {
                            message = "SubModule could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same subModule name found!";
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
                    if (this.subModuleService.UpdateSubModule(subModule))
                    {
                        isSuccess = true;
                        message = "SubModule updated successfully!";
                        cacheProvider.Invalidate("module" + Helpers.UserSession.GetUserFromSession().RoleId);
                        var moduleIdList = this.moduleService.GetAllModuleByRoleId(Helpers.UserSession.GetUserFromSession().RoleId).Select(a => a.Id).Distinct();
                        foreach (var moduleId in moduleIdList)
                        {
                            cacheProvider.Invalidate("submodule" + moduleId.ToString() + Helpers.UserSession.GetUserFromSession().RoleId);
                        }
                    }
                    else
                    {
                        message = "SubModule could not updated!";
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
        private bool CheckIsExist(Model.Models.SubModule subModule)
        {
            return this.subModuleService.CheckIsExist(subModule);
        }

        [HttpPost]
        public JsonResult DeleteSubModule(SubModule subModule)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.subModuleService.DeleteSubModule(subModule.Id);
                if (isSuccess)
                {
                    message = "SubModule deleted successfully!";
                }
                else
                {
                    message = "SubModule can't be deleted!";
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

        public JsonResult GetSubModuleList()
        {
            var subModuleListObj = this.subModuleService.GetAllSubModule();
            List<SubModuleViewModel> subModuleVMList = new List<SubModuleViewModel>();

            foreach (var subModule in subModuleListObj)
            {
                SubModuleViewModel subModuleTemp = new SubModuleViewModel();
                subModuleTemp.Id = subModule.Id;
                subModuleTemp.Name = subModule.Name;
                subModuleTemp.ModuleName = moduleService.GetModule(Convert.ToInt32(subModule.ModuleId)).Name;
                subModuleTemp.ModuleId = subModule.ModuleId;
                subModuleTemp.Ordering = subModule.Ordering;

                if (subModule.IsActive != null)
                    subModuleTemp.IsActive = subModule.IsActive.Value;

                subModuleVMList.Add(subModuleTemp);
            }
            return Json(subModuleVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetActiveSubModuleList()
        {
            var subModuleListObj = this.subModuleService.GetAllSubModule().Where(sm => sm.IsActive == true);
            List<SubModuleViewModel> subModuleVMList = new List<SubModuleViewModel>();

            foreach (var subModule in subModuleListObj)
            {
                SubModuleViewModel subModuleTemp = new SubModuleViewModel();
                subModuleTemp.Id = subModule.Id;
                subModuleTemp.Name = subModule.Name;
                subModuleTemp.ModuleName = moduleService.GetModule(Convert.ToInt32(subModule.ModuleId)).Name;
                subModuleTemp.ModuleId = subModule.ModuleId;
                subModuleTemp.Ordering = subModule.Ordering;

                if (subModule.IsActive != null)
                    subModuleTemp.IsActive = subModule.IsActive.Value;

                subModuleVMList.Add(subModuleTemp);
            }
            return Json(subModuleVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOnlySubModuleByModuleId(int id)// calling from sub module item ui (select module then load sub modules)
        {
            var subModuleListObj = this.subModuleService.GetAllSubModule().Where(sm => sm.IsActive == true && sm.ModuleId == id);
            List<SubModuleViewModel> subModuleVMList = new List<SubModuleViewModel>();

            foreach (var subModule in subModuleListObj)
            {
                SubModuleViewModel subModuleTemp = new SubModuleViewModel();
                subModuleTemp.Id = subModule.Id;
                subModuleTemp.Name = subModule.Name;
                subModuleTemp.ModuleName = moduleService.GetModule(Convert.ToInt32(subModule.ModuleId)).Name;
                subModuleTemp.ModuleId = subModule.ModuleId;
                subModuleTemp.Ordering = subModule.Ordering;
                
                if (subModule.IsActive != null)
                    subModuleTemp.IsActive = subModule.IsActive.Value;

                subModuleVMList.Add(subModuleTemp);
            }
            return Json(subModuleVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSubModulesByModuleId(int id)//id is ModuleId
        {
            int mouduleId = id;
            var loggedUser = UserSession.GetUserFromSession();
            List<SubModuleViewModel> subModuleVMList = new List<SubModuleViewModel>();
            if (loggedUser != null)
            {
                var subModuleListObj = this.subModuleService.GetSubModulesByModuleIdAndRoleId(mouduleId, (int)loggedUser.RoleId).Where(sm => sm.IsActive == true);
                List<ModuleViewModel> moduleVMList = new List<ModuleViewModel>();

                foreach (var subModule in subModuleListObj.OrderBy(sm => sm.Ordering))
                {
                    SubModuleViewModel subModuleVM = new SubModuleViewModel();
                    subModuleVM.Id = subModule.Id;
                    subModuleVM.Name = subModule.Name;
                    if (subModule.Name != null)
                    {
                        subModuleVM.NameFromResource = GetMenuResourceValueByDatabaseId(subModule.Name);
                    }
                    subModuleVM.ModuleId = subModule.ModuleId;
                    subModuleVM.Ordering = subModule.Ordering;

                    if (subModule.IsActive != null)
                        subModuleVM.IsActive = subModule.IsActive.Value;

                    if (subModule.SubModuleItems.Count >= 1)
                    {
                        List<SubModuleItemViewModel> subModuleItemVMList = new List<SubModuleItemViewModel>();

                        foreach (var subModuleItem in subModule.SubModuleItems.Where(smi => smi.IsActive == true).OrderBy(smi => smi.Ordering))
                        {
                            SubModuleItemViewModel subModuleItemVM = new SubModuleItemViewModel();
                            subModuleItemVM.Id = subModuleItem.Id;
                            subModuleItemVM.Name = subModuleItem.Name;
                            if (subModuleItem.Name != null)
                            {
                                subModuleItemVM.NameFromResource = GetSubModuleItemResourceValueByDatabaseId(subModuleItem.UrlPath); //subModuleItem.Name;// GetMenuResourceValueByDatabaseId(subModuleItem.Name);
                            }
                            subModuleItemVM.SubModuleId = subModuleItem.SubModuleId;
                            subModuleItemVM.UrlPath = subModuleItem.UrlPath;
                            //subModuleItemVM.ActivityId = subModuleItem.ActivityId;
                            subModuleItemVM.Ordering = subModuleItem.Ordering;

                            subModuleItemVMList.Add(subModuleItemVM);
                        }
                        subModuleVM.SubModuleItems = subModuleItemVMList;
                        subModuleVMList.Add(subModuleVM);
                    }

                }
                return Json(subModuleVMList, JsonRequestBehavior.AllowGet);
            }
            return null;
        }
        private string GetMenuResourceValueByDatabaseId(string resourceId)
        {
            try
            {
                string nospaceresourceId = string.Empty;
                if (resourceId != null)
                {
                    nospaceresourceId = resourceId.Replace(" ", "");
                }
                return HttpContext.GetGlobalResourceObject("ResourceMenu", nospaceresourceId).ToString();  
            }
            catch (Exception e)
            {
                //
            }
            return null;
        }

        //private string GetSubModuleItemResourceValueByDatabaseId(string resourceId)
        //{
        //    string subModuleResourceId = string.Empty;
        //    if (resourceId.Contains(" "))
        //    {
        //        subModuleResourceId = resourceId.Replace(" ", "");
        //    }
        //    else if(resourceId.Contains("/"))
        //    {
        //        string [] arResources = resourceId.Split(new string[]{"/"}, StringSplitOptions.RemoveEmptyEntries);
        //        if (arResources.Length > 0)
        //        {
        //            if((arResources.Length==1)){
        //                subModuleResourceId= arResources[0];

        //            }else{
        //                string resourcevalue = arResources[arResources.Length - 2];
        //                if (resourcevalue.ToUpper() == "REPORT")
        //                {
        //                    subModuleResourceId = arResources[arResources.Length - 1];
        //                }
        //                else
        //                {
        //                    subModuleResourceId = resourcevalue;
        //                }
                       
                       
        //            }
        //        }else{
        //             subModuleResourceId= resourceId;
        //        }
        //    }
        //    try
        //    {
        //        return HttpContext.GetGlobalResourceObject("ResourceSubModuleItem", subModuleResourceId).ToString();
        //    }
        //    catch (Exception e)
        //    {
        //        return subModuleResourceId;
        //    }
        //    return null;
        //}

        private string GetSubModuleItemResourceValueByDatabaseId(string resourceId)
        {
            string subModuleResourceId = string.Empty;
            if (resourceId.Contains(" "))
            {
                subModuleResourceId = resourceId.Replace(" ", "");
            }
            else if (resourceId.Contains("/"))
            {
                subModuleResourceId = resourceId.Replace("/", "");
                
            }
            try
            {
                return HttpContext.GetGlobalResourceObject("ResourceSubModuleItem", subModuleResourceId).ToString();
            }
            catch (Exception e)
            {
                return subModuleResourceId;
            }
            return null;
        }

        public JsonResult GetSubModule(int id)
        {
            var subModule = this.subModuleService.GetSubModule(id);
            return Json(subModule);
        }
    }
}