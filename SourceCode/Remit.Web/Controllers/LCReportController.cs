using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;

namespace Remit.Web.Controllers
{
    public class LCReportController : Controller
    {
        public readonly IUnitOfMeasurementService unitOfMeasurementService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly ILCService lcService;
        public readonly ILCDocumentService lcDocumentService;
        public readonly ILCAmendmentService lcAmendmentService;
        public readonly IProformaInvoiceService proformaInvoiceService;
        public readonly IProformaInvoiceDetailService proformaInvoiceDetailService;
        public readonly ISupplierService supplierService;
        public readonly ICountryService countryService;
        public readonly ILCShipmentService lcshipmentService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public LCReportController(ILCService lcService, ILCShipmentService lcshipmentService, ILCDocumentService lcDocumentService, ILCAmendmentService lcAmendmentService, ICountryService countryService, ISupplierService supplierService, IProformaInvoiceService proformaInvoiceService, IProformaInvoiceDetailService proformaInvoiceDetailService, IUnitOfMeasurementService unitOfMeasurementService, IWorkflowactionSettingService workflowactionSettingService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.unitOfMeasurementService = unitOfMeasurementService;
            this.workflowactionSettingService = workflowactionSettingService;
            this.lcService = lcService;
            this.lcDocumentService = lcDocumentService;
            this.lcAmendmentService = lcAmendmentService;
            this.proformaInvoiceService = proformaInvoiceService;
            this.proformaInvoiceDetailService = proformaInvoiceDetailService;
            this.supplierService = supplierService;
            this.countryService = countryService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.lcshipmentService = lcshipmentService;
        }

        const string url = "/LC/Index";
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:lc" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /LC/Index
        public ActionResult Index()
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("LCReport");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        public ActionResult GetLcReportList(string fromDate, string toDate, string partNumber)
        {
            List<LCViewModel> lcList = new List<LCViewModel>();
            DateTime frmDateTime = DateTime.Parse(fromDate);
            DateTime toDateTime = DateTime.Parse(toDate);

            var getLc = lcService.GetAllLC()
                .Where(allLc => allLc.LCIssueDate >= frmDateTime && allLc.LCIssueDate <= toDateTime);
            
            if (getLc.Any())
            {
                foreach (var aLc in getLc)
                {
                    LCViewModel tempLc = new LCViewModel();
                    tempLc.Id = aLc.Id;
                    tempLc.LCNo = aLc.LCNo;
                    if (aLc.LCIssueDate != null) tempLc.LCIssueDate = aLc.LCIssueDate.ToString(dateTimeFormat);
                    tempLc.ItemsDescription = "";
                    tempLc.ItemsAmountsUnit = "";
                    var getAmendent = lcAmendmentService.GetHighestLCAmendmentByLcId(tempLc.Id);

                    if (getAmendent != null)
                    {
                        tempLc.LcAmendentTotal = getAmendent.TotalAmount;
                        if (getAmendent.Currency != null)
                            tempLc.LcAmendentCurrency = getAmendent.Currency.Symbol;
                        else
                            tempLc.LcAmendentCurrency = "";

                        if (getAmendent.ProformaInvoice != null)
                        {
                            var pIds = getAmendent.ProformaInvoice.ProformaInvoiceDetails;
                            if (pIds != null)
                            {
                                foreach (var aPId in pIds)
                                {
                                    var unitName = aPId.UnitOfMeasurement != null ? aPId.UnitOfMeasurement.Name : "";
                                    tempLc.ItemsDescription += ", " + aPId.Item.Name + " (" + aPId.Quantity + " "+ unitName + ")";
                                    
                                    //tempLc.ItemsAmountsUnit +=;
                                }
                            }
                        }
                    }
                    List<CustomInvoiceShipmentMixedViewModel> aList = new List<CustomInvoiceShipmentMixedViewModel>();
                    if (aLc.CommercialInvoices.Any())
                    {
                        
                        foreach (var ci in aLc.CommercialInvoices)  
                        {
                            CustomInvoiceShipmentMixedViewModel aModel = new CustomInvoiceShipmentMixedViewModel();
                            aModel.CommercialInvoiceNo = ci.CommercialInvoiceNo;
                            if (ci.InvoiceDate != null)
                                aModel.InvoiceDateString = ci.InvoiceDate.Value.ToString(dateTimeFormat);
                            aModel.CurrenyCode = ci.Currency != null ? ci.Currency.Symbol : "";
                            aModel.ItemTotalAmount = ci.ItemTotalAmount;

                            var getShipment = lcshipmentService.GetLcShipmentByCI(ci.Id);
                            if (getShipment != null)
                            {
                                if (getShipment.ShipmentDate != null)
                                    aModel.ShipmentDateString = getShipment.ShipmentDate.Value.ToString(dateTimeFormat);
                                if (getShipment.ArrivalDate != null)
                                    aModel.ArrivalDateString = getShipment.ArrivalDate.Value.ToString(dateTimeFormat);
                                if (getShipment.BillOfEntryDate != null)
                                    aModel.BillOfEntryDateString = getShipment.BillOfEntryDate.Value.ToString(dateTimeFormat);
                                if (getShipment.PaymentDate != null)
                                    aModel.PaymentDateString = getShipment.PaymentDate.Value.ToString(dateTimeFormat);
                                aModel.Container = getShipment.Container;
                                aModel.Duty = getShipment.Duty;
                                aModel.PaymentAmount = getShipment.PaidAmount;
                                aModel.BillOfEntryNo = getShipment.BillOfEntryNo;
                            }

                            if (ci.CommercialInvoiceDetails.Any())
                            {
                                var name = string.Empty;
                                foreach (var ciDet in ci.CommercialInvoiceDetails)
                                {
                                    name += ciDet.Item != null ? ciDet.Item.Name : "";
                                    name += " (" + ciDet.Quantity + " ";
                                    name += ciDet.UnitOfMeasurement != null ? ciDet.UnitOfMeasurement.Name : "";
                                    name += ")" + ", ";
                                }
                                name = name.Trim();
                                aModel.InvoiceQtyWithUnitAndItemName = name.Trim(',');
                            }

                            aList.Add(aModel);
                        }
                    }

                    tempLc.customInvoiceShipmentMixed = aList;
                    tempLc.ItemsDescription = tempLc.ItemsDescription.TrimStart(',');
                    //tempLc.ItemsAmountsUnit = tempLc.ItemsAmountsUnit.TrimStart(',');
                    lcList.Add(tempLc);

                }
            }
            return Json(lcList,JsonRequestBehavior.AllowGet);
        }



	}

    public class CustomInvoiceShipmentMixedViewModel
    {
        public string CommercialInvoiceNo { get; set; }
        public Nullable<System.DateTime> InvoiceDate { get; set; }
        public string InvoiceDateString { get; set; }
        public Nullable<double> ItemTotalAmount { get; set; }

        public string ShipmentDateString { get; set; }
        public string Container { get; set; }
        public string ArrivalDateString { get; set; }
        public string BillOfEntryNo { get; set; }
        public string BillOfEntryDateString { get; set; }
        public Nullable<double> Duty { get; set; }
        public string PaymentDateString { get; set; }
        public double? PaymentAmount { set; get; }
        public string CurrenyCode { set; get; }
        public string InvoiceQtyWithUnitAndItemName { set; get; }
       


    }
}