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
    public partial class EmpWiseItemUseReport1 : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/EmpWiseItemUseReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    DateTime fromDate = new DateTime(0);
                    string fromDatestring = string.Empty;
                    DateTime toDate = new DateTime(0);
                    string toDatestring = string.Empty;

                    int empId = 0;
                    int groupTypeId = Convert.ToInt32(WebConfigurationManager.AppSettings["GroupType"]);
                    bool rawMaterial = true;
                    int groupId = 0;
                    int categoryId = 0;
                    string titleString = string.Empty;
                    string empName = string.Empty;
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

                    string whereText = "where a.IsDeleted != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", a.Date) as date) >= '" + fromDatestring + "' and CAST(DATEADD(minute, " + timeZoneOffset + ", a.Date) as date) <= '" + toDatestring + "'";

                    if (Request.QueryString["empId"] != null && Request.QueryString["empId"] != "")
                    {
                        empId = Convert.ToInt32(Request.QueryString["empId"]);
                    }

                    var empObj = _context.Employees.FirstOrDefault(a => a.Id == empId);
                    if (empObj != null)
                    {
                        empName = empObj.FullName;
                        whereText += " and a.ReceivedBy = " + empId;
                    }
                    else
                    {
                        empName = "ALL";
                    }

                    if (Request.QueryString["groupTypeId"] != null && Request.QueryString["groupTypeId"] != "")
                    {
                        titleString = "Employee Wise Ceramic Raw Materials Use Report.";
                        whereText += " and ig.TypeId = " + groupTypeId;
                    }
                    else
                    {
                        titleString = "Employee Wise Spare Parts and Others Use Report.";
                        whereText += " and ig.TypeId != " + groupTypeId;
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
                        whereText += " and ic.ItemGroupId = " + groupId;
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
                        whereText += " and i.ItemCategoryId = " + categoryId;
                    }
                    else
                    {
                        categoryName = "ALL";
                    }

                    var query =
                        "SELECT CONVERT(date,DATEADD(minute, " + timeZoneOffset + ", a.Date)) as Date, a.IsuueNo, i.Name as Item,i.Specification as Specification,i.Size as Size, b.Quantity, u.Name as Unit FROM ItemIssue as a  join ItemIssuedetail as b on b.ItemIssueId = a.Id  join Item as i on b.ItemId = i.Id  join ItemCategory as ic ON i.ItemCategoryId = ic.Id  join ItemGroup as ig ON ic.ItemGroupId = ig.Id join UnitOfMeasurement as u on b.UnitId = u.Id left join Employee as c on a.ReceivedBy = c.Id " + whereText + " order by a.Date, i.Name";

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

                    ReportDataSource rdc1 = new ReportDataSource("EmpWiseItemUseReport", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();

                    parms = new ReportParameter("rawMaterial", rawMaterial.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("fromDate", fromDate.ToString(dateFormat));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("toDate", toDate.ToString(dateFormat));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    titleString += " Employee: " + empName;// +", Group: " + groupName + ", Category: " + categoryName;
                    parms = new ReportParameter("subTitle", titleString);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    var dateRange = "From : " + fromDate.ToString(dateFormat) + " To: " + toDate.ToString(dateFormat);
                    parms = new ReportParameter("dateRange", dateRange);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //language base data: show data from resource file

                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("Date", utility.GetResourceValueById("ResourceRDLCEmpWiseItemUseReport", "Date"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("IsuueNo", utility.GetResourceValueById("ResourceRDLCEmpWiseItemUseReport", "IsuueNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Item", utility.GetResourceValueById("ResourceRDLCEmpWiseItemUseReport", "Item"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Specification", utility.GetResourceValueById("ResourceRDLCEmpWiseItemUseReport", "Specification"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Size", utility.GetResourceValueById("ResourceRDLCEmpWiseItemUseReport", "Size"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Quantity", utility.GetResourceValueById("ResourceRDLCEmpWiseItemUseReport", "Quantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Unit", utility.GetResourceValueById("ResourceRDLCEmpWiseItemUseReport", "Unit"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}