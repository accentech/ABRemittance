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
    public partial class ProductWiseSales : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ProductWiseSales.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    DateTime datefrom = new DateTime(0);
                    DateTime dateto = new DateTime(0);
                    string datefromstring = "";
                    string datetostring = "";
                    int fgGradeId = 0;
                    int salesUnitId = 0;
                    int dealerId = 0;
                    int reason = 0;
                    string reasonName = "";
                    string salesUnitName = "";
                    string fgGradeName = "";
                    string dealerName = "";
                    var wheretextGrade = "";
                    var wheretextSalesUnit = "";
                    var wheretextDealer = "";

                    if (Request.QueryString["DateFrom"] != null && Request.QueryString["DateFrom"] != "")
                    {
                        datefrom = Convert.ToDateTime(Request.QueryString["DateFrom"]);
                        datefromstring = datefrom.ToString("yyyy-MM-dd");
                    }

                    if (Request.QueryString["DateTo"] != null && Request.QueryString["DateTo"] != "")
                    {
                        dateto = Convert.ToDateTime(Request.QueryString["DateTo"]);
                        datetostring = dateto.ToString("yyyy-MM-dd");
                    }

                    if (Request.QueryString["SalesUnitId"] != null && Request.QueryString["SalesUnitId"] != "")
                    {
                        salesUnitId = Convert.ToInt32(Request.QueryString["SalesUnitId"]);
                    }

                    if (Request.QueryString["FGGradeId"] != null && Request.QueryString["FGGradeId"] != "")
                    {
                        fgGradeId = Convert.ToInt32(Request.QueryString["FGGradeId"]);
                    }

                    if (Request.QueryString["DealerId"] != null && Request.QueryString["DealerId"] != "")
                    {
                        dealerId = Convert.ToInt32(Request.QueryString["DealerId"]);
                    }

                    if (Request.QueryString["Reason"] != null && Request.QueryString["Reason"] != "")
                    {
                        reason = Convert.ToInt32(Request.QueryString["Reason"]);
                        if (reason == 1)
                        {
                            reasonName = "SALES";
                        }
                        else if (reason == 2)
                        {
                            reasonName = "SAMPLE";
                        }
                        else if (reason == 3)
                        {
                            reasonName = "FREE OF COST";
                        }
                        else if (reason == 4)
                        {
                            reasonName = "INTERNAL USE";
                        }
                    }                   

                    var gradeObj = _context.FGGrades.FirstOrDefault(a => a.Id == fgGradeId);
                    if (gradeObj != null)
                    {
                        fgGradeName = gradeObj.Grade;
                        wheretextGrade = " and a.FGGradeId = " + fgGradeId + "";
                    }
                    else
                    {
                        fgGradeName = "ALL"; 
                        wheretextGrade = "";
                    }

                    var salesUnitObj = _context.FGUOMs.FirstOrDefault(a => a.Id == salesUnitId);
                    if (salesUnitObj != null)
                    {
                        salesUnitName = salesUnitObj.UnitName;
                        wheretextSalesUnit = " and p.SalesUnitId = " + salesUnitId + "";
                    }
                    else
                    {
                        salesUnitName = "ALL";
                        wheretextSalesUnit = "";
                    }

                    var dealerObj = _context.FGDealers.FirstOrDefault(a => a.Id == dealerId);
                    if (dealerObj != null)
                    {
                        dealerName = dealerObj.Name;
                        wheretextDealer = " and b.DealerId = " + dealerId + "";
                    }
                    else
                    {
                        dealerName = "ALL";
                        wheretextDealer = "";
                    }

                    var query = "select p.Code, g.Grade, p.PcsPerCartoon, p.PackageToSalesRatio, sum(a.QuantityInCTN) QuantityInCTN, sum(a.QuantityInPCs) QuantityInPCs, sum(a.QuantityInSFT) QuantityInSFT, sum(a.QuantityInSMT) QuantityInSMT, avg(a.UnitRateAfterDiscount) Rate, sum(a.Amount) Amount " +
                        "from FGSalesDetail as a "+
                        "inner join FGSales as b on a.FGSalesInvoiceNo = b.InvoiceNo "+
                        "inner join FGItem as p on a.FGItemId = p.Id "+
                        "inner join FGGrade as g on a.FGGradeId = g.Id  " +
                        "WHERE b.IsDelete != 1 and b.Reason=" + reason + " and CAST(DATEADD(minute, " + timeZoneOffset + ", b.InvoiceDate) as date) BETWEEN '" + datefromstring + "' AND '" + datetostring + "' " + wheretextGrade + " " + wheretextSalesUnit + " " + wheretextDealer + " " +
                        "group by p.Code, g.Grade, p.PcsPerCartoon, p.PackageToSalesRatio order by QuantityInCTN desc";

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

                    ReportDataSource rdc1 = new ReportDataSource("ProductWiseSales", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    datefromstring = datefrom.ToString(dateFormat);
                    parms = new ReportParameter("DateFrom", datefromstring);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    datetostring = dateto.ToString(dateFormat);
                    parms = new ReportParameter("DateTo", datetostring);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("SalesUnit", salesUnitName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Grade", fgGradeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Dealer", dealerName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Reason", reasonName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    var DateRange = "From : " + datefrom.ToString(dateFormat) + "     To: " + dateto.ToString(dateFormat);
                    parms = new ReportParameter("DateRange", DateRange);
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    //language base data: show data from resource file
                    ReportUtility utility = new ReportUtility();
                    parms = new ReportParameter("ColSl", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColSl"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColCode", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColCode"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColGrade", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColGrade"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColPcsCartoon", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColPcsCartoon"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColMeasurement", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColMeasurement"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColQuantityInCTN", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColQuantityInCTN"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColQuantityInPCs", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColQuantityInPCs"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColQuantityInSFT", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "ColQuantityInSFT"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCProductWiseSalesReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}