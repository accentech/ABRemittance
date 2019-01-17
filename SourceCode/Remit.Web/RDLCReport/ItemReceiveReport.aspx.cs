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
using System.Web.Configuration;

namespace Remit.Web.RDLCReport
{
    public partial class ItemReceiveReport : System.Web.UI.Page
    {
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        protected void Page_Load(object sender, EventArgs e)
        {
            long timeZoneOffset = UserSession.GetTimeZoneOffset();
            if (!IsPostBack)
            {
                using (var _context = new ApplicationEntities())
                {
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ItemReceiveReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    DateTime fromDate = new DateTime(0);
                    string fromDatestring = string.Empty;
                    DateTime toDate = new DateTime(0);
                    string toDatestring = string.Empty;

                    int groupTypeId = Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                    bool rawMaterial = true;
                    int groupId = 0;
                    int categoryId = 0;
                    string titleString = string.Empty;
                    string groupName = string.Empty;
                    string categoryName = string.Empty;

                    if (Request.QueryString["fromDate"] != null)
                    {
                        fromDate = Convert.ToDateTime(Request.QueryString["fromDate"]);
                        fromDatestring = fromDate.ToString("yyyy-MM-dd");
                    }

                    if (Request.QueryString["toDate"] != null)
                    {
                        toDate = Convert.ToDateTime(Request.QueryString["toDate"]);
                        toDatestring = toDate.ToString("yyyy-MM-dd");
                    }

                    string whereText = "where ItemReceive.IsDeleted != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", ItemReceive.ReceivedDate) as date) >= '" + fromDatestring + "' and CAST(DATEADD(minute, " + timeZoneOffset + ", ItemReceive.ReceivedDate) as date) <= '" + toDatestring + "'";

                    if (Request.QueryString["groupTypeId"] != null && Request.QueryString["groupTypeId"] != "")
                    {
                        titleString = "Ceramic Raw Materials Buying & Receiving Stock Riport.";
                        whereText += " and ItemGroup.TypeId = " + groupTypeId;
                    }
                    else
                    {
                        titleString = "Spare Parts and Others Buying & Receiving Stock Riport.";
                        whereText += " and ItemGroup.TypeId != " + groupTypeId;
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
                        whereText += " and ItemCategory.ItemGroupId = " + groupId;
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
                        whereText += " and Item.ItemCategoryId = " + categoryId;
                    }
                    else
                    {
                        categoryName = "ALL";
                    }

                    var query =
                        "SELECT ItemReceive.InvoiceNo, CONVERT(date,DATEADD(minute, " + timeZoneOffset + ", ItemReceive.ReceivedDate)) as ReceivedDate , Item.Name as ItemName, ItemReceiveDetail.ReceivedQuantity as Quantity, ItemReceiveDetail.UnitId, UnitOfMeasurement.Name as UnitName, ItemReceiveDetail.PerUnitPrice as UnitPrice, ItemReceiveDetail.Remarks FROM ItemReceive INNER JOIN ItemReceiveDetail ON ItemReceive.Id = ItemReceiveDetail.ItemReceiveId INNER JOIN Item ON ItemReceiveDetail.ItemId = Item.Id INNER JOIN ItemCategory ON Item.ItemCategoryId = ItemCategory.Id INNER JOIN ItemGroup ON ItemCategory.ItemGroupId = ItemGroup.Id INNER JOIN UnitOfMeasurement ON ItemReceiveDetail.UnitId = UnitOfMeasurement.Id " + whereText + " order by ItemReceive.ReceivedDate, ItemReceive.InvoiceNo";

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

                    ReportDataSource rdc1 = new ReportDataSource("ItemReceiveReport", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();

                    parms = new ReportParameter("rawMaterial", rawMaterial.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("fromDate", fromDate.ToString(dateFormat));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("toDate", toDate.ToString(dateFormat));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    titleString += " Group: " + groupName + ", Category: " + categoryName;
                    parms = new ReportParameter("subTitle", titleString);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    var dateRange = "From : " + fromDate.ToString(dateFormat) + " To: " + toDate.ToString(dateFormat);
                    parms = new ReportParameter("dateRange", dateRange);
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    //language base data: show data from resource file

                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("ReceivedDate", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "ReceivedDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InvNo", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "InvNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ItemName", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "ItemName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Unit", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "Unit"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Qty", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "Qty"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("UnitPrice", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "UnitPrice"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("PriceTK", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "PriceTK"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("TotalPriceTK", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "TotalPriceTK"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Remarks", utility.GetResourceValueById("ResourceRDLCItemReceiveReport", "Remarks"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}