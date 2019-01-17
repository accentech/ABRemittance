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
using Remit.Web.Models;
using Remit.Service.Enums;
using Remit.Service.Utilities;
using System.Linq.Dynamic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class ItemOpeningController : Controller
    {
        public readonly IItemOpeningService itemOpeningService;
        public readonly IItemService itemService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly INotificationSettingService notificationSettingService;

        public readonly IItemInventoryService itemInventoryService;
        public readonly IItemInventoryHistoryService itemInventoryHistoryService;

        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public ItemOpeningController(
            IItemOpeningService itemOpeningService,
            ISubModuleItemService subModuleItemService,
            IRoleSubModuleItemService roleSubModuleItemService,
            IItemInventoryService itemInventoryService,
            IItemInventoryHistoryService itemInventoryHistoryService,
            IWorkflowactionSettingService workflowactionSettingService,
            INotificationSettingService notificationSettingService, IItemService itemService)
        {
            this.itemOpeningService = itemOpeningService;
            this.subModuleItemService = subModuleItemService;
            this.itemInventoryService = itemInventoryService;
            this.itemInventoryHistoryService = itemInventoryHistoryService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.notificationSettingService = notificationSettingService;
            this.itemService = itemService;
        }

        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string cacheKey = "permission:itemOpening" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;
        string url = "/ItemOpening/Index";

        string grpType = WebConfigurationManager.AppSettings["GroupType"];

        // GET: /ItemOpening/
        public ActionResult Index()
        {
            Session["GroupT"] = "raw";
            return CommonView();
        }

        public ActionResult SparePartsAndOtherIndex()
        {
            Session["GroupT"] = "spare";
            return CommonView();
        }

        private ActionResult CommonView()
        {
            permission = CheckPermission();

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("ItemOpening");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        private RoleSubModuleItem CheckPermission()
        {
            if ((string)Session["GroupT"] == "raw")
            {
                cacheKey += grpType;
            }
            else
            {
                url = "/ItemOpening/SparePartsAndOtherIndex";
            }

            return (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);
        }

        public ActionResult GetRemainingItemList()
        {
            var success = false;
            var message = string.Empty;
            
            var itemOpenningIdList = itemOpeningService.GetAllItemOpening().GroupBy(a => a.ItemId).Select(c => c.Key).ToList();            

            var itemOpenningList = new List<ItemOpeningViewModel>();
            var itemList = new List<Item>();

            if ((string)Session["GroupT"] == "raw")
            {
                itemList = itemService.GetAllItem().Where(a => !itemOpenningIdList.Contains(a.Id) && a.ItemCategory.ItemGroup.TypeId == Convert.ToInt32(grpType)).OrderBy(b => b.ItemCategory.Name).ToList();
            }
            else
            {
                itemList = itemService.GetAllItem().Where(a => !itemOpenningIdList.Contains(a.Id) && a.ItemCategory.ItemGroup.TypeId != Convert.ToInt32(grpType)).OrderBy(b => b.ItemCategory.Name).ToList();
            }

            var itemObjList = itemList.ToList();
            if (itemObjList.Any())
            {
                foreach (var itemObj in itemObjList)
                {
                    var itemOpening = new ItemOpeningViewModel();
                    itemOpening.ItemId = itemObj.Id;
                    itemOpening.ItemName = itemObj.Name;
                    itemOpening.ItemCategoryId = itemObj.ItemCategoryId;
                    itemOpening.CategoryName = itemObj.ItemCategory != null ? itemObj.ItemCategory.Name : "";

                    itemOpening.UnitId = itemObj.UsageUnitId;
                    itemOpening.UnitName = itemObj.UnitOfMeasurement1 != null ? itemObj.UnitOfMeasurement1.Name:"";

                    itemOpenningList.Add(itemOpening);
                }
                success = true;
            }

            return Json(new
            {
                isSuccess = success,
                List = itemOpenningList
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItemOpening(string openingdate)
        {
            var success = false;
            var message = string.Empty;
            var itemOpenningObjList = new List<ItemOpening>();

            if ((string)Session["GroupT"] == "raw")
            {
                itemOpenningObjList = itemOpeningService.GetAllItemOpening().Where(a => a.OpeningDate.Value.AddMinutes(timeZoneOffset).Date == DateTime.Parse(openingdate) && a.Item.ItemCategory.ItemGroup.TypeId == Convert.ToInt32(grpType)).OrderBy(b => b.Item.ItemCategory.Name).ThenBy(b => b.Item.Name).ToList();
            }
            else
            {
                itemOpenningObjList = itemOpeningService.GetAllItemOpening().Where(a => a.OpeningDate.Value.AddMinutes(timeZoneOffset).Date == DateTime.Parse(openingdate) && a.Item.ItemCategory.ItemGroup.TypeId != Convert.ToInt32(grpType)).OrderBy(b => b.Item.ItemCategory.Name).ThenBy(b => b.Item.Name).ToList();
            }

            var itemOpenningList = new List<ItemOpeningViewModel>();

            if (itemOpenningObjList.Any())
            {
                foreach (var itemOpenningObj in itemOpenningObjList.ToList())
                {
                    var itemOpening = new ItemOpeningViewModel();
                    itemOpening.Id = itemOpenningObj.Id;
                    itemOpening.ItemId = itemOpenningObj.ItemId;
                    if (itemOpenningObj.Item != null)
                    {
                        itemOpening.ItemName = itemOpenningObj.Item.Name;
                        itemOpening.ItemCategoryId = itemOpenningObj.Item.ItemCategoryId;
                        itemOpening.CategoryName = itemOpenningObj.Item.ItemCategory != null ? itemOpenningObj.Item.ItemCategory.Name : "";
                    }
                    itemOpening.OpeningDate = itemOpenningObj.OpeningDate != null ? itemOpenningObj.OpeningDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";
                    itemOpening.UnitId = itemOpenningObj.UnitId;
                    itemOpening.UnitName = itemOpenningObj.UnitOfMeasurement != null ? itemOpenningObj.UnitOfMeasurement.Name:"";
                    itemOpening.BinCardId = itemOpenningObj.BinCardId;
                    itemOpening.Quantity = itemOpenningObj.Quantity;
                    itemOpening.Status = itemOpenningObj.Status;
                    itemOpening.ApprovedBy = itemOpenningObj.ApprovedBy;
                    itemOpening.ApprovedDate = itemOpenningObj.ApprovedDate != null ? itemOpenningObj.ApprovedDate.Value.AddMinutes(timeZoneOffset).ToString(dateTimeFormat) : "";

                    itemOpenningList.Add(itemOpening);
                }
                success = true;
            }

            return Json(new
            {
                isSuccess = success,
                List = itemOpenningList
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOpeningDateListByYear(int year)
        {
            var itemOpeningDates = new List<DateTime>();
            
            if ((string)Session["GroupT"] == "raw")
            {
                itemOpeningDates = this.itemOpeningService.GetAllItemOpening().Where(a => a.OpeningDate.Value.AddMinutes(timeZoneOffset).Year == year && a.Item.ItemCategory.ItemGroup.TypeId == Convert.ToInt32(grpType)).GroupBy(b => b.OpeningDate.Value.AddMinutes(timeZoneOffset).Date).Select(c => c.Key).ToList();
            }
            else
            {
                itemOpeningDates = this.itemOpeningService.GetAllItemOpening().Where(a => a.OpeningDate.Value.AddMinutes(timeZoneOffset).Year == year && a.Item.ItemCategory.ItemGroup.TypeId != Convert.ToInt32(grpType)).GroupBy(b => b.OpeningDate.Value.AddMinutes(timeZoneOffset).Date).Select(c => c.Key).ToList();
            }
            
            List<string> openingDatelist = new List<string>();

            foreach (var itemOpeningDate in itemOpeningDates)
            {
                string openingDate = itemOpeningDate.ToString(dateFormat);
                openingDatelist.Add(openingDate);
            }
            return Json(openingDatelist, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateItemOpeningList(List<ItemOpening> itemOpeningList)
        {
            var isSuccess = false;
            var message = string.Empty;

            permission = CheckPermission(); 

            foreach (var itemOpening in itemOpeningList)
            {
                var isNew = itemOpening.Id == Guid.Empty ? true : false;
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {
                        itemOpening.Id = Guid.NewGuid();
                        itemOpening.OpeningDate = itemOpening.OpeningDate != null ? itemOpening.OpeningDate.Value.ToUniversalTime():DateTime.UtcNow;

                        if (this.itemOpeningService.CreateItemOpening(itemOpening))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceItemOpening.LblItemOpening);
                        }
                        else
                        {
                            isSuccess = false;
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceItemOpening.LblItemOpening);
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
                        var itemOpeningObj = this.itemOpeningService.GetItemOpening(itemOpening.Id);
                        if (itemOpeningObj.Status != (int)CommonEnum.Approved)
                        {
                            if(itemOpening.OpeningDate != null)
                                itemOpeningObj.OpeningDate = itemOpening.OpeningDate.Value.ToUniversalTime();
                            itemOpeningObj.Quantity = itemOpening.Quantity;

                            if (this.itemOpeningService.UpdateItemOpening(itemOpeningObj))
                            {
                                isSuccess = true;
                                message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceItemOpening.LblItemOpening);
                            }
                            else
                            {
                                isSuccess = false;
                                message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceItemOpening.LblItemOpening);
                            }
                        }
                        else
                        {
                            message = Resources.ResourceCommon.MsgNotAllowedToUpdate;
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToUpdate;
                    }
                }
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ApproveItemOpeningList(List<ItemOpening> itemOpeningList)
        {
            var isSuccess = false;
            var message = string.Empty;

            permission = CheckPermission(); 

            WorkflowactionSetting workflowactionSettingObj = this.workflowactionSettingService.GetAllWorkflowactionSetting().Where(a => a.EmployeeId == UserSession.GetUserFromSession().EmployeeId && a.SubModuleItem.UrlPath == url && a.WorkflowactionId == (int)WorkFlowActionEnum.Approve).FirstOrDefault();
                
            foreach (var itemOpening in itemOpeningList)
            {
                var isNew = itemOpening.Id == Guid.Empty ? true : false;
                if (!isNew)
                {
                    if (permission.UpdateOperation == true && workflowactionSettingObj != null)
                    {
                        var itemOpeningObj = this.itemOpeningService.GetItemOpening(itemOpening.Id);
                        if (itemOpeningObj.Status != (int)CommonEnum.Approved)
                        {
                            itemOpeningObj.Status = (int)CommonEnum.Approved;
                            if (itemOpening.OpeningDate != null)
                                itemOpeningObj.OpeningDate = itemOpening.OpeningDate.Value.ToUniversalTime();
                            itemOpeningObj.Quantity = itemOpening.Quantity;
                            itemOpeningObj.ApprovedBy = UserSession.GetUserFromSession().EmployeeId;
                            itemOpeningObj.ApprovedDate = DateTime.UtcNow;

                            InventoryUtility inventoryUtility = new InventoryUtility(itemInventoryHistoryService, itemInventoryService);
                            try
                            {
                                ItemInventory itemInventory = this.itemInventoryService.GetAllItemInventory().Where(a => a.BinCardId == itemOpeningObj.BinCardId && a.ItemId == itemOpeningObj.ItemId).FirstOrDefault();
                                if (itemInventory != null)
                                {
                                    inventoryUtility.UpdateItmeBalance(itemInventory, itemOpening.Quantity, true);
                                }
                                else
                                {
                                    itemInventory = new ItemInventory();
                                    itemInventory.Id = Guid.NewGuid();
                                    itemInventory.ItemId = itemOpeningObj.ItemId;
                                    itemInventory.Quantity = itemOpening.Quantity;
                                    itemInventory.UnitId = itemOpeningObj.UnitId;
                                    itemInventory.BinCardId = itemOpeningObj.BinCardId;

                                    this.itemInventoryService.CreateItemInventory(itemInventory);
                                }
                                inventoryUtility.SaveInventoryHistory(itemOpeningObj.ItemId, itemOpening.Quantity, itemOpeningObj.UnitId, itemOpeningObj.BinCardId, "", "Item Opening", UserSession.GetUserFromSession().EmployeeId, (int?)actionEnum.Create);
                            }
                            catch { }

                            if (this.itemOpeningService.UpdateItemOpening(itemOpeningObj))
                            {
                                isSuccess = true;
                                message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceItemOpening.LblItemOpening);
                            }
                            else
                            {
                                isSuccess = false;
                                message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceItemOpening.LblItemOpening);
                            }
                        }
                        else
                        {
                            message = Resources.ResourceCommon.MsgNotAllowedToUpdate;
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToApprove;
                    }
                }
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteItemOpeningList(List<ItemOpening> itemOpeningList)
        {
            var isSuccess = false;
            var message = string.Empty;

            permission = CheckPermission(); 
            
            foreach (var itemOpening in itemOpeningList)
            {
                var isNew = itemOpening.Id == Guid.Empty ? true : false;
                if (!isNew)
                {
                    if (permission.DeleteOperation == true)
                    {
                        var itemOpeningObj = this.itemOpeningService.GetItemOpening(itemOpening.Id);
                        if (itemOpeningObj.Status != (int)CommonEnum.Approved)
                        {
                            if (this.itemOpeningService.DeleteItemOpening(itemOpeningObj.Id))
                            {
                                isSuccess = true;
                                message = string.Format(Resources.ResourceCommon.CMsg_delete, Resources.ResourceItemOpening.LblItemOpening);
                            }
                            else
                            {
                                isSuccess = false;
                                message = string.Format(Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceItemOpening.LblItemOpening);
                            }
                        }
                        else
                        {
                            message = Resources.ResourceCommon.MsgNotAllowedToDelete;
                        }
                    }
                    else
                    {
                        message = Resources.ResourceCommon.MsgNoPermissionToDelete;
                    }
                }
            }

            return Json(new
            {
                isSuccess = isSuccess,
                message = message,
            }, JsonRequestBehavior.AllowGet);
        }
    }

    public class ItemOpeningViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string OpeningDate { get; set; }
        public double Quantity { get; set; }
        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<int> BinCardId { get; set; }
        public string BinCardNo { get; set; }
        public int Status { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public string ApprovedDate { get; set; }
        public virtual BinCard BinCard { get; set; }
        public virtual Item Item { get; set; }
        public virtual UnitOfMeasurement UnitOfMeasurement { get; set; }
    }
}