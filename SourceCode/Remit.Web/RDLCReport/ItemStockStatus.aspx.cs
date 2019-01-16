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
    public partial class ItemStockStatus : System.Web.UI.Page
    {
        string dateTimeFormat = WebConfigurationManager.AppSettings["DateTimeFormat"];
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ItemStockStatus.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    int groupTypeId = Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                    bool rawMaterial = true;
                    int groupId = 0;
                    int categoryId = 0;
                    string titleString = string.Empty;
                    string groupName = string.Empty;
                    string categoryName = string.Empty;

                    string whereText = " where b.ThresholdLevel > 0 ";

                    if (Request.QueryString["groupTypeId"] != null && Request.QueryString["groupTypeId"] != "")
                    {
                        titleString = "Re Order Level Wise Ceramic Raw Materials Stock Report.";
                        whereText += " and d.TypeId = " + groupTypeId;
                    }
                    else
                    {
                        titleString = "Re Order Level Wise Spare Parts and Others Stock Report.";
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

                    var query = "select c.Name as Category, b.Name as Item, b.ThresholdLevel, SUM(a.Quantity) as Quantity, u.Name as Unit from ItemInventory a inner join Item b on a.ItemId = b.Id inner join ItemCategory c on b.ItemCategoryId = c.Id inner join ItemGroup d on c.ItemGroupId = d.Id left join UnitOfMeasurement u on a.UnitId = u.Id  " + whereText + " group by c.Name, b.Name, b.ThresholdLevel, u.Name having avg(b.ThresholdLevel) >= SUM(a.Quantity) order by c.Name, b.Name, b.ThresholdLevel, u.Name";

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

                    ReportDataSource rdc1 = new ReportDataSource("ItemStockStatus", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("rawMaterial", rawMaterial.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    titleString += " Group: " + groupName + ", Category: " + categoryName;
                    parms = new ReportParameter("subTitle", titleString);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    var printDate = DateTime.UtcNow.AddMinutes(timeZoneOffset).ToString(dateTimeFormat);
                    parms = new ReportParameter("PrintDate", printDate);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //language base data: show data from resource file

                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("SN", utility.GetResourceValueById("ResourceRDLCItemStockStatusReport", "SN"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Item", utility.GetResourceValueById("ResourceRDLCItemStockStatusReport", "Item"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("StockQuantity", utility.GetResourceValueById("ResourceRDLCItemStockStatusReport", "StockQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ReOrderQuantity", utility.GetResourceValueById("ResourceRDLCItemStockStatusReport", "ReOrderQuantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Unit", utility.GetResourceValueById("ResourceRDLCItemStockStatusReport", "Unit"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}