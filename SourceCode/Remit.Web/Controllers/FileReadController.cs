using OfficeOpenXml;
using System;
using System.Activities.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using log4net;
using log4net.Core;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using Remit.CachingService;
using Remit.LoggerService;
using Remit.Model.Models;
using Remit.Service;
using Remit.Service.Enums;
using Remit.Service.Utilities;
using Remit.Web.Helpers;
using WebGrease.Css.Extensions;


namespace Remit.Web.Controllers
{
    public class FileReadController : Controller
    {
        public readonly IItemService itemService;
        public readonly IFGItemService fgItemService;
        public readonly IFGGradeService fgGradeService;
        public readonly IItemCategoryService itemCategoryService;
        public readonly IFGTypeService fgTypeService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IItemOpeningService itemOpeningService;
        public readonly IFGItemOpeningService fgItemOpeningService;
        public readonly IUnitOfMeasurementService unitOfMeasurementService;
        public readonly IFGUOMService fgUOMService;
        public readonly IWarehouseService warehouseService;
        public readonly IBinCardService binCardService;
        public readonly IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService;
        public readonly IFGItemInventoryService fgItemInventoryService;
        public readonly IFGItemInventoryHistoryService fgItemFGInventoryHistoryService;

        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public FileReadController(ISubModuleItemService subModuleItemService,
            IRoleSubModuleItemService roleSubModuleItemService, IItemService itemService,IFGGradeService fgGradeService,IItemCategoryService itemCategoryService,
            IFGTypeService fgTypeService, IFGItemService fgItemService, ISupplierService supplierService, IItemOpeningService itemOpeningService,IWarehouseService warehouseService,
            IBinCardService binCardService,IFGItemOpeningService fgItemOpeningService, IUnitOfMeasurementService unitOfMeasurementService, ICurrencyService currencyService,
            IFGUOMService fgUOMService, IFGItemInventoryService fgItemInventoryService, IFGItemInventoryHistoryService fgItemFGInventoryHistoryService, IFGItemInventoryWithoutBinService fgItemInventoryWithoutBinService)
        {
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.itemService = itemService;
            this.fgGradeService = fgGradeService;
            this.fgItemService = fgItemService;
            this.itemCategoryService = itemCategoryService;
            this.fgTypeService = fgTypeService;
            this.itemOpeningService = itemOpeningService;
            this.fgItemOpeningService = fgItemOpeningService;
            this.warehouseService = warehouseService;
            this.binCardService = binCardService;
            this.unitOfMeasurementService = unitOfMeasurementService;
            this.fgUOMService = fgUOMService;
            this.fgItemInventoryService = fgItemInventoryService;
            this.fgItemFGInventoryHistoryService = fgItemFGInventoryHistoryService;
            this.fgItemInventoryWithoutBinService = fgItemInventoryWithoutBinService;
        }

        string cacheKeyIO = "permission:fileRead-ItemOpening" +
                                   Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permissionIO = null;

        public ActionResult ItemOpeningFileRead()
        {
            const string url = "/ItemOpening/Index";
            permissionIO = (RoleSubModuleItem)cacheProvider.Get(cacheKeyIO) ??
                           roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                               Helpers.UserSession.GetUserFromSession().RoleId);

            if (permissionIO != null)
            {
                if (permissionIO.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKeyIO, permissionIO, 240);
                    return View("ItemOpeningFileRead");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ActionResult FGItemOpeningFileRead()
        {
            const string url = "/FGItemOpening/Index";
            permissionIO = (RoleSubModuleItem)cacheProvider.Get(cacheKeyIO) ??
                           roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                               Helpers.UserSession.GetUserFromSession().RoleId);

            if (permissionIO != null)
            {
                if (permissionIO.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKeyIO, permissionIO, 240);
                    return View("FGItemOpeningFileRead");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }

        public ActionResult ReadItemOpeningFile(string config, string openingDate, HttpPostedFileBase file)
        {
            var fileExtension = System.IO.Path.GetExtension(file.FileName);
            if (fileExtension == ".xlsx" || fileExtension == ".xls")
            {
                string message = string.Empty;

                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelWorksheet worksheetForItemOpening = package.Workbook.Worksheets[1];

                    if (worksheetForItemOpening != null)
                    {
                        if (worksheetForItemOpening.Dimension != null)
                        {
                            var start = worksheetForItemOpening.Dimension.Start;
                            var end = worksheetForItemOpening.Dimension.End;
                            int startRowIndex = worksheetForItemOpening.Dimension.Start.Row + 1;

                            var hash = new HashSet<string>();
                            var parthash = new HashSet<string>();
                            var index = 0;
                            for (int row = startRowIndex; row <= end.Row; row++)
                            {
                                var itemOpeningObj = new ItemOpening();
                                var itemObj = new Item();

                                var warehouse = "";
                                for (int col = start.Column; col <= end.Column; col++)
                                {
                                    var cellNameFromIntValue = GetExcelColumnName(col);
                                    object cellValue = worksheetForItemOpening.Cells[row, col].Text;

                                    if (cellValue != null && (string)cellValue != "")
                                    {
                                        switch (cellNameFromIntValue)
                                        {
                                            case "A":
                                                var name = cellValue.ToString();
                                                var chkname = itemService.GetAllItem().FirstOrDefault(com => com.Name == name);
                                                if (chkname != null)
                                                {
                                                    itemOpeningObj.ItemId = chkname.Id;
                                                }
                                                break;
                                            case "B":
                                                warehouse = cellValue.ToString();
                                                break;
                                            case "C":
                                                var bincard = cellValue.ToString();
                                                var chkbincard = binCardService.GetAllBinCard().FirstOrDefault(com => com.CardNo == bincard && com.Warehouse.Name == warehouse);
                                                if (chkbincard != null)
                                                {
                                                    itemOpeningObj.BinCardId = chkbincard.Id;
                                                }
                                                break;
                                            case "D":
                                                if (cellValue != null && cellValue.ToString() != "0")
                                                {
                                                    try
                                                    {
                                                        var quantity = Int32.Parse(cellValue.ToString());
                                                        itemOpeningObj.Quantity = Int32.Parse(cellValue.ToString());
                                                    }
                                                    catch (Exception)
                                                    {
                                                        itemOpeningObj.Quantity = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    itemOpeningObj.Quantity = 0;
                                                }
                                                break;
                                            case "E":
                                                var unit = cellValue.ToString();
                                                var chkunit = unitOfMeasurementService.GetAllUnitOfMeasurement().FirstOrDefault(com => com.Name == unit);
                                                if (chkunit != null)
                                                {
                                                    itemOpeningObj.UnitId = chkunit.Id;
                                                }
                                                break;
                                        }
                                    }
                                }

                                if (itemOpeningObj.ItemId != Guid.Empty && itemOpeningObj.BinCardId != null && itemOpeningObj.Quantity != 0)
                                {
                                    var invOpdet = itemOpeningService.GetAllItemOpening().FirstOrDefault(a => a.ItemId == itemOpeningObj.ItemId && a.BinCardId == itemOpeningObj.BinCardId);
                                    if (invOpdet == null)
                                    {
                                        index++;
                                        itemOpeningObj.Id = Guid.NewGuid();
                                        itemOpeningObj.OpeningDate = DateTime.Parse(openingDate).ToUniversalTime();
                                        this.itemOpeningService.CreateItemOpening(itemOpeningObj);
                                    }
                                    else
                                    {
                                        if (invOpdet.Status != (int)CommonEnum.Approved)
                                        {
                                            index++;
                                            invOpdet.OpeningDate = DateTime.Parse(openingDate).ToUniversalTime();
                                            invOpdet.Status = 0;
                                            invOpdet.ApprovedBy = null;
                                            invOpdet.Quantity = itemOpeningObj.Quantity;
                                            invOpdet.UnitId = itemOpeningObj.UnitId;
                                            this.itemOpeningService.UpdateItemOpening(invOpdet);
                                        }
                                    }
                                }
                            }
                            message = string.Format(Resources.ResourceCommon.CMsg_uploaded, index);
                        }
                    }
                }
                return Json(message);
            }
            else
            {
                string message = Resources.ResourceCommon.MsgChooseXLXFile;
                return Json(message);
            }
        }

        public ActionResult ReadFGItemOpeningFile(string config, string openingDate, HttpPostedFileBase file)
        {
            FGInventoryUtility fgInventoryUtility = new FGInventoryUtility(fgItemFGInventoryHistoryService, fgItemInventoryService, fgItemInventoryWithoutBinService);
            var fileExtension = System.IO.Path.GetExtension(file.FileName);
            if (fileExtension == ".xlsx" || fileExtension == ".xls")
            {
                string message = string.Empty;

                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelWorksheet worksheetForItemOpening = package.Workbook.Worksheets[1];

                    if (worksheetForItemOpening != null)
                    {
                        if (worksheetForItemOpening.Dimension != null)
                        {
                            var start = worksheetForItemOpening.Dimension.Start;
                            var end = worksheetForItemOpening.Dimension.End;
                            int startRowIndex = worksheetForItemOpening.Dimension.Start.Row + 1;

                            List<FGItemOpening> fgItemOpeninglist = new List<FGItemOpening>();
                            var hash = new HashSet<string>();
                            var parthash = new HashSet<string>();
                            var index = 0;
                            //....... ItemOpening and Item Save....................
                            for (int row = startRowIndex; row <= end.Row; row++)
                            {
                                var fgItemOpeningObj = new FGItemOpening();
                                var fgItemObj = new FGItem();

                                var warehouse = "";

                                for (int col = start.Column; col <= end.Column; col++)
                                {
                                    var cellNameFromIntValue = GetExcelColumnName(col);
                                    object cellValue = worksheetForItemOpening.Cells[row, col].Text;

                                    if (cellValue != null && (string)cellValue != "")
                                    {
                                        switch (cellNameFromIntValue)
                                        {
                                            case "A":
                                                var code = cellValue.ToString();
                                                var chkcode = fgItemService.GetAllFGItem().FirstOrDefault(com => com.Code == code);
                                                if (chkcode != null)
                                                {
                                                    fgItemOpeningObj.FGItemId = chkcode.Id;
                                                }
                                                break;
                                            case "B":
                                                var grade = cellValue.ToString();
                                                var chkgrade = fgGradeService.GetAllFGGrade().FirstOrDefault(com => com.Grade == grade);
                                                if (chkgrade != null)
                                                {
                                                    fgItemOpeningObj.FGGradeId = chkgrade.Id;
                                                }
                                                break;
                                            case "C":
                                                var lot = cellValue.ToString();
                                                fgItemOpeningObj.Lot = lot;
                                                break;
                                            case "D":
                                                warehouse = cellValue.ToString();
                                                break;
                                            case "E":
                                                var bincard = cellValue.ToString();
                                                var chkbincard = binCardService.GetAllBinCard().FirstOrDefault(com => com.CardNo == bincard && com.Warehouse.Name == warehouse);
                                                if (chkbincard != null)
                                                {
                                                    fgItemOpeningObj.BinCardId = chkbincard.Id;
                                                }
                                                break;
                                            case "F":
                                                if (cellValue != null && cellValue.ToString() != "0")
                                                {
                                                    try
                                                    {
                                                        var quantity = Int32.Parse(cellValue.ToString());
                                                        fgItemOpeningObj.Quantity = Int32.Parse(cellValue.ToString());
                                                    }
                                                    catch (Exception)
                                                    {
                                                        fgItemOpeningObj.Quantity = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    fgItemOpeningObj.Quantity = 0;
                                                }
                                                break;
                                            case "G":
                                                var unit = cellValue.ToString();
                                                var chkunit = fgUOMService.GetAllFGUOM().FirstOrDefault(com => com.UnitName == unit);
                                                if (chkunit != null)
                                                {
                                                    fgItemOpeningObj.UnitId = chkunit.Id;
                                                }
                                                break;
                                        }
                                    }
                                }

                                if (fgItemOpeningObj.FGItemId != 0 && fgItemOpeningObj.FGGradeId != 0 && fgItemOpeningObj.BinCardId != null && fgItemOpeningObj.Quantity != 0)
                                {
                                    var invOpdet = fgItemOpeningService.GetAllFGItemOpening().FirstOrDefault(a => a.FGItemId == fgItemOpeningObj.FGItemId && a.FGGradeId == fgItemOpeningObj.FGGradeId && a.Lot == fgItemOpeningObj.Lot && a.BinCardId == fgItemOpeningObj.BinCardId);
                                    if (invOpdet == null)
                                    {
                                        index++;
                                        fgItemOpeningObj.Id = Guid.NewGuid();
                                        fgItemOpeningObj.OpeningDate = DateTime.Parse(openingDate).ToUniversalTime();
                                        FGItem fgItem = this.fgItemService.GetFGItem(fgItemOpeningObj.FGItemId);
                                        
                                        FGItemQuanty fgiq = fgInventoryUtility.GetConvertedQuantity(fgItem, fgItemOpeningObj.Quantity, fgItemOpeningObj.UnitId);
                                        if (fgiq != null)
                                        {
                                            fgItemOpeningObj.QuantityInPcs = fgiq.QuantityInPcs;
                                            fgItemOpeningObj.QuantityInCTN = fgiq.QuantityInCTN;
                                            fgItemOpeningObj.QuantityInSFT = fgiq.QuantityInSFT;
                                            fgItemOpeningObj.QuantityInSMT = fgiq.QuantityInSMT;
                                        }
                                        this.fgItemOpeningService.CreateFGItemOpening(fgItemOpeningObj);
                                    }
                                    else
                                    {
                                        if (invOpdet.Status != (int)CommonEnum.Approved)
                                        {
                                            index++;
                                            invOpdet.OpeningDate = DateTime.Parse(openingDate).ToUniversalTime();
                                            invOpdet.Status = 0;
                                            invOpdet.ApprovedBy = null;
                                            invOpdet.Quantity = fgItemOpeningObj.Quantity;
                                            invOpdet.UnitId = fgItemOpeningObj.UnitId;
                                            
                                            FGItemQuanty fgiq = fgInventoryUtility.GetConvertedQuantity(invOpdet.FGItem, fgItemOpeningObj.Quantity, fgItemOpeningObj.UnitId);
                                            if (fgiq != null)
                                            {
                                                fgItemOpeningObj.QuantityInPcs = fgiq.QuantityInPcs;
                                                fgItemOpeningObj.QuantityInCTN = fgiq.QuantityInCTN;
                                                fgItemOpeningObj.QuantityInSFT = fgiq.QuantityInSFT;
                                                fgItemOpeningObj.QuantityInSMT = fgiq.QuantityInSMT;
                                            }

                                            this.fgItemOpeningService.UpdateFGItemOpening(invOpdet);
                                        }
                                    }
                                }
                            }
                            message = string.Format(Resources.ResourceCommon.CMsg_uploaded, index);
                        }
                    }
                }
                return Json(message);
            }
            else
            {
                string message = Resources.ResourceCommon.MsgChooseXLXFile;
                return Json(message);
            }
        }        
    }

    public class NewPartFromSerialFIleList
    {
        public string NewBinCardId { set; get; }
        public int NewPartQty { set; get; }
    }

}