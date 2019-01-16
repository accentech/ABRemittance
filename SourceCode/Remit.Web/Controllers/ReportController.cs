using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Remit.Web.Controllers
{
    public class ReportController : Controller
    {
        public ActionResult Index()
        {
            return View("Report");
        }

        public ActionResult ItemInScale()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            return View("ItemInScale");
        }
        public ActionResult SparePartsAndOtherItemInScale()
        {
            ViewBag.GroupType = "";

            return View("ItemInScale");
        }
        public ActionResult ItemReceive()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            return View("ItemReceive");
        }
        public ActionResult SparePartsAndOtherItemReceive()
        {
            ViewBag.GroupType ="";
            return View("ItemReceive");
        }
        public ActionResult ItemStock()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            return View("ItemStock");
        }
        public ActionResult SparePartsAndOtherItemStock()
        {
            ViewBag.GroupType = "";
            return View("ItemStock");
        }
        public ActionResult ItemStockStatus()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            return View("ItemStockStatus");
        }
        public ActionResult SparePartsAndOtherItemStockStatus()
        {
            ViewBag.GroupType = "";
            return View("ItemStockStatus");
        }
        public ActionResult ItemStockRegister()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            return View("ItemStockRegister");
        }
        public ActionResult SparePartsAndOtherItemStockRegister()
        {
            ViewBag.GroupType = "";
            return View("ItemStockRegister");
        }
        public ActionResult ItemUse()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            return View("ItemUse");
        }
        public ActionResult SparePartsAndOtherItemUse()
        {
            ViewBag.GroupType ="";
            return View("ItemUse");
        }
        public ActionResult EmpWiseItemUse()
        {
            ViewBag.GroupType = WebConfigurationManager.AppSettings["GroupType"];
            return View("EmpWiseItemUse");
        }
        public ActionResult SparePartsAndOtherEmpWiseItemUse()
        {
            ViewBag.GroupType = "";
            return View("EmpWiseItemUse");
        }
        public ActionResult MonthlyDelivery()
        {
            return View("MonthlyDelivery");
        }
        public ActionResult MonthlyDelivery2()
        {
            return View("MonthlyDelivery2");
        }
        public ActionResult SizeWiseGradeWiseDelivery()
        {
            return View("SizeWiseGradeWiseDelivery");
        }
        public ActionResult ProductIn()
        {
            return View("ProductIn");
        }
        public ActionResult ProductStock()
        {
            return View("ProductStock");
        }
        public ActionResult SalesStatement()
        {
            return View("SalesStatement");
        }
        public ActionResult DealerWiseSales()
        {
            return View("DealerWiseSales");
        }
        public ActionResult DealerWiseSizeWiseSales()
        {
            return View("DealerWiseSizeWiseSales");
        }
        public ActionResult ProductWiseSales()
        {
            return View("ProductWiseSales");
        }
        public ActionResult DealerInfo()
        {
            return View("DealerInfo");
        }
        public ActionResult EmpWiseDealerWiseReport()
        {
            return View();
        }
        public ActionResult YearGradeUnitWiseDealerSalesReport()
        {
            return View("YearGradeUnitWiseDealerSalesReport");
        }
        public ActionResult SupplierWiseCommercialReport()
        {
            return View("SupplierWiseCommercialReport");
        }
        public ActionResult ItemWiseCommercialReport()
        {
            return View("ItemWiseCommercialReport");
        }

        public ActionResult MonthlySizeWiseSalesAndUndelivery()
        {
            return View("MonthlySizeWiseSalesAndUndelivery");
        }
    }
}