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
    public partial class ItemStockRegister : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ItemStockRegister.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
        
                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    int year = 0;
                    int month = 1;
                    int categoryId = 0;
                    Guid itemId = Guid.Empty;
                    string categoryName = string.Empty;
                    string itemName = string.Empty;

                    if (Request.QueryString["Year"] != null)
                    {
                        year = Convert.ToInt32(Request.QueryString["Year"]);
                    }

                    if (Request.QueryString["Month"] != null)
                    {
                        month = Convert.ToInt32(Request.QueryString["Month"]);
                    }

                    string whereText = " where a.Year = " + year + " and a.Month = " + month;

                    if (Request.QueryString["categoryId"] != null && Request.QueryString["categoryId"] != "")
                    {
                        categoryId = Convert.ToInt32(Request.QueryString["categoryId"]);
                    }

                    var categoryObj = _context.ItemCategories.FirstOrDefault(a => a.Id == categoryId);
                    if (categoryObj != null)
                    {
                        categoryName = categoryObj.Name;
                    }
                    else
                    {
                        categoryName = "ALL";
                    }

                    if (Request.QueryString["itemId"] != null && Request.QueryString["itemId"] != "")
                    {
                        itemId = new Guid(Request.QueryString["itemId"]);
                    }

                    var itemObj = _context.Items.FirstOrDefault(a => a.Id == itemId);
                    if (itemObj != null)
                    {
                        itemName = itemObj.Name;
                        whereText += " and a.ItemId = '" + itemId + "'";
                        SqlCommand cmdp = new SqlCommand("EXEC [dbo].[ItemStockRegisterProc] @year = " + year + ", @month = " + month + ", @ItemId = '" + itemId + "', @timeZoneOffset = " + timeZoneOffset, con);
                        cmdp.ExecuteNonQuery();
                    }
                    else
                    {
                        itemName = "ALL";
                    }

                    var query = "Select a.* from ItemStockRegister a " + whereText;

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

                    ReportDataSource rdc1 = new ReportDataSource("ItemStockRegister", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Year", year.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Month", month.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    var titleString = "Item Stock Register. Item: " + itemName + ", Category: " + categoryName;
                    parms = new ReportParameter("subTitle", titleString);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    string monthName = new DateTime(year, month, 1).ToString("MMM", CultureInfo.InvariantCulture);

                    parms = new ReportParameter("ReportMonthName", monthName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //language base data: show data from resource file

                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("Date", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "Date"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("BF", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "BF"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Receipts", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "Receipts"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("QuantitySold", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "QuantitySold"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Dispose", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "Dispose"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Adjustment", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "Adjustment"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Balance", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "Balance"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Remarks", utility.GetResourceValueById("ResourceRDLCItemStockRegisterReport", "Remarks"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}