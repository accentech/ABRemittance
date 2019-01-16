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


namespace Remit.Web.Controllers
{
    public class LCShipmentExpenseHeadCategoryController : Controller
    {
        public readonly ILCShipmentExpenseHeadCategoryService lcshipmentexpenseheadcategoryService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public LCShipmentExpenseHeadCategoryController(ILCShipmentExpenseHeadCategoryService lcshipmentexpenseheadcategoryService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.lcshipmentexpenseheadcategoryService = lcshipmentexpenseheadcategoryService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        string cacheKey = "permission:lcshipmentexpenseheadcategory" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;


        // GET: /LCShipmentExpenseHeadCategory/
        public ActionResult Index()
        {
            const string url = "/LCShipmentExpenseHeadCategory/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("LCShipmentExpenseHeadCategory");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult LCShipmentExpenseHeadCategory()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateLCShipmentExpenseHeadCategory(LCShipmentExpenseHeadCategory lcshipmentexpenseheadcategory)
        {
            var isSuccess = false;
            var message = string.Empty;
            var isNew = lcshipmentexpenseheadcategoryService.GetLCShipmentExpenseHeadCategory(lcshipmentexpenseheadcategory.Id);
            const string url = "/LCShipmentExpenseHeadCategory/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew == null)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(lcshipmentexpenseheadcategory))
                    {
                        if (this.lcshipmentexpenseheadcategoryService.CreateLCShipmentExpenseHeadCategory(lcshipmentexpenseheadcategory))
                        {
                            isSuccess = true;
                            message = "LCShipmentExpenseHeadCategory saved successfully!";
                        }
                        else
                        {
                            message = "LCShipmentExpenseHeadCategory could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same lcshipmentexpenseheadcategory name found!";
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
                    isNew.HeadCategory = lcshipmentexpenseheadcategory.HeadCategory;

                    if (this.lcshipmentexpenseheadcategoryService.UpdateLCShipmentExpenseHeadCategory(isNew))
                    {
                        isSuccess = true;
                        message = "LCShipmentExpenseHeadCategory updated successfully!";
                    }
                    else
                    {
                        message = "LCShipmentExpenseHeadCategory could not updated!";
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
        private bool CheckIsExist(Model.Models.LCShipmentExpenseHeadCategory lcshipmentexpenseheadcategory)
        {
            return this.lcshipmentexpenseheadcategoryService.CheckIsExist(lcshipmentexpenseheadcategory);
        }
        [HttpPost]
        public JsonResult DeleteLCShipmentExpenseHeadCategory(LCShipmentExpenseHeadCategory lcshipmentexpenseheadcategory)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/LCShipmentExpenseHeadCategory/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.lcshipmentexpenseheadcategoryService.DeleteLCShipmentExpenseHeadCategory(lcshipmentexpenseheadcategory.Id);
                if (isSuccess)
                {
                    message = "LCShipmentExpenseHeadCategory deleted successfully!";

                }
                else
                {
                    message = "LCShipmentExpenseHeadCategory can't be deleted!";
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

        public JsonResult GetLCShipmentExpenseHeadCategoryList()
        {
            var lcshipmentexpenseheadcategoryListObj = this.lcshipmentexpenseheadcategoryService.GetAllLCShipmentExpenseHeadCategory();
            List<LCShipmentExpenseHeadCategoryViewModel> lcshipmentexpenseheadcategoryVMList = new List<LCShipmentExpenseHeadCategoryViewModel>();
            foreach (var lcshipmentexpenseheadcategory in lcshipmentexpenseheadcategoryListObj)
            {
                LCShipmentExpenseHeadCategoryViewModel lcshipmentexpenseheadcategoryTemp = new LCShipmentExpenseHeadCategoryViewModel();
                lcshipmentexpenseheadcategoryTemp.Id = lcshipmentexpenseheadcategory.Id;
                lcshipmentexpenseheadcategoryTemp.HeadCategory = lcshipmentexpenseheadcategory.HeadCategory;

                List<LCShipmentExpenseHeadViewModel> expenceHeadList = new List<LCShipmentExpenseHeadViewModel>();
                if (lcshipmentexpenseheadcategory.LCShipmentExpenseHeads.Any())
                {
                    if (lcshipmentexpenseheadcategory.LCShipmentExpenseHeads.Any())
                    {
                        foreach (var aExpenseHead in lcshipmentexpenseheadcategory.LCShipmentExpenseHeads)
                        {
                            LCShipmentExpenseHeadViewModel lcshipmentexpenseheadTemp = new LCShipmentExpenseHeadViewModel();
                            lcshipmentexpenseheadTemp.Id = aExpenseHead.Id;
                            lcshipmentexpenseheadTemp.Head = aExpenseHead.Head;
                            lcshipmentexpenseheadTemp.AmountInLocal = 0;
                            expenceHeadList.Add(lcshipmentexpenseheadTemp);
                        }
                    }
                    lcshipmentexpenseheadcategoryTemp.LCShipmentExpenseHeads = expenceHeadList;

                }
                lcshipmentexpenseheadcategoryVMList.Add(lcshipmentexpenseheadcategoryTemp);
            }
            return Json(lcshipmentexpenseheadcategoryVMList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetLCShipmentExpenseHeadCategoryListForShipment()
        {
            var lcshipmentexpenseheadcategoryListObj = this.lcshipmentexpenseheadcategoryService.GetAllLCShipmentExpenseHeadCategory();
            List<LCShipmentExpenseHeadCategoryViewModel> lcshipmentexpenseheadcategoryVMList = new List<LCShipmentExpenseHeadCategoryViewModel>();
            foreach (var lcshipmentexpenseheadcategory in lcshipmentexpenseheadcategoryListObj)
            {
                if (lcshipmentexpenseheadcategory.LCShipmentExpenseHeads.Any())
                {
                    LCShipmentExpenseHeadCategoryViewModel lcshipmentexpenseheadcategoryTemp = new LCShipmentExpenseHeadCategoryViewModel();
                    lcshipmentexpenseheadcategoryTemp.Id = lcshipmentexpenseheadcategory.Id;
                    lcshipmentexpenseheadcategoryTemp.HeadCategory = lcshipmentexpenseheadcategory.HeadCategory;

                    List<LCShipmentExpenseHeadViewModel> expenceHeadList = new List<LCShipmentExpenseHeadViewModel>();
                    if (lcshipmentexpenseheadcategory.LCShipmentExpenseHeads.Any())
                    {
                        foreach (var aExpenseHead in lcshipmentexpenseheadcategory.LCShipmentExpenseHeads)
                        {
                            LCShipmentExpenseHeadViewModel lcshipmentexpenseheadTemp = new LCShipmentExpenseHeadViewModel();
                            lcshipmentexpenseheadTemp.Id = aExpenseHead.Id;
                            lcshipmentexpenseheadTemp.Head = aExpenseHead.Head;
                            lcshipmentexpenseheadTemp.AmountInLocal = 0;
                            expenceHeadList.Add(lcshipmentexpenseheadTemp);
                        }
                    }
                    lcshipmentexpenseheadcategoryTemp.LCShipmentExpenseHeads = expenceHeadList;
                    lcshipmentexpenseheadcategoryVMList.Add(lcshipmentexpenseheadcategoryTemp);
                }
            }
            return Json(lcshipmentexpenseheadcategoryVMList, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetLCShipmentExpenseHeadCategory(int id)
        {
            var lcshipmentexpenseheadcategory = this.lcshipmentexpenseheadcategoryService.GetLCShipmentExpenseHeadCategory(id);
            return Json(lcshipmentexpenseheadcategory);
        }
    }

    public class LCShipmentExpenseHeadCategoryViewModel
    {
        public LCShipmentExpenseHeadCategoryViewModel()
        {
            this.LCShipmentExpenseHeads = new List<LCShipmentExpenseHeadViewModel>();
        }
        public int Id { get; set; }
        public string HeadCategory { get; set; }

        public ICollection<LCShipmentExpenseHeadViewModel> LCShipmentExpenseHeads { set; get; }

    }
}