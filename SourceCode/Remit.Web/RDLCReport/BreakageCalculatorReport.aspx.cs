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
    public partial class BreakageCalculatorReport : System.Web.UI.Page
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
               // string grade = string.Empty;
                string year = string.Empty;
                string periodFrom = string.Empty;
                string periodTo = string.Empty;
               // int periodtype = 0;
               // string periodName = "MAY";
                string periodYear=String.Empty;
                int type = 3;


               

                if (Request.QueryString["year"] != null)
                {
                  
                    periodYear = Request.QueryString["year"];
                }



                //if (Request.QueryString["type"] != null)
                //{
                //     type = Convert.ToInt32(Request.QueryString["type"]);
                    
                //}



                var periodName = string.Empty;
                //var type = Convert.ToInt32(Request.QueryString["type"]);
                var period = Convert.ToInt32(Request.QueryString["periodTo"]);
               
                 if (type == 3)
                {
                    if (period == 6)
                        periodName = Resources.ResourceCommissionCalculator.JanToJun;
                    if (period == 12)
                        periodName = Resources.ResourceCommissionCalculator.JulyToDec;
                }
               


                
                    using (var _context = new ApplicationEntities())
                    {

                        System.Data.DataTable breakageCalculator = new System.Data.DataTable();
                        System.Data.DataTable company = new System.Data.DataTable();
                        ReportViewer1.LocalReport.ReportPath =
                            Server.MapPath("~/RDLCReport/BreakageCalculatorReport.rdlc");
                        ReportViewer1.LocalReport.DataSources.Clear();

                        
                        var query =
                            "SELECT BreakageCalculator.InvoiceAmount as InvoiceAmount,BreakageCalculator.BreakageRate as BreakageRate, BreakageCalculator.BreakageAmount as BreakageAmount," +
                            "FGDealer.Name as DealerName  FROM BreakageCalculator " +
                             "JOIN FGDealer on  BreakageCalculator.DealerId = FGDealer.Id " +
                             "WHERE BreakageCalculator.BreakageAmount!=0 AND BreakageCalculator.PeriodType=" + type + " AND YEAR= '" + periodYear +
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
                                sda.Fill(breakageCalculator);

                                cmdComp.Connection = con;
                                sda.SelectCommand = cmdComp;
                                sda.Fill(company);
                            }
                        }



                        ReportDataSource rdc1 = new ReportDataSource("BreakageCalculator", breakageCalculator);
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

                        parms = new ReportParameter("SL", utility.GetResourceValueById("ResourceRDLCBreakageCalculatorReport", "SL"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("DealerName", utility.GetResourceValueById("ResourceRDLCBreakageCalculatorReport", "DealerName"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("InvoiceAmount", utility.GetResourceValueById("ResourceRDLCBreakageCalculatorReport", "InvoiceAmount"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("BreakageRate", utility.GetResourceValueById("ResourceRDLCBreakageCalculatorReport", "BreakageRate"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("BreakageAmount", utility.GetResourceValueById("ResourceRDLCBreakageCalculatorReport", "BreakageAmount"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                        parms = new ReportParameter("Total", utility.GetResourceValueById("ResourceRDLCBreakageCalculatorReport", "Total"));
                        this.ReportViewer1.LocalReport.SetParameters(parms);

                       
                        ReportViewer1.LocalReport.Refresh();
                    }
                }
            }
        }
    }
