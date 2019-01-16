using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Office.Interop.Excel;
using Microsoft.Reporting.WebForms;
using Remit.Data.Models;
using Remit.Model.Models;
using Remit.Web.RDLCReportDataset;
using Remit.Web.Helpers;
using System.Web.Configuration;

namespace Remit.Web.RDLCReport
{
    public partial class DealerWiseSalesStatement : System.Web.UI.Page
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
                    DateTime dailyDate = new DateTime(0);
                    string dailyDatestring = string.Empty;
                    DateTime fromDate = new DateTime(0);
                    string fromDatestring = string.Empty;
                    DateTime toDate = new DateTime(0);
                    string toDatestring = string.Empty;
                    int salesUnit = 0;
                    int productType = 0;
                    string unitName = string.Empty;
                    string typeName = string.Empty;
                    string typefilter = string.Empty;
                    int reason = 0;
                    int selectedDateType = 0;
                    string dateTypeName = "";
                    #endregion

                    #region parameter Checking

                    if (Request.QueryString["reason"] != null)
                    {
                        reason = Convert.ToInt32(Request.QueryString["reason"]);
                    }

                    if (Request.QueryString["selectedDateType"] != null)
                    {
                        selectedDateType = Convert.ToInt32(Request.QueryString["selectedDateType"]);
                    }

                    if (Request.QueryString["dailyDate"] != null)
                    {
                        dailyDate = DateTime.Parse(Request.QueryString["dailyDate"]);
                        dailyDatestring = dailyDate.AddMinutes(-timeZoneOffset).AddHours(24).ToString();
                    }

                    if (selectedDateType == 2)
                    {
                        dateTypeName = "DateRange";
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
                    }
                    else
                    {
                        dateTypeName = "Daily";
                        fromDate = dailyDate; toDate = dailyDate;
                        fromDatestring = dailyDate.AddMinutes(-timeZoneOffset).ToString();
                        toDatestring = dailyDatestring;
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
                            typefilter = "and FGItem.TypeId = " + productType;
                        }
                        else
                        {
                            typefilter = " ";
                        }
                    }

                    var unitObj = _context.FGUOMs.FirstOrDefault(a => a.Id == salesUnit);
                    if (unitObj != null)
                    {
                        unitName = unitObj.UnitName;
                    }
                    else
                    {
                        unitName = "SFT";
                    }

                    var typeObj = _context.FGTypes.FirstOrDefault(a => a.Id == productType);
                    if (typeObj != null)
                    {
                        typeName = typeObj.TypeName;
                    }
                    else
                    {
                        typeName = "ALL";
                    }


                    #endregion

                    System.Data.DataTable dws = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();
                    //System.Data.DataTable paymentInfo = new System.Data.DataTable();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/DealerWiseSales.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    var query =
                        " DECLARE @checkval AS int " +
                        " set @checkval = " + salesUnit +
                        " declare @tmp table(Name varchar(max), Grade varchar(max), qty float ); " +
                        " INSERT INTO @tmp " +
                        " SELECT Dimension.Name, " +
                        " case when Dimension.Grade in ('A') then 'Grade A' " +
                        " when Dimension.Grade in ('B') then 'Grade B' " +
                        " when Dimension.Grade in ('C') then 'Grade C' " +
                        " when Dimension.Grade in ('D') then 'Grade D' " +
                        " when Dimension.Grade in ('E') then 'Grade E' " +
                        " when Dimension.Grade in ('F') then 'Grade F' " +
                        " else 'Grade '+ Dimension.Grade end as Grade, " +
                        " isNull(Sales.Qty, 0)" +
                        " from " +
                        " (Select FGDealer.Id as DealerId, FGDealer.Name, FGGrade.Id as GradeId, FGGrade.Grade FROM" +
                        " FGDealer, FGGrade" +
                        //" where FGDealer.IsActive = 1" +
                        " ) as Dimension left join( select FGSales.DealerId, FGSalesDetail.FGGradeId as GradeId, " +
                        " SUM( case " +
                        " when @checkval in (1) then FGSalesDetail.QuantityInCTN " +
                        " when @checkval in (2) then FGSalesDetail.QuantityInPCs " +
                        " when @checkval in (3) then FGSalesDetail.QuantityInSFT " +
                        " when @checkval in (4) then FGSalesDetail.QuantityInSMT " +
                        " else 0 end" +
                        " ) as Qty from FGSales left join FGSalesDetail ON FGSales.InvoiceNo = FGSalesDetail.FGSalesInvoiceNo " +
                        " left join  FGItem on FGSalesDetail.FGItemId = FGItem.Id " +
                        " where FGSales.IsDelete != 1 " +
                        " and FGSales.Reason = " + reason +
                        " and FGSales.InvoiceDate >= '" + fromDatestring +
                        "' and FGSales.InvoiceDate <  '" + toDatestring + "' " + typefilter +
                        " group by FGSales.DealerId, FGSalesDetail.FGGradeId " +
                        ") as Sales ON Dimension.DealerId = Sales.DealerId and Dimension.GradeId = Sales.GradeId " +


                        " union all " +
                        " select a.Name, a.payMode, sum(a.PaymentAmount) as PaymentAmount from ( SELECT FGDealer.Name, " +
                        " case  " +
                        " when FGSalesPayment.PaymentMode IN (1, 4) then 'Cash Pay.' " +
                        " when FGSalesPayment.PaymentMode IN (2, 3) then 'Cheq. Pay' " +
                        " else 'others' " +
                        " End as payMode," +
                        " FGSalesPayment.PaymentAmount FROM FGDealer left join  FGSales on FGDealer.Id = FGSales.DealerId " +
                        " INNER JOIN FGSalesPayment on FGSales.InvoiceNo = FGSalesPayment.FGSalesInvoiceNo " +
                        " where FGSales.IsDelete != 1 and FGSales.Reason = " + reason +
                        //" and FGDealer.IsActive = 1 " + 
                        " and FGSales.InvoiceDate >= '" +
                        fromDatestring + "' and FGSales.InvoiceDate < '" + toDatestring +
                        "' ) as a group by  a.Name, a.payMode " +

                        " union all " +
                        " Select FGDealer.Name, 'Due/Adv.' as grade, sum(FGSales.TotalDueAdvancedAmount) from FGDealer left join FGSales on FGDealer.Id = FGSales.DealerId" +
                        " where FGSales.IsDelete != 1 and FGSales.IsDueOrAdvanceAdjusted != 1 " +
                        //" and FGSales.Reason = " + reason +
                        //" and FGDealer.IsActive = 1 " + 
                        //" and FGSales.InvoiceDate >= '" +
                        // fromDatestring + "' " +
                        " and FGSales.InvoiceDate < '" + toDatestring + "'" +
                        " group by FGDealer.Name " +

                        " union all " +
                        " Select isNull(one.Name,two.name),'Net Total.' as grade, isNull(one.PaymentAmount,0) + isNull(two.amount,0) as tAm  from (" +
                        " Select a.Name, sum(isNull(a.PaymentAmount,0)) as PaymentAmount from ( SELECT FGDealer.Name, case  " +
                        " when FGSalesPayment.PaymentMode IN (1,2,3,4) then 'Payment.' End as payMode, " +
                        " FGSalesPayment.PaymentAmount FROM FGDealer left join  FGSales on FGDealer.Id = FGSales.DealerId " +
                        " INNER JOIN FGSalesPayment on FGSales.InvoiceNo = FGSalesPayment.FGSalesInvoiceNo " +
                        " where FGSales.IsDelete != 1 and FGSales.Reason = " + reason +
                        //" and FGDealer.IsActive = 1 " + 
                        " and FGSales.InvoiceDate >= '" +
                        fromDatestring + "' and FGSales.InvoiceDate < '" + toDatestring + "' ) as a group by  a.Name" +
                        ") as one Full outer join (" +
                        " Select FGDealer.Name, 'Due/Adv.' as grade, sum(isNull(FGSales.TotalDueAdvancedAmount,0)) as amount from FGDealer left join FGSales on FGDealer.Id = FGSales.DealerId " +
                        " where FGSales.IsDelete != 1 and FGSales.IsDueOrAdvanceAdjusted != 1 " +
                        //" and FGSales.Reason = " + reason + 
                        //" and FGDealer.IsActive = 1 " + 
                        //" and FGSales.InvoiceDate >= '" + fromDatestring + 
                        //"' and" +
                        " and FGSales.InvoiceDate < '" + toDatestring + "' group by FGDealer.Name " +
                        ") as two on one.Name = two.Name " +

                        " union all " +
                        " Select FGDealer.Name, b.AdjustmnetName, sum( b.AdjustmentAmount) as amount from FGDealer " +
                        " left join( Select FGAdjustmentSetup.AdjustmnetName, FGSalesAdjustment.AdjustmentAmount, FGSalesAdjustment.FGSalesInvoiceNo, FGSales.DealerId " +
                        " From FGAdjustmentSetup left join  FGSalesAdjustment on FGAdjustmentSetup.Id = FGSalesAdjustment.FGAdjustmentSetupId left join FGSales On FGSales.InvoiceNo = FGSalesAdjustment.FGSalesInvoiceNo and FGSales.IsDelete != 1 " +
                        " and FGSales.Reason = " + reason + " and FGSales.InvoiceDate >= '" + fromDatestring +
                        "' and FGSales.InvoiceDate < '" + toDatestring + "' ) " +
                        " as b on FGDealer.Id = b.DealerId " +
                        //" where FGDealer.IsActive = 1 " + 
                        " group by FGDealer.Name, b.AdjustmnetName " +

                        //" union all " +
                        //" Select FGDealer.Name, 'Discount' as grade, 0.0 as value from FGDealer " +
                        //" where FGDealer.IsActive = 1 " +

                        " INSERT INTO @tmp " +
                        " SELECT a.Name,'Total', Sum(Qty) from @tmp a " +
                        " where grade not in ('Grade A', 'Grade B', 'Grade C', 'Grade D','Grade E', 'Grade F', 'Net Total.', 'Grade B1' )" +
                        " GROUP BY a.Name " +
                        " Select aa.* from @tmp as aa ";
                    if (selectedDateType == 1)
                    {
                        query += " Join ( Select aa.Name from @tmp as aa where (aa.Grade= \'Grade A\' and aa.Qty != 0) or (aa.Grade= \'Grade B\' and aa.Qty != 0)" +
                                 " or (aa.Grade= \'Grade C\' and aa.Qty != 0) or (aa.Grade= \'Grade D\' and aa.Qty != 0) or (aa.Grade= \'Grade E\' and aa.Qty != 0)" +
                                 " or (aa.Grade= \'Grade F\' and aa.Qty != 0) or (aa.Grade= \'Grade B1\' and aa.Qty != 0)) as bb on aa.Name = bb.Name";
                    }
                    else { }
                    query += " order by " +
                             " case when aa.Name='Other' then 1 else 0 end, " +
                             " case when aa.Grade= 'Grade A' then aa.qty end desc," +
                             " case when aa.Grade= 'Total' then 1 else 0 end";


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

                    ReportDataSource rdc1 = new ReportDataSource("DWSDataSet", dws);
                    ReportDataSource companyDc = new ReportDataSource("ConpanyInfo", company);
                    //ReportDataSource payment = new ReportDataSource("DWSPaymentModeDataSet", paymentInfo);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);
                    //ReportViewer1.LocalReport.DataSources.Add(payment);

                    ReportParameter parms = new ReportParameter();
                    var dateRange = "From : " + fromDate.ToString(dateFormat) + "     To: " + toDate.ToString(dateFormat);
                    parms = new ReportParameter("dateRange", dateRange);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("unitName", unitName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("typeName", typeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("dateTypeName", dateTypeName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}