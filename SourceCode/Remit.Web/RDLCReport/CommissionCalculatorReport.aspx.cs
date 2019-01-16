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
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class CommissionCalculatorReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            long timeZoneOffset = UserSession.GetTimeZoneOffset();

            if (!IsPostBack)
            {


                DateTime fromDate = new DateTime(0);
                string fromDatestring = string.Empty;
                DateTime toDate = new DateTime(0);
                string toDatestring = string.Empty;


                string c = string.Empty;
                string typefield = string.Empty;
                string grade = string.Empty;
                string year = string.Empty;
                string periodFrom = string.Empty;
                string periodTo = string.Empty;
               // int periodtype = 0;
               // string periodName = "MAY";
                string periodYear=String.Empty;
                int type = 0;


                if (Request.QueryString["grade"] != null)
                {
                    grade = " and FGSalesDetail.FGGradeId=" + Request.QueryString["grade"];
                }


                if (Request.QueryString["year"] != null)
                {
                   //year = "  Year(DATEADD(minute, " + timeZoneOffset + ", FGSales.InvoiceDate))=" +
                   //        Request.QueryString["year"];
                    periodYear = Request.QueryString["year"];
                }



                if (Request.QueryString["type"] != null)
                {
                     type = Convert.ToInt32(Request.QueryString["type"]);
                    
                }



                var periodName = string.Empty;
                //var type = Convert.ToInt32(Request.QueryString["type"]);
                var period = Convert.ToInt32(Request.QueryString["periodTo"]);
                if (type == 1)
                {
                    periodName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(period);
                }
                else if (type == 2)
                {
                    if (period == 3)
                        periodName = Resources.ResourceCommissionCalculator.JanToMar;
                    if (period == 6)
                        periodName = Resources.ResourceCommissionCalculator.AprToJun;
                    if (period == 9)
                        periodName = Resources.ResourceCommissionCalculator.JulToSep;
                    if (period == 12)
                        periodName = Resources.ResourceCommissionCalculator.OctToDec;
                }
                else if (type == 3)
                {
                    if (period == 6)
                        periodName = Resources.ResourceCommissionCalculator.JanToJun;
                    if (period == 12)
                        periodName = Resources.ResourceCommissionCalculator.JulyToDec;
                }
                else if (type == 4) { periodName = Resources.ResourceCommissionCalculator.JanToDec; }






                    //if (Request.QueryString["type"] == "1")
                    //{

                    //    if (Request.QueryString["periodFrom"] != null)
                    //    {
                    //        periodFrom = Request.QueryString["periodFrom"];

                    //        c = " and Month(DATEADD(minute, " + timeZoneOffset + ", FGSales.InvoiceDate))=" +
                    //            periodFrom;

                    //    }

                    //    typefield = "b.MonthlyCommission";

                    //}
                    //else if (Request.QueryString["type"] == "2" || Request.QueryString["type"] == "3")
                    //{
                    //    if (Request.QueryString["periodFrom"] != null)
                    //    {
                    //        periodFrom = Request.QueryString["periodFrom"];
                    //    }

                    //    if (Request.QueryString["periodTo"] != null)
                    //    {
                    //        periodTo = Request.QueryString["periodTo"];
                    //    }

                    //    c = " and Month(DATEADD(minute, " + timeZoneOffset + ", FGSales.InvoiceDate))>=" + periodFrom +
                    //        " and Month(DATEADD(minute, " + timeZoneOffset + ", FGSales.InvoiceDate))<=" + periodTo;

                    //    if (Request.QueryString["type"] == "2")
                    //    {
                    //        typefield = "b.QuarterlyCommission";
                    //    }
                    //    else
                    //    {
                    //        typefield = "b.HalfYearlyCommission";
                    //    }

                    //}
                    //else
                    //{
                    //    c = " ";
                    //    typefield = "b.YearlyCommission";
                    //}







                    using (var _context = new ApplicationEntities())
                    {

                        System.Data.DataTable commissionCalculator = new System.Data.DataTable();
                        System.Data.DataTable company = new System.Data.DataTable();
                        ReportViewer1.LocalReport.ReportPath =
                            Server.MapPath("~/RDLCReport/CommissionCalculatorReport.rdlc");
                        ReportViewer1.LocalReport.DataSources.Clear();

                        //var query =
                        //    "select Division.Name as Division,FGDealerZone.ZoneName,FGDealer.Name as DealerName,S.Quantity,S.Commission  FROM FGDealer" +
                        //    " join FGDealerZone ON FGDealer.DealersZoneId=FGDealerZone.Id " +
                        //    "join Division ON FGDealerZone.DivisionId=Division.Id" +
                        //    " left join (  select a.DealerId, a.Quantity, max("+typefield+") as Commission from " +
                        //    " ( select DealerId, Sum(SalesQuantity) as Quantity from FGSales " +
                        //    " Join  FGSalesDetail ON FGSales.InvoiceNo=FGSalesDetail.FGSalesInvoiceNo "+ grade  +
                        //    " where FGSales.CustomerType=1 and FGSales.IsDelete != 1 and " + year + c +
                        //    " group by DealerId  ) a   " +
                        //    " left join DealerCommisionDetails b ON a.DealerId = b.DealerId and a.Quantity > b.MonthlyTarget    group by a.DealerId, a.Quantity  " +
                        //    " ) S on S.DealerId = FGDealer.Id";

                        var query =
                            "SELECT CommissionCalculator.SFTSale as Quantity,CommissionCalculator.SftRate as Commission, CommissionCalculator.Total as Total,CommissionCalculator.MonthlyTarget as MonthlyTarget , " +
                            "Division.Name as Division,FGDealerZone.ZoneName,FGDealer.Name as DealerName  FROM CommissionCalculator " +
                            "JOIN FGDealerZone ON CommissionCalculator.ZoneId= FGDealerZone.Id " +
                            "JOIN FGDealer on  CommissionCalculator.DealerId = FGDealer.Id " +
                            "JOIN Division ON FGDealerZone.DivisionId=Division.Id " +
                            "WHERE  CommissionCalculator.Total!=0 and CommissionCalculator.PeriodType=" + type + " AND YEAR= '" + periodYear +
                            "' AND UPPER(PeriodName) ='" + periodName.ToUpper() + "'";


                      
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
                                sda.Fill(commissionCalculator);

                                cmdComp.Connection = con;
                                sda.SelectCommand = cmdComp;
                                sda.Fill(company);
                            }
                        }



                        ReportDataSource rdc1 = new ReportDataSource("CommissionCalculator", commissionCalculator);
                        ReportDataSource companyDc = new ReportDataSource("CompanyDataSet", company);
                        ReportViewer1.LocalReport.DataSources.Add(rdc1);
                        ReportViewer1.LocalReport.DataSources.Add(companyDc);

                        ReportParameter parms = new ReportParameter();
                        parms = new ReportParameter("periodName", periodName);
                        this.ReportViewer1.LocalReport.SetParameters(parms);


                        parms = new ReportParameter("periodYear", periodYear);
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        //language base data: show data from resource file
                        ReportUtility utility = new ReportUtility();

                        parms = new ReportParameter("SL", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "SL"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("DealerName", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "DealerName"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("Location", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "Location"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("MonthlyTarget", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "MonthlyTarget"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("PurchaseQty", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "PurchaseQty"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("Commission", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "Commission"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("ComAmount", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "ComAmount"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCCommissionCalculatorReport", "Total"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        ReportViewer1.LocalReport.Refresh();
                    }
                }
            }
        }
    }
