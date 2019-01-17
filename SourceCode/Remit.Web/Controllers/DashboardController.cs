using Remit.Model.Models;
using Remit.Service;
using Remit.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Configuration;

namespace Remit.Web.Controllers
{
    public class DashboardController : Controller
    {
        public readonly IItemInventoryService itemInventoryService;
        public readonly IFGItemInService fgItemInService;
        public readonly IFGSaleService fgSaleService;
        public readonly IItemService itemService;
        public readonly IFGItemService fgitemService;
        public readonly IFGItemInventoryService fgitemInventoryService;
        public readonly ISubModuleItemService subModuleItemService;
        public readonly IRoleSubModuleItemService roleSubModuleItemService;

        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];

        public DashboardController(IItemInventoryService itemInventoryService, IFGItemInService fgItemInService, IFGSaleService fgSaleService, IItemService itemService, IFGItemService fgitemService, IFGItemInventoryService fgitemInventoryService, ISubModuleItemService subModuleItemService, IRoleSubModuleItemService roleSubModuleItemService)
        {
            this.itemInventoryService = itemInventoryService;
            this.fgItemInService = fgItemInService;
            this.fgSaleService = fgSaleService;
            this.itemService = itemService;
            this.fgitemService = fgitemService;
            this.fgitemInventoryService = fgitemInventoryService;
            this.subModuleItemService = subModuleItemService;
            this.roleSubModuleItemService = roleSubModuleItemService;
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ShowAdminPage(string id)
        {
            return View("AdminViewPage");

        }

        public ActionResult HomePage()
        {
            List<double> quantities = new List<double>();
            List<double> thresholdlist = new List<double>();
            List<string> itemnamelist = new List<string>();
            var priority = WebConfigurationManager.AppSettings["Priority"];
            int[] priorities = Array.ConvertAll(priority.Split(','), int.Parse);

            for (int i = 0; i <= priorities.Length; i++)
            {
                var itemLists = this.itemService.GetAllItem().ToList().Where(p => p.Priority == i);
                foreach (var item in itemLists)
                {

                    var itemInventoryByItemId = this.itemInventoryService.GetItemInventoryByItemId(item.Id).ToList();
                    var qty = 0.00;
                    foreach (var itemName in itemInventoryByItemId)
                    {
                        if (itemName != null)
                        {
                            qty = Math.Round((double)(qty + itemName.Quantity));

                        }
                    }
                    quantities.Add(qty);

                    if (item.ThresholdLevel != null)
                    {
                        thresholdlist.Add(Math.Round((double)item.ThresholdLevel));
                    }
                    else
                    {
                        thresholdlist.Add(Math.Round(0.0));
                    }

                    itemnamelist.Add(item.Name);

                }

            }

            var qtylist = quantities.Take(10).ToArray();
            var threslist = thresholdlist.Take(10).ToArray();
            var namelist = itemnamelist.Take(10).ToArray();
            ViewData["qtylist"] = qtylist;
            ViewData["threslist"] = threslist;
            ViewData["namelist"] = namelist;
            Session["selectedmoduleid"] = 99;
            ViewBag.homepage = 99;




            //fg item code wise and grade wise stock........


            List<string> codes = new List<string>();
            List<double> gradeAlist = new List<double>();
            //List<double> gradeBlist = new List<double>();



            //var fgitemLists = this.fgitemService.GetAllFGItem().ToList();
            var fgitemLists = this.fgitemInventoryService.GetAllFGItemInventory().Where(x => x.FGGradeId == 1).GroupBy(x => x.FGItemId).OrderByDescending(z => z.Sum(y=>y.QuantityInCTN)).Take(20);
            foreach (var fgitem in fgitemLists)
            {
                
                //var fgitemlistFromFgInventory = this.fgitemInventoryService.GetAllFGItemInventory().Where(b => b.FGItemId == fgitem.Id && b.FGGradeId == 1).ToList();
                //var gradeAsum = fgitemlistFromFgInventory.Sum(p => p.QuantityInCTN);
                //gradeAlist.Add(gradeAsum);

                //var fgitemlistFromFgInventory2 = this.fgitemInventoryService.GetAllFGItemInventory().Where(b => b.FGItemId == fgitem.Id && b.FGGradeId == 2).ToList();
                //var gradeBsum = fgitemlistFromFgInventory2.Sum(q => q.QuantityInCTN);
                //gradeBlist.Add(gradeBsum);
                
                //codes.Add(fgitem.Code);

                var gradeAsum = fgitem.Sum(x => x.QuantityInCTN);
                gradeAlist.Add(gradeAsum);

                //var gradeBsum = fgitem.Where(x => x.FGGradeId == 2).Sum(x => x.QuantityInCTN);
                //gradeBlist.Add(gradeBsum);

                var fgitemObj =this.fgitemService.GetFGItem(fgitem.Key);
                codes.Add(fgitemObj.Code);


            }

            var codelist = codes.Take(20).ToArray();
            var gradeAlists = gradeAlist.Take(20).ToArray();
           // var gradeBlists = gradeBlist.Take(20).ToArray();

            ViewData["codelist"] = codelist;
            ViewData["gradeAlists"] = gradeAlists;
           // ViewData["gradeBlists"] = gradeBlists;





            //For sales highchart..................start


            List<double> QuantityInSftList = new List<double>();
            List<double> QuantityInCtnList = new List<double>();
            List<int> QuantityInPcsList = new List<int>();
            List<string> dateList = new List<string>();
          //  var dt = DateTime.UtcNow.AddDays(-10);
          //  var salesList = this.fgSaleService.GetAllFGSale().OrderByDescending(a => a.InvoiceDate).Where(a => a.InvoiceDate.Value.Date >= dt.Date && a.IsDelete==false).GroupBy(a => a.InvoiceDate.Value.Date).ToList();
            
            var salesList = this.fgSaleService.GetAllFGSale().OrderByDescending(a => a.InvoiceDate).Where(a => a.IsDelete == false).GroupBy(a => a.InvoiceDate.Value.Date).ToList();



            foreach (var salesInvoiceByDate in salesList)
            {
                var QuantityInCTN = 0.00;
                var QuantityInSFT = 0.00;
                var QuantityInPCs = 0;
                var salesdate = string.Empty;

                var salesdetails = salesInvoiceByDate.ToList();
                foreach (var salesdetail in salesdetails)
                {

                    QuantityInCTN += Math.Round((double)salesdetail.FGSalesDetails.Sum(a => a.QuantityInCTN));
                    QuantityInSFT += Math.Round((double)salesdetail.FGSalesDetails.Sum(a => a.QuantityInSFT));
                    QuantityInPCs += (int)salesdetail.FGSalesDetails.Sum(a => a.QuantityInPCs);


                }

                QuantityInCtnList.Add(QuantityInCTN);
                QuantityInSftList.Add(QuantityInSFT);
                QuantityInPcsList.Add(QuantityInPCs);
                dateList.Add(salesInvoiceByDate.Key.ToString("dd-MMM-yy"));

            }


            var ctnList = QuantityInCtnList.Take(10).ToArray();
            var sftList = QuantityInSftList.Take(10).ToArray();
            var pcsList = QuantityInPcsList.Take(10).ToArray();
            var datesList = dateList.ToArray();

            ViewData["ctnList"] = ctnList;
            ViewData["sftList"] = sftList;
            ViewData["pcsList"] = pcsList;
            ViewData["datesList"] = datesList;

            //For sales highchart..................End


            // for Product In Highchart..........

            List<double> QuantityInSftForProInList = new List<double>();
            List<double> QuantityInCtnForProInList = new List<double>();
            List<int> QuantityInPcsForProInList = new List<int>();
            List<string> dateForProInList = new List<string>();
           // var proIndt = DateTime.UtcNow.AddDays(-10);
           // var productInList = this.fgItemInService.GetAllFGItemIn().OrderByDescending(b => b.ReceivedDate).Where(b => b.ReceivedDate.Date >= proIndt.Date && b.DeleteFlag!=true).GroupBy(b => b.ReceivedDate.Date).ToList();

            var productInList = this.fgItemInService.GetAllFGItemIn().OrderByDescending(b => b.ReceivedDate).Where(b =>b.DeleteFlag != true).GroupBy(b => b.ReceivedDate.Date).ToList();

            foreach (var productInListByDate in productInList)
            {
                var QuantityInCTN = 0.00;
                var QuantityInSFT = 0.00;
                var QuantityInPCs = 0;
                var salesdate = string.Empty;
                var productIns = productInListByDate.ToList();
                foreach (var productIn in productIns)
                {

                    QuantityInCTN +=Math.Round(productIn.FGItemInDetails.Sum(b => b.QuantityInCTN));
                    QuantityInSFT += Math.Round(productIn.FGItemInDetails.Sum(b => b.QuantityInSFT));
                    QuantityInPCs += productIn.FGItemInDetails.Sum(b => b.QuantityInPCs);


                }

                QuantityInSftForProInList.Add(QuantityInSFT);
                QuantityInCtnForProInList.Add(QuantityInCTN);
                QuantityInPcsForProInList.Add(QuantityInPCs);
                dateForProInList.Add(productInListByDate.Key.ToString("dd-MMM-yy"));


            }

            var productInSft = QuantityInSftForProInList.Take(10).ToArray();
            var productInCtn = QuantityInCtnForProInList.Take(10).ToArray();
            var productInPcs = QuantityInPcsForProInList.Take(10).ToArray();
            var productInDate = dateForProInList.ToArray();

            ViewData["productInSft"] = productInSft;
            ViewData["productInCtn"] = productInCtn;
            ViewData["productInPcs"] = productInPcs;
            ViewData["productInDate"] = productInDate;


            return View();
        }




        public JsonResult SetModuleInSession(string id)
        {
            Helpers.UserSession.SetModuleClicked(id);
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Submodules(int id)
        {
            ViewBag.ModuleId = id;
            Session["SelectedModuleId"] = id;
            return View();
        }
    }
}