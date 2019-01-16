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
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Remit.Data.Models;
using Remit.Model.Models;
using Remit.Web.Helpers;
using System.Web.Configuration;

namespace Remit.Web.RDLCReport
{
    public partial class ItemUseReport : System.Web.UI.Page
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
                    System.Data.DataTable ballMill = new System.Data.DataTable();
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ItemUseReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
        
                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    int year = 0;
                    int month = 1;
                    int groupTypeId = Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                    bool rawMaterial = true;
                    int groupId = 0;
                    int categoryId = 0;
                    string titleString = string.Empty;
                    string groupName = string.Empty;
                    string categoryName = string.Empty;

                    if (Request.QueryString["Year"] != null)
                    {
                        year = Convert.ToInt32(Request.QueryString["Year"]);
                    }

                    if (Request.QueryString["Month"] != null)
                    {
                        month = Convert.ToInt32(Request.QueryString["Month"]);
                    }

                    SqlCommand cmdp = new SqlCommand("EXEC [dbo].[ItemUseReportProc] @year = " + year + ", @month = " + month + ", @timeZoneOffset = " + timeZoneOffset, con);
                    cmdp.ExecuteNonQuery();

                    string whereText = " where a.Year = " + year + " and a.Month = " + month;

                    if (Request.QueryString["groupTypeId"] != null && Request.QueryString["groupTypeId"] != "")
                    {
                        titleString = "Ceramic Raw Materials Use & Stock Report.";
                        whereText += " and d.TypeId = " + groupTypeId;
                    }
                    else
                    {
                        titleString = "Spare Parts and Others Use & Stock Report.";
                        whereText += " and d.TypeId != " + groupTypeId;
                        rawMaterial = false;
                    }

                    if (Request.QueryString["groupId"] != null && Request.QueryString["groupId"] != "")
                    {
                        groupId = Convert.ToInt32(Request.QueryString["groupId"]);
                    }

                    var groupObj = _context.ItemGroups.FirstOrDefault(a => a.Id == groupId);
                    if (groupObj != null)
                    {
                        groupName = groupObj.Name;
                        whereText += " and c.ItemGroupId = " + groupId;
                    }
                    else
                    {
                        groupName = "ALL";
                    }

                    if (Request.QueryString["categoryId"] != null && Request.QueryString["categoryId"] != "")
                    {
                        categoryId = Convert.ToInt32(Request.QueryString["categoryId"]);
                    }

                    var categoryObj = _context.ItemCategories.FirstOrDefault(a => a.Id == categoryId);
                    if (categoryObj != null)
                    {
                        categoryName = categoryObj.Name;
                        whereText += " and b.ItemCategoryId = " + categoryId;
                    }
                    else
                    {
                        categoryName = "ALL";
                    }

                    var query = "Select ROW_NUMBER() Over (Order by a.ItemName) As [SN], a.* from ItemUseReport a INNER JOIN Item b ON " +
                                "a.ItemId = b.Id INNER JOIN ItemCategory c ON b.ItemCategoryId = c.Id INNER JOIN ItemGroup d ON c.ItemGroupId = d.Id " + whereText;


                    var queryBallMill = "DECLARE @startDate DATE= \'" + month + "\' + \'/\' + \'01/\' +  + \'" + year +
                                        "\'DECLARE @endDate DATE=DATEADD(Month,1,@startDate)\r\nSelect se.Date, fi.BallMill From \r\n(\r\n\t  Select cast(Date as date) as Date, Sum([DailyBallMill]) as BallMill FROM ItemIssue\r\n\t Group By cast(Date as date) \r\n) \r\nas fi RIGHT Join \r\n(\r\n\tSELECT Date = DATEADD(Day,Number,@startDate) \r\n\tFROM  master..spt_values \r\n\tWHERE Type=\'P\'\r\n\tAND DATEADD(day,Number,@startDate) < @endDate\r\n) \r\n\tas se on fi.Date = se.Date";

                    var queryCom =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";

                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlCommand cmdComp = new SqlCommand(queryCom, con);
                    SqlCommand cmdBallMill = new SqlCommand(queryBallMill,con);
                    SqlDataAdapter sda = new SqlDataAdapter();
                    using (sda)
                    {
                        sda.SelectCommand = cmd;
                        sda.Fill(dtItem);

                        sda.SelectCommand = cmdComp;
                        sda.Fill(company);

                        sda.SelectCommand = cmdBallMill;
                        sda.Fill(ballMill);
                    }
                    con.Close();

                    ReportDataSource companyDc = new ReportDataSource("CompanyTable", company);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportDataSource rdc1 = new ReportDataSource("ItemUseReport", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportDataSource ballMillDc = new ReportDataSource("BallMillDataSet", ballMill);
                    ReportViewer1.LocalReport.DataSources.Add(ballMillDc);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("rawMaterial", rawMaterial.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Year", year.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Month", month.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    titleString += " Group: " + groupName + ", Category: " + categoryName;
                    parms = new ReportParameter("subTitle", titleString);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    string monthName = new DateTime(year, month, 1).ToString("MMM", CultureInfo.InvariantCulture);

                    parms = new ReportParameter("ReportMonthName", monthName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    //language base data: show data from resource file

                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("SN", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "SN"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Rawmatrialsname", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "Rawmatrialsname"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("OpenningStock", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "OpenningStock"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InQuantity", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "InQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("TotalQuantity", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "TotalQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("MonthlyUseQuantity", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "MonthlyUseQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DisposeQuantity", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "DisposeQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("MonthEndStock", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "MonthEndStock"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("AdjustmentQuantity", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "AdjustmentQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ClosingStock", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "ClosingStock"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DailyBallMillReport", utility.GetResourceValueById("ResourceRDLCItemUseReport2", "DailyBallMillReport"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}