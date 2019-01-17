using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Remit.Data.Models;
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class ItemWiseCommercialReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
            long timeZoneOffset = UserSession.GetTimeZoneOffset();
            if (!IsPostBack)
            {
                using (var _context = new ApplicationEntities())
                {
                    #region fields

                    DateTime fromDate = new DateTime(0);
                    string fromDatestring = string.Empty;
                    DateTime toDate = new DateTime(0);
                    string toDatestring = string.Empty;
                    Guid itemId = Guid.Empty;
                    string itemName = string.Empty;
                    #endregion

                    #region parameter Checking

                    if (Request.QueryString["ItemId"] != null && Request.QueryString["ItemId"] != "")
                    {
                        itemId = new Guid(Request.QueryString["ItemId"]);
                    }
                    if (Request.QueryString["ItemName"] != null)
                    {
                        itemName = Request.QueryString["ItemName"];
                    }
                    
                    if (Request.QueryString["fromDate"] != null)
                    {
                        fromDate = Convert.ToDateTime(Request.QueryString["fromDate"]);
                        fromDatestring = fromDate.AddMinutes(-timeZoneOffset).ToString();
                    }

                    if (Request.QueryString["toDate"] != null)
                    {
                        toDate = DateTime.Parse(Request.QueryString["toDate"]);
                        toDatestring = toDate.AddMinutes(-timeZoneOffset).AddHours(24).ToString();
                    }
                    
                    #endregion

                    System.Data.DataTable dws = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ItemWiseCommercialReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
                    var getCompany =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";

                    var query =" Select Supplier.Name as SupplierName, LC.LCNo, LC.LCIssueDate, pidet.Quantity, um.Name, pidet.UnitPrice," +
                               "  pidet.Amount, puku.CommercialInvoiceNo, puku.InvoiceDate, puku.invoiceQty, cu.Symbol" +
                               "  from ProformaInvoiceDetail as pidet" +
                               "  Inner Join ProformaInvoice as pi on pidet.ProformaInvoiceId = pi.Id" +
                               "  Inner join ( " +
                               "    select a.* from LCAmendment a join (" +
                               "        select LCId, MAX(AmendmentNo) as AmendmentNo from LCAmendment group by LCId " +
                               "  ) b on a.LCId = b.LCId and a.AmendmentNo = b.AmendmentNo) as amn on  " +
                               "  amn.ProformaInvoiceId = pi.Id" +
                               "  Inner join Supplier on pi.SupplierId = Supplier.Id" +
                               "  Inner join LC on LC.Id = amn.LCId" +
                               "  Inner join Item on Item.Id = pidet.ItemId" +
                               "  Inner join UnitOfMeasurement um on pidet.UnitId = um.Id " +
                               "  Inner join Currency cu on pi.CurrencyId = cu.Id " +
                               "  left join (   " +
                               "  select ci.LCId,ci.CommercialInvoiceNo, ci.InvoiceDate, cidet.ItemId,cidet.Quantity as invoiceQty from CommercialInvoice ci " +
                               "  inner join CommercialInvoiceDetail cidet on ci.Id = cidet.CommercialInvoiceId" +
                               "  ) " +
                               "  as puku on puku.LCId = LC.Id and puku.ItemId = Item.Id" +
                               "  where pidet.ItemId = '" + itemId + "' and LC.LCIssueDate >= '" + fromDatestring +
                               "' and LC.LCIssueDate <  '" + toDatestring + "' order by SupplierName";



                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand(query);
                    SqlCommand cmdComp = new SqlCommand(getCompany);
                    SqlDataAdapter sda = new SqlDataAdapter();
                    using (SqlConnection con = new SqlConnection(conString))
                    {
                        using (sda)
                        {
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;
                            sda.Fill(dws);

                            cmdComp.Connection = con;
                            sda.SelectCommand = cmdComp;
                            sda.Fill(company);

                        }
                    }

                    ReportDataSource rdc1 = new ReportDataSource("ItemWiseCommercial", dws);
                    ReportDataSource companyDc = new ReportDataSource("CompanyInfo", company);

                    ReportViewer1.LocalReport.DataSources.Add(rdc1);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);


                    ReportParameter parms = new ReportParameter();
                    var DateRange = "From : " + fromDate.ToString(dateFormat) + "     To: " + toDate.ToString(dateFormat);
                    parms = new ReportParameter("DateRange", DateRange);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ItemName", itemName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    //language base data: show data from resource file
                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("SL", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "SL"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("SupplierName", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "SupplierName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("LCNo", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "LCNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("CommercialInvNo", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "CommercialInvNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("LCIssueDate", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "LCIssueDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Quantity", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "Quantity"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("UnitPrice", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "UnitPrice"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InvoiceDate", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "InvoiceDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InvoiceQty", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "InvoiceQty"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCItemWiseCommercialReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}