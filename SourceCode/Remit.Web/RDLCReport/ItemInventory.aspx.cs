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
    public partial class InemInventory : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ItemInventory.rdlc");
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
                    
                    string whereText = " where 1=1 ";

                    if (Request.QueryString["groupTypeId"] != null && Request.QueryString["groupTypeId"] != "")
                    {
                        titleString = "Current Ceramic Raw Materials Stock Report.";
                        whereText += " and dd.TypeId = " + groupTypeId;
                    }
                    else
                    {
                        titleString = "Current Spare Parts and Others Stock Report.";
                        whereText += " and dd.TypeId != " + groupTypeId;
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

                    var query = "SELECT c.name as ItemCategory, b.name as Item,b.Specification as Specification,b.Size as Size, e.name as Warehouse, d.CardNo as BinCard, a.Quantity, u.Name as Unit FROM ItemInventory a inner join Item b on a.ItemId = b.Id inner join ItemCategory c on b.ItemCategoryId = c.Id inner join ItemGroup dd on c.ItemGroupId = dd.Id left join BinCard d on a.BinCardId = d.Id left join UnitOfMeasurement u on a.UnitId = u.Id left join Warehouse e on d.WarehouseId = e.Id " + whereText + " order by c.Name, b.Name, e.Name, d.CardNo";

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

                    ReportDataSource rdc1 = new ReportDataSource("ItemInventory",dtItem);
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

                    parms = new ReportParameter("ItemCategory", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "ItemCategory"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Item", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "Item"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Specification", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "Specification"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Size", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "Size"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("WH", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "WH"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("BinCard", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "BinCard"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Quantity", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "Quantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Unit", utility.GetResourceValueById("ResourceRDLCItemInventoryReport", "Unit"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}