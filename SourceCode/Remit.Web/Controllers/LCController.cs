using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Model.Models;
using Remit.Service;
using Helpers;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.IO;
using MoreLinq;
using Remit.Service.Enums;
using System.Web.Configuration;
using Remit.Web.Helpers;

namespace Remit.Web.Controllers
{
    public class LCController : Controller
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
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();
        public LCController(ILCService lcService, ILCDocumentService lcDocumentService, ILCAmendmentService lcAmendmentService, ICountryService countryService, ISupplierService supplierService, IProformaInvoiceService proformaInvoiceService, IProformaInvoiceDetailService proformaInvoiceDetailService, IUnitOfMeasurementService unitOfMeasurementService, IWorkflowactionSettingService workflowactionSettingService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
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
                    return View("LC");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateLC(LC lc)
        {
            var isSuccess = false;
            var message = string.Empty;
            
            try
            {
                var isNew = lc.Id == Guid.Empty ? true : false;
                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                  roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                      Helpers.UserSession.GetUserFromSession().RoleId);

                if (isNew)
                {
                    lc.Id = Guid.NewGuid();
                    foreach (var a in lc.LCAmendments)
                    {
                        a.Id = Guid.NewGuid();
                        a.LCId = lc.Id;
                    }
                    foreach (var a in lc.LCDocuments)
                    {
                        a.Id = Guid.NewGuid();
                        a.LCId = lc.Id;
                    }
                    if (permission.CreateOperation == true)
                    {
                        if (lcService.CreateLC(lc))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceLC.LblLC);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceLC.LblLC);
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
                        var lcObj = this.lcService.GetLC(lc.Id);
                        if (lcObj.LCAmendments != null)
                        {
                            foreach (var a in lcObj.LCAmendments.ToList())
                            {
                                this.lcAmendmentService.DeleteLCAmendment(a.Id);
                            }
                        }

                        if (lc.LCAmendments != null)
                        {
                            foreach (var a in lc.LCAmendments)
                            {
                                a.Id = Guid.NewGuid();
                                a.LCId = lc.Id;
                                this.lcAmendmentService.CreateLCAmendment(a);
                            }
                        }

                        /*if (lcObj.LCDocuments != null)
                        {
                            foreach (var a in lcObj.LCDocuments.ToList())
                            {
                                this.lcDocumentService.DeleteLCDocument(a.Id);
                            }
                        }

                        if (lc.LCDocuments != null)
                        {
                            foreach (var a in lc.LCDocuments)
                            {
                                a.Id = Guid.NewGuid();
                                a.LCId = lc.Id;
                                this.lcDocumentService.CreateLCDocument(a);
                            }
                        }*/

                        lcObj.LCNo = lc.LCNo;
                        lcObj.LCIssueDate = lc.LCIssueDate;
                        lcObj.OpeningBankId = lc.OpeningBankId;
                        lcObj.OpeningBranchId = lc.OpeningBranchId;
                        lcObj.Status = lc.Status;
                        lcObj.PaymentTerms = lc.PaymentTerms;

                        if (this.lcService.UpdateLC(lcObj))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceLC.LblLC);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceLC.LblLC);
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
        public JsonResult DeleteLC(Guid id)
        {
            var isSuccess = true;
            var message = string.Empty;
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);
            
            if (permission.DeleteOperation == true)
            {
                var chkLCDocument = lcDocumentService.GetAllLCDocument().Where(lc => lc.Id == id);

                foreach (var document in chkLCDocument)
                {
                    this.lcDocumentService.DeleteLCDocument(document.Id);
                }

                var chkLCAmendment = lcAmendmentService.GetAllLCAmendment().Where(lc => lc.Id == id);

                foreach (var amendment in chkLCAmendment)
                {
                    this.lcAmendmentService.DeleteLCAmendment(amendment.Id);
                }


                isSuccess = this.lcService.DeleteLC(id);
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

        public JsonResult GetLC(Guid id)
        {
            var lc = this.lcService.GetLC(id);
            LCViewModel lcTemp = null;

            if (lc != null)
            {
                lcTemp = new LCViewModel();
                lcTemp.Id = lc.Id;
                lcTemp.LCNo = lc.LCNo;
                lcTemp.LCIssueDate = lc.LCIssueDate != null ? lc.LCIssueDate.ToString(dateTimeFormat) : "";
                lcTemp.OpeningBankId = lc.OpeningBankId;
                lcTemp.OpeningBranchId = lc.OpeningBranchId;
                lcTemp.Status = lc.Status;
                lcTemp.PaymentTerms = lc.PaymentTerms;
                if (lc.LCAmendments.Count() > 0)
                {
                    List<LCAmendmentViewModel> LCAmendmentsVMList = new List<LCAmendmentViewModel>();
                    foreach (var lcAmendment in lc.LCAmendments)
                    {
                        LCAmendmentViewModel lcAmendmentTtemp = new LCAmendmentViewModel();
                        lcAmendmentTtemp.Id = lcAmendment.Id;
                        lcAmendmentTtemp.LCId = lcAmendment.LCId;
                        lcAmendmentTtemp.ProformaInvoiceId = lcAmendment.ProformaInvoiceId;
                        lcAmendmentTtemp.AmendmentNo = lcAmendment.AmendmentNo;
                        lcAmendmentTtemp.AmendmentDate = lcAmendment.AmendmentDate != null ? lcAmendment.AmendmentDate.Value.ToString(dateTimeFormat) : "";
                        lcAmendmentTtemp.AmendmentNote = lcAmendment.AmendmentNote;
                        lcAmendmentTtemp.TotalAmount = lcAmendment.TotalAmount;
                        lcAmendmentTtemp.CurrencyId = lcAmendment.CurrencyId;
                        lcAmendmentTtemp.ConversionRateInLocalCurrency = lcAmendment.ConversionRateInLocalCurrency;
                        
                        LCAmendmentsVMList.Add(lcAmendmentTtemp);
                    }
                    lcTemp.LCAmendments = LCAmendmentsVMList;
                }
                if (lc.LCDocuments.Count() > 0)
                {
                    List<LCDocumentViewModel> LCDocumentsVMList = new List<LCDocumentViewModel>();
                    foreach (var lcDoc in lc.LCDocuments)
                    {
                        LCDocumentViewModel lcDetailTtemp = new LCDocumentViewModel();
                        lcDetailTtemp.Id = lcDoc.Id;
                        lcDetailTtemp.LCId = lcDoc.LCId;
                        lcDetailTtemp.DocumentName = lcDoc.DocumentName;
                        LCDocumentsVMList.Add(lcDetailTtemp);
                    }
                    lcTemp.LCDocuments = LCDocumentsVMList;
                }
            }
            return Json(lcTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLCList()
        {
            var lcListObj = this.lcService.GetAllLC();
            List<LCViewModel> lcVMList = new List<LCViewModel>();

            foreach (var lc in lcListObj)
            {
                LCViewModel lcTemp = new LCViewModel();
                lcTemp.Id = lc.Id;
                lcTemp.LCNo = lc.LCNo;
                lcTemp.Status = lc.Status;
                lcVMList.Add(lcTemp);
            }
            return Json(lcVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLCListBySupplierAndYear(int year, int supplierId, bool incomplete)
        {
            var lcListObj = this.lcService.GetAllLC();//.ToList();

            if (incomplete)
                lcListObj = lcListObj.Where(c => c.Status == (int)StatusEnum.Running).ToList();

            if (year != 0)
                lcListObj = lcListObj.Where(c => c.LCIssueDate.Year == year).ToList();

            if (supplierId != 0)
            {   
                //List<LC> lcList = new List<LC>();
                //foreach (var aLc in lcListObj)
                //{
                //    var amend = aLc.LCAmendments.FirstOrDefault(lc => lc.LCId == aLc.Id);
                //    if (amend != null)
                //    {
                //        if (amend.ProformaInvoice.SupplierId == supplierId)
                //        {
                //            lcList.Add(aLc);
                //        }
                //    }
                //}
                lcListObj = lcListObj.Where(c => c.LCAmendments.First().ProformaInvoice.SupplierId == supplierId).ToList();
                //lcListObj = lcListObj.Where(c => c.LCAmendments.FirstOrDefault(xx=> xx.LCId == c.Id).ProformaInvoice.SupplierId == supplierId).ToList();
            }
                

            List<LCViewModel> lcVMList = new List<LCViewModel>();
            foreach (var lc in lcListObj)
            {
                LCViewModel lcTemp = new LCViewModel();

                lcTemp.Id = lc.Id;
                lcTemp.LCNo = lc.LCNo;

                lcVMList.Add(lcTemp);
            }
            return Json(lcVMList, JsonRequestBehavior.AllowGet);
        }
    }
    public class LCViewModel
    {
        public LCViewModel()
        {
            this.LCDocuments = new List<LCDocumentViewModel>();
            this.LCAmendments = new List<LCAmendmentViewModel>();
            this.customInvoiceShipmentMixed = new List<CustomInvoiceShipmentMixedViewModel>();
        }
        public Guid Id { get; set; }
        public string LCNo { get; set; }
        public string LCIssueDate { get; set; }
        public int? OpeningBankId { get; set; }
        public int? OpeningBranchId { get; set; }
        public int? Status { get; set; }
        public string PaymentTerms { get; set; }
        public double? LcAmendentTotal { get; set; }
        public string ItemsDescription { get; set; }
        public string ItemsAmountsUnit { set; get; }
        public string LcAmendentCurrency { set; get; }
        public virtual ICollection<LCDocumentViewModel> LCDocuments { get; set; }
        public virtual ICollection<LCAmendmentViewModel> LCAmendments { get; set; }
        public virtual ICollection<CustomInvoiceShipmentMixedViewModel> customInvoiceShipmentMixed { get; set; }
    }
    public class LCAmendmentViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid LCId { get; set; }
        public Nullable<System.Guid> ProformaInvoiceId { get; set; }
        public int AmendmentNo { get; set; }
        public string AmendmentDate { get; set; }
        public string AmendmentNote { get; set; }
        public Nullable<double> TotalAmount { get; set; }
        public int? CurrencyId { get; set; }
        public Nullable<double> ConversionRateInLocalCurrency { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual LC LC { get; set; }
        public virtual ProformaInvoice ProformaInvoice { get; set; }
    }
    public class LCDocumentViewModel
    {
        public Guid Id { get; set; }
        public Guid LCId { get; set; }
        public string DocumentName { get; set; }
    }
}