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
    public partial class ProductInReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            long timeZoneOffset = UserSession.GetTimeZoneOffset();
            if (!IsPostBack)
            {
                using (var _context = new ApplicationEntities())
                {
                    var recv = _context.ItemReceives.ToList();
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ProductInReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
        
                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    int year = 0;
                    int month = 1;
                    int fgItemId = 0;
                    var wheretext = "";

                    if (Request.QueryString["Year"] != null && Request.QueryString["Year"] != "")
                    {
                        year = Convert.ToInt32(Request.QueryString["Year"]);
                    }

                    if (Request.QueryString["Month"] != null && Request.QueryString["Month"] != "")
                    {
                        month = Convert.ToInt32(Request.QueryString["Month"]);
                    }

                    if (Request.QueryString["FGItemId"] != null && Request.QueryString["FGItemId"] != "")
                    {
                        fgItemId = Convert.ToInt32(Request.QueryString["FGItemId"]);
                        wheretext = " and FGItemId = " + fgItemId + "";
                    }

                    SqlCommand cmdp = new SqlCommand("EXEC [dbo].[ProductInReportProc] @year = " + year + ", @month = " + month + ", @timeZoneOffset = " + timeZoneOffset, con);
                    cmdp.ExecuteNonQuery();

                    var query = "Select a.* from ProductInReport a where Year = " + year + " and Month = " + month + wheretext;

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

                    ReportDataSource companyDc = new ReportDataSource("CompanyTable", company);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportDataSource rdc1 = new ReportDataSource("ProductInReport", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Year", year.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Month", month.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    string monthName = new DateTime(year, month, 1)
                            .ToString("MMM", CultureInfo.InvariantCulture);

                    parms = new ReportParameter("ReportMonthName", monthName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                   
                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("GrdColProductCode", utility.GetResourceValueById("GrdColProductCode"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColGrade", utility.GetResourceValueById("GrdColGrade"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColLot", utility.GetResourceValueById("GrdColLot"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColOpeningStock", utility.GetResourceValueById("GrdColOpeningStock"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColMonthlyIn", utility.GetResourceValueById("GrdColMonthlyIn"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColTotalQty", utility.GetResourceValueById("GrdColTotalQty"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColDeliveryQty", utility.GetResourceValueById("GrdColDeliveryQty"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColDisposeQty", utility.GetResourceValueById("GrdColDisposeQty"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColMonthEndStock", utility.GetResourceValueById("GrdColMonthEndStock"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColRemarks", utility.GetResourceValueById("GrdColRemarks"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("TitleWareHouseStockReport", utility.GetResourceValueById("TitleWareHouseStockReport"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    //new add for column name 

                    parms = new ReportParameter("MonthlyIn", utility.GetResourceValueById("ResourceRDLCProductInReport", "MonthlyIn"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCProductInReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                 
                    parms = new ReportParameter("SalesQuantity", utility.GetResourceValueById("ResourceRDLCProductInReport", "SalesQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DeliveryDate", utility.GetResourceValueById("ResourceRDLCProductInReport", "DeliveryDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("SalesOut", utility.GetResourceValueById("ResourceRDLCProductInReport", "SalesOut"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("SalesReturn", utility.GetResourceValueById("ResourceRDLCProductInReport", "SalesReturn"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Sample", utility.GetResourceValueById("ResourceRDLCProductInReport", "Sample"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Free", utility.GetResourceValueById("ResourceRDLCProductInReport", "Free"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InternalUse", utility.GetResourceValueById("ResourceRDLCProductInReport", "InternalUse"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Broken", utility.GetResourceValueById("ResourceRDLCProductInReport", "Broken"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Adjustment", utility.GetResourceValueById("ResourceRDLCProductInReport", "Adjustment"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("NetTotal", utility.GetResourceValueById("ResourceRDLCProductInReport", "NetTotal"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}