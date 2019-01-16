
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
    public class LCShipmentExpenseHeadController : Controller
    {
        public readonly ICountryService countryService;
        public readonly ILCShipmentExpenseHeadService lcshipmentexpenseheadService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:lcshipmentexpensehead" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /LCShipmentExpenseHead/
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
                    return View("LCShipmentExpenseHead");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public LCShipmentExpenseHeadController(ILCShipmentExpenseHeadService lcshipmentexpenseheadService, ICountryService countryService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.lcshipmentexpenseheadService = lcshipmentexpenseheadService;
            this.countryService = countryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        
        [HttpPost]
        public JsonResult CreateLCShipmentExpenseHead(LCShipmentExpenseHead lcshipmentexpensehead)
        {
            const string url = "/LCShipmentExpenseHead/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey);
            if (permission == null)
                permission = roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = lcshipmentexpensehead.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(lcshipmentexpensehead))
                    {
                        if (this.lcshipmentexpenseheadService.CreateLCShipmentExpenseHead(lcshipmentexpensehead))
                        {
                            isSuccess = true;
                            message = "LCShipmentExpenseHead saved successfully!";
                        }
                        else
                        {
                            message = "LCShipmentExpenseHead could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same lcshipmentexpensehead name found!";
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
                    if (this.lcshipmentexpenseheadService.UpdateLCShipmentExpenseHead(lcshipmentexpensehead))
                    {
                        isSuccess = true;
                        message = "LCShipmentExpenseHead updated successfully!";
                    }
                    else
                    {
                        message = "LCShipmentExpenseHead could not updated!";
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
        private bool CheckIsExist(Model.Models.LCShipmentExpenseHead lcshipmentexpensehead)
        {
            return this.lcshipmentexpenseheadService.CheckIsExist(lcshipmentexpensehead);
        }


        [HttpPost]
        public JsonResult DeleteLCShipmentExpenseHead(LCShipmentExpenseHead lcshipmentexpensehead)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.lcshipmentexpenseheadService.DeleteLCShipmentExpenseHead(lcshipmentexpensehead.Id);
                if (isSuccess)
                {
                    message = "LCShipmentExpenseHead deleted successfully!";
                }
                else
                {
                    message = "LCShipmentExpenseHead can't be deleted!";
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

        public JsonResult GetLCShipmentExpenseHeadList()
        {
            var lcshipmentexpenseheadListObj = this.lcshipmentexpenseheadService.GetAllLCShipmentExpenseHead();
            List<LCShipmentExpenseHeadViewModel> lcshipmentexpenseheadVMList = new List<LCShipmentExpenseHeadViewModel>();

            foreach (var lcshipmentexpensehead in lcshipmentexpenseheadListObj)
            {
                LCShipmentExpenseHeadViewModel lcshipmentexpenseheadTemp = new LCShipmentExpenseHeadViewModel();
                lcshipmentexpenseheadTemp.Id = lcshipmentexpensehead.Id;
                lcshipmentexpenseheadTemp.Head = lcshipmentexpensehead.Head;
                if (lcshipmentexpensehead.LCShipmentExpenseHeadCategory != null)
                {
                    lcshipmentexpenseheadTemp.HeadCategory = lcshipmentexpensehead.LCShipmentExpenseHeadCategory.HeadCategory;
                    lcshipmentexpenseheadTemp.LCShipmentExpenseHeadCategoryId = lcshipmentexpensehead.LCShipmentExpenseHeadCategoryId;
                    lcshipmentexpenseheadTemp.LCShipmentExpenseHeadCategoryName = lcshipmentexpensehead.LCShipmentExpenseHeadCategory.HeadCategory;
                }
                lcshipmentexpenseheadVMList.Add(lcshipmentexpenseheadTemp);
            }
            return Json(lcshipmentexpenseheadVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLCShipmentExpenseHead(int id)
        {
            var lcshipmentexpensehead = this.lcshipmentexpenseheadService.GetLCShipmentExpenseHead(id);
            return Json(lcshipmentexpensehead);
        }
    }

    public class LCShipmentExpenseHeadViewModel
    {
        public int Id { get; set; }
        public int LCShipmentExpenseHeadCategoryId { get; set; }
        public string Head { get; set; }
        public string HeadCategory { get; set; }
        public string LCShipmentExpenseHeadCategoryName { get; set; }

        public double? AmountInLocal { set; get; }
        public virtual LCShipmentExpenseHeadCategory LCShipmentExpenseHeadCategory { get; set; }
        

    }

}