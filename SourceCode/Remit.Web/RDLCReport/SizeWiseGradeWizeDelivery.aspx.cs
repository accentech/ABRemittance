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
    public partial class SizeWiseGradeWizeDelivery : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/SizeWiseGradeWizeDelivery.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    int year = 0;
                    int month = 1;
                    int fgSizeId = 0;
                    int fgGradeId = 0;
                    string sizeName = string.Empty;
                    string gradeName = string.Empty;
                    var sizewheretext = "";
                    var gradewheretext = "";

                    if (Request.QueryString["Year"] != null && Request.QueryString["Year"] != "")
                    {
                        year = Convert.ToInt32(Request.QueryString["Year"]);
                    }

                    if (Request.QueryString["Month"] != null && Request.QueryString["Month"] != "")
                    {
                        month = Convert.ToInt32(Request.QueryString["Month"]);
                    }

                    if (Request.QueryString["FGSizeId"] != null && Request.QueryString["FGSizeId"] != "")
                    {
                        fgSizeId = Convert.ToInt32(Request.QueryString["FGSizeId"]);
                        sizewheretext = " and b.FGSizeId = " + fgSizeId + "";
                    }

                    var sizeObj = _context.FGSizes.FirstOrDefault(a => a.Id == fgSizeId);
                    if (sizeObj != null)
                    {
                        sizeName = sizeObj.Size;
                    }
                    else
                    {
                        sizeName = "ALL";
                    }

                    if (Request.QueryString["FGGradeId"] != null && Request.QueryString["FGGradeId"] != "")
                    {
                        fgGradeId = Convert.ToInt32(Request.QueryString["FGGradeId"]);
                        gradewheretext = " and b.FGGradeId = " + fgGradeId + "";
                    }

                    var gradeObj = _context.FGGrades.FirstOrDefault(a => a.Id == fgGradeId);
                    if (gradeObj != null)
                    {
                        gradeName = gradeObj.Grade;
                    }
                    else
                    {
                        gradeName = "ALL";
                    }

                    var query = "Select ROW_NUMBER() Over (Order by a.DeliveryDate) As [SN], CAST(DATEADD(minute, 360, a.DeliveryDate) as date) as DeliveryDate, a.DeliverySite, p.Code, s.Size, b.Lot, a.DeliveryChallanNo, a.VATChallanNo, b.DeliveryQuantity, p.PackageToSalesRatio, b.DeliveryQuantityInSFT, y.UnitRateAfterDiscount, b.DeliveryQuantityInSFT * y.UnitRateAfterDiscount as Amount " +
                        " from FGSalesDelivery as a "+
                        " inner join FGSalesDeliveryDetail as b on a.DeliveryChallanNo = b.DeliveryChallanNo "+
                        " inner join FGSalesDetail y on a.InvoiceNo = y.FGSalesInvoiceNo and b.FGItemId = y.FGItemId and b.FGGradeId = y.FGGradeId and b.FGSizeId = y.FGSizeId and b.Lot = y.Lot "+
                        " inner join FGItem as p on b.FGItemId = p.Id "+
                        " inner join FGSize as s on p.SizeId = s.Id where a.IsDelete != 1 and Year(DATEADD(minute, " + timeZoneOffset + ", a.DeliveryDate)) = " + year + 
                        " and Month(DATEADD(minute, " + timeZoneOffset + ", a.DeliveryDate)) = " + month + sizewheretext + "" + gradewheretext +
                        " ORDER BY DeliveryDate, a.DeliverySite, p.Code, " +
                        " LEFT( b.Lot,PATINDEX(\'%[0-9]%\', b.Lot)-1), " +
                        " CONVERT(INT,SUBSTRING( b.Lot,PATINDEX(\'%[0-9]%\', b.Lot),LEN( b.Lot)))";

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
                    ReportUtility utility = new ReportUtility();
                    ReportDataSource companyDc = new ReportDataSource("CompanyTable", company);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportDataSource rdc1 = new ReportDataSource("SizeWiseGradeWizeDelivery", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Year", year.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Month", month.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Size", sizeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Grade", gradeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    string monthName = new DateTime(year, month, 1)
                            .ToString("MMM", CultureInfo.InvariantCulture);

                    parms = new ReportParameter("MonthName", monthName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                   
                    parms = new ReportParameter("Amount", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "Amount"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Code", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "Code"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Date", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "Date"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Delivery", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "Delivery"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DeliveryName", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "DeliveryName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DeliverySite", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "DeliverySite"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    
                    parms = new ReportParameter("Rate", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "Rate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ReportMonthName", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "ReportMonthName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("SizeName", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "SizeName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("TotalSft", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "TotalSft"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("VATChall", utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "VATChall"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("LotLabel",
                        utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "LotLabel"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("PerCartonSft",
                        utility.GetResourceValueById("ResourceRDLCSizeWiseGradeWIseDeliveryReport", "PerCartonSft"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);                    
                  
                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}