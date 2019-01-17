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
using Microsoft.Ajax.Utilities;

namespace Remit.Web.RDLCReport
{
    public partial class DealerWiseSizeWiseSales : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/DealerWiseSizeWiseSales.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    DateTime datefrom = new DateTime(0);
                    DateTime dateto = new DateTime(0);
                    string datefromstring = "";
                    string datetostring = "";
                    int fgGradeId = 0;
                    int reason = 0;
                    string fgGradeName = "";
                    string reasonName = "";
                    var wheretext = "";

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

                    if (Request.QueryString["FGGradeId"] != null && Request.QueryString["FGGradeId"] != "")
                    {
                        fgGradeId = Convert.ToInt32(Request.QueryString["FGGradeId"]);
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
                        wheretext = " and FGSalesDetail.FGGradeId = " + fgGradeId + "";
                    }
                    else
                    {
                        fgGradeName = "ALL";
                        wheretext = "";
                    }

                    var query = "SElect  Data.Name, Dimension.TypeName, Dimension.Size, Data.SalesQuantity from  " +
                        "(SELECT FGType.TypeName, FGSize.Id As SizeId, FGSize.Size  " +
                        "	FROM FGSize, FGType  " +
                        "	WHERE FGSize.TypeId = FGType.Id " +
                        ") AS Dimension Left Join  " +
                        "(SELECT FGSales.DealerId, FGDealer.Name, FGSalesDetail.FGSizeId, SUM(FGSalesDetail.SalesQuantity) AS SalesQuantity  " +
                        "	FROM FGSales " +
                        "	INNER JOIN FGDealer ON FGSales.DealerId = FGDealer.Id " +
                        "	INNER JOIN FGSalesDetail ON FGSales.InvoiceNo = FGSalesDetail.FGSalesInvoiceNo " +
                        "	WHERE FGSales.IsDelete != 1 and FGSales.DealerId != -1 and FGSales.Reason=" + reason + " and CAST(DATEADD(minute, " + timeZoneOffset + ", FGSales.InvoiceDate) as date) BETWEEN '" + datefromstring + "' AND '" + datetostring + "' " + wheretext +
                        "	GROUP BY FGSales.DealerId, FGDealer.Name, FGSalesDetail.FGSizeId " +
                        ") AS Data ON Dimension.SizeId = Data.FGSizeId " +
                        " Union All " +
                        "SElect  Data.Name, Dimension.TypeName, Dimension.Size, Data.SalesQuantity from  " +
                        "(SELECT FGType.TypeName, FGSize.Id As SizeId, FGSize.Size  " +
                        "	FROM FGSize, FGType  " +
                        "	WHERE FGSize.TypeId = FGType.Id " +
                        ") AS Dimension Left Join  " +
                        "(SELECT FGSales.DealerId, FGDealer.Name, FGSalesDetail.FGSizeId, SUM(FGSalesDetail.SalesQuantity) AS SalesQuantity  " +
                        "	FROM FGSales " +
                        "	INNER JOIN FGDealer ON FGSales.DealerId = FGDealer.Id " +
                        "	INNER JOIN FGSalesDetail ON FGSales.InvoiceNo = FGSalesDetail.FGSalesInvoiceNo " +
                        "	WHERE FGSales.IsDelete != 1and FGSales.DealerId = -1  and FGSales.Reason=" + reason + " and CAST(DATEADD(minute, " + timeZoneOffset + ", FGSales.InvoiceDate) as date) BETWEEN '" + datefromstring + "' AND '" + datetostring + "' " + wheretext +
                        "	GROUP BY FGSales.DealerId, FGDealer.Name, FGSalesDetail.FGSizeId " +
                        ") AS Data ON Dimension.SizeId = Data.FGSizeId ";

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

                    ReportDataSource rdc1 = new ReportDataSource("DealerWiseSizeWiseSales", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    datefromstring = datefrom.ToString(dateFormat);
                    parms = new ReportParameter("DateFrom", datefromstring);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    datetostring = dateto.ToString(dateFormat);
                    parms = new ReportParameter("DateTo", datetostring);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Grade", fgGradeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Reason", reasonName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    //language base data: show data from resource file
                    ReportUtility utility = new ReportUtility();
                    parms = new ReportParameter("ColSl", utility.GetResourceValueById("ResourceRDLCDealerWiseSizeWiseSalesReport", "ColSl"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColDealerName", utility.GetResourceValueById("ResourceRDLCDealerWiseSizeWiseSalesReport", "ColDealerName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ColTotal", utility.GetResourceValueById("ResourceRDLCDealerWiseSizeWiseSalesReport", "ColTotal"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}