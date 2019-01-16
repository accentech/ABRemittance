using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Remit.Data.Models;
using Remit.Model.Models;
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class ProductStock : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            long timeZoneOffset = UserSession.GetTimeZoneOffset();
            if (!IsPostBack)
            {
                using (var _context = new ApplicationEntities())
                {
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ProductStock.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    DateTime inventorydate = new DateTime(0);
                    string inventorydatestring = "";

                    if (Request.QueryString["InventoryDate"] != null && Request.QueryString["InventoryDate"] != "")
                    {
                        inventorydate = Convert.ToDateTime(Request.QueryString["InventoryDate"]);
                        inventorydatestring = inventorydate.ToString("dd-MMM-yyyy");
                    }

                    var query = "select FGType.TypeName,FGItem.Code,FGItem.PackageToSalesRatio,FGGrade.Grade,Inventory.Lot, (ISNULL(OpeningQuantity,0) + ISNULL(InQuantity,0) - ISNULL(SalesQuantity,0) - ISNULL(DisposeQuantity,0) - ISNULL(AdjustmentQuantity,0)) as BookQuantity, (ISNULL(OpeningQuantity,0) + ISNULL(InQuantity,0) - ISNULL(DeliveryQuantity,0) - ISNULL(DisposeQuantity,0) - ISNULL(AdjustmentQuantity,0)) as StockQuantity, ISNULL(SalesQuantity,0) - ISNULL(DeliveryQuantity,0) as NotDeliveredQuantity from FGItemInventoryWithoutBin as Inventory " +
                        "inner join FGItem on Inventory.FGItemId = FGItem.Id " +
                        "inner join FGGrade on Inventory.FGGradeId = FGGrade.Id " +
                        "inner join FGType on FGItem.TypeId = FGType.Id " +
                        "left join ( " +
                        "Select a.FGItemId,a.FGGradeId,a.Lot,ISNULL(SUM(a.Quantity),0) as OpeningQuantity from FGItemOpening a where CAST(DATEADD(minute, " + timeZoneOffset + ", a.OpeningDate) as date) <= '" + inventorydatestring + "' group by a.FGItemId,a.FGGradeId,a.Lot " +
                        ") as Opening on Inventory.FGItemId = Opening.FGItemId and Inventory.FGGradeId = Opening.FGGradeId and Inventory.Lot = Opening.Lot " +
                        "left join ( " +
                        "Select a.FGItemId,a.FGGradeId,a.Lot,ISNULL(SUM(InQuantity),0) as InQuantity from FGItemInDetail a inner join FGItemIn b on a.FGItemInId = b.Id where b.DeleteFlag != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", b.ReceivedDate) as date) <= '" + inventorydatestring + "' group by a.FGItemId,a.FGGradeId,a.Lot " +
                        ") as Production on Inventory.FGItemId = Production.FGItemId and Inventory.FGGradeId = Production.FGGradeId and Inventory.Lot = Production.Lot " +
                        "left join ( " +
                        "Select a.FGItemId,a.FGGradeId,a.Lot,ISNULL(SUM(a.PackQuantity),0) as SalesQuantity from FGSalesDetail a inner join FGSales b on a.FGSalesInvoiceNo = b.InvoiceNo where b.IsDelete != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", b.InvoiceDate) as date) <= '" + inventorydatestring + "' group by a.FGItemId,a.FGGradeId,a.Lot " +
                        ") as Sales on Inventory.FGItemId = Sales.FGItemId and Inventory.FGGradeId = Sales.FGGradeId and Inventory.Lot = Sales.Lot " +
                        "left join ( " +
                        "Select a.FGItemId,a.FGGradeId,a.Lot,ISNULL(SUM(DeliveryQuantity),0) as DeliveryQuantity from FGSalesDeliveryDetail a inner join FGSalesDelivery b on a.DeliveryChallanNo = b.DeliveryChallanNo where b.IsDelete != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", b.DeliveryDate) as date) <= '" + inventorydatestring + "' group by a.FGItemId,a.FGGradeId,a.Lot " +
                        ") as Delivery on Inventory.FGItemId = Delivery.FGItemId and Inventory.FGGradeId = Delivery.FGGradeId and Inventory.Lot = Delivery.Lot " +
                        "left join ( " +
                        "Select a.FGItemId,a.FGGradeId,a.Lot,ISNULL(SUM(DisposeQuantity),0) as DisposeQuantity from FGItemDisposeDetail a inner join FGItemDispose b on a.FGItemDisposeId = b.Id where b.DeleteFlag != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", b.Date) as date) <= '" + inventorydatestring + "' group by a.FGItemId,a.FGGradeId,a.Lot " +
                        ") as Dispose on Inventory.FGItemId = Dispose.FGItemId and Inventory.FGGradeId = Dispose.FGGradeId and Inventory.Lot = Dispose.Lot " +
                        "left join ( " +
                        "Select a.FGItemId,a.FGGradeId,a.Lot,ISNULL(SUM(a.InventoryQuantity - a.ActualQuantity),0) as AdjustmentQuantity from FGItemAdjustmentDetail a inner join FGItemAdjustment b on a.FGItemAdjustmentId = b.Id where b.DeleteFlag != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", b.AdjustmentDate) as date) <= '" + inventorydatestring + "' group by a.FGItemId,a.FGGradeId,a.Lot " +
                        ") as Adjustment on Inventory.FGItemId = Adjustment.FGItemId and Inventory.FGGradeId = Adjustment.FGGradeId and Inventory.Lot = Adjustment.Lot " +
                        //"order by FGItem.Code,FGGrade.Grade,Inventory.Lot";
                        " Where (ISNULL(OpeningQuantity,0) + ISNULL(InQuantity,0) - ISNULL(DeliveryQuantity,0) - ISNULL(DisposeQuantity,0) - ISNULL(AdjustmentQuantity,0)) > 0" +
                        " ORDER BY FGItem.Code,FGGrade.Grade,LEFT( Inventory.Lot,PATINDEX(\'%[0-9]%\', Inventory.Lot)-1), " +
                        " CONVERT(INT,SUBSTRING( Inventory.Lot,PATINDEX(\'%[0-9]%\', Inventory.Lot),LEN( Inventory.Lot)))";


                    var queryCom =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";

                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlCommand cmdComp = new SqlCommand(queryCom, con);
                    SqlDataAdapter sda = new SqlDataAdapter();
                    using (sda)
                    {
                        sda.SelectCommand = cmd;
                        sda.Fill(dtItem);

                        sda.SelectCommand = cmdComp;
                        sda.Fill(company);
                    }
                    con.Close();

                    ReportUtility utility = new ReportUtility();

                    ReportDataSource companyDc = new ReportDataSource("CompanyTable", company);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportDataSource rdc1 = new ReportDataSource("ProductStock", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("InventoryDate", inventorydatestring);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("TypeName", utility.GetResourceValueById("ResourceRDLCProductStock", "TypeName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Code", utility.GetResourceValueById("ResourceRDLCProductStock", "Code"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Grade", utility.GetResourceValueById("ResourceRDLCProductStock", "Grade"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Lot", utility.GetResourceValueById("ResourceRDLCProductStock", "Lot"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("SaleableQuantity", utility.GetResourceValueById("ResourceRDLCProductStock", "SaleableQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("NotDeliveredQuantity", utility.GetResourceValueById("ResourceRDLCProductStock", "NotDeliveredQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("StockQuantity", utility.GetResourceValueById("ResourceRDLCProductStock", "StockQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("StockQuantitySFT", utility.GetResourceValueById("ResourceRDLCProductStock", "StockQuantitySFT"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("FactoryStockReport", utility.GetResourceValueById("ResourceRDLCProductStock", "FactoryStockReport"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCProductStock", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}