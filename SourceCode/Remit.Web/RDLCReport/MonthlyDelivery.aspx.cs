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
    public partial class MonthlyDelivery : System.Web.UI.Page
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
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/MonthlyDelivery.rdlc");
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

                    var query = "select DeliveryOption, DeliveryDate, DealerName, ZoneName, DeliverySite, count(*) as NumTruck, " +
                                
                                "STUFF(" +
                                "(SELECT ', ' + InvoiceNo FROM FGSalesDelivery " +
                                "x join FGDealerZone y on x.DeliverZoneId = y.Id WHERE x.DeliverySite = A.DeliverySite " +
                                //"and x.DeliveryOption ='Company' " +
                                "and x.IsDelete != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", x.DeliveryDate) as date) = A.DeliveryDate and y.ZoneName = A.ZoneName " +
                                "group by InvoiceNo FOR XML PATH('')), 1, 1, '') AS Invoices, " +
                                
                                "STUFF(" +
                                "(SELECT ', ' + CONVERT(varchar, DATEADD(minute, " + timeZoneOffset + ", InvoiceDate), 101) FROM FGSalesDelivery " +
                                "x join FGSales s on x.InvoiceNo = s.InvoiceNo " +
                                "join FGDealerZone y on x.DeliverZoneId = y.Id WHERE x.DeliverySite = A.DeliverySite " +
                                //"and x.DeliveryOption ='Company' " +
                                "and x.IsDelete != 1 and CAST(DATEADD(minute, " + timeZoneOffset + ", x.DeliveryDate) as date) = A.DeliveryDate and y.ZoneName = A.ZoneName " +
                                "group by InvoiceDate FOR XML PATH('')), 1, 1, '') AS InvoiceDates " +
                                "from " +

                                "( select a.DeliveryOption, a.DeliverySite, a.Name as DealerName, b.ZoneName, CAST(DATEADD(minute, " + timeZoneOffset + ", a.DeliveryDate) as date) as DeliveryDate, a.TruckNo from FGSalesDelivery a " +
                                "join FGDealerZone b on a.DeliverZoneId = b.Id where " +
                                //"a.DeliveryOption ='Company' and " +
                                "a.IsDelete != 1 and YEAR(DATEADD(minute, " + timeZoneOffset + ", a.DeliveryDate)) = " + year + " and MONTH(DATEADD(minute, " + timeZoneOffset + ", a.DeliveryDate)) = " + month + " " +
                                "group by a.DeliveryOption, a.DeliverySite, a.Name, b.ZoneName, CAST(DATEADD(minute, " + timeZoneOffset + ", a.DeliveryDate) as date), a.TruckNo " +
                                ") as A " +
                                "group by DeliveryOption, DeliveryDate, ZoneName, DealerName, DeliverySite";

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

                    ReportDataSource rdc1 = new ReportDataSource("MonthlyDelivery", dtItem);
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

                    //language base data: show data from resource file
                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("GrdColSl", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColSl"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColDeliveryDate", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColDeliveryDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColDealerName", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColDealerName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColDeliverySite", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColDeliverySite"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("GrdColZoneName", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColZoneName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColInvoices", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColInvoices"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColInvoiceDates", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColInvoiceDates"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("GrdColNumberOfTruck", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdColNumberOfTruck"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("GrdTotal", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "GrdTotal"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("TitleMonthlyDeliveryReport", utility.GetResourceValueById("ResourceRDLCMMDeliveryReport", "TitleMonthlyDeliveryReport"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);                   

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}