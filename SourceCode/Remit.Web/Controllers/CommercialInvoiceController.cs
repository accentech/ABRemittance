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
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class CommercialInvoiceController : Controller
    {
        public readonly ICommercialInvoiceService commercialInvoiceService;
        public readonly IProformaInvoiceService proformaInvoiceService;
        public readonly ICommercialInvoiceDetailService commercialInvoiceDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IItemService itemService;
        public readonly ILCService lcService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly INotificationSettingService notificationSettingService;

        public readonly ISupplierService supplierService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public CommercialInvoiceController(ISupplierService supplierService, IProformaInvoiceService proformaInvoiceService, ICommercialInvoiceService commercialInvoiceService, ILCService lcService,
            ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService,
            ICommercialInvoiceDetailService commercialInvoiceDetailService, IWorkflowactionSettingService workflowactionSettingService,
            INotificationSettingService notificationSettingService, IItemService itemService)
        {
            this.supplierService = supplierService;
            this.proformaInvoiceService = proformaInvoiceService;
            this.commercialInvoiceService = commercialInvoiceService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.commercialInvoiceDetailService = commercialInvoiceDetailService;
            this.lcService = lcService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.notificationSettingService = notificationSettingService;
            this.itemService = itemService;
        }

        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:commercialInvoice" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        const string url = "/CommercialInvoice/Index";

        // GET: /CommercialInvoice/
        public ActionResult Index()
        {
            return CommercialInvoiceList();
        }

        public ActionResult CommercialInvoiceList()
        {
            RoleSubModuleItem permissionDemand = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permissionDemand != null)
            {
                if (permissionDemand.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permissionDemand, 240);
                    return View("CommercialInvoice");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateCommercialInvoice(CommercialInvoice commercialInvoice, string fileName, double? dueFreightCost)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                var isNew = commercialInvoice.Id == Guid.Empty ? true : false;
                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                  roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                      Helpers.UserSession.GetUserFromSession().RoleId);
                if (isNew)
                {
                    if (permission.CreateOperation == true)
                    {
                        if (!CheckIsExist(commercialInvoice))
                        {
                            commercialInvoice.Id = Guid.NewGuid();

                            foreach (var a in commercialInvoice.CommercialInvoiceDetails)
                            {
                                a.Id = Guid.NewGuid();
                                a.CommercialInvoiceId = commercialInvoice.Id;
                            }
                            if (fileName != null)
                            {
                                commercialInvoice.InvoiceFilePath = fileName;
                            }
                            // change................
                            var lc = this.lcService.GetLC(commercialInvoice.LCId);
                            var proformaInvoice = lc.LCAmendments.FirstOrDefault(lcd => lcd.AmendmentNo == lc.LCAmendments.Max(a => a.AmendmentNo)).ProformaInvoice;

                            proformaInvoice.DueFreightCost = dueFreightCost;
                            this.proformaInvoiceService.UpdateProformaInvoice(proformaInvoice);
                            //..................

                            if (this.commercialInvoiceService.CreateCommercialInvoice(commercialInvoice))
                            {
                                isSuccess = true;
                                message += "CommercialInvoice saved successfully!";
                            }
                            else
                            {
                                message = "CommercialInvoice could not saved!";
                            }
                        }
                        else
                        {
                            isSuccess = false;
                            message = "Can't save. Same Commercial Invoice No  found!";
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
                        var commercialInvoiceObj = this.commercialInvoiceService.GetCommercialInvoice(commercialInvoice.Id);
                        if (commercialInvoiceObj.CommercialInvoiceDetails != null)
                        {
                            foreach (var a in commercialInvoiceObj.CommercialInvoiceDetails.ToList())
                            {
                                this.commercialInvoiceDetailService.DeleteCommercialInvoiceDetail(a.Id);
                            }
                        }

                        if (commercialInvoice.CommercialInvoiceDetails != null)
                        {
                            foreach (var a in commercialInvoice.CommercialInvoiceDetails)
                            {
                                a.Id = Guid.NewGuid();
                                a.CommercialInvoiceId = commercialInvoice.Id;
                                this.commercialInvoiceDetailService.CreateCommercialInvoiceDetail(a);
                            }
                        }

                        var commercialInvoiceObjAttach = this.commercialInvoiceService.GetCommercialInvoice(commercialInvoice.Id);
                        if (commercialInvoiceObjAttach != null)
                        {
                            commercialInvoiceObjAttach.LCId = commercialInvoice.LCId;
                            commercialInvoiceObjAttach.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;
                            commercialInvoiceObjAttach.InvoiceDate = commercialInvoice.InvoiceDate;
                            commercialInvoiceObjAttach.InvoiceFilePath = commercialInvoice.InvoiceFilePath;
                            commercialInvoiceObjAttach.ItemTotalAmount = commercialInvoice.ItemTotalAmount;

                            commercialInvoiceObjAttach.FreightCost = commercialInvoice.FreightCost;
                            commercialInvoiceObjAttach.InsurrenceCost = commercialInvoice.InsurrenceCost;
                            commercialInvoiceObjAttach.CurrencyId = commercialInvoice.CurrencyId;
                            commercialInvoiceObjAttach.ConversionRateInLocalCurrency = commercialInvoice.ConversionRateInLocalCurrency;


                            // change................
                            var lc = this.lcService.GetLC(commercialInvoice.LCId);
                            var proformaInvoice = lc.LCAmendments.FirstOrDefault(lcd => lcd.AmendmentNo == lc.LCAmendments.Max(a => a.AmendmentNo)).ProformaInvoice;

                            proformaInvoice.DueFreightCost = dueFreightCost;
                            this.proformaInvoiceService.UpdateProformaInvoice(proformaInvoice);
                            //..................

                            if (this.commercialInvoiceService.UpdateCommercialInvoice(commercialInvoiceObjAttach))
                            {
                                isSuccess = true;
                                message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceCommercialInvoice.LblCommercialInvoice);
                            }
                            else
                            {
                                message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceCommercialInvoice.LblCommercialInvoice);
                            }
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

        private bool CheckIsExist(Model.Models.CommercialInvoice commercialInvoice)
        {
            return this.commercialInvoiceService.CheckIsExist(commercialInvoice);
        }

        [HttpPost]
        public JsonResult DeleteCommercialInvoice(CommercialInvoice commercialInvoice)
        {
            var isSuccess = true;
            var message = string.Empty;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                foreach (var a in commercialInvoice.CommercialInvoiceDetails)
                {
                    this.commercialInvoiceDetailService.DeleteCommercialInvoiceDetail(a.Id);
                }

                isSuccess = this.commercialInvoiceService.DeleteCommercialInvoice(commercialInvoice.Id);

                if (isSuccess)
                {
                    message = "CommercialInvoice deleted successfully!";
                }
                else
                {
                    message = "CommercialInvoice can't be deleted!";
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

        public JsonResult GetUsedCommercialInvoiceByLcId(Guid id)
        {
            var allUnusedCommercialInvoiceList = this.commercialInvoiceService.GetAllUsedCommercialInvoice(id).OrderBy(ci => ci.InvoiceDate);
            List<CommercialInvoiceViewModel> commercialInvoiceList = new List<CommercialInvoiceViewModel>();
            foreach (var commercialInvoice in allUnusedCommercialInvoiceList)
            {
                CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();
                commercialInvoiceTemp.Id = commercialInvoice.Id;
                commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;
                commercialInvoiceTemp.InvoiceDate = commercialInvoice.InvoiceDate != null ? commercialInvoice.InvoiceDate.Value.ToString(dateTimeFormat) : "";
                commercialInvoiceList.Add(commercialInvoiceTemp);
            }
            return Json(commercialInvoiceList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUnusedCommercialInvoiceList(Guid id)
        {
            var allUnusedCommercialInvoiceList = this.commercialInvoiceService.GetAllUnusedCommercialInvoice(id).OrderBy(ci => ci.InvoiceDate);
            List<CommercialInvoiceViewModel> commercialInvoiceList = new List<CommercialInvoiceViewModel>();
            foreach (var commercialInvoice in allUnusedCommercialInvoiceList)
            {
                CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();
                commercialInvoiceTemp.Id = commercialInvoice.Id;
                commercialInvoiceTemp.LCId = commercialInvoice.LCId;
                commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;
                commercialInvoiceTemp.InvoiceDate = commercialInvoice.InvoiceDate != null ? commercialInvoice.InvoiceDate.Value.ToString(dateTimeFormat) : "";
                commercialInvoiceList.Add(commercialInvoiceTemp);
            }
            return Json(commercialInvoiceList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCommercialInvoiceDocumentList(Guid id)
        {
            var commercialInvoice = this.commercialInvoiceService.GetCommercialInvoice(id);
            List<LCShipmentDocumentViewModel> lcDocuments = new List<LCShipmentDocumentViewModel>();
            if (commercialInvoice.LC != null)
            {
                foreach (var doc in commercialInvoice.LC.LCDocuments)
                {
                    LCShipmentDocumentViewModel lcDoc = new LCShipmentDocumentViewModel();
                    lcDoc.Id = Guid.Empty;
                    lcDoc.LCDocumentId = doc.Id;
                    lcDoc.LCShipmentId = Guid.Empty;
                    lcDoc.DocumentName = doc.DocumentName;
                    lcDoc.DocumentPath = string.Empty;
                    lcDoc.Status = String.Empty;
                    lcDoc.Comments = String.Empty;
                    lcDocuments.Add(lcDoc);
                }
            }
            return Json(lcDocuments, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCommercialInvoiceList()
        {
            var allCommercialInvoiceList = this.commercialInvoiceService.GetAllCommercialInvoice();
            List<CommercialInvoiceViewModel> commercialInvoiceList = GetPreparedInvoiceList(allCommercialInvoiceList);

            return Json(commercialInvoiceList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCommercialInvoiceByLC(Guid id)
        {
            var commercialInvoiceListObj = this.commercialInvoiceService.GetAllCommercialInvoice().Where(a => a.LCId == id);
            List<CommercialInvoiceViewModel> commercialInvoiceVMList = new List<CommercialInvoiceViewModel>();

            foreach (var commercialInvoice in commercialInvoiceListObj)
            {
                CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();
                commercialInvoiceTemp.Id = commercialInvoice.Id;
                commercialInvoiceTemp.LCId = commercialInvoice.LCId;
                commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;

                commercialInvoiceVMList.Add(commercialInvoiceTemp);
            }
            return Json(commercialInvoiceVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetNotReceivedCommercialInvoiceByLC(Guid id)
        {
            var ciids = this.commercialInvoiceDetailService.GetAllCommercialInvoiceDetail().Where(a => a.CommercialInvoice.LCId == id && a.IsReceived != true).GroupBy(b => b.CommercialInvoiceId).Select(c => c.Key);
            var commercialInvoiceListObj = this.commercialInvoiceService.GetAllCommercialInvoice().Where(a => a.LCId == id && ciids.Contains(a.Id));
            List<CommercialInvoiceViewModel> commercialInvoiceVMList = new List<CommercialInvoiceViewModel>();

            foreach (var commercialInvoice in commercialInvoiceListObj)
            {
                CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();
                commercialInvoiceTemp.Id = commercialInvoice.Id;
                commercialInvoiceTemp.LCId = commercialInvoice.LCId;
                commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;

                commercialInvoiceVMList.Add(commercialInvoiceTemp);
            }
            return Json(commercialInvoiceVMList, JsonRequestBehavior.AllowGet);
        }

        public List<CommercialInvoiceViewModel> GetPreparedInvoiceList(IEnumerable<CommercialInvoice> commercialInvoiceListObj)
        {
            List<CommercialInvoiceViewModel> commercialInvoiceVmList = new List<CommercialInvoiceViewModel>();
            foreach (var commercialInvoice in commercialInvoiceListObj)
            {
                var comInvoice = aCommercialInvoice(commercialInvoice);
                commercialInvoiceVmList.Add(comInvoice);
            }
            return commercialInvoiceVmList;
        }

        public JsonResult GetCiListByYear(int year)
        {
            var commercialInvoiceListObj = this.commercialInvoiceService.GetAllCommercialInvoice().Where(a => a.InvoiceDate.Value.Year == year);

            List<CommercialInvoiceViewModel> commercialInvoiceVMList = new List<CommercialInvoiceViewModel>();
            foreach (var commercialInvoice in commercialInvoiceListObj)
            {
                CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();

                commercialInvoiceTemp.Id = commercialInvoice.Id;
                commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;

                commercialInvoiceVMList.Add(commercialInvoiceTemp);
            }
            return Json(commercialInvoiceVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCommercialInvoice(Guid id)
        {
            var commercialInvoice = this.commercialInvoiceService.GetCommercialInvoice(id);
            CommercialInvoiceViewModel commercialInvoiceTemp = null;

            if (commercialInvoice != null)
            {
                commercialInvoiceTemp = aCommercialInvoice(commercialInvoice);
            }
            return Json(commercialInvoiceTemp, JsonRequestBehavior.AllowGet);
        }


        public CommercialInvoiceViewModel aCommercialInvoice(CommercialInvoice commercialInvoice)
        {
            CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();
            commercialInvoiceTemp.Id = commercialInvoice.Id;
            commercialInvoiceTemp.LCId = commercialInvoice.LCId;
            commercialInvoiceTemp.CommercialInvoiceNo = commercialInvoice.CommercialInvoiceNo;
            commercialInvoiceTemp.InvoiceDate = commercialInvoice.InvoiceDate != null ? commercialInvoice.InvoiceDate.Value.ToString(dateTimeFormat) : "";
            commercialInvoiceTemp.InvoiceFilePath = commercialInvoice.InvoiceFilePath;

            //commercialInvoiceTemp.DueFreightCost =
            var piDet =
                commercialInvoice.LC.LCAmendments.FirstOrDefault(
                    lcd => lcd.AmendmentNo == commercialInvoice.LC.LCAmendments.Max(a => a.AmendmentNo));
            if (piDet != null)
            {
                var pi = piDet.ProformaInvoice;
                commercialInvoiceTemp.DueFreightCost = pi.DueFreightCost;
            }

            commercialInvoiceTemp.ItemTotalAmount = commercialInvoice.ItemTotalAmount;
            commercialInvoiceTemp.FreightCost = commercialInvoice.FreightCost;
            commercialInvoiceTemp.InsurrenceCost = commercialInvoice.InsurrenceCost;
            commercialInvoiceTemp.CurrencyId = commercialInvoice.CurrencyId;
            commercialInvoiceTemp.CurrencyName = commercialInvoice.Currency != null ? commercialInvoice.Currency.Name : "";
            commercialInvoiceTemp.ConversionRateInLocalCurrency = commercialInvoice.ConversionRateInLocalCurrency;

            List<CommercialInvoiceDetailViewModel> commercialInvoiceDetailVMList = new List<CommercialInvoiceDetailViewModel>();
            if (commercialInvoice.CommercialInvoiceDetails.Any())
            {
                foreach (var commercialInvoiceDetai in commercialInvoice.CommercialInvoiceDetails)
                {
                    CommercialInvoiceDetailViewModel commercialInvoiceDetailTtemp = new CommercialInvoiceDetailViewModel();
                    commercialInvoiceDetailTtemp.Id = commercialInvoiceDetai.Id;
                    commercialInvoiceDetailTtemp.CommercialInvoiceId = commercialInvoiceDetai.CommercialInvoiceId;
                    commercialInvoiceDetailTtemp.ItemId = commercialInvoiceDetai.ItemId;
                    commercialInvoiceDetailTtemp.ItemName = commercialInvoiceDetai.Item != null ? commercialInvoiceDetai.Item.Name : "";
                    commercialInvoiceDetailTtemp.Quantity = commercialInvoiceDetai.Quantity;
                    commercialInvoiceDetailTtemp.UnitId = commercialInvoiceDetai.UnitId;
                    commercialInvoiceDetailTtemp.UnitName = commercialInvoiceDetai.UnitOfMeasurement != null ? commercialInvoiceDetai.UnitOfMeasurement.Name : "";
                    commercialInvoiceDetailTtemp.UnitPrice = commercialInvoiceDetai.UnitPrice;
                    commercialInvoiceDetailTtemp.Amount = commercialInvoiceDetai.Amount;
                    if (commercialInvoiceDetai.PackingUnitId != null)
                    {
                        commercialInvoiceDetailTtemp.PackingUnitId = (int)commercialInvoiceDetai.PackingUnitId;
                        commercialInvoiceDetailTtemp.PackingUnitName = commercialInvoiceDetai.PackingUnit.Name;
                    }
                    if (commercialInvoiceDetai.PackingQuantity != null)
                        commercialInvoiceDetailTtemp.PackingQuantity = (int)commercialInvoiceDetai.PackingQuantity;
                    commercialInvoiceDetailTtemp.NetWeight = commercialInvoiceDetai.NetWeight;
                    commercialInvoiceDetailTtemp.GrossWeight = commercialInvoiceDetai.GrossWeight;
                    if (commercialInvoiceDetai.WeightUnitId != null)
                        commercialInvoiceDetailTtemp.WeightUnitId = (int)commercialInvoiceDetai.WeightUnitId;
                    commercialInvoiceDetailTtemp.WeightUnitName = commercialInvoiceDetai.UnitOfMeasurement != null ? commercialInvoiceDetai.UnitOfMeasurement.Name : "";
                    commercialInvoiceDetailVMList.Add(commercialInvoiceDetailTtemp);
                }
            }
            commercialInvoiceTemp.CommercialInvoiceDetails = commercialInvoiceDetailVMList;

            List<LCDocument> lcDocuments = new List<LCDocument>();
            if (commercialInvoice.LC != null)
            {
                foreach (var doc in commercialInvoice.LC.LCDocuments)
                {
                    LCDocument lcDoc = new LCDocument();
                    lcDoc.Id = doc.Id;
                    lcDoc.DocumentName = doc.DocumentName;
                    lcDocuments.Add(lcDoc);
                }
            }
            commercialInvoiceTemp.LcDocuments = lcDocuments;

            return commercialInvoiceTemp;
        }

        public JsonResult GetCommercialInvoiceByLCId(Guid id)
        {
            var commercialInvoice = this.commercialInvoiceService.GetCommercialInvoice(id);
            CommercialInvoiceViewModel commercialInvoiceTemp = null;

            if (commercialInvoice != null)
            {
                commercialInvoiceTemp = aCommercialInvoice(commercialInvoice);
            }
            return Json(commercialInvoiceTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLCFinalItemById(Guid id)
        {
            var lc = this.lcService.GetLC(id);
            List<CommercialInvoiceViewModel> CommercialInvoiceVMList = new List<CommercialInvoiceViewModel>();

            var proformaInvoice = lc.LCAmendments.FirstOrDefault(lcd => lcd.AmendmentNo == lc.LCAmendments.Max(a => a.AmendmentNo)).ProformaInvoice;
            CommercialInvoiceViewModel commercialInvoiceTemp = new CommercialInvoiceViewModel();

            commercialInvoiceTemp.CurrencyId = proformaInvoice.CurrencyId;
            commercialInvoiceTemp.CurrencyName = proformaInvoice.Currency != null ? proformaInvoice.Currency.Name : "";
            commercialInvoiceTemp.DueFreightCost = proformaInvoice.DueFreightCost;
            commercialInvoiceTemp.ItemTotalAmount = proformaInvoice.TotalAmount;
            List<CommercialInvoiceDetailViewModel> commercialInvoiceDetailVMList = new List<CommercialInvoiceDetailViewModel>();
            if (proformaInvoice.ProformaInvoiceDetails.Any())
            {
                foreach (var proformaInvoiceDetai in proformaInvoice.ProformaInvoiceDetails)
                {
                    CommercialInvoiceDetailViewModel commercialInvoiceDetailTtemp = new CommercialInvoiceDetailViewModel();
                    commercialInvoiceDetailTtemp.ItemId = proformaInvoiceDetai.ItemId;
                    if (proformaInvoiceDetai.Item != null)
                    {
                        commercialInvoiceDetailTtemp.ItemName = proformaInvoiceDetai.Item.Name;
                    }
                    commercialInvoiceDetailTtemp.Quantity = proformaInvoiceDetai.Quantity;
                    commercialInvoiceDetailTtemp.UnitId = proformaInvoiceDetai.UnitId;
                    if (proformaInvoiceDetai.UnitOfMeasurement != null)
                    {
                        commercialInvoiceDetailTtemp.UnitName = proformaInvoiceDetai.UnitOfMeasurement.Name;
                    }

                    commercialInvoiceDetailTtemp.UnitPrice = proformaInvoiceDetai.UnitPrice;
                    commercialInvoiceDetailTtemp.Amount = proformaInvoiceDetai.Amount;
                    commercialInvoiceDetailVMList.Add(commercialInvoiceDetailTtemp);
                }
            }
            commercialInvoiceTemp.CommercialInvoiceDetails = commercialInvoiceDetailVMList;

            return Json(commercialInvoiceTemp, JsonRequestBehavior.AllowGet);
        }

        public void UpoladFile(string name, string commercialInvoiceNo, HttpPostedFileBase file)
        {
            var length = Request.ContentLength;
            var bytes = new byte[length];
            Request.InputStream.Read(bytes, 0, length);

            string str = file.FileName;
            string ext = str.Substring(0, str.LastIndexOf(".") + 1).TrimEnd('.');

            if (System.IO.File.Exists(Server.MapPath("~/Files/CommercialInvoice/" + commercialInvoiceNo + '_' + ext + Path.GetExtension(file.FileName))))
            {
                System.IO.File.Delete(Server.MapPath("~/Files/CommercialInvoice/" + commercialInvoiceNo + '_' + ext + Path.GetExtension(file.FileName)));
            }
            var saveToFileLoc = Server.MapPath("~/Files/CommercialInvoice/" + commercialInvoiceNo + '_' + ext + Path.GetExtension(file.FileName));

            var fileStream = new FileStream(saveToFileLoc, FileMode.Create, FileAccess.ReadWrite);
            fileStream.Write(bytes, 0, length);
            fileStream.Close();
        }
    }


    public class CommercialInvoiceViewModel
    {
        public CommercialInvoiceViewModel()
        {
            this.CommercialInvoiceDetails = new List<CommercialInvoiceDetailViewModel>();
            this.LcDocuments = new List<LCDocument>();
        }
        public System.Guid Id { get; set; }
        public Guid LCId { get; set; }
        public string LCNo { get; set; }
        public string CommercialInvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceFilePath { get; set; }
        public double? DueFreightCost { get; set; }
        public double? ItemTotalAmount { get; set; }
        public double? FreightCost { get; set; }
        public double? InsurrenceCost { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public double? ConversionRateInLocalCurrency { get; set; }
        public virtual ICollection<CommercialInvoiceDetailViewModel> CommercialInvoiceDetails { get; set; }
        public ICollection<LCDocument> LcDocuments { get; set; }
    }

    public partial class CommercialInvoiceDetailViewModel
    {
        public System.Guid Id { get; set; }
        public Guid CommercialInvoiceId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; }
        public double Quantity { get; set; }
        public Nullable<double> PrevReceivedQuantity { get; set; }
        public Nullable<double> UnitPrice { get; set; }
        public Nullable<double> Amount { get; set; }
        public int? PackingUnitId { get; set; }
        public string PackingUnitName { get; set; }
        public int PackingQuantity { get; set; }
        public Nullable<double> NetWeight { get; set; }
        public Nullable<double> GrossWeight { get; set; }
        public int? WeightUnitId { get; set; }
        public string WeightUnitName { get; set; }
    }

    public partial class LCShipmentDocumentViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid LCShipmentId { get; set; }
        public System.Guid LCDocumentId { get; set; }
        public string DocumentPath { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public string DocumentName { get; set; }
    }
}