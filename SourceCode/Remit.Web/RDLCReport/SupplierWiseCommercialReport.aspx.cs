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
using Remit.Model.Models;
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class SupplierWiseCommercialReport : System.Web.UI.Page
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
                    //DateTime dailyDate = new DateTime(0);
                    //string dailyDatestring = string.Empty;
                    //int salesUnit = 0;
                    //int productType = 0;
                    //string unitName = string.Empty;
                    //string typeName = string.Empty;
                    //string typefilter = string.Empty;
                    //int reason = 0;
                    //int selectedDateType = 0;

                    DateTime fromDate = new DateTime(0);
                    string fromDatestring = string.Empty;
                    DateTime toDate = new DateTime(0);
                    string toDatestring = string.Empty;
                    int supplierId = 0;
                    string supplierName = string.Empty;
                    #endregion

                    #region parameter Checking
                    
                    if (Request.QueryString["SupplierId"] != null)
                    {
                        supplierId = Convert.ToInt32(Request.QueryString["SupplierId"]);
                    }
                    if (Request.QueryString["SupplierName"] != null)
                    {
                        supplierName = Request.QueryString["SupplierName"];
                    }
                    //if (Request.QueryString["selectedDateType"] != null)
                    //{
                    //    selectedDateType = Convert.ToInt32(Request.QueryString["selectedDateType"]);
                    //}

                    //if (Request.QueryString["dailyDate"] != null)
                    //{
                    //    dailyDate = DateTime.Parse(Request.QueryString["dailyDate"]);
                    //    dailyDatestring = dailyDate.AddMinutes(-timeZoneOffset).AddHours(24).ToString();
                    //}

                    //if (selectedDateType == 2)
                    //{
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
                    //}
                    //else
                    //{
                    //    fromDate = dailyDate; toDate = dailyDate;
                    //    fromDatestring = dailyDate.AddMinutes(-timeZoneOffset).ToString();
                    //    toDatestring = dailyDatestring;
                    //}
                    

                    #endregion

                    System.Data.DataTable dws = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/SupplierWiseCommercialReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();
                    var getCompany =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";

                    var query =
                        " Select LC.Id,LC.LCNo,LC.LCIssueDate,Item.Id as ItemId,Item.Name,pidet.Quantity,pidet.UnitPrice,pidet.Amount,um.Name, puku.invoiceQty, cu.Symbol from ProformaInvoiceDetail as pidet " +
                        " Inner Join ProformaInvoice as pi on pidet.ProformaInvoiceId = pi.Id" +
                        " Inner join ( select a.* from LCAmendment a join " +
                        " (Select LCId, MAX(AmendmentNo) as AmendmentNo from LCAmendment group by LCId" +
                        " ) b on a.LCId = b.LCId and a.AmendmentNo = b.AmendmentNo) as amn on  " +
                        " amn.ProformaInvoiceId = pi.Id Inner join LC on LC.Id = amn.LCId" +
                        " Inner join Item on Item.Id = pidet.ItemId" +
                        " Inner join UnitOfMeasurement um on pidet.UnitId = um.Id" +
                        " Inner join Currency cu on pi.CurrencyId = cu.Id " +
                        " left join (Select ci.LCId,cidet.ItemId, SUM(cidet.Quantity) as invoiceQty from CommercialInvoice ci " +
                        " inner join CommercialInvoiceDetail cidet on ci.Id = cidet.CommercialInvoiceId  group by ci.LCId,cidet.ItemId) as puku on puku.LCId = LC.Id and puku.ItemId = Item.Id" +
                        " where SupplierId =" + supplierId + "and LC.LCIssueDate >= '" + fromDatestring +
                        "' and LC.LCIssueDate <  '" + toDatestring + "' order By LC.LCIssueDate desc";



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

                    ReportDataSource rdc1 = new ReportDataSource("SupplierWiseCommercial", dws);
                    ReportDataSource companyDc = new ReportDataSource("CompanyInfo", company);
                   
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);
                    

                    ReportParameter parms = new ReportParameter();
                    var DateRange = "From : " + fromDate.ToString(dateFormat) + "     To: " + toDate.ToString(dateFormat);
                    parms = new ReportParameter("DateRange", DateRange);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("SupplierName", supplierName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    //language base data: show data from resource file

                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("SL", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "SL"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("LCNo", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "LCNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("LCDate", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "LCDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Material", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "Material"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("LCQty", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "LCQty"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Rate", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "Rate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InvoiceQty", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "InvoiceQty"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCSupplierWiseCommercialReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    



                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}