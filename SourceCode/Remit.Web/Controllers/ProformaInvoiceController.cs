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
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class ProformaInvoiceController : Controller
    {
        public readonly IProformaInvoicePurchaseRequisitionMappingService proformaInvoicePurchaseRequisitionMappingService;
        public readonly IProformaInvoiceService proformaInvoiceService;
        public readonly IProformaInvoiceDetailService proformaInvoiceDetailService;
        public readonly IPurchaseRequisitionDetailService purchaseRequisitionDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IItemService itemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly IWorkflowactionSettingService workflowactionSettingService;
        public readonly INotificationSettingService notificationSettingService;

        public readonly ISupplierService supplierService;
        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        protected long timeZoneOffset = UserSession.GetTimeZoneOffset();

        public ProformaInvoiceController(ISupplierService supplierService, IProformaInvoiceService proformaInvoiceService, IPurchaseRequisitionDetailService purchaseRequisitionDetailService,
            ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService, IProformaInvoicePurchaseRequisitionMappingService proformaInvoicePurchaseRequisitionMappingService, 
            IProformaInvoiceDetailService proformaInvoiceDetailService, IWorkflowactionSettingService workflowactionSettingService,
            INotificationSettingService notificationSettingService, IItemService itemService)
        {
            this.supplierService = supplierService;
            this.proformaInvoicePurchaseRequisitionMappingService = proformaInvoicePurchaseRequisitionMappingService;
            this.proformaInvoiceService = proformaInvoiceService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.proformaInvoiceDetailService = proformaInvoiceDetailService;
            this.purchaseRequisitionDetailService = purchaseRequisitionDetailService;

            this.workflowactionSettingService = workflowactionSettingService;
            this.notificationSettingService = notificationSettingService;
            this.itemService = itemService;
        }

        const string url = "/ProformaInvoice/Index";
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:proformaInvoice" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /ProformaInvoice/Index
        public ActionResult Index()
        {
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("ProformaInvoice");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }

        [HttpPost]
        public JsonResult CreateProformaInvoice(ProformaInvoice proformaInvoice)
        {
            var isSuccess = false;
            var message = string.Empty;
            try
            {
                var isNew = proformaInvoice.Id == Guid.Empty ? true : false;
                permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                                  roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                      Helpers.UserSession.GetUserFromSession().RoleId);

                if (isNew)
                {
                    proformaInvoice.Id = Guid.NewGuid();
                    proformaInvoice.CreatedBy = UserSession.GetUserFromSession().EmployeeId;
                    foreach (var a in proformaInvoice.ProformaInvoiceDetails)
                    {
                        a.Id = Guid.NewGuid();
                        a.ProformaInvoiceId = proformaInvoice.Id;
                    }
                    foreach (var PIPRMapping in proformaInvoice.ProformaInvoicePurchaseRequisitionMappings)
                    {
                        PIPRMapping.Id = Guid.NewGuid();
                        PIPRMapping.ProformaInvoiceId = proformaInvoice.Id;
                    }
                    if (permission.CreateOperation == true)
                    {
                        if (this.proformaInvoiceService.CreateProformaInvoice(proformaInvoice))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_save, Resources.ResourceProformaInvoice.LblProformaInvoice);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_unsave, Resources.ResourceProformaInvoice.LblProformaInvoice);
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
                        var proformaInvoiceObj = this.proformaInvoiceService.GetProformaInvoice(proformaInvoice.Id);
                        if (proformaInvoiceObj.ProformaInvoiceDetails != null)
                        {
                            foreach (var a in proformaInvoiceObj.ProformaInvoiceDetails.ToList())
                            {
                                this.proformaInvoiceDetailService.DeleteProformaInvoiceDetail(a.Id);
                            }
                        }

                        if (proformaInvoice.ProformaInvoiceDetails != null)
                        {
                            foreach (var a in proformaInvoice.ProformaInvoiceDetails)
                            {
                                a.Id = Guid.NewGuid();
                                a.ProformaInvoiceId = proformaInvoice.Id;
                                this.proformaInvoiceDetailService.CreateProformaInvoiceDetail(a);
                            }
                        }
                        if (proformaInvoiceObj.ProformaInvoicePurchaseRequisitionMappings != null)
                        {
                            foreach (var PIPRMapping in proformaInvoiceObj.ProformaInvoicePurchaseRequisitionMappings.ToList())
                            {
                                this.proformaInvoicePurchaseRequisitionMappingService.DeleteProformaInvoicePurchaseRequisitionMapping(PIPRMapping.Id);
                            }
                        }
                        if (proformaInvoice.ProformaInvoicePurchaseRequisitionMappings != null)
                        {
                            foreach (var PIPRMapping in proformaInvoice.ProformaInvoicePurchaseRequisitionMappings)
                            {
                                PIPRMapping.Id = Guid.NewGuid();
                                PIPRMapping.ProformaInvoiceId = proformaInvoice.Id;
                                this.proformaInvoicePurchaseRequisitionMappingService.CreateProformaInvoicePurchaseRequisitionMapping(PIPRMapping);
                            }
                        }

                        proformaInvoiceObj.PINo = proformaInvoice.PINo;
                        proformaInvoiceObj.Date = proformaInvoice.Date;
                        proformaInvoiceObj.SupplierId = proformaInvoice.SupplierId;
                        proformaInvoiceObj.FreightCost = proformaInvoice.FreightCost;
                        proformaInvoiceObj.TotalAmount = proformaInvoice.TotalAmount;
                        proformaInvoiceObj.DueFreightCost = proformaInvoice.FreightCost;
                        proformaInvoiceObj.TotalAmountWithFreightCost = proformaInvoice.TotalAmountWithFreightCost;
                        proformaInvoiceObj.CurrencyId = proformaInvoice.CurrencyId;
                        proformaInvoiceObj.ShipFrom = proformaInvoice.ShipFrom;
                        proformaInvoiceObj.LastDateOfShipment = proformaInvoice.LastDateOfShipment;
                        proformaInvoiceObj.ShipToAddress = proformaInvoice.ShipToAddress;
                        proformaInvoiceObj.PIValidityInDays = proformaInvoice.PIValidityInDays;
                        proformaInvoiceObj.CreatedBy = proformaInvoice.CreatedBy;
                        proformaInvoiceObj.BeneficiaryBankInfo = proformaInvoice.BeneficiaryBankInfo;
                        proformaInvoiceObj.NegotiatingBankInfo = proformaInvoice.NegotiatingBankInfo;

                        if (this.proformaInvoiceService.UpdateProformaInvoice(proformaInvoiceObj))
                        {
                            isSuccess = true;
                            message = string.Format(Resources.ResourceCommon.CMsg_update, Resources.ResourceProformaInvoice.LblProformaInvoice);
                        }
                        else
                        {
                            message = string.Format(Resources.ResourceCommon.CMsg_notupdate, Resources.ResourceProformaInvoice.LblProformaInvoice);
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

        public JsonResult DeleteProformaInvoice(Guid id)
        {
            var isSuccess = true;
            var message = string.Empty;

            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                                Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = DeleteChildProformaInvoice(id);
                if (isSuccess)
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_delete, Resources.ResourceProformaInvoice.LblProformaInvoice);
                }
                else
                {
                    message = string.Format(Resources.ResourceCommon.CMsg_notdelete, Resources.ResourceProformaInvoice.LblProformaInvoice);
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

        public Boolean DeleteChildProformaInvoice(Guid id)
        {
            var isSuccess = true;

            var proformaInvoiceDetails = this.proformaInvoiceDetailService.GetAllProformaInvoiceDetail().Where(a => a.ProformaInvoiceId == id);

            foreach (var proformaInvoiceDetail in proformaInvoiceDetails)
            {
                this.proformaInvoiceDetailService.DeleteProformaInvoiceDetail(proformaInvoiceDetail.Id);
            }

            var chkPRs = this.proformaInvoicePurchaseRequisitionMappingService.GetAllProformaInvoicePurchaseRequisitionMapping().Where(a => a.ProformaInvoiceId == id);

            foreach (var chkPR in chkPRs)
            {
                this.proformaInvoicePurchaseRequisitionMappingService.DeleteProformaInvoicePurchaseRequisitionMapping(chkPR.Id);
            }

            var chkPIs = this.proformaInvoiceService.GetAllProformaInvoice().Where(a => a.ParentPIId == id);

            foreach (var chkPI in chkPIs)
            {
                DeleteChildProformaInvoice(chkPI.Id);
            }

            isSuccess = this.proformaInvoiceService.DeleteProformaInvoice(id);

            return isSuccess;
        }

        public JsonResult GetProformaInvoice(Guid id)
        {
            var proformaInvoice = this.proformaInvoiceService.GetProformaInvoice(id);
            ProformaInvoiceViewModel proformaInvoiceTemp = null;

            if (proformaInvoice != null)
            {
                proformaInvoiceTemp = new ProformaInvoiceViewModel();
                proformaInvoiceTemp.Id = proformaInvoice.Id;
                proformaInvoiceTemp.ParentPIId = proformaInvoice.ParentPIId;
                proformaInvoiceTemp.PINo = proformaInvoice.PINo;
                proformaInvoiceTemp.Date = proformaInvoice.Date != null ? proformaInvoice.Date.Value.ToString(dateTimeFormat) : "";
                proformaInvoiceTemp.SupplierId = proformaInvoice.SupplierId;
                proformaInvoiceTemp.SupplierName = proformaInvoice.Supplier != null ? proformaInvoice.Supplier.Name : "";
                proformaInvoiceTemp.FreightCost = proformaInvoice.FreightCost;
                proformaInvoiceTemp.TotalAmount = proformaInvoice.TotalAmount;

                proformaInvoiceTemp.DueFreightCost = proformaInvoice.DueFreightCost;
                proformaInvoiceTemp.TotalAmountWithFreightCost = proformaInvoice.TotalAmountWithFreightCost;

                proformaInvoiceTemp.CurrencyName = proformaInvoice.Currency != null ? proformaInvoice.Currency.Name : "";
                proformaInvoiceTemp.CurrencyId = proformaInvoice.CurrencyId;
                proformaInvoiceTemp.ShipFrom = proformaInvoice.ShipFrom;
                proformaInvoiceTemp.LastDateOfShipment = proformaInvoice.LastDateOfShipment != null ? proformaInvoice.LastDateOfShipment.Value.ToString(dateTimeFormat) : "";
                proformaInvoiceTemp.ShipToAddress = proformaInvoice.ShipToAddress;
                proformaInvoiceTemp.PIValidityInDays = proformaInvoice.PIValidityInDays;
                proformaInvoiceTemp.CreatedBy = proformaInvoice.CreatedBy;
                proformaInvoiceTemp.BeneficiaryBankInfo = proformaInvoice.BeneficiaryBankInfo;
                proformaInvoiceTemp.NegotiatingBankInfo = proformaInvoice.NegotiatingBankInfo;
                if (proformaInvoice.ProformaInvoiceDetails.Count() > 0)
                {
                    List<ProformaInvoiceDetailViewModel> proformaInvoiceDetailVMList = new List<ProformaInvoiceDetailViewModel>();
                    foreach (var proformaInvoiceDetai in proformaInvoice.ProformaInvoiceDetails.OrderBy(a => a.Item.ItemCategory.Name).ThenBy(b => b.Item.Name))
                    {
                        ProformaInvoiceDetailViewModel proformaInvoiceDetailTtemp = new ProformaInvoiceDetailViewModel();
                        proformaInvoiceDetailTtemp.Id = proformaInvoiceDetai.Id;
                        proformaInvoiceDetailTtemp.ProformaInvoiceId = proformaInvoiceDetai.ProformaInvoiceId;
                        proformaInvoiceDetailTtemp.ItemId = proformaInvoiceDetai.ItemId;
                        proformaInvoiceDetailTtemp.ItemCategoryId = proformaInvoiceDetai.Item.ItemCategoryId;
                        proformaInvoiceDetailTtemp.ItemName = proformaInvoiceDetai.Item != null ? proformaInvoiceDetai.Item.Name : "";
                        proformaInvoiceDetailTtemp.Quantity = proformaInvoiceDetai.Quantity;
                        proformaInvoiceDetailTtemp.UnitId = proformaInvoiceDetai.UnitId;
                        proformaInvoiceDetailTtemp.UnitName = proformaInvoiceDetai.UnitOfMeasurement != null ? proformaInvoiceDetai.UnitOfMeasurement.Name : "";
                        proformaInvoiceDetailTtemp.UnitPrice = proformaInvoiceDetai.UnitPrice;
                        proformaInvoiceDetailTtemp.Amount = proformaInvoiceDetai.Amount;
                        proformaInvoiceDetailVMList.Add(proformaInvoiceDetailTtemp);
                    }
                    proformaInvoiceTemp.ProformaInvoiceDetails = proformaInvoiceDetailVMList;
                }
                if (proformaInvoice.ProformaInvoicePurchaseRequisitionMappings.Count() > 0)
                {
                    List<ProformaInvoicePurchaseRequisitionMappingViewModel> proformaInvoicePRVMList = new List<ProformaInvoicePurchaseRequisitionMappingViewModel>();
                    foreach (var IRPRMapping in proformaInvoice.ProformaInvoicePurchaseRequisitionMappings)
                    {
                        ProformaInvoicePurchaseRequisitionMappingViewModel proformaInvoicePRTtemp = new ProformaInvoicePurchaseRequisitionMappingViewModel();
                        proformaInvoicePRTtemp.Id = IRPRMapping.Id;
                        proformaInvoicePRTtemp.ProformaInvoiceId = (Guid)IRPRMapping.ProformaInvoiceId;
                        proformaInvoicePRTtemp.PurchaseRequisitionId = (Guid)IRPRMapping.PurchaseRequisitionId;
                        proformaInvoicePRVMList.Add(proformaInvoicePRTtemp);
                    }
                    proformaInvoiceTemp.ProformaInvoicePurchaseRequisitionMappings = proformaInvoicePRVMList;
                }
            }
            return Json(proformaInvoiceTemp, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProformaInvoiceList()
        {
            var proformaInvoiceListObj = this.proformaInvoiceService.GetAllProformaInvoice();
            List<ProformaInvoiceViewModel> proformaInvoiceVMList = new List<ProformaInvoiceViewModel>();

            foreach (var proformaInvoice in proformaInvoiceListObj)
            {
                ProformaInvoiceViewModel proformaInvoiceTemp = new ProformaInvoiceViewModel();

                proformaInvoiceTemp.Id = proformaInvoice.Id;
                proformaInvoiceTemp.PINo = proformaInvoice.PINo;
                proformaInvoiceTemp.Date = proformaInvoice.Date != null ? proformaInvoice.Date.Value.ToString(dateTimeFormat) : "";
                proformaInvoiceTemp.SupplierId = proformaInvoice.SupplierId;
                proformaInvoiceTemp.FreightCost = proformaInvoice.FreightCost;
                proformaInvoiceTemp.TotalAmount = proformaInvoice.TotalAmount;
                proformaInvoiceTemp.DueFreightCost = proformaInvoice.DueFreightCost;
                proformaInvoiceTemp.TotalAmountWithFreightCost = proformaInvoice.TotalAmountWithFreightCost;

                proformaInvoiceTemp.CurrencyName = proformaInvoice.Currency != null ? proformaInvoice.Currency.Name : "";
                proformaInvoiceTemp.CurrencyId = proformaInvoice.CurrencyId;
                proformaInvoiceTemp.ShipFrom = proformaInvoice.ShipFrom;
                proformaInvoiceTemp.LastDateOfShipment = proformaInvoice.LastDateOfShipment != null ? proformaInvoice.LastDateOfShipment.Value.ToString(dateTimeFormat) : "";
                proformaInvoiceTemp.ShipToAddress = proformaInvoice.ShipToAddress;
                proformaInvoiceTemp.PIValidityInDays = proformaInvoice.PIValidityInDays;
                proformaInvoiceTemp.CreatedBy = proformaInvoice.CreatedBy;
                proformaInvoiceTemp.BeneficiaryBankInfo = proformaInvoice.BeneficiaryBankInfo;
                proformaInvoiceTemp.NegotiatingBankInfo = proformaInvoice.NegotiatingBankInfo;

                proformaInvoiceVMList.Add(proformaInvoiceTemp);
            }
            return Json(proformaInvoiceVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetParentProformaInvoiceList()
        {
            var proformaInvoiceListObj = this.proformaInvoiceService.GetAllProformaInvoice();
            List<ProformaInvoiceViewModel> proformaInvoiceVMList = new List<ProformaInvoiceViewModel>();

            foreach (var proformaInvoice in proformaInvoiceListObj)
            {
                if (proformaInvoice.ParentPIId == null)
                {
                    ProformaInvoiceViewModel proformaInvoiceTemp = new ProformaInvoiceViewModel();

                    proformaInvoiceTemp.Id = proformaInvoice.Id;
                    proformaInvoiceTemp.PINo = proformaInvoice.PINo;

                    proformaInvoiceVMList.Add(proformaInvoiceTemp);
                }
            }
            return Json(proformaInvoiceVMList, JsonRequestBehavior.AllowGet);
        }


//new....

        public JsonResult GetParentProformaInvoiceListWithoutLC()
        {


            var proformaInvoiceListObj = this.proformaInvoiceService.GetAllProformaInvoiceWithoutLc();
            List<ProformaInvoiceViewModel> proformaInvoiceVMList = new List<ProformaInvoiceViewModel>();

            foreach (var proformaInvoice in proformaInvoiceListObj)
            {
                if (proformaInvoice.ParentPIId == null)
                {
                    ProformaInvoiceViewModel proformaInvoiceTemp = new ProformaInvoiceViewModel();

                    proformaInvoiceTemp.Id = proformaInvoice.Id;
                    proformaInvoiceTemp.PINo = proformaInvoice.PINo;

                    proformaInvoiceVMList.Add(proformaInvoiceTemp);
                }
            }
            return Json(proformaInvoiceVMList, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetChildProformaInvoiceList(Guid id)
        {
            var proformaInvoiceListObj = this.proformaInvoiceService.GetAllProformaInvoice().Where(c => c.ParentPIId == id); ;
            List<ProformaInvoiceViewModel> proformaInvoiceVMList = new List<ProformaInvoiceViewModel>();

            foreach (var proformaInvoice in proformaInvoiceListObj)
            {
                ProformaInvoiceViewModel proformaInvoiceTemp = new ProformaInvoiceViewModel();

                proformaInvoiceTemp.Id = proformaInvoice.Id;
                proformaInvoiceTemp.PINo = proformaInvoice.PINo;

                proformaInvoiceVMList.Add(proformaInvoiceTemp);
            }
            return Json(proformaInvoiceVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPiListBySupplierAndYear(int year, int supplierId)
        {
            var proformaInvoiceListObj = this.proformaInvoiceService.GetAllProformaInvoice();
            if (supplierId != 0)
                proformaInvoiceListObj = proformaInvoiceListObj.Where(c => c.SupplierId == supplierId);

            proformaInvoiceListObj = proformaInvoiceListObj.Where(c => c.Date.Value.Year == year);

            List<ProformaInvoiceViewModel> proformaInvoiceVMList = new List<ProformaInvoiceViewModel>();
            foreach (var proformaInvoice in proformaInvoiceListObj)
            {
                ProformaInvoiceViewModel proformaInvoiceTemp = new ProformaInvoiceViewModel();

                proformaInvoiceTemp.Id = proformaInvoice.Id;
                if (proformaInvoice.ParentPIId != null)
                    proformaInvoiceTemp.PINo = "----" + proformaInvoice.PINo;
                else
                    proformaInvoiceTemp.PINo = proformaInvoice.PINo;

                proformaInvoiceVMList.Add(proformaInvoiceTemp);
            }
            return Json(proformaInvoiceVMList.OrderBy(a => a.PINo), JsonRequestBehavior.AllowGet);
        }
        public JsonResult getPRItemByPRIDs(Guid[] ids)
        {
            var itemDetails = this.purchaseRequisitionDetailService.GetAllPurchaseRequisitionDetail().Where(a => ids.Contains(a.PurchaseRequisitionId)).GroupBy(b => b.Item).Select(c => new { Item = c.Key, Quantity = c.Sum(d => d.Quantity) });

            ProformaInvoiceViewModel proformaInvoiceTemp = new ProformaInvoiceViewModel();

            List<ProformaInvoiceDetailViewModel> proformaInvoiceDetailVMList = new List<ProformaInvoiceDetailViewModel>();
            if (itemDetails.Any())
            {
                foreach (var itemDetai in itemDetails)
                {
                    ProformaInvoiceDetailViewModel proformaInvoiceDetailTtemp = new ProformaInvoiceDetailViewModel();
                    proformaInvoiceDetailTtemp.ItemId = itemDetai.Item.Id;
                    if (itemDetai.Item != null)
                    {
                        proformaInvoiceDetailTtemp.ItemName = itemDetai.Item.Name;
                        proformaInvoiceDetailTtemp.ItemCategoryId = itemDetai.Item.ItemCategoryId;
                        proformaInvoiceDetailTtemp.UnitId = itemDetai.Item.PurchaseunitId;
                        if (itemDetai.Item.UnitOfMeasurement != null)
                            proformaInvoiceDetailTtemp.UnitName = itemDetai.Item.UnitOfMeasurement.Name;
                    }
                    proformaInvoiceDetailTtemp.Quantity = itemDetai.Quantity;
                    proformaInvoiceDetailVMList.Add(proformaInvoiceDetailTtemp);
                }
            }
            proformaInvoiceTemp.ProformaInvoiceDetails = proformaInvoiceDetailVMList;

            return Json(proformaInvoiceTemp, JsonRequestBehavior.AllowGet);
        }
    }

    public class ProformaInvoiceViewModel
    {
        public ProformaInvoiceViewModel()
        {
            this.ProformaInvoiceDetails = new List<ProformaInvoiceDetailViewModel>();
            this.ProformaInvoicePurchaseRequisitionMappings = new List<ProformaInvoicePurchaseRequisitionMappingViewModel>();
        }
        public System.Guid Id { get; set; }
        public Guid? ParentPIId { get; set; }
        public string PINo { get; set; }
        public string Date { get; set; }
        public string ShipFrom { get; set; }
        public string ShipToAddress { get; set; }
        public double? FreightCost { get; set; }
        public double? TotalAmount { get; set; }

        public double? DueFreightCost { get; set; }
        public double? TotalAmountWithFreightCost { get; set; }
        public int? SupplierId { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string SupplierName { get; set; }
        public string SupplierAddress { get; set; }
        public string LastDateOfShipment { get; set; }
        public bool IsIncludedInLC { get; set; }
        public int? PIValidityInDays { get; set; }
        public string BeneficiaryBankInfo { get; set; }
        public string NegotiatingBankInfo { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public virtual ICollection<ProformaInvoiceDetailViewModel> ProformaInvoiceDetails { get; set; }
        public virtual ICollection<ProformaInvoicePurchaseRequisitionMappingViewModel> ProformaInvoicePurchaseRequisitionMappings { get; set; }
    }

    public partial class ProformaInvoiceDetailViewModel
    {
        public System.Guid Id { get; set; }
        public Guid ProformaInvoiceId { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public int? ItemCategoryId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; }
        public double Quantity { get; set; }
        public Nullable<double> UnitPrice { get; set; }
        public Nullable<double> Amount { get; set; }
    }
    public partial class ProformaInvoicePurchaseRequisitionMappingViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid ProformaInvoiceId { get; set; }
        public System.Guid PurchaseRequisitionId { get; set; }
        public virtual ProformaInvoice ProformaInvoice { get; set; }
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }
    }
}