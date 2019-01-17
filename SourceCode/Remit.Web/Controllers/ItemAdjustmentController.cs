using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using Remit.Web.Helpers;
using Remit.Service.Enums;
using System.Linq.Dynamic;
using Remit.Web.Models;
using System.Text.RegularExpressions;
using Remit.Service.Utilities;
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class ItemAdjustmentController : Controller
    {
        public readonly IItemAdjustmentService itemAdjustmentService;
        public readonly IItemAdjustmentDetailService itemAdjustmentDetailService;
        public readonly IItemInventoryHistoryService itemInventoryHistoryService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly IItemInventoryService itemInventoryService;

        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public ItemAdjustmentController(IItemAdjustmentService itemAdjustmentService, IItemInventoryHistoryService itemInventoryHistoryService,
            IRoleSubModuleItemService roleSubModuleItemService, IItemAdjustmentDetailService itemAdjustmentDetailService, 
            IWorkflowactionSettingService workflowactionSettingService, IItemInventoryService itemInventoryService, IItemService itemService)
        {
            this.itemAdjustmentService = itemAdjustmentService;
            this.itemInventoryHistoryService = itemInventoryHistoryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.itemAdjustmentDetailService = itemAdjustmentDetailService;

            this.workflowactionSettingService = workflowactionSettingService;
            this.itemInventoryService = itemInventoryService;
            this.itemService = itemService;
        }

        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:itemadjustment" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /ItemAdjustment/
        public ActionResult Index()
        {
            const string url = "/ItemAdjustment/Index";
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            Session["GroupT"] = "raw";
            cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            return CommonItemAdjustment(url, cacheKey, "ItemAdjustment");
        }

        public ActionResult SparePartsAndOtherIndex()
        {
            ViewBag.GroupType = "";
            Session["GroupT"] = "spare";
            const string url = "/ItemAdjustment/SparePartsAndOtherIndex";
            return CommonItemAdjustment(url, cacheKey, "ItemAdjustment");
        }

        public ActionResult CommonItemAdjustment(string url, string cacheKey, string viewName)
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                                 roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);
            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View(viewName);
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateItemAdjustment(ItemAdjustment itemadjustment)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                var isNew = itemadjustment.Id == 0 ? true : false;

                string url = string.Empty;
                if ((string)Session["GroupT"] == "raw")
                {
                    url = "/ItemAdjustment/Index";
                    cacheKey += WebConfigurationManager.AppSettings["GroupType"];
                }
                else{
                    url = "/ItemAdjustment/SparePartsAndOtherIndex";
                }

                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                  roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                      Helpers.UserSession.GetUserFromSession().RoleId);
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {
                        itemadjustment.Date = itemadjustment.Date != null ? itemadjustment.Date.Value.ToUniversalTime() : DateTime.UtcNow;
                        if (this.itemAdjustmentService.CreateItemAdjustment(itemadjustment))
                        {
                            isSuccess = true;
                            message += string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceItemAdjustment.LblItemAdjustment);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceItemAdjustment.LblItemAdjustment);
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
                        var itemadjustmentObj = this.itemAdjustmentService.GetItemAdjustment(itemadjustment.Id);
                        if (itemadjustmentObj.ItemAdjustmentDetails != null)
                        {
                            foreach (var a in itemadjustmentObj.ItemAdjustmentDetails.ToList())
                            {
                                InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService,
                                    itemInventoryService);
                                try
                                {
                                    ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(b => b.BinCardId == a.BinCardId && b.ItemId == a.ItemId).FirstOrDefault();
                                    if (itemInventory != null)
                                    {
                                        inventoryUtility.UpdateItmeBalance(itemInventory, a.InventoryQuantity - a.ActualQuantity, true);
                                    }
                                    inventoryUtility.SaveInventoryHistory(a.ItemId, a.InventoryQuantity - a.ActualQuantity, a.UnitId, a.BinCardId, a.Id.ToString(), "ItemAdjustment", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Delete);
                                }
                                catch { }

                                this.itemAdjustmentDetailService.DeleteItemAdjustmentDetail(a.Id);
                            }
                        }

                        if (itemadjustmentObj.ItemAdjustmentDetails != null)
                        {
                            foreach (var detin in itemadjustment.ItemAdjustmentDetails)
                            {
                                try
                                {
                                    detin.ItemAdjustmentId = itemadjustment.Id;
                                    this.itemAdjustmentDetailService.CreateItemAdjustmentDetail(detin);
                                }
                                catch { };
                            }
                        }

                        itemadjustmentObj.Date = itemadjustment.Date.Value.ToUniversalTime();

                        if (this.itemAdjustmentService.UpdateItemAdjustment(itemadjustmentObj))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceItemAdjustment.LblItemAdjustment);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceItemAdjustment.LblItemAdjustment);
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                message = ex.Message;
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteItemAdjustment(ItemAdjustment itemadjustment)
        {
            var isSuccess = true;
            var message = string.Empty;

            string url = string.Empty;
            if ((string)Session["GroupT"] == "raw")
            {
                url = "/ItemAdjustment/Index";
                cacheKey += WebConfigurationManager.AppSettings["GroupType"];
            }else{
                url = "/ItemAdjustment/SparePartsAndOtherIndex";
            }

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {

                foreach (var a in itemadjustment.ItemAdjustmentDetails.ToList())
                {
                    this.itemAdjustmentDetailService.DeleteItemAdjustmentDetail(a.Id);
                }
                isSuccess = this.itemAdjustmentService.DeleteItemAdjustment(itemadjustment.Id);
                message = isSuccess ? "Item Adjustment deleted successfully!" : "Item Adjustment can't be deleted!";

                itemadjustment = this.itemAdjustmentService.GetItemAdjustment(itemadjustment.Id);
                if (itemadjustment != null)
                {
                    foreach (var ItemAdjustmentDetail in itemadjustment.ItemAdjustmentDetails.ToList())
                    {
                        InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService, itemInventoryService);
                        try
                        {
                            ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(a => a.BinCardId == ItemAdjustmentDetail.BinCardId && a.ItemId == ItemAdjustmentDetail.ItemId).FirstOrDefault();
                            if (itemInventory != null)
                            {
                                inventoryUtility.UpdateItmeBalance(itemInventory, ItemAdjustmentDetail.InventoryQuantity - ItemAdjustmentDetail.ActualQuantity, true);
                            }
                            inventoryUtility.SaveInventoryHistory(ItemAdjustmentDetail.ItemId, ItemAdjustmentDetail.InventoryQuantity - ItemAdjustmentDetail.ActualQuantity, ItemAdjustmentDetail.Item.UsageUnitId, ItemAdjustmentDetail.BinCardId, ItemAdjustmentDetail.Id.ToString(), "Item Adjustment", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Delete);
                        }
                        catch { }
                    }

                    itemadjustment.IsDeleted = true;
                    itemadjustment.DeletedBy = UserSession.GetUserFromSession().EmployeeId;
                    itemadjustment.DeletedOn = DateTime.UtcNow;

                    isSuccess = this.itemAdjustmentService.UpdateItemAdjustment(itemadjustment);
                    message = string.Format(isSuccess ? Resources.ResourceCommon.CMsg_delete : Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceItemAdjustment.LblItemAdjustment);
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

        public JsonResult GetItemAdjustmentListByGroupType()
        {
            var itemadjustmentListObj = new List<ItemAdjustment>();
            if ((string)Session["GroupT"] == "raw")
            {
                itemadjustmentListObj = this.itemAdjustmentService.GetAllItemAdjustment().Where(dis =>
                {
                    var itemAdjustmentDetail = dis.ItemAdjustmentDetails.FirstOrDefault();
                    return itemAdjustmentDetail != null && itemAdjustmentDetail.Item.ItemCategory.ItemGroup.TypeId ==
                           Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                }).ToList();
            }
            else
            {
                itemadjustmentListObj = this.itemAdjustmentService.GetAllItemAdjustment().Where(dis =>
                {
                    var itemAdjustmentDetail = dis.ItemAdjustmentDetails.FirstOrDefault();
                    return itemAdjustmentDetail != null && itemAdjustmentDetail.Item.ItemCategory.ItemGroup.TypeId !=
                           Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                }).ToList();
            }
           
            List<ItemAdjustmentViewModel> itemadjustmentVMList = new List<ItemAdjustmentViewModel>();

            foreach (var itemadjustment in itemadjustmentListObj)
            {
                ItemAdjustmentViewModel itemadjustmentTemp = new ItemAdjustmentViewModel();

                itemadjustmentTemp.Id = itemadjustment.Id;

                itemadjustmentTemp.Date = itemadjustment.Date != null ? itemadjustment.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemadjustmentTemp.AuthorisedBy = itemadjustment.AuthorisedBy;

                if (itemadjustment.Employee != null)
                {
                    itemadjustmentTemp.AuthorisedByName = itemadjustment.Employee.FullName;
                }

                itemadjustmentTemp.AuthorisedDate = itemadjustment.AuthorisedDate != null ? itemadjustment.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemadjustmentVMList.Add(itemadjustmentTemp);
            }
            return Json(itemadjustmentVMList, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetItemAdjustmentList()
        {
            var itemadjustmentListObj = this.itemAdjustmentService.GetAllItemAdjustment().Where(a => a.IsDeleted != true);
            List<ItemAdjustmentViewModel> itemadjustmentVMList = new List<ItemAdjustmentViewModel>();

            foreach (var itemadjustment in itemadjustmentListObj)
            {
                ItemAdjustmentViewModel itemadjustmentTemp = new ItemAdjustmentViewModel();

                itemadjustmentTemp.Id = itemadjustment.Id;

                itemadjustmentTemp.Date = itemadjustment.Date != null ? itemadjustment.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemadjustmentTemp.AuthorisedBy = itemadjustment.AuthorisedBy;

                if (itemadjustment.Employee != null)
                {
                    itemadjustmentTemp.AuthorisedByName = itemadjustment.Employee.FullName;
                }

                itemadjustmentTemp.AuthorisedDate = itemadjustment.AuthorisedDate != null ? itemadjustment.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemadjustmentVMList.Add(itemadjustmentTemp);
            }
            return Json(itemadjustmentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPiListByYear(int year)
        {

            var itemadjustmentListObj  = new List<ItemAdjustment>();
            if ((string) Session["GroupT"] == "raw")
            {
                itemadjustmentListObj = this.itemAdjustmentService.GetAllItemAdjustment().Where(a =>
                {
                    var itemAdjustmentDetail = a.ItemAdjustmentDetails.FirstOrDefault(); 
                    return itemAdjustmentDetail != null && (a.Date != null && a.Date.Value.AddMinutes(timeZoneOffset).Year == year && a.IsDeleted != true &&
                                                                                 itemAdjustmentDetail.Item.ItemCategory.ItemGroup.TypeId ==
                                                                                 Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]));
                }).ToList();
            }
            else
            {
                itemadjustmentListObj = this.itemAdjustmentService.GetAllItemAdjustment().Where(a =>
                {
                    var itemAdjustmentDetail = a.ItemAdjustmentDetails.FirstOrDefault();
                    return itemAdjustmentDetail != null && (a.Date != null && a.Date.Value.AddMinutes(timeZoneOffset).Year == year && a.IsDeleted != true &&
                                                         itemAdjustmentDetail.Item.ItemCategory.ItemGroup.TypeId !=
                                                         Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]));
                }).ToList();
            }
            

            List<ItemAdjustmentViewModel> itemadjustmentVMList = new List<ItemAdjustmentViewModel>();
            foreach (var itemadjustment in itemadjustmentListObj)
            {
                ItemAdjustmentViewModel itemadjustmentTemp = new ItemAdjustmentViewModel();
                itemadjustmentTemp.Id = itemadjustment.Id;
                itemadjustmentTemp.Date = itemadjustment.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                itemadjustmentVMList.Add(itemadjustmentTemp);
            }
            return Json(itemadjustmentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemAdjustment(int id)
        {
            var itemadjustment = this.itemAdjustmentService.GetItemAdjustment(id);
            ItemAdjustmentViewModel itemadjustmentTemp = null;
            var BinCardQty = new ItemInventory();
            if (itemadjustment != null)
            {
                itemadjustmentTemp = new ItemAdjustmentViewModel();
                itemadjustmentTemp.Id = itemadjustment.Id;

                itemadjustmentTemp.Date = itemadjustment.Date != null ? itemadjustment.Date.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                itemadjustmentTemp.AuthorisedBy = itemadjustment.AuthorisedBy;
                itemadjustmentTemp.AuthorisedDate = itemadjustment.AuthorisedDate != null ? itemadjustment.AuthorisedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";

                if (itemadjustment.ItemAdjustmentDetails.Count() > 0)
                {
                    List<ItemAdjustmentDetailViewModel> itemadjustmentDetailVMList = new List<ItemAdjustmentDetailViewModel>();
                    foreach (var itemadjustmentDetai in itemadjustment.ItemAdjustmentDetails)
                    {
                        ItemAdjustmentDetailViewModel itemadjustmentDetailTtemp = new ItemAdjustmentDetailViewModel();
                        itemadjustmentDetailTtemp.Id = itemadjustmentDetai.Id;
                        itemadjustmentDetailTtemp.ItemAdjustmentId = (int)itemadjustmentDetai.ItemAdjustmentId;
                        itemadjustmentDetailTtemp.ItemId = (Guid)itemadjustmentDetai.ItemId;
                        itemadjustmentDetailTtemp.ItemName = itemadjustmentDetai.Item.Name;
                        if (itemadjustmentDetai.Item != null)
                        {
                            itemadjustmentDetailTtemp.ItemCategoryId = itemadjustmentDetai.Item.ItemCategoryId;
                        }

                        itemadjustmentDetailTtemp.UnitId = itemadjustmentDetai.UnitId;
                        itemadjustmentDetailTtemp.UnitName = itemadjustmentDetai.UnitOfMeasurement.Name;
                        if (itemadjustmentDetai.BinCard != null)
                        {
                            itemadjustmentDetailTtemp.BinCardId = itemadjustmentDetai.BinCard.Id;
                            itemadjustmentDetailTtemp.BinCardNo = itemadjustmentDetai.BinCard.CardNo;
                        }
                        itemadjustmentDetailTtemp.InventoryQuantity = itemadjustmentDetai.InventoryQuantity;
                        itemadjustmentDetailTtemp.ActualQuantity = itemadjustmentDetai.ActualQuantity;

                        itemadjustmentDetailTtemp.AdjustmentReason = itemadjustmentDetai.AdjustmentReason;

                        itemadjustmentDetailVMList.Add(itemadjustmentDetailTtemp);
                    }
                    itemadjustmentTemp.ItemAdjustmentDetails = itemadjustmentDetailVMList;
                }
            }
            return Json(itemadjustmentTemp, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemAdjustmentViewModel
    {
        public ItemAdjustmentViewModel()
        {
            this.ItemAdjustmentDetails = new List<ItemAdjustmentDetailViewModel>();
        }
        public int Id { get; set; }
        public int? AuthorisedBy { get; set; }
        public string Date { get; set; }
        public string AuthorisedByName { get; set; }
        public string AuthorisedDate { get; set; }
        public virtual ICollection<ItemAdjustmentDetailViewModel> ItemAdjustmentDetails { get; set; }
    }

    public partial class ItemAdjustmentDetailViewModel
    {
        public int Id { get; set; }
        public int ItemAdjustmentId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemCategoryId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public string BinCardNo { get; set; }
        public double InventoryQuantity { get; set; }
        public double ActualQuantity { get; set; }
        public string AdjustmentReason { get; set; }
    }
}