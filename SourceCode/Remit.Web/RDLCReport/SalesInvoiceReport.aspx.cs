using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Remit.Data.Models;
using Remit.Web.Helpers;

namespace Remit.Web.RDLCReport
{
    public partial class SalesInvoiceReport : System.Web.UI.Page
    {       
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                long timeZoneOffset = UserSession.GetTimeZoneOffset();
                var PhotoPath = string.Empty;
                var FullName = string.Empty;
                var PhotoPath2 = string.Empty;
                var FullName2 = string.Empty;
                var PhotoPath3 = string.Empty;
                var FullName3 = string.Empty;
               
               

                var invoiceNum = string.Empty;
                if (Request.QueryString["invoiceNo"] != null)
                {
                    invoiceNum = Request.QueryString["invoiceNo"].ToString();
                }


                var delvOption = string.Empty;
                if (Request.QueryString["deliverOptionType"] != null)
                {
                    delvOption = Request.QueryString["deliverOptionType"];
                    if (delvOption == "1")
                    {
                        delvOption = "DELIVERY BY COMPANY";
                    } if (delvOption == "2")
                    {
                        delvOption = "SELF DELIVERY";
                    }
                }

                using (var _context = new ApplicationEntities())
                {
                    System.Data.DataTable salseInvoice = new System.Data.DataTable();
                    System.Data.DataTable adjustment = new System.Data.DataTable();
                    System.Data.DataTable modeOfPayment = new System.Data.DataTable();
                    System.Data.DataTable payment = new System.Data.DataTable();
                    System.Data.DataTable invoiceNolist = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/SalesInvoice.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    var siQuery =
                        "SELECT FGItem.Code, FGSize.Size, FGGrade.Grade, FGSalesDetail.Lot, FGSalesDetail.PackQuantity, FGSalesDetail.UnitPrice, FGSalesDetail.Amount, FGSalesDetail.QuantityInCTN, FGSalesDetail.QuantityInPCs, FGSalesDetail.QuantityInSFT, FGItem.PackUnitId, FGItem.SalesUnitId, FGItem.PcsPerCartoon, FGItem.SftPerPiece, FGSales.Name ,FGSales.InvoiceNo, DATEADD(minute,  " + timeZoneOffset +" , FGSales.InvoiceDate) as InvoiceDate, FGSales.ContactPersonName, FGSales.ContactPhone, FGSales.Address, FGSales.DefaultDeliverySite, FGSales.DeliverZoneId, FGSales.TotalAmount, FGSales.TotalDueOrAdvanceAdjustment, FGSales.TotalAdjustment, FGSales.TotalPriceAfterAdjustment, FGSales.TotalPaidAmount, FGSales.TotalSFT, FGSales.TotalCTN, FGSalesDetail.DiscountPerUnit, FGSalesDetail.UnitRateAfterDiscount, FGSales.DiscountPercentage, FGSales.DiscountAmount, FGSales.Reason " +
                        "FROM FGSales " +
                        "INNER JOIN FGSalesDetail ON FGSales.InvoiceNo = FGSalesDetail.FGSalesInvoiceNo " +
                        "INNER JOIN FGItem ON FGSalesDetail.FGItemId = FGItem.Id " +
                        "INNER JOIN FGGrade ON FGSalesDetail.FGGradeId = FGGrade.Id " +
                        "INNER JOIN FGSize ON FGSalesDetail.FGSizeId = FGSize.Id AND FGItem.SizeId = FGSize.Id " +
                        "WHERE FGSales.InvoiceNo = '" + invoiceNum + "' Order By FGItem.Code"
                        ;

                    //var adjQuery =
                    //    "SELECT FGAdjustmentSetup.AdjustmnetName, FGAdjustmentSetup.DefaultValue, fgAdj.AdjustmentAmount,FGAdjustmentSetup.Id, fgAdj.FGSalesInvoiceNo, fgAdj.Percentage, fgAdj.Note FROM (select* from FGSalesAdjustment where FGSalesAdjustment.FGSalesInvoiceNo = '" +
                    //    invoiceNum + "') as fgAdj RIGHT OUTER JOIN FGAdjustmentSetup ON fgAdj.FGAdjustmentSetupId = FGAdjustmentSetup.Id";



                    var adjQuery =
                        "SELECT FGAdjustmentSetup.AdjustmnetName, FGAdjustmentSetup.DefaultValue, fgAdj.AdjustmentAmount,FGAdjustmentSetup.Id, fgAdj.FGSalesInvoiceNo, fgAdj.Percentage, fgAdj.Note,CommissionCalculator.PeriodName,CommissionCalculator.Year,BreakageCalculator.PeriodName as BreakagePeriodName  FROM (select* from FGSalesAdjustment where FGSalesAdjustment.FGSalesInvoiceNo = '" +
                        invoiceNum + "') as fgAdj RIGHT OUTER JOIN FGAdjustmentSetup ON fgAdj.FGAdjustmentSetupId = FGAdjustmentSetup.Id  left JOIN CommissionCalculator on fgAdj.CommissionCalculatorId=CommissionCalculator.Id  left JOIN BreakageCalculator on fgAdj.BreakageCalculatorId=BreakageCalculator.Id";



                    //var mopQuery =
                    //    " SELECT " +
                    //    " case " +
                    //    " when PaymentMode in (1) then 'Cash' " +
                    //    " when PaymentMode in (2) then 'CD' " +
                    //    " when PaymentMode in (3) then 'PDC' " +
                    //    " else 'online' " +
                    //    " End as BankName, " +
                    //    " FGSalesInvoiceNo,  DATEADD(minute,  " + timeZoneOffset +" , PaymentDate) as PaymentDate, PaymentAmount FROM FGSalesPayment " +
                    //    " where FGSalesInvoiceNo = '" + invoiceNum + "'" +
                    //    " union " +
                    //    " SELECT Bank.Name as BankName, sp.FGSalesInvoiceNo, DATEADD(minute,  " + timeZoneOffset +" , sp.PaymentDate) as PaymentDate, sp.PaymentAmount " +
                    //    " FROM Bank left join FGSalesPayment as sp on Bank.Id = sp.BankId " +
                    //    " where sp.FGSalesInvoiceNo = '" + invoiceNum + "'";


                    var mopQuery =
                        " SELECT  "+
                            " case "+
                            " when PaymentMode in (1) then 'Cash' "+
                            " when PaymentMode in (2) then 'CD' "+
                            " when PaymentMode in (3) then 'PDC' "+
                            " else 'Online' End as BankName, "+
                            " FGSalesInvoiceNo," +
                            " DATEADD(minute,  " + timeZoneOffset + " , PaymentDate) as PaymentDate, " +
                            " PaymentAmount, "+
                            " ' ( ' + Bank.Name + ' )' as Name, " +
                            " ChequeNo "+
                    " FROM FGSalesPayment left join Bank on Bank.Id = FGSalesPayment.BankId " +
                        " where FGSalesInvoiceNo = '" + invoiceNum + "'";

                    var paymentCashQuery =
                        "SELECT PaymentMode, FGSalesInvoiceNo, PaymentDate, PaymentAmount FROM FGSalesPayment where FGSalesInvoiceNo = '" +
                        invoiceNum + "' and PaymentMode = 1";

                    var previousInvoiceList =
                        "DECLARE @listStr VARCHAR(MAX) SELECT @listStr = COALESCE(@listStr + ', ', '') + InvoiceNo " +
                        "FROM FGSalesDueAdvancedAdjustment " +
                        "WHERE FGSalesInvoiceNo = '" +
                        invoiceNum + "' " +
                        "SELECT @listStr as InvoiceList";

                    var getCompany =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";



                    //var createdBy = "Select Employee.PhotoPath FROM Employee LEFT JOIN FGSales  ON Employee.Id=FGSales.CreatedBy " +
                    //                "LEFT JOIN FGSales  ON Employee.Id=FGSales.ReviewedBy" +
                    //                "LEFT JOIN FGSales  ON Employee.Id=FGSales.ApprovedBy" +
                    //                " where FGSales.InvoiceNo='" + invoiceNum + "' ";


                    var createdBy = "Select Employee.PhotoPath,Employee.FullName FROM Employee JOIN FGSales  ON Employee.Id=FGSales.CreatedBy where FGSales.InvoiceNo='" + invoiceNum + "' ";

                    var authorisedBy = "Select Employee.PhotoPath,Employee.FullName FROM Employee JOIN FGSales  ON Employee.Id=FGSales.ReviewedBy where FGSales.InvoiceNo='" + invoiceNum + "' ";
                    var accounts = "Select Employee.PhotoPath,Employee.FullName FROM Employee JOIN FGSales  ON Employee.Id=FGSales.ApprovedBy where FGSales.InvoiceNo='" + invoiceNum + "' ";


                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

                    SqlCommand siCmd = new SqlCommand(siQuery);
                    SqlCommand cadjCmd = new SqlCommand(adjQuery);
                    SqlCommand mopCmd = new SqlCommand(mopQuery);
                    SqlCommand paymentCmd = new SqlCommand(paymentCashQuery);
                    SqlCommand invoiceListCmd = new SqlCommand(previousInvoiceList);
                    SqlCommand cmdComp = new SqlCommand(getCompany);
                    SqlCommand cmdCreatedBy = new SqlCommand(createdBy);
                   // SqlCommand cmdauthorisedBy = new SqlCommand(authorisedBy);
                   

                    SqlDataAdapter sda = new SqlDataAdapter();


                    SqlConnection conTst = new SqlConnection(conString);
                    SqlCommand command = new SqlCommand();
                   
                    command.CommandText = createdBy;
                    
                  //  command.CommandText = accounts;
                    command.Connection = conTst;
                    conTst.Open();
                   
                    using (SqlDataReader rdr = command.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                             PhotoPath = rdr["PhotoPath"].ToString();
                            FullName = rdr["FullName"].ToString();
                           
                        }
                    }


                    SqlConnection conTst2 = new SqlConnection(conString);
                    SqlCommand command2 = new SqlCommand();
                    command2.Connection = conTst2;
                    command2.CommandText = authorisedBy;
                    conTst2.Open();
                    using (SqlDataReader rdr = command2.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            PhotoPath2 = rdr["PhotoPath"].ToString();
                            FullName2 = rdr["FullName"].ToString();

                        }
                    }

                    SqlConnection conTst3 = new SqlConnection(conString);
                    SqlCommand command3 = new SqlCommand();
                    command3.Connection = conTst3;
                    command3.CommandText = accounts;
                    conTst3.Open();
                    using (SqlDataReader rdr = command3.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            PhotoPath3 = rdr["PhotoPath"].ToString();
                            FullName3 = rdr["FullName"].ToString();

                        }
                    }

                    conTst.Close();
                    conTst2.Close();
                    conTst3.Close();
                    
                    
                   


                    using (SqlConnection con = new SqlConnection(conString))
                    {
                        using (sda)
                        {
                            siCmd.Connection = con;
                            sda.SelectCommand = siCmd;
                            sda.Fill(salseInvoice);

                            cadjCmd.Connection = con;
                            sda.SelectCommand = cadjCmd;
                            sda.Fill(adjustment);

                            mopCmd.Connection = con;
                            sda.SelectCommand = mopCmd;
                            sda.Fill(modeOfPayment);

                            paymentCmd.Connection = con;
                            sda.SelectCommand = paymentCmd;
                            sda.Fill(payment);

                            invoiceListCmd.Connection = con;
                            sda.SelectCommand = invoiceListCmd;
                            sda.Fill(invoiceNolist);

                            cmdComp.Connection = con;
                            sda.SelectCommand = cmdComp;
                            sda.Fill(company);
                        }
                    }

                    double itemQuantityTotal = 0.0;
                    string takainword= string.Empty;

                    if (salseInvoice.Rows.Count > 0)
                    {
                        if (salseInvoice.Rows[0]["TotalPaidAmount"] != System.DBNull.Value)
                        {
                            itemQuantityTotal += Convert.ToInt32(salseInvoice.Rows[0]["TotalPaidAmount"]);
                        }
                    }
                                        
                    takainword = ReportUtility.ConvertToWords(itemQuantityTotal.ToString());
                    takainword = takainword.Trim();

                    if (string.IsNullOrEmpty(takainword))
                    {
                        takainword = "ZERO ONLY";                       
                    }
                    else
                    {
                        takainword += " ONLY";
                    }                    

                    var invoiceNoList = invoiceNolist.Rows[0]["InvoiceList"].ToString();

                    ReportDataSource sI = new ReportDataSource("SalesInvoiceDataTable", salseInvoice);
                    ReportDataSource adj = new ReportDataSource("AdjustmentDataSet", adjustment);
                    ReportDataSource mop = new ReportDataSource("PaymentModeDataset", modeOfPayment);
                    ReportDataSource pay = new ReportDataSource("fgSalesPayment", payment);
                    ReportDataSource companyDc = new ReportDataSource("CompanyDataSet", company);

                    ReportViewer1.LocalReport.DataSources.Add(companyDc);
                    ReportViewer1.LocalReport.DataSources.Add(sI);
                    ReportViewer1.LocalReport.DataSources.Add(adj);
                    ReportViewer1.LocalReport.DataSources.Add(mop);
                    ReportViewer1.LocalReport.DataSources.Add(pay);

                    ReportParameter parms = new ReportParameter();
                    ReportViewer1.LocalReport.EnableExternalImages = true;
                    parms = new ReportParameter("PaymentinWord", takainword.ToUpper());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("invoiceNoList", invoiceNoList);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    var c = Request.Url.AbsoluteUri.Replace(Request.Url.Query, String.Empty);
                    Uri url = new Uri(c);
                    string mainpath = String.Format("{0}{1}{2}", url.Scheme,
                        Uri.SchemeDelimiter, url.Authority);
                    if (PhotoPath != String.Empty)
                    {
                        PhotoPath = mainpath + "/files/EmployeeImage/" + PhotoPath; 
                    }
                    else
                    {
                        PhotoPath = "";
                    }


                    if (PhotoPath2 != String.Empty)
                    {
                        PhotoPath2 = mainpath + "/files/EmployeeImage/" + PhotoPath2;
                    }
                    else
                    {
                        PhotoPath2 = "";
                    }


                    if (PhotoPath3 != String.Empty)
                    {
                        PhotoPath3 = mainpath + "/files/EmployeeImage/" + PhotoPath3;
                    }
                    else
                    {
                        PhotoPath3 = "";
                    }
                    

                    //PhotoPath2 = mainpath + "/files/EmployeeImage/" + PhotoPath2;

                    //PhotoPath3 = mainpath + "/files/EmployeeImage/" + PhotoPath3;

                   // accounts = mainpath + "/files/EmployeeImage/" + accounts;
                   
                    //Request.RawUrl;

                    //var url = Request.Url.AbsoluteUri.Replace(Request.Url.Query, String.Empty);
                    //string path = String.Format("{0}{1}{2}{3}", url.Scheme,
                    //    Uri.SchemeDelimiter, url.Authority, url.AbsolutePath);

                   

                   //var d= Request.Url.Host,
                    
                    
                    parms = new ReportParameter("PhotoPath", PhotoPath);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("FullName", FullName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("PhotoPath2", PhotoPath2);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("FullName2", FullName2);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("PhotoPath3", PhotoPath3);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("FullName3", FullName3);
                    this.ReportViewer1.LocalReport.SetParameters(parms);


                    parms = new ReportParameter("delvOption", delvOption);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.DisplayName = invoiceNum;
                    
                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}