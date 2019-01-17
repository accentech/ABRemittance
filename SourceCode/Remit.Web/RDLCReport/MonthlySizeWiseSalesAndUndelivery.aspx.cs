using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Remit.Data.Models;
using Remit.Model.Models;
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class MonthlySizeWiseSalesAndUndelivery : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            long timeZoneOffset = UserSession.GetTimeZoneOffset();
            string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
            if (!IsPostBack)
            {
                using (var _context = new ApplicationEntities())
                {
                    DateTime selectedDate = new DateTime(0);
                    string selectedDatestring = string.Empty;
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();
                    System.Data.DataTable netSalesDatatable = new System.Data.DataTable();
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/MonthlySizeWiseSalesAndUndelivery.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    int year = 0;
                    int month = 1;
                    int unit = 0;
                    int selectedReason = 0;

                    if (Request.QueryString["Year"] != null)
                    {
                        year = Convert.ToInt32(Request.QueryString["Year"]);
                    }

                    if (Request.QueryString["Month"] != null)
                    {
                        month = Convert.ToInt32(Request.QueryString["Month"]);
                    }

                    if (Request.QueryString["saleUnit"] != null)
                    {
                        unit = Convert.ToInt32(Request.QueryString["saleUnit"]);
                    }

                    string monthName = new DateTime(year, month, 1).ToString("MMM-yyyy", CultureInfo.InvariantCulture);

                    
                    if (Request.QueryString["selectedReason"] != null)
                    {
                        selectedReason = Convert.ToInt32(Request.QueryString["selectedReason"]);
                    }
                    

                    var query =
                        "Select size.Size,grade.Grade,Qty,Particulars from FGSize as size, FGGrade as grade, ( " +
                        "( Select saleDet.FGSizeId,saleDet.FGGradeId,Sum(saleDet.QuantityInSFT) as Qty , 'Sales' as Particulars from FGSalesDetail as saleDet " +
                        " Join FGSales as sale on saleDet.FGSalesInvoiceNo = sale.InvoiceNo " +
                        " where Year(DATEADD(minute, " + timeZoneOffset + ", sale.InvoiceDate)) = " + year + " and Month(DATEADD(minute, " + timeZoneOffset + ", sale.InvoiceDate)) = "+ month +" and sale.IsDelete = 0 and sale.Reason = " + selectedReason + " and saleDet.SalesFGUnitId = " + unit +
                        " group by saleDet.FGSizeId,saleDet.FGGradeId ) " +

                        " Union All " + "( " +
                        " Select a.FGSizeId, a.FGGradeId, (IsNull(a.SalesSumQty,0) - IsNull(b.MonthlDeliverySumQty, 0)) as Qty, 'Un delivered (In " + monthName + ")'  as Particulars " +
                        " from ( " +
                        " Select saleDet.FGSizeId,saleDet.FGGradeId,Sum(saleDet.QuantityInSFT) as SalesSumQty from FGSalesDetail as saleDet " +
                        " Join FGSales as sale on saleDet.FGSalesInvoiceNo = sale.InvoiceNo " +
                        " where Year(DATEADD(minute, " + timeZoneOffset + ", sale.InvoiceDate)) = " + year + " and Month(DATEADD(minute, " + timeZoneOffset + ", sale.InvoiceDate)) = " + month + " and sale.IsDelete = 0 and sale.Reason = " + selectedReason + " and saleDet.SalesFGUnitId = " + unit +
                        " group by saleDet.FGSizeId,saleDet.FGGradeId ) as a " +
                        " left Join ( " +
                        " select " +
                        " delvDet3.FGSizeId,delvDet3.FGGradeId,Sum(delvDet3.DeliveryQuantityInSFT) as MonthlDeliverySumQty from FGSalesDeliveryDetail as delvDet3 " +
                        " join FGSalesDelivery as delv3 on delv3.DeliveryChallanNo = delvDet3.DeliveryChallanNo " +
                        " Join FGSales as saleChild3 on delv3.InvoiceNo = saleChild3.InvoiceNo " +
                        " where Year(DATEADD(minute, " + timeZoneOffset + ", delv3.DeliveryDate)) = " + year + " and Month(DATEADD(minute, " + timeZoneOffset + ", delv3.DeliveryDate)) = " + month + "  and delv3.IsDelete = 0 and saleChild3.IsDelete = 0 and Year(DATEADD(minute, " + timeZoneOffset + ", saleChild3.InvoiceDate)) = " + year + " and Month(DATEADD(minute, " + timeZoneOffset + ", saleChild3.InvoiceDate)) = "+ month +"  " +
                        " and saleChild3.Reason = " + selectedReason + " group by delvDet3.FGSizeId,delvDet3.FGGradeId ) as b on a.FGSizeId = b.FGSizeId and  a.FGGradeId = b.FGGradeId " +
                        " ) " +

                        " Union All " +
                        " ( Select delvDet2.FGSizeId,delvDet2.FGGradeId,Sum(delvDet2.DeliveryQuantityInSFT) as Qty, 'Invoice Previous Delivered (In " + monthName + ")' as Particulars from FGSalesDeliveryDetail as delvDet2 " +
                        " join FGSalesDelivery as delv2 on delv2.DeliveryChallanNo = delvDet2.DeliveryChallanNo " +
                        " Join FGSales as saleChild2 on delv2.InvoiceNo = saleChild2.InvoiceNo " +
                        " where Year(DATEADD(minute, " + timeZoneOffset + ", delv2.DeliveryDate)) = " + year + " and Month(DATEADD(minute, " + timeZoneOffset + ", delv2.DeliveryDate)) = "+ month +"  and (Year(DATEADD(minute, " + timeZoneOffset + ", saleChild2.InvoiceDate)) < " + year + " or ( Year(DATEADD(minute, " + timeZoneOffset + ", saleChild2.InvoiceDate)) = " + year + " and Month(DATEADD(minute, " + timeZoneOffset + ", saleChild2.InvoiceDate)) < "+ month +"  )) and delv2.IsDelete = 0 and " +
                        " saleChild2.IsDelete = 0 and saleChild2.Reason = " + selectedReason + " " +
                        " group by delvDet2.FGSizeId,delvDet2.FGGradeId " +
                        " )" +

                        " Union All" +
                        "( Select delvDet3.FGSizeId,delvDet3.FGGradeId,Sum(delvDet3.DeliveryQuantityInSFT) as Qty, 'Net Sales (In " + monthName + ")' as Particulars from FGSalesDeliveryDetail as delvDet3 " +
                        " join FGSalesDelivery as delv3 on delv3.DeliveryChallanNo = delvDet3.DeliveryChallanNo " +
                        " Join FGSales as saleChild3 on delv3.InvoiceNo = saleChild3.InvoiceNo " +
                        " where Year(DATEADD(minute, " + timeZoneOffset + ", delv3.DeliveryDate)) = " + year + " and Month(DATEADD(minute, " + timeZoneOffset + ", delv3.DeliveryDate)) = "+ month +"  and delv3.IsDelete = 0 and" +
                        " saleChild3.IsDelete = 0 and saleChild3.Reason = " + selectedReason + " group by delvDet3.FGSizeId,delvDet3.FGGradeId )" +
                        " ) as x where x.FGSizeId = size.Id and x.FGGradeId = grade.Id";

                    

                    var queryCom =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";


                    var netSales = "Select FGGrade.Grade as GradeNew,Sum(delvDet3.DeliveryQuantityInSFT) as Qty, 'Net Sales In " + monthName + ")' as Particulars from FGSalesDeliveryDetail as delvDet3 " + 
                      "  join FGSalesDelivery as delv3 on delv3.DeliveryChallanNo = delvDet3.DeliveryChallanNo"+
                      " Join FGSales as saleChild3 on delv3.InvoiceNo = saleChild3.InvoiceNo" +
                     " Join FGGrade ON FGGrade.Id=delvDet3.FGGradeId" +
                     "  where Year(DATEADD(minute,  " + timeZoneOffset + ", delv3.DeliveryDate)) =" + year + "  " +
                    " and Month(DATEADD(minute,  " + timeZoneOffset + ", delv3.DeliveryDate)) = " + month + "  and delv3.IsDelete = 0 and saleChild3.IsDelete = 0 and saleChild3.Reason =" + selectedReason + " " +
                    " group by FGGrade.Grade";



                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlCommand cmdComp = new SqlCommand(queryCom, con);
                    SqlCommand cmdNetSales = new SqlCommand(netSales, con);
                    SqlDataAdapter sda = new SqlDataAdapter();

                    using (sda)
                    {
                        sda.SelectCommand = cmd;
                        sda.Fill(dtItem);

                        sda.SelectCommand = cmdComp;
                        sda.Fill(company);

                        sda.SelectCommand = cmdNetSales;
                        sda.Fill(netSalesDatatable);
                    }
                    con.Close();

                    ReportDataSource companyDc = new ReportDataSource("CompanyTable", company);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportDataSource rdc1 = new ReportDataSource("MonthlySizeWiseSalesAndUndelivery", dtItem);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);

                    ReportDataSource rdc2 = new ReportDataSource("netSalesDataTable", netSalesDatatable);
                    ReportViewer1.LocalReport.DataSources.Add(rdc2);

                    ReportParameter parms = new ReportParameter();
                    var titleString = "Monthly Size Wise Sales and Undelivery Report";
                    parms = new ReportParameter("Title", titleString);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ReportMonthName", monthName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    var dateText = "Sales Report Month of " + selectedDatestring;
                    parms = new ReportParameter("subTitle", dateText);
                    this.ReportViewer1.LocalReport.SetParameters(parms);



                    //language base data: show data from resource file
                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("SL", utility.GetResourceValueById("ResourceRDLCMonthlySizeWiseSalesUndeliveryReport", "SL"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Size", utility.GetResourceValueById("ResourceRDLCMonthlySizeWiseSalesUndeliveryReport", "Size"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Particulars", utility.GetResourceValueById("ResourceRDLCMonthlySizeWiseSalesUndeliveryReport", "Particulars"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCMonthlySizeWiseSalesUndeliveryReport", "Total"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}