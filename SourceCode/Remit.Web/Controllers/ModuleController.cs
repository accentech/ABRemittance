using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using System.Globalization;
using Remit.CachingService;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class ModuleController : Controller
    {
        public readonly IModuleService moduleService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:module" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /Module/
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
                    return View("Module");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ModuleController(IModuleService moduleService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.moduleService = moduleService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        [HttpPost]
        public JsonResult CreateModule(Module module)
        {
            const string url = "/Module/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = module.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(module))
                    {

                        if (this.moduleService.CreateModule(module))
                        {
                            isSuccess = true;
                            message = "Module saved successfully!";
                        }
                        else
                        {
                            message = "Module could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same module name found!";
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
                    if (this.moduleService.UpdateModule(module))
                    {
                        isSuccess = true;
                        message = "Module updated successfully!";
                        cacheProvider.Invalidate("module" + Helpers.UserSession.GetUserFromSession().RoleId);
                    }
                    else
                    {
                        message = "Module could not updated!";
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
        private bool CheckIsExist(Model.Models.Module module)
        {
            return this.moduleService.CheckIsExist(module);
        }

        [HttpPost]
        public JsonResult DeleteModule(Module module)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/Moduel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.moduleService.DeleteModule(module.Id);
                if (isSuccess)
                {
                    message = "Module deleted successfully!";

                }
                else
                {
                    message = "Module can't be deleted!";
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

        private string GetMenuResourceValueByDatabaseId(string resourceId)
        {
            try
            {
                return HttpContext.GetGlobalResourceObject("ResourceMenu", resourceId).ToString();
            }
            catch (Exception e)
            {
                return resourceId;
            }
            //return null;
        }

        public JsonResult GetAllModuleList()
        {
            var LoggedUser = UserSession.GetUserFromSession();
            if (LoggedUser != null)
            {
                var moduleListObj = this.moduleService.GetAllModule();
                List<ModuleViewModel> moduleVMList = new List<ModuleViewModel>();


                foreach (var module in moduleListObj)
                {
                    ModuleViewModel moduleTemp = new ModuleViewModel();
                    moduleTemp.Id = module.Id;
                    if (module.Name != null)
                    {
                        moduleTemp.NameFromResource = GetMenuResourceValueByDatabaseId(module.Name.Replace(" ",""));
                    }
                    moduleTemp.Name = module.Name;
                    moduleTemp.ImageName = module.ImageName;
                    moduleTemp.Ordering = module.Ordering;

                    if (module.IsActive != null)
                        moduleTemp.IsActive = module.IsActive.Value;

                    moduleVMList.Add(moduleTemp);
                }
                return Json(moduleVMList, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public JsonResult GetActiveModuleList()
        {
            var LoggedUser = UserSession.GetUserFromSession();
            if (LoggedUser != null)
            {
                var moduleListObj = this.moduleService.GetAllModule().Where(m=>m.IsActive==true);
                List<ModuleViewModel> moduleVMList = new List<ModuleViewModel>();


                foreach (var module in moduleListObj)
                {
                    ModuleViewModel moduleTemp = new ModuleViewModel();
                    moduleTemp.Id = module.Id;
                    if (module.Name != null)
                    {
                        moduleTemp.NameFromResource = module.Name;//GetMenuResourceValueByDatabaseId(module.Name);
                    }
                    moduleTemp.Name = module.Name;
                    moduleTemp.ImageName = module.ImageName;
                    moduleTemp.Ordering = module.Ordering;

                    if (module.IsActive != null)
                        moduleTemp.IsActive = module.IsActive.Value;

                    moduleVMList.Add(moduleTemp);
                }
                return Json(moduleVMList, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public JsonResult GetModuleList()
        {
            var LoggedUser = UserSession.GetUserFromSession();
            if (LoggedUser != null)
            {
                var moduleListObj = this.moduleService.GetAllModuleByRoleId(LoggedUser.RoleId);
                List<ModuleViewModel> moduleVMList = new List<ModuleViewModel>();


                foreach (var module in moduleListObj)
                {
                    ModuleViewModel moduleTemp = new ModuleViewModel();
                    moduleTemp.Id = module.Id;
                    if (module.Name != null)
                    {
                        moduleTemp.NameFromResource = GetMenuResourceValueByDatabaseId(module.Name.Replace(" ",""));
                    }
                    moduleTemp.Name = module.Name;
                    moduleTemp.ImageName = module.ImageName;
                    moduleTemp.Ordering = module.Ordering;

                    if (module.IsActive!=null)
                        moduleTemp.IsActive = module.IsActive.Value;

                    moduleVMList.Add(moduleTemp);
                }
                return Json(moduleVMList, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        public JsonResult GetModule(int id)
        {
            var module = this.moduleService.GetModule(id);
            return Json(module);
        }

    }



    public class ModuleViewModel
    {
        public ModuleViewModel()
        {
            this.SubModules = new List<SubModuleViewModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string NameFromResource { get; set; }
        public string ImageName { get; set; }
        public int? Ordering { get; set; }
        public bool IsActive { get; set; }
        public List<SubModuleViewModel> SubModules { get; set; }
    }

    public class SubModuleViewModel
    {
        public SubModuleViewModel()
        {
            this.SubModuleItems = new List<SubModuleItemViewModel>();
        }

        public int Id { get; set; }
        public Nullable<int> ModuleId { get; set; }
        public string Name { get; set; }
        public string ModuleName { get; set; }
        public string NameFromResource { get; set; }
        public int? Ordering { get; set; }
        public bool IsActive { get; set; }
        public virtual Module Module { get; set; }

        public List<SubModuleItemViewModel> SubModuleItems { get; set; }
    }

    public class SubModuleItemViewModel
    {
        public SubModuleItemViewModel()
        {
            this.RoleSubModuleItem = new RoleSubModuleItemViewModel();
        }

        public int Id { get; set; }
        public Nullable<int> SubModuleId { get; set; }
        public string Name { get; set; }
        public string NameFromResource { get; set; }
        public string UrlPath { get; set; }      
        public int? Ordering { get; set; }
        public bool? IsBaseItem { get; set; }   
        public string SubModuleName { get; set; }
        public string ModuleName { get; set; }
        public int? ModuleId { get; set; }
        //public Nullable<int> StationId { get; set; }
        //public string StationName { get; set; }
        public int? BaseItemId { get; set; }
        public string BaseItemName { get; set; }
        public bool IsActive { get; set; }
        RoleSubModuleItemViewModel RoleSubModuleItem { get; set; }
    }

    public class ActivityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool SelectionStatus { get; set; }
        public string StationName { get; set; }
    }

}