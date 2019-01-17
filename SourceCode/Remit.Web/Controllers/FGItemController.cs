using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using iTextSharp.text.pdf;
using Remit.CachingService;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class FGItemController : Controller
    {
        public readonly IFGTypeService fgItemTypeService;
        public readonly IFGItemService fgItemService;
        public readonly IFGItemInventoryService fgItemInventoryService;
        public readonly IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService;
        public readonly IFGItemPriceDetailService fGItemPriceDetailService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        string cacheKey = "permission:fgItem" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        public FGItemController(IFGItemService fgItemService, IFGTypeService fgItemTypeService,
            IRoleSubModuleItemService roleSubModuleItemService,
            IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService,
            IFGItemInventoryService fgItemInventoryService, IFGItemPriceDetailService fGItemPriceDetailService)
        {
            this.fgItemService = fgItemService;
            this.fgItemTypeService = fgItemTypeService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.fgItemInventoryService = fgItemInventoryService;
            this.fGItemPriceDetailService = fGItemPriceDetailService;
            this.fgItemInventoryWithoutBinService = fgItemInventoryWithoutBinService;
        }

        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        string timeFormat = WebConfigurationManager.AppSettings["TimeFormat"];
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];

        // GET: /FGItem/
        public ActionResult Index()
        {
            var url = Request.RawUrl;

            permission = (RoleSubModuleItem) cacheProvider.Get(cacheKey);
            if (permission == null)
                permission =
                    roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                        Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("FGItem");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }
            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateFGItem(FGItem fgItem, string fileName)
        {
            const string url = "/FGItem/Index";
            permission = (RoleSubModuleItem) cacheProvider.Get(cacheKey);
            if (permission == null)
                permission =
                    roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                        Helpers.UserSession.GetUserFromSession().RoleId);

            var isSuccess = false;
            var message = string.Empty;
            var isNew = fgItem.Id == 0 ? true : false;

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    if (!CheckIsExist(fgItem))
                    {

                        if (fileName != null)
                        {

                            fgItem.ImagePath = fileName;

                        }
                        if (this.fgItemService.CreateFGItem(fgItem))
                        {
                            isSuccess = true;
                            message = "Item saved successfully!!!";
                        }
                        else
                        {
                            message = "Item could not saved!";
                        }
                    }
                    else
                    {
                        isSuccess = false;
                        message = "Can't save. Same Item name found!";
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

                    var fgItemObjAttach = this.fgItemService.GetFGItem(fgItem.Id);

                    fgItemObjAttach.TypeId = fgItem.TypeId;
                    fgItemObjAttach.SizeId = fgItem.SizeId;
                    fgItemObjAttach.Code = fgItem.Code;
                    fgItemObjAttach.Name = fgItem.Name;
                    fgItemObjAttach.Color = fgItem.Color;
                    fgItemObjAttach.PackUnitId = fgItem.PackUnitId;
                    fgItemObjAttach.SalesUnitId = fgItem.SalesUnitId;
                    fgItemObjAttach.PackageToSalesRatio = fgItem.PackageToSalesRatio;
                    fgItemObjAttach.PcsPerCartoon = fgItem.PcsPerCartoon;
                    fgItemObjAttach.WeightPerCartoon = fgItem.WeightPerCartoon;
                    fgItemObjAttach.SftPerPiece = fgItem.SftPerPiece;
                    fgItemObjAttach.SmtPerPiece = fgItem.SmtPerPiece;
                    fgItemObjAttach.ImagePath = fgItem.ImagePath;
                    fgItemObjAttach.IsActive = fgItem.IsActive;


                    if (this.fgItemService.UpdateFGItem(fgItemObjAttach))
                    {
                        isSuccess = true;
                        message = "Item updated successfully!";
                    }
                    else
                    {
                        message = "Item could not updated!";
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

        private bool CheckIsExist(Model.Models.FGItem fgItem)
        {
            return this.fgItemService.CheckIsExist(fgItem);
        }


        [HttpPost]
        public JsonResult DeleteFGItem(FGItem fgItem)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/SubModuel/Index";
            permission = (RoleSubModuleItem) cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.fgItemService.DeleteFGItem(fgItem.Id);
                if (isSuccess)
                {
                    message = "Item deleted successfully!";
                }
                else
                {
                    message = "Item can't be deleted!";
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

        [HttpGet]
        public JsonResult GetFGItemList()
        {
            var fgItemListObj = this.fgItemService.GetAllFGItem();
            List<FGItemViewModel> fgItemVMList = new List<FGItemViewModel>();

            foreach (var fgItem in fgItemListObj)
            {
                var fgItemTemp = AFGItem(fgItem);
                fgItemVMList.Add(fgItemTemp);
            }
            return Json(fgItemVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFGItemListByTypeId(int id)
        {
            var fgItemListObj = this.fgItemService.GetAllFGItem().Where(c => c.TypeId == id);
            List<FGItemViewModel> fgItemVMList = new List<FGItemViewModel>();
            foreach (var fgItem in fgItemListObj)
            {
                var fgItemTemp = AFGItem(fgItem);
                fgItemVMList.Add(fgItemTemp);
            }
            return Json(fgItemVMList, JsonRequestBehavior.AllowGet);
        }

        public void UpoladFile(string name, string code, HttpPostedFileBase file)
        {
            var length = Request.ContentLength;
            var bytes = new byte[length];
            Request.InputStream.Read(bytes, 0, length);
            if (file != null)
            {
                string str = file.FileName;
                string ext = str.Substring(0, str.LastIndexOf(".") + 1).TrimEnd('.');

                if (System.IO.File.Exists(Server.MapPath("~/Files/FGItem/" + code + '_' + ext +
                                                         Path.GetExtension(file.FileName))))
                {
                    System.IO.File.Delete(Server.MapPath("~/Files/FGItem/" + code + '_' + ext +
                                                         Path.GetExtension(file.FileName)));
                }
                var saveToFileLoc =
                    Server.MapPath("~/Files/FGItem/" + code + '_' + ext + Path.GetExtension(file.FileName));

                try
                {
                    file.SaveAs(saveToFileLoc);
                }
                catch (Exception e)
                {
                    Console.WriteLine("File Save Error: " + e);
                }
            }


            //var fileStream = new FileStream(saveToFileLoc, FileMode.Create, FileAccess.ReadWrite);
            //fileStream.Write(bytes, 0, length);
            //fileStream.Close();

        }

        public FGItemViewModel AFGItem(FGItem fgItem)
        {
            FGItemViewModel fgItemTemp = new FGItemViewModel();
            fgItemTemp.Id = fgItem.Id;
            fgItemTemp.Name = fgItem.Name;
            fgItemTemp.Color = fgItem.Color;
            if (fgItem.FGUOM != null)
            {
                fgItemTemp.PackUnit = fgItem.FGUOM.UnitName;
                fgItemTemp.PackUnitId = fgItem.PackUnitId;
            }
            if (fgItem.FGUOM1 != null)
            {
                fgItemTemp.SalesUnit = fgItem.FGUOM1.UnitName;
                fgItemTemp.SalesUnitId = fgItem.SalesUnitId;
            }
            fgItemTemp.PackageToSalesRatio = fgItem.PackageToSalesRatio;
            if (fgItem.FGType != null)
            {
                fgItemTemp.TypeName = fgItem.FGType.TypeName;
                fgItemTemp.TypeId = fgItem.TypeId;
            }
            fgItemTemp.SizeId = fgItem.SizeId;
            fgItemTemp.Size = fgItem.FGSize.Size;
            fgItemTemp.Code = fgItem.Code;
            fgItemTemp.PcsPerCartoon = fgItem.PcsPerCartoon;
            fgItemTemp.WeightPerCartoon = fgItem.WeightPerCartoon;
            fgItemTemp.SftPerPiece = fgItem.SftPerPiece;
            fgItemTemp.SmtPerPiece = fgItem.SmtPerPiece;
            fgItemTemp.ImagePath = fgItem.ImagePath;
            fgItemTemp.IsActive = fgItem.IsActive;

            return fgItemTemp;
        }

        public JsonResult GetFGItem(int id)
        {
            var fgItem = this.fgItemService.GetFGItem(id);
            var fgItemTemp = AFGItem(fgItem);
            return Json(fgItemTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFgItemGradeListFromFGInventory(int itemId)
        {
            var fgItemList = fgItemInventoryService.GetAllFGItemInventory()
                .Where(p => p.FGItemId == itemId && p.FGItem.IsActive)
                .GroupBy(pp => new {pp.FGGradeId, pp.FGGrade.Grade}).Select(
                    g =>
                        new
                        {
                            gradeId = g.Key.FGGradeId,
                            grade = g.Key.Grade,
                        });

            List<FGGradeViewModel> fgGradeList = new List<FGGradeViewModel>();

            foreach (var fgItem in fgItemList)
            {
                FGGradeViewModel fgGradeViewModel = new FGGradeViewModel();
                fgGradeViewModel.Id = fgItem.gradeId;
                fgGradeViewModel.Grade = fgItem.grade;
                fgGradeList.Add(fgGradeViewModel);
            }
            return Json(fgGradeList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemFrmInventoryByItemAndGrade(int itemId, int gradeId)
        {
            var fgItemList = fgItemInventoryWithoutBinService.GetAllFGItemInventoryWithoutBin()
                .Where(p => p.FGItemId == itemId && p.FGItem.IsActive && p.FGGradeId == gradeId &&
                            p.BookQuantityInCTN > 0).ToList();

            var unitId = 0;
            List<string> lotList = new List<string>();
            foreach (var aItem in fgItemList)
            {
                unitId = aItem.FGItem.PackUnitId;
                lotList.Add(aItem.Lot);
            }
            var stockQty = 0.00;
            if (fgItemList.Any())
            {
                if (unitId == 1)
                {
// CTN
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInCTN);
                }
                else if (unitId == 2)
                {
// Pcs
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInPcs);
                }
                else if (unitId == 3)
                {
// SFT
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInSFT);
                }
                else if (unitId == 4)
                {
// SMT
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInSMT);
                }
            }
            var unitPrice = 0.00;
            var discount = 0.00;
            var fgObj = fGItemPriceDetailService.GetFGItemPriceDetailByItemAndGrade(itemId, gradeId); //.SalesUnitPrice;
            if (fgObj != null)
            {
                unitPrice = fgObj.SalesUnitPrice;
                if (fgObj.SpecialDiscount != null) discount = (double) fgObj.SpecialDiscount;
            }

            var unitPriceRate = unitPrice;
            var discountPerUnit = discount;
            
            //var list6 = lotList.OrderBy(x => x, new NaturalSort()).ToArray();

            return Json(new { lotList = lotList.OrderBy(x => x, new NaturalSort()).ToArray(), stockQty = stockQty, unitPrice = unitPrice, unitPriceRate = unitPriceRate, discountPerUnit = discountPerUnit }, JsonRequestBehavior.AllowGet);
            //return Json(new { lotList = lotList.OrderBy(a => a), stockQty = stockQty, unitPrice = unitPrice, unitPriceRate = unitPriceRate, discountPerUnit = discountPerUnit }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemFrmInventoryByItemAndGradeWhenPopulate(int itemId, int gradeId)
        {
            var fgItemList = fgItemInventoryWithoutBinService.GetAllFGItemInventoryWithoutBin()
                .Where(p => p.FGItemId == itemId && p.FGItem.IsActive && p.FGGradeId == gradeId).ToList();

            var unitId = 0;
            List<string> lotList = new List<string>();
            foreach (var aItem in fgItemList)
            {
                unitId = aItem.FGItem.PackUnitId;
                lotList.Add(aItem.Lot);
            }
            var stockQty = 0.00;
            if (fgItemList.Any())
            {
                if (unitId == 1)
                {// CTN
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInCTN);
                }
                else if (unitId == 2)
                {// Pcs
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInPcs);
                }
                else if (unitId == 3)
                {// SFT
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInSFT);
                }
                else if (unitId == 4)
                {// SMT
                    stockQty = fgItemList.Sum(xx => xx.BookQuantityInSMT);
                }
            }
            var unitPrice = 0.00;
            var discount = 0.00;
            var fgObj = fGItemPriceDetailService.GetFGItemPriceDetailByItemAndGrade(itemId, gradeId);//.SalesUnitPrice;
            if (fgObj != null)
            {
                unitPrice = fgObj.SalesUnitPrice;
                if (fgObj.SpecialDiscount != null) discount = (double)fgObj.SpecialDiscount;
            }

            var unitPriceRate = unitPrice;
            var discountPerUnit = discount;
            return Json(new { lotList = lotList.OrderBy(x => x, new NaturalSort()).ToArray(), stockQty = stockQty, unitPrice = unitPrice, unitPriceRate = unitPriceRate, discountPerUnit = discountPerUnit }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetUnitPriceInfoOfItem(int itemId, int gradeId)
        {
            var unitPrice = 0.00;
            var discount = 0.00;
            var fgObj = fGItemPriceDetailService.GetFGItemPriceDetailByItemAndGradeForPrice(itemId, gradeId);//.SalesUnitPrice;
            if (fgObj != null)
            {
                unitPrice = fgObj.SalesUnitPrice;
                if (fgObj.SpecialDiscount != null) discount = (double)fgObj.SpecialDiscount;
            }
           
            return Json(new { unitPrice = unitPrice, discount = discount }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetItemFrmInventoryByItemGradeAndLot(int itemId, int gradeId, string lot)
        {
            var fgItem = fgItemInventoryWithoutBinService.GetAllFGItemInventoryWithoutBin()
                .FirstOrDefault(p => p.FGItemId == itemId && p.FGItem.IsActive && p.FGGradeId == gradeId && p.Lot == lot);
            var stockQty = 0.00;
            if (fgItem != null)
            {
                var unitId = fgItem.FGItem.PackUnitId;

                if (unitId == 1)
                {// CTN
                    stockQty = fgItem.BookQuantityInCTN;
                }
                else if (unitId == 2)
                {// Pcs
                    stockQty = fgItem.BookQuantityInPcs;
                }
                else if (unitId == 3)
                {// SFT
                    stockQty = fgItem.BookQuantityInSFT;
                }
                else if (unitId == 4)
                {// SMT
                    stockQty = fgItem.BookQuantityInSMT;
                }
            }
            return Json(stockQty, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetFgItemListInFgItemInventoryByType(int typeId)
        {
            var fgItemList = fgItemInventoryService.GetAllFGItemInventory()
                    .Where(p => p.FGItem.TypeId == typeId && p.FGItem.IsActive).GroupBy(pp => new { pp.FGItemId }).Select(
                        g =>
                            new
                            {
                                itemId = g.Key.FGItemId,
                                //lot = fgItemInventoryService.GetAllFGItemInventory().Where(ff => ff.FGItemId == g.Key.FGItemId).Select(fff => fff.Lot),
                                //gradeObjList = fgItemInventoryService.GetAllFGItemInventory().Where(ff=> ff.FGItemId == g.Key.FGItemId).Select(fff=> fff.FGGrade),
                            })
                ; //.ToList();

            List<FGItemViewModel> fgItemViewModelList = new List<FGItemViewModel>();

            foreach (var fgItem in fgItemList)
            {
                var getFGItem = fgItemService.GetFGItem(fgItem.itemId);
                var preparedFgItemList = AFGItem(getFGItem);
                //preparedFgItemList.FGGrades = fgItem.gradeObjList.ToList();
                fgItemViewModelList.Add(preparedFgItemList);
            }
            return Json(fgItemViewModelList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFgItemListBySelectedTypeId(int typeId)
        {
            var fgItemList = fgItemService.GetAllFGItem().Where(p => p.TypeId == typeId && p.IsActive).ToList();

            List<FGItemViewModel> fgItemViewModelList = new List<FGItemViewModel>();

            foreach (var fgItem in fgItemList)
            {
                var preparedFgItemList = AFGItem(fgItem);
                fgItemViewModelList.Add(preparedFgItemList);
            }

            return Json(fgItemViewModelList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnitInfoOfItem(int itemId)
        {
            UnitOfMeasurementViewModel uomVm = new UnitOfMeasurementViewModel();
            var fgItem = fgItemService.GetFGItem(itemId);
            var previousPricingDate = string.Empty;
            if (fgItem != null)
            {
                uomVm.Id = fgItem.FGUOM1.Id;
                uomVm.Name = fgItem.FGUOM1.UnitName;
            }

            var itemPriceDetailObj = fGItemPriceDetailService.GetFGItemPriceDetailByItemId(itemId);
            if (itemPriceDetailObj != null)
            {
                previousPricingDate = itemPriceDetailObj.EffectiveDate.ToString(dateFormat);
            }

            return Json(new
            {
                uom = uomVm,
                ppd = previousPricingDate
            }, JsonRequestBehavior.AllowGet);
        }

    }

    public class FGItemViewModel
    {
        public FGItemViewModel()
        {
            this.FGItemInventories = new List<FGItemInventoryViewModel>();
            this.FGItemOpenings = new List<FGItemOpeningViewModel>();
            this.FGGrades = new List<FGGrade>();
        }
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int SizeId { get; set; }
        public string Size { get; set; }
        public string Code { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public int PackUnitId { get; set; }
        public int SalesUnitId { get; set; }
        public string PackUnit { get; set; }
        public string SalesUnit { get; set; }
        public double PackageToSalesRatio { get; set; }
        public int PcsPerCartoon { get; set; }
        public Nullable<double> WeightPerCartoon { get; set; }
        public Nullable<double> SftPerPiece { get; set; }
        public Nullable<double> SmtPerPiece { get; set; }
        public string ImagePath { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<FGItemInventoryViewModel> FGItemInventories { get; set; }
        public virtual ICollection<FGItemOpeningViewModel> FGItemOpenings { get; set; }
        public virtual List<FGGrade> FGGrades { get; set; }
    }
    public class NaturalSort : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string x, string y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }
    }

}