using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Remit.Data.Models;
using Remit.Web.Helpers;
using System.Web.Configuration;

namespace Remit.Web.RDLCReport
{
    public partial class SaleStatementReport : System.Web.UI.Page
    {
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        protected void Page_Load(object sender, EventArgs e)
        {
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
                    int salesUnit = 0;
                    int productType = 0;
                    int dealer = 0;
                    string unitName = string.Empty;
                    string typeName = string.Empty;
                    string whereText = string.Empty;
                    string typefilter = string.Empty;
                    string dealerName = string.Empty;
                    int reason = 0;
                    #endregion

                    #region parameter Checking

                    if (Request.QueryString["reason"] != null)
                    {
                        reason = Convert.ToInt32(Request.QueryString["reason"]);
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

                    if (Request.QueryString["saleUnit"] != null)
                    {
                        salesUnit = Convert.ToInt32(Request.QueryString["saleUnit"]);
                    }

                    if (Request.QueryString["productType"] != null)
                    {
                        productType = Convert.ToInt32(Request.QueryString["productType"]);

                        if (productType != 0)
                        {
                            typefilter = "and FGType.Id = " + productType;
                        }
                        else
                        {
                            typefilter = " ";
                        }
                    }

                    var unitObj = _context.FGUOMs.FirstOrDefault(a => a.Id == salesUnit);
                    unitName = unitObj != null ? unitObj.UnitName : "SFT";

                    var typeObj = _context.FGTypes.FirstOrDefault(a => a.Id == productType);
                    typeName = typeObj != null ? typeObj.TypeName : "ALL";

                    if (Request.QueryString["dealer"] != null)
                    {
                        dealer = Convert.ToInt32(Request.QueryString["dealer"]);

                        var dealerObj = _context.FGDealers.FirstOrDefault(m => m.Id == dealer);
                        if (dealerObj != null)
                        {
                            dealerName = dealerObj.Name;
                        }
                    }

                    #endregion

                    System.Data.DataTable dws = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/SalesStatementReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    var query = " DECLARE @checkval AS int " +
                                " DECLARE @dealerId AS int " +
                                " set @checkval= " + salesUnit +
                                " set @dealerId=" + dealer +
                                " declare @tmp table (InvoiceDate Date, InvoiceNo nvarchar(100), Grade varchar(max), qty float ); " +
                                " INSERT INTO @tmp " +
                                " SELECT CAST(FGSales.InvoiceDate as date) as InvoiceDate , FGSales.InvoiceNo, " +
                                " case " +
                                " when FGGrade.Grade in ('A') then 'Grade A' " +
                                " when FGGrade.Grade in ('B') then 'Grade B' " +
                                " when FGGrade.Grade in ('C') then 'Grade C' " +
                                " when FGGrade.Grade in ('D') then 'Grade D' " +
                                " when FGGrade.Grade in ('E') then 'Grade E' " +
                                " when FGGrade.Grade in ('F') then 'Grade F' " +
                                " else 'Grade ' + FGGrade.Grade end as Grade , " +
                                " SUM( " +
                                " case " +
                                " when @checkval in (1) then FGSalesDetail.QuantityInCTN " +
                                " when @checkval in (2) then FGSalesDetail.QuantityInPCs " +
                                " when @checkval in (3) then FGSalesDetail.QuantityInSFT " +
                                " when @checkval in (4) then FGSalesDetail.QuantityInSMT " +
                                //" when @checkval in (0) then FGSalesDetail.QuantityInSFT " +
                                " else 0 end) as Qty FROM FGDealer " +
                                " left JOIN ( " +
                                " FGSales inner JOIN FGSalesDetail ON FGSales.InvoiceNo = FGSalesDetail.FGSalesInvoiceNo " +
                                " inner JOIN FGGrade ON FGSalesDetail.FGGradeId = FGGrade.Id " +
                                " left join FGItem on FGSalesDetail.FGItemId = FGItem.Id " +
                                " inner join FGType on FGItem.TypeId = FGType.Id " +
                                " ) ON FGDealer.Id = FGSales.DealerId " +
                                " where FGSales.IsDelete != 1 and FGSales.Reason = " + reason + " and FGDealer.Id = @dealerId and FGSales.InvoiceDate >= '" + fromDatestring + "' and FGSales.InvoiceDate < '" + toDatestring + "' " +
                                " group by CAST(FGSales.InvoiceDate as date), FGSales.InvoiceNo, FGGrade.Grade " +

                                " union all " +
                                " Select CAST(a.InvoiceDate as date)as invoiceDate,a.InvoiceNo, a.payMode, sum(a.PaymentAmount) as PaymentAmount " +
                                " from ( SELECT CAST(FGSales.InvoiceDate as date) as InvoiceDate , FGSales.InvoiceNo, case " +
                                " when FGSalesPayment.PaymentMode IN (1, 4) then 'Cash Pay.' " +
                                " when FGSalesPayment.PaymentMode IN (2, 3) then 'Cheq. Pay' " +
                                " else 'others' End as payMode, FGSalesPayment.PaymentAmount " +
                                " FROM FGDealer left Join FGSales on FGDealer.Id = FGSales.DealerId " +
                                " INNER JOIN FGSalesPayment on FGSales.InvoiceNo = FGSalesPayment.FGSalesInvoiceNo " +
                                " where FGSales.IsDelete != 1 and FGSales.Reason = " + reason + " and FGDealer.Id = @dealerId and FGSales.InvoiceDate >= '" + fromDatestring + "' and FGSales.InvoiceDate < '" + toDatestring + "' ) as a " +
                                " group by  invoiceDate, a.InvoiceNo, a.payMode " +

                                " union all " +
                                " Select CAST(b.InvoiceDate as date)as invoiceDate, b.InvoiceNo, b.AdjustmnetName, sum( b.AdjustmentAmount) as amount " +
                                " from FGDealer left join ( " +
                                " Select FGSales.InvoiceDate, FGSales.InvoiceNo, FGAdjustmentSetup.AdjustmnetName, FGSalesAdjustment.AdjustmentAmount, FGSalesAdjustment.FGSalesInvoiceNo, FGSales.DealerId " +
                                " From FGAdjustmentSetup LEFT JOIN FGSalesAdjustment on FGAdjustmentSetup.Id = FGSalesAdjustment.FGAdjustmentSetupId " +
                                " left join FGSales On FGSales.InvoiceNo = FGSalesAdjustment.FGSalesInvoiceNo and FGSales.IsDelete != 1 and FGSales.Reason = " + reason + " and FGSales.InvoiceDate >= '" + fromDatestring +
                                "' and FGSales.InvoiceDate < '" + toDatestring + "' ) as b on FGDealer.Id = b.DealerId " +
                                " where FGDealer.Id = @dealerId  group by invoiceDate, b.InvoiceNo, b.AdjustmnetName " +

                                //"union all " +
                                //    " Select CAST(FGSales.InvoiceDate as date) as InvoiceDate , " +
                                //    " FGSales.InvoiceNo, 'Free' as grade, " +
                                //    " case " +
                                //    " when FGSales.Reason in (3) then " +
                                //    " sum(FGSales.TotalAmount) " +
                                //    " else 0 end as value " +
                                //    " from FGDealer left join FGSales on FGSales.DealerId = FGDealer.Id " +
                                //    " where FGSales.IsDelete != 1 and FGDealer.IsActive = 1 and FGSales.InvoiceDate >= '" + fromDatestring +
                                //    "' and FGSales.InvoiceDate <'" + toDatestring + "'" +
                                //    " group by InvoiceDate, FGSales.InvoiceNo, FGSales.Reason " +
                                //"union all " +
                                //    " Select CAST(FGSales.InvoiceDate as date) as InvoiceDate , " +
                                //    " FGSales.InvoiceNo, 'Sample' as grade, " +
                                //    " case " +
                                //    " when FGSales.Reason in (2) then " +
                                //    " sum(FGSales.TotalAmount) " +
                                //    " else 0 end as value " +
                                //    " from FGDealer left join FGSales on FGSales.DealerId = FGDealer.Id " +
                                //    " where FGSales.IsDelete != 1 and FGDealer.IsActive = 1 and FGSales.InvoiceDate >= '" + fromDatestring +
                                //    "' and FGSales.InvoiceDate <'" + toDatestring + "'" +
                                //    " group by InvoiceDate, FGSales.InvoiceNo, FGSales.Reason " +
                                //"union all " +
                                //    " Select CAST(FGSales.InvoiceDate as date) as InvoiceDate , " +
                                //    " FGSales.InvoiceNo, 'Internal Use' as grade, " +
                                //    " case " +
                                //    " when FGSales.Reason in (4) then " +
                                //    " sum(FGSales.TotalAmount) " +
                                //    " else 0 end as value " +
                                //    " from FGDealer left join FGSales on FGSales.DealerId = FGDealer.Id " +
                                //    " where FGSales.IsDelete != 1 and FGDealer.IsActive = 1 and FGSales.InvoiceDate >= '" + fromDatestring +
                                //    "' and FGSales.InvoiceDate <'" + toDatestring + "'" +
                                //    " group by InvoiceDate, FGSales.InvoiceNo, FGSales.Reason " +
                                //"union all " +
                                //" Select  CAST(FGSales.InvoiceDate as date) as InvoiceDate , FGSales.InvoiceNo, 'Free' as grade, 0.0 as value from FGDealer left join FGSales on FGSales.DealerId = FGDealer.Id where FGSales.IsDelete != 1 and FGDealer.IsActive = 1 and FGDealer.Id = @dealerId and FGSales.InvoiceDate >= '" +
                                //fromDatestring + "' and FGSales.InvoiceDate < '" + toDatestring + "' " +


                                " union all " +
                                " Select  CAST(FGSales.InvoiceDate as date) as InvoiceDate , FGSales.InvoiceNo, 'Discount' as grade, sum(DiscountAmount) as value  from FGDealer " +
                                "left join FGSales on FGSales.DealerId = FGDealer.Id where FGSales.IsDelete != 1 and FGSales.Reason = " + reason + " and FGDealer.Id = @dealerId and FGSales.InvoiceDate >= '" +
                                fromDatestring + "' and FGSales.InvoiceDate < '" + toDatestring + "' " + "group by InvoiceDate , FGSales.InvoiceNo  " +

                                " union all " +
                                " Select CAST(FGSales.InvoiceDate as date) as InvoiceDate , FGSales.InvoiceNo, 'Due/Adv.' as grade, sum(FGSales.TotalDueAdvancedAmount) as amount from FGDealer " +
                                " left join FGSales on FGDealer.Id = FGSales.DealerId where FGSales.IsDelete != 1 and FGSales.Reason = " + reason + " and FGDealer.Id = @dealerId and FGSales.InvoiceDate >= '" +
                                  fromDatestring + "' and FGSales.InvoiceDate < '" + toDatestring + "' group by InvoiceDate , FGSales.InvoiceNo " +
                                

                                " INSERT INTO @tmp " +
                                " SELECT a.InvoiceDate,a.InvoiceNo,'Total', Sum( Qty) from @tmp a " +
                                " where a.grade not in ('Grade A', 'Grade B', 'Grade C', 'Grade D','Grade E', 'Grade F' ) GROUP BY a.InvoiceDate, a.InvoiceNo " +
                                " Select * from @tmp ";

                    var getCompany =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";

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

                    ReportDataSource rdc1 = new ReportDataSource("SalesStatementDataSet", dws);
                    ReportDataSource companyDc = new ReportDataSource("CompanyInfo", company);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportParameter parms = new ReportParameter();
                    var dateRange = "From : " + fromDate.ToString(dateFormat) + "     To: " + toDate.ToString(dateFormat);
                    parms = new ReportParameter("dateRange", dateRange);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("unitName", unitName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("typeName", typeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("dealerName", dealerName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    //language base data: show data from resource file
                    
                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("SN", utility.GetResourceValueById("ResourceRDLCDSaleStatementReport", "SN"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InvoiceDate", utility.GetResourceValueById("ResourceRDLCDSaleStatementReport", "InvoiceDate"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InvoiceNo", utility.GetResourceValueById("ResourceRDLCDSaleStatementReport", "InvoiceNo"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCDSaleStatementReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}