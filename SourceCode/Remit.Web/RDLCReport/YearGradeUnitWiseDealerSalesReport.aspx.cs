using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Office.Interop.Excel;
using Microsoft.Reporting.WebForms;
using Remit.Data.Models;
using Remit.Model.Models;
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class YearGradeUnitWiseDealerSalesReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                long timeZoneOffset = UserSession.GetTimeZoneOffset();
                using (var _context = new ApplicationEntities())
                {
                    int salesUnit = 0;
                    string unitName = string.Empty;
                    int fgGradeId = 0;
                    string fgGradeName = "";
                    int year = 0;
                    var zoneObj = new FGDealerZone();
                    String ZoneName = string.Empty;
                    var dealersZoneId = -1;
                    int reason = 0;

                    if (Request.QueryString["SelectedYear"] != null)
                    {
                        year = Convert.ToInt32(Request.QueryString["SelectedYear"]);
                    }

                    if (Request.QueryString["reason"] != null)
                    {
                        reason = Convert.ToInt32(Request.QueryString["reason"]);
                    }

                    if (Request.QueryString["saleUnit"] != null)
                    {
                        salesUnit = Convert.ToInt32(Request.QueryString["saleUnit"]);
                    }
                    var unitObj = _context.FGUOMs.FirstOrDefault(a => a.Id == salesUnit);
                    unitName = unitObj != null ? unitObj.UnitName : "SFT";

                    if (Request.QueryString["FGGradeId"] != null && Request.QueryString["FGGradeId"] != "")
                    {
                        fgGradeId = Convert.ToInt32(Request.QueryString["FGGradeId"]);
                    }
                    var gradeObj = _context.FGGrades.FirstOrDefault(a => a.Id == fgGradeId);
                    fgGradeName = gradeObj != null ? gradeObj.Grade : "ALL";

                    if (Request.QueryString["dealersZoneId"] != null && Request.QueryString["dealersZoneId"] != "")
                    {
                        dealersZoneId = Convert.ToInt32(Request.QueryString["dealersZoneId"]);
                        zoneObj = _context.FGDealerZones.FirstOrDefault(a => a.Id == dealersZoneId);
                    }

                    ZoneName = zoneObj != null ? zoneObj.ZoneName : "ALL";
                    
                    var recv = _context.ItemReceives.ToList();
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/YearGradeUnitWiseDealerSalesReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    var queryCom =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";

                    var query =
                        " WITH R(N) AS(SELECT 1 UNION ALL SELECT N+1 FROM R WHERE N < 12) " +
                        " Select R.N AS month, " +
                        " q1.DealerName, " +
                        " q1.Qty, " +
                        " q1.DealerZone, " +
                        " q1.DealerZoneId, q1.DivisionName"+
                        " FROM R left join " +
                        " (Select FGDealerZone.ZoneName as DealerZone,"+ "FGDealerZone.Id as DealerZoneId, "+
                        " FGDealer.Name as DealerName, Division.Name as DivisionName," +
                        " month(DATEADD(minute," + timeZoneOffset + ", InvoiceDate)) as MonthId," +
                        " SUM([QuantityInSFT]) as Qty " +
                        " FROM FGSales" +
                        " LEFT JOIN FGSalesDetail ON FGSales.InvoiceNo = FGSalesDetail.FGSalesInvoiceNo" +
                        " LEFT JOIN FGDealer ON FGSales.DealerId = FGDealer.Id" +
                        " LEFT JOIN FGDealerZone ON FGDealer.DealersZoneId = FGDealerZone.Id"+
                        " LEFT JOIN Division ON FGDealerZone.DivisionId = Division.Id" +
                        " where FGSales.IsDelete != 1 and year(DATEADD(minute," + timeZoneOffset + ", InvoiceDate)) = " + year +
                        " and FGSales.Reason = " + reason +
                        " And FGSalesDetail.SalesFGUnitId =" + salesUnit;
                    if (fgGradeName == "ALL")
                    {}
                    else
                    {
                        query += " And FGSalesDetail.FGGradeId = " + fgGradeId;
                    }

                    if (ZoneName == "ALL")
                    { }
                    else
                    {
                        query += " And FGDealerZone.Id = " + dealersZoneId;
                    }

                    query += " And FGDealer.Name IS NOT NULL" +
                             " group by month(DATEADD(minute," + timeZoneOffset + ", InvoiceDate)), FGDealer.Name, FGDealerZone.ZoneName, Division.Name, FGDealerZone.Id )" +
                             " as q1 on R.N = q1.MonthId " +
                             " order by " +
                             " case when q1.DealerZoneId= " + ConfigurationManager.AppSettings["OtherDealerId"] +
                             " then 1 else 0 end , DivisionName";
                                        
                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    
                    SqlCommand cmd = new SqlCommand(query);
                    SqlCommand cmdComp = new SqlCommand(queryCom);

                    SqlDataAdapter sda = new SqlDataAdapter();
                    using (SqlConnection con = new SqlConnection(conString))
                    {
                        using (sda)
                        {
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;
                            sda.Fill(dtItem);

                            cmdComp.Connection = con;
                            sda.SelectCommand = cmdComp;
                            sda.Fill(company);
                        }
                    }


                    ReportDataSource rdc1 = new ReportDataSource("YGUWDSReportTable", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportDataSource companyDc = new ReportDataSource("CompanyTable", company);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportParameter parms = new ReportParameter();

                    parms = new ReportParameter("YearDate", year.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("unitName", unitName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Grade", fgGradeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ZoneName", ZoneName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);



                    //language base data: show data from resource file
                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("ColSl", utility.GetResourceValueById("ResourceRDLCYearGradeUnitWiseDealerSalesReport", "ColSl"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColDealerZone", utility.GetResourceValueById("ResourceRDLCYearGradeUnitWiseDealerSalesReport", "ColDealerZone"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColTotal", utility.GetResourceValueById("ResourceRDLCYearGradeUnitWiseDealerSalesReport", "ColTotal"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }

            }
        }
    }
}