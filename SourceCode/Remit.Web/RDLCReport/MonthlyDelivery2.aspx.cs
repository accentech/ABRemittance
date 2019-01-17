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
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class MonthlyDelivery2 : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/MonthlyDelivery2.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    int year = 0;
                    int month = 1;

                    if (Request.QueryString["Year"] != null)
                    {
                        year = Convert.ToInt32(Request.QueryString["Year"]);
                    }

                    if (Request.QueryString["Month"] != null)
                    {
                        month = Convert.ToInt32(Request.QueryString["Month"]);
                    }

                    var query = "select DATEADD(minute, 360, b.DeliveryDate) as DeliveryDate, b.DeliveryChallanNo, d.Reason, i.Code as Item, s.Size, a.Lot, g.Grade, a.DeliveryQuantity, b.VATChallanNo, z.ZoneName, b.DriverName, b.TruckNo, b.DriverPhone, b.Remarks "+
                        "from FGSalesDeliveryDetail a "+
                        "join FGSalesDelivery b on a.DeliveryChallanNo = b.DeliveryChallanNo "+
                        "join FGDealerZone z on b.DeliverZoneId = z.Id "+ 
                        "join FGSales d on b.InvoiceNo = d.InvoiceNo "+ 
                        "join FGItem i on a.FGItemId = i.Id "+ 
                        "join FGSize s on a.FGSizeId = s.Id "+ 
                        "join FGGrade g on a.FGGradeId = g.Id "+
                        "where b.IsDelete != 1 " +
                        "and YEAR(DATEADD(minute, 360, b.DeliveryDate)) = " + year + " and MONTH(DATEADD(minute, 360, b.DeliveryDate)) = " + month + "" +
                        " ORDER BY DeliveryDate, b.DeliveryChallanNo, Item, " +
                                " LEFT( a.Lot,PATINDEX(\'%[0-9]%\', a.Lot)-1), " +
                        " CONVERT(INT,SUBSTRING( a.Lot,PATINDEX(\'%[0-9]%\', a.Lot),LEN( a.Lot)))";
                        

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

                    ReportDataSource rdc1 = new ReportDataSource("MonthlyDelivery2", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("Year", year.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("Month", month.ToString());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    string monthName = new DateTime(year, month, 1)
                            .ToString("MMM-yyyy", CultureInfo.InvariantCulture);

                    parms = new ReportParameter("ReportMonthName", monthName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportUtility utility = new ReportUtility();

                    /*parms = new ReportParameter("GrdColSl", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColSl"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColDeliveryDate", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColDeliveryDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColDeliverySite", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColDeliverySite"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("GrdColZoneName", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColZoneName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColInvoices", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColInvoices"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColNumberOfTruck", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColNumberOfTruck"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("GrdTotal", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdTotal"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);*/


                    //change column name.......

                    parms = new ReportParameter("SL", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "SL"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DeliveryDate", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "DeliveryDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ChallanNo", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "ChallanNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("SampleChallanNo", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "SampleChallanNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ProductCode", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "ProductCode"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Size", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "Size"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Lot", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "Lot"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("A", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "A"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("B", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "B"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("C", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "C"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("VATChallanNo", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "VATChallanNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ZoneName", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "ZoneName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DriverName", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "DriverName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("TruckNo", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "TruckNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DriverPhone", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "DriverPhone"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Time", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "Time"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Remarks", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "Remarks"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    parms = new ReportParameter("TitleMonthlyDelivery2Report", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "TitleMonthlyDelivery2Report"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);



                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}