using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Remit.CachingService;
using Remit.Data.Repository;
using Remit.Model.Models;
using Remit.Service;

namespace Remit.Web.Controllers
{
    public class LCShipmentController : Controller
    {
        public readonly ILCShipmentService lcShipmentService;
        public readonly ICommercialInvoiceService commercialInvoiceService;
        public readonly ICommercialInvoiceDetailService commercialInvoiceDetailService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;
        public readonly ILCShipmentExpenseHeadCategoryService lcShipmentExpenseHeadCategoryService;

        private static readonly ICacheProvider cacheProvider = new DefaultCacheProvider();

        public LCShipmentController(ILCShipmentService lcShipmentService, ICommercialInvoiceService commercialInvoiceService, ICommercialInvoiceDetailService commercialInvoiceDetailService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService, ILCShipmentExpenseHeadCategoryService lcShipmentExpenseHeadCategoryService)
        {
            this.lcShipmentService = lcShipmentService;
            this.commercialInvoiceService = commercialInvoiceService;
            this.commercialInvoiceDetailService = commercialInvoiceDetailService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
            this.lcShipmentExpenseHeadCategoryService = lcShipmentExpenseHeadCategoryService;
        }

        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
        string cacheKey = "permission:lcShipment" + Helpers.UserSession.GetUserFromSession().RoleId;
        RoleSubModuleItem permission = null;

        // GET: /LCShipment/
        public ActionResult Index()
        {
            const string url = "/LCShipment/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission != null)
            {
                if (permission.ReadOperation == true)
                {
                    ViewBag.LocalCurrency = ConfigurationManager.AppSettings["LocalCurrency"];
                    cacheProvider.Set(cacheKey, permission, 240);
                    return View("LCShipment");
                }
                else
                {
                    return View("~/Views/Shared/NoPermission.cshtml");
                }
            }

            return View("~/Views/Shared/NoPermission.cshtml");
        }


        public ActionResult LCShipment()
        {
            return View();
        }



        [HttpPost]
        public JsonResult CreateLCShipment(LCShipment lcShipment, List<LCShipmentExpenseHeadCategoryViewModel> lcShipmentExpenseHeadCategory)
        {
            //return null;
            var isSuccess = false;
            var message = string.Empty;
            var isNew = lcShipment.Id == Guid.Empty ? true : false;
            const string url = "/LCShipment/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ??
                         roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url, Helpers.UserSession.GetUserFromSession().RoleId);

            if (isNew)
            {
                if (permission.CreateOperation == true)
                {
                    lcShipment.Id = Guid.NewGuid();
                    List<LCShipmentExpense> lcShipmentExpenses = new List<LCShipmentExpense>();
                    if (lcShipmentExpenseHeadCategory.Any())
                    {
                        foreach (var lcexpensecat in lcShipmentExpenseHeadCategory)
                        {
                            foreach (var expenseHead in lcexpensecat.LCShipmentExpenseHeads)
                            {
                                LCShipmentExpense aLcShipmentExpense = new LCShipmentExpense();
                                aLcShipmentExpense.Id = Guid.NewGuid();
                                aLcShipmentExpense.LCShipmentExpenseHeadId = expenseHead.Id;
                                aLcShipmentExpense.LCShipmentId = lcShipment.Id;
                                aLcShipmentExpense.AmountInLocalCurrency = expenseHead.AmountInLocal;
                                lcShipmentExpenses.Add(aLcShipmentExpense);
                            }
                        }
                    }

                    if (lcShipmentExpenses.Any())
                    {
                        lcShipment.LCShipmentExpenses = lcShipmentExpenses;
                    }

                    if (lcShipment.LCShipmentDocuments.Any())
                    {
                        foreach (var aDocument in lcShipment.LCShipmentDocuments)
                        {
                            aDocument.Id = Guid.NewGuid();
                            aDocument.LCShipmentId = lcShipment.Id;
                        }

                    }

                    //if (!CheckIsExist(lcShipment))
                    //{
                    if (this.lcShipmentService.CreateLCShipment(lcShipment))
                    {
                        isSuccess = true;
                        message = Resources.ResourceLCShipment.SavedSuccess;
                    }
                    else
                    {
                        message = Resources.ResourceLCShipment.SavedUnSuccess; 
                    }
                    //}
                    //else
                    //{
                    //    isSuccess = false;
                    //    message = "Can't save. Same lcShipment name found!";
                    //}
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
                    var oldLcShipment = lcShipmentService.GetLCShipment(lcShipment.Id);

                    if (lcShipment.LCShipmentDocuments.Any())
                    {
                        foreach (var aDocument in lcShipment.LCShipmentDocuments)
                        {
                            var getDoc = this.lcShipmentService.GetLCShipmentDocument(aDocument.Id);
                            if (getDoc != null)
                            {
                                getDoc.Status = aDocument.Status;
                                getDoc.DocumentPath = aDocument.DocumentPath;
                                getDoc.Comments = aDocument.Comments;
                                try
                                {
                                    this.lcShipmentService.UpdateLCShipmentDocument(getDoc);
                                }
                                catch {}
                            }
                        }
                    }
                    
                    if (oldLcShipment.LCShipmentExpenses.Any())
                    {
                        if (lcShipmentExpenseHeadCategory.Any())
                        {
                            foreach (var lcexpensecat in lcShipmentExpenseHeadCategory)
                            {
                                foreach (var expenseHead in lcexpensecat.LCShipmentExpenseHeads)
                                {
                                    var getexpense =
                                        oldLcShipment.LCShipmentExpenses.FirstOrDefault(
                                            exp => exp.LCShipmentId == oldLcShipment.Id &&
                                                   exp.LCShipmentExpenseHeadId == expenseHead.Id);
                                    if (getexpense != null)
                                    {
                                        getexpense.AmountInLocalCurrency = expenseHead.AmountInLocal;
                                        this.lcShipmentService.UpdateLCShipmentExpense(getexpense);
                                    }
                                }
                            }
                        }
                    }

                    oldLcShipment.AssesmentValueInLocalCurrency = lcShipment.AssesmentValueInLocalCurrency;
                    oldLcShipment.Container = lcShipment.Container;
                    oldLcShipment.Duty = lcShipment.Duty;
                    oldLcShipment.PaidAmount = lcShipment.PaidAmount;
                    oldLcShipment.BillOfEntryNo = lcShipment.BillOfEntryNo;
                    oldLcShipment.ShipmentDate = lcShipment.ShipmentDate;
                    oldLcShipment.ArrivalDate = lcShipment.ArrivalDate;
                    oldLcShipment.PaymentDate = lcShipment.PaymentDate;
                    oldLcShipment.BillOfEntryDate = lcShipment.BillOfEntryDate;
                    oldLcShipment.PortReleaseDate = lcShipment.PortReleaseDate;
                    oldLcShipment.VesselDescription = lcShipment.VesselDescription;

                    if (this.lcShipmentService.UpdateLCShipment(oldLcShipment))
                    {
                        isSuccess = true;
                        message = Resources.ResourceLCShipment.UpdateSuccess;
                    }
                    else
                    {
                        message = Resources.ResourceLCShipment.UpdateUnSuccess;
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
        private bool CheckIsExist(Model.Models.LCShipment lcShipment)
        {
            return this.lcShipmentService.CheckIsExist(lcShipment);
        }
        [HttpPost]
        public JsonResult DeleteLCShipment(LCShipment lcShipment)
        {
            var isSuccess = true;
            var message = string.Empty;
            const string url = "/LCShipment/Index";
            permission = (RoleSubModuleItem)cacheProvider.Get(cacheKey) ?? roleSubModuleItemService.GetRoleSubModuleItemBySubModuleIdandRole(url,
                             Helpers.UserSession.GetUserFromSession().RoleId);

            if (permission.DeleteOperation == true)
            {
                isSuccess = this.lcShipmentService.DeleteLCShipment(lcShipment.Id);
                if (isSuccess)
                {
                    message = "LCShipment deleted successfully!";
                }
                else
                {
                    message = "LCShipment can't be deleted!";
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


        public JsonResult GetLCShipmentByCI(Guid id)
        {
            var getLcShipment = this.lcShipmentService.GetLcShipmentByCI(id);
            if (getLcShipment != null)
            {
                LCShipmentViewModel lcShipment = new LCShipmentViewModel();
                lcShipment.Id = getLcShipment.Id;
                lcShipment.CommercialInvoiceId = getLcShipment.CommercialInvoiceId;
                lcShipment.AssesmentValueInLocalCurrency = getLcShipment.AssesmentValueInLocalCurrency;
                lcShipment.Container = getLcShipment.Container;
                lcShipment.Duty = getLcShipment.Duty;
                lcShipment.PaidAmount = getLcShipment.PaidAmount;
                lcShipment.BillOfEntryNo = getLcShipment.BillOfEntryNo;
                lcShipment.VesselDescription = getLcShipment.VesselDescription;

                lcShipment.ShipmentDate = getLcShipment.ShipmentDate;
                lcShipment.ArrivalDate = getLcShipment.ArrivalDate;
                lcShipment.PaymentDate = getLcShipment.PaymentDate;
                lcShipment.BillOfEntryDate = getLcShipment.BillOfEntryDate;
                lcShipment.PortReleaseDate = getLcShipment.PortReleaseDate;

                if (getLcShipment.ShipmentDate != null)
                    lcShipment.ShipmentDateString = getLcShipment.ShipmentDate.Value.ToString(dateTimeFormat);
                if (getLcShipment.ArrivalDate != null)
                    lcShipment.ArrivalDateString = getLcShipment.ArrivalDate.Value.ToString(dateTimeFormat);
                if (getLcShipment.PaymentDate != null)
                    lcShipment.PaymentDateString = getLcShipment.PaymentDate.Value.ToString(dateTimeFormat);
                if (getLcShipment.BillOfEntryDate != null)
                    lcShipment.BillOfEntryDateString = getLcShipment.BillOfEntryDate.Value.ToString(dateTimeFormat);
                if (getLcShipment.PortReleaseDate != null)
                    lcShipment.PortReleaseDateString = getLcShipment.PortReleaseDate.Value.ToString(dateTimeFormat);


                List<LCShipmentDocumentViewModel> lcShipmentDocuments = new List<LCShipmentDocumentViewModel>();
                foreach (var doc in getLcShipment.LCShipmentDocuments)
                {
                    LCShipmentDocumentViewModel lcShipmentDocument = new LCShipmentDocumentViewModel();
                    lcShipmentDocument.DocumentPath = doc.DocumentPath;
                    lcShipmentDocument.Id = doc.Id;
                    lcShipmentDocument.Comments = doc.Comments;
                    lcShipmentDocument.Status = doc.Status;
                    lcShipmentDocument.DocumentName = doc.LCDocument.DocumentName;
                    lcShipmentDocument.LCDocumentId = doc.LCDocumentId;
                    lcShipmentDocument.LCShipmentId = doc.LCShipmentId;

                    lcShipmentDocuments.Add(lcShipmentDocument);
                }
                lcShipment.LCShipmentDocuments = lcShipmentDocuments;

                List<LCShipmentExpenseViewModel> lcShipmentExpenses = new List<LCShipmentExpenseViewModel>();
                foreach (var expence in getLcShipment.LCShipmentExpenses)
                {
                    LCShipmentExpenseViewModel lcShipmentExpense = new LCShipmentExpenseViewModel();
                    lcShipmentExpense.LCShipmentId = expence.LCShipmentId;
                    lcShipmentExpense.Id = expence.Id;
                    lcShipmentExpense.LCShipmentExpenseHeadId = expence.LCShipmentExpenseHeadId;
                    lcShipmentExpense.AmountInLocalCurrency = expence.AmountInLocalCurrency;

                    lcShipmentExpenses.Add(lcShipmentExpense);
                }
                lcShipment.LCShipmentExpenses = lcShipmentExpenses;

                var expenceHeadCategory =
                    this.lcShipmentExpenseHeadCategoryService.GetAllLCShipmentExpenseHeadCategory();
                List<LCShipmentExpenseHeadCategoryViewModel> lcshipmentexpenseheadcategoryVmList = new List<LCShipmentExpenseHeadCategoryViewModel>();

                if (expenceHeadCategory != null)
                {
                    foreach (var expenseHeadcat in expenceHeadCategory)
                    {
                        if (expenseHeadcat.LCShipmentExpenseHeads.Any())
                        {
                            LCShipmentExpenseHeadCategoryViewModel lcshipmentexpenseheadcategoryTemp = new LCShipmentExpenseHeadCategoryViewModel();
                            lcshipmentexpenseheadcategoryTemp.Id = expenseHeadcat.Id;
                            lcshipmentexpenseheadcategoryTemp.HeadCategory = expenseHeadcat.HeadCategory;
                            List<LCShipmentExpenseHeadViewModel> lcShipmentExpenseHeads = new List<LCShipmentExpenseHeadViewModel>();

                            foreach (var expenseHead in expenseHeadcat.LCShipmentExpenseHeads)
                            {
                                LCShipmentExpenseHeadViewModel aLcShipmentExpenseHead = new LCShipmentExpenseHeadViewModel();
                                aLcShipmentExpenseHead.Id = expenseHead.Id;
                                aLcShipmentExpenseHead.Head = expenseHead.Head;
                                var getlcExpence = expenseHead.LCShipmentExpenses.FirstOrDefault(ex => ex.LCShipmentId == lcShipment.Id && ex.LCShipmentExpenseHeadId == expenseHead.Id);
                                if (getlcExpence != null)
                                {
                                    aLcShipmentExpenseHead.AmountInLocal = getlcExpence.AmountInLocalCurrency ?? 0;
                                }
                                lcShipmentExpenseHeads.Add(aLcShipmentExpenseHead);
                            }
                            lcshipmentexpenseheadcategoryTemp.LCShipmentExpenseHeads = lcShipmentExpenseHeads;

                            lcshipmentexpenseheadcategoryVmList.Add(lcshipmentexpenseheadcategoryTemp);
                        }
                    }
                }

                return Json(new
                {
                    isOk = true,
                    lcShipment = lcShipment,
                    lcshipmentexpenseheadcategoryVmList = lcshipmentexpenseheadcategoryVmList,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                isOk = false,
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLCShipmentList()
        {
            var lcShipmentListObj = this.lcShipmentService.GetAllLCShipment();
            List<LCShipmentViewModel> lcShipmentVMList = new List<LCShipmentViewModel>();

            foreach (var lcShipment in lcShipmentListObj)
            {
                LCShipmentViewModel lcShipmentTemp = new LCShipmentViewModel();
                lcShipmentTemp.Id = lcShipment.Id;
                //lcShipmentTemp.HeadCategory = lcShipment.HeadCategory;

                lcShipmentVMList.Add(lcShipmentTemp);
            }
            return Json(lcShipmentVMList, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLCShipment(Guid id)
        {
            var lcShipment = this.lcShipmentService.GetLCShipment(id);
            return Json(lcShipment);
        }

        [HttpPost]
        public virtual string UploadFiles(object obj, string lcName, string commercialInvoiceNo)
        {
            var saveToFileLoc = "";
            var length = Request.ContentLength;
            var bytes = new byte[length];
            Request.InputStream.Read(bytes, 0, length);
            var fileName = Request.Headers["X-File-Name"];
            var fileSize = Request.Headers["X-File-Size"];
            var fileType = Request.Headers["X-File-Type"];


            if (!Directory.Exists(Server.MapPath("~/Files/LCShipment/")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Files/LCShipment/"));
            }

            if (System.IO.File.Exists(Server.MapPath("~/Files/LCShipment/" + lcName + '_' + commercialInvoiceNo + '_' + fileName)))
            {
                System.IO.File.Delete(Server.MapPath("~/Files/LCShipment/" + lcName + '_' + commercialInvoiceNo + '_' + fileName));
            }
            saveToFileLoc = Server.MapPath("~/Files/LCShipment/" + lcName + '_' + commercialInvoiceNo + '_' + fileName);

            // save the file.
            var fileStream = new FileStream(saveToFileLoc, FileMode.Create, FileAccess.ReadWrite);
            fileStream.Write(bytes, 0, length);
            fileStream.Close();
            return string.Format("{0} bytes uploaded", bytes.Length);
        }

    }

    public class LCShipmentViewModel
    {
        public LCShipmentViewModel()
        {
            this.LCShipmentDocuments = new List<LCShipmentDocumentViewModel>();
            this.LCShipmentExpenses = new List<LCShipmentExpenseViewModel>();
        }

        public System.Guid Id { get; set; }
        public System.Guid CommercialInvoiceId { get; set; }
        public string CommercialInvoiceNo { get; set; }
        public string BillOfEntryNo { get; set; }
        public Nullable<System.DateTime> BillOfEntryDate { get; set; }
        public string BillOfEntryDateString { get; set; }
        public double? Duty { get; set; }
        public double? PaidAmount { get; set; }
        public string VesselDescription { get; set; }
        public string Container { get; set; }
        public Nullable<System.DateTime> ShipmentDate { get; set; }
        public string ShipmentDateString { get; set; }
        public string ArrivalDateString { get; set; }
        public Nullable<System.DateTime> ArrivalDate { get; set; }
        public Nullable<System.DateTime> PortReleaseDate { get; set; }
        public string PortReleaseDateString { get; set; }
        public string PaymentDateString { get; set; }
        public Nullable<double> AssesmentValueInLocalCurrency { get; set; }
        public Nullable<System.DateTime> PaymentDate { get; set; }

        public virtual ICollection<LCShipmentDocumentViewModel> LCShipmentDocuments { get; set; }
        public virtual ICollection<LCShipmentExpenseViewModel> LCShipmentExpenses { get; set; }

    }

    public class LCShipmentExpenseViewModel
    {
        public System.Guid Id { get; set; }
        public System.Guid LCShipmentId { get; set; }
        public int? LCShipmentExpenseHeadId { get; set; }
        public Nullable<double> AmountInLocalCurrency { get; set; }
    }

}