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

namespace Remit.Web.RDLCReport
{
    public partial class ChallanGatePass : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var PhotoPath = string.Empty;
                var FullName = string.Empty;
                var PhotoPath2 = string.Empty;
                var FullName2 = string.Empty;
                var PhotoPath3 = string.Empty;
                var FullName3 = string.Empty;


                var challanNo = string.Empty;
                if (Request.QueryString["challanNo"] != null)
                {
                    challanNo = Request.QueryString["challanNo"].ToString();
                }

                string whereText = string.Empty;

                whereText = " WHERE FGSalesDelivery.DeliveryChallanNo = '" + challanNo + "' and FGSalesDeliveryDetail.DeliveryQuantityInCTN > 0";

                using (var _context = new ApplicationEntities())
                {
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();

                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/ChallaGatePass.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    var query =

                        "SELECT FGItem.Code, FGSize.Size, FGGrade.Grade, FGSalesDeliveryDetail.Lot, " +
                        "FGItem.PackageToSalesRatio as PcsPerCartoon, FGSalesDeliveryDetail.DeliveryQuantityInSFT, " +
                        "FGSalesDeliveryDetail.DeliveryQuantityInCTN, FGSalesDelivery.DeliveryChallanNo, " +
                        "FGSalesDelivery.InvoiceNo, FGSalesDelivery.DeliveryDate, FGSalesDelivery.CustomerType, " +
                        "FGSalesDelivery.DealerId, FGSalesDelivery.Name, FGSalesDelivery.ContactPersonName, " +
                        "FGSalesDelivery.ContactPhone, FGSalesDelivery.Address, FGSalesDelivery.DeliverySite, " +
                        "FGSalesDelivery.DeliveryOption, FGItem.SalesUnitId, FGUOM.UnitName AS UnitName FROM " +
                        "FGSalesDelivery INNER JOIN FGSalesDeliveryDetail ON FGSalesDelivery.DeliveryChallanNo = " +
                        "FGSalesDeliveryDetail.DeliveryChallanNo INNER JOIN FGItem ON FGSalesDeliveryDetail.FGItemId = " +
                        "FGItem.Id INNER JOIN FGSize ON FGSalesDeliveryDetail.FGSizeId = FGSize.Id AND FGItem.SizeId = " +
                        "FGSize.Id INNER JOIN FGGrade ON FGSalesDeliveryDetail.FGGradeId = FGGrade.Id LEFT JOIN FGUOM ON " +
                        "FGItem.SalesUnitId = FGUOM.Id " +
                        whereText + "Order By FGItem.Code";

                    var getCompany =
                        "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";


                    var createdBy = "Select Employee.PhotoPath,Employee.FullName FROM Employee JOIN FGSalesDelivery  ON Employee.Id=FGSalesDelivery.CreatedBy where FGSalesDelivery.DeliveryChallanNo='" + challanNo + "' ";
                    var authorisedBy = "Select Employee.PhotoPath,Employee.FullName FROM Employee JOIN FGSalesDelivery  ON Employee.Id=FGSalesDelivery.ReviewedBy where FGSalesDelivery.DeliveryChallanNo='" + challanNo + "' ";
                    var approvedBy = "Select Employee.PhotoPath,Employee.FullName FROM Employee JOIN FGSalesDelivery  ON Employee.Id=FGSalesDelivery.ApprovedBy where FGSalesDelivery.DeliveryChallanNo='" + challanNo + "' ";
                    
                    


                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand(query);
                    SqlCommand cmdComp = new SqlCommand(getCompany);
                    SqlDataAdapter sda = new SqlDataAdapter();

                    SqlConnection conTst = new SqlConnection(conString);
                    SqlCommand command = new SqlCommand();

                    command.CommandText = createdBy;
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
                    command3.CommandText = approvedBy;
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
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;
                            sda.Fill(dtItem);

                            cmdComp.Connection = con;
                            sda.SelectCommand = cmdComp;
                            sda.Fill(company);
                        }
                    }

                    double itemQuantityTotal = 0.0;
                    string takainword = string.Empty;
                    for (int i = 0; i < dtItem.Rows.Count; i++)
                    {
                        if (dtItem.Rows[i]["DeliveryQuantityInCTN"] != System.DBNull.Value)
                        {
                            itemQuantityTotal += Convert.ToInt32(dtItem.Rows[i]["DeliveryQuantityInCTN"]);
                        }
                    }

                    takainword = ReportUtility.ConvertToWords(itemQuantityTotal.ToString());
                    takainword += "CARTON ONLY";

                    ReportDataSource rdc1 = new ReportDataSource("DataSet1", dtItem);
                    ReportDataSource companyDc = new ReportDataSource("CompanyDataSet", company);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);

                    ReportParameter parms = new ReportParameter();

                    ReportViewer1.LocalReport.EnableExternalImages = true;
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



                    // created By
                    parms = new ReportParameter("PhotoPath", PhotoPath);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("FullName", FullName);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    // reviewed By
                    parms = new ReportParameter("PhotoPath2", PhotoPath2);
                    this.ReportViewer1.LocalReport.SetParameters(parms);
                    parms = new ReportParameter("FullName2", FullName2);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    // Approved By
                    parms = new ReportParameter("PhotoPath3", PhotoPath3);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("FullName3", FullName3);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("inWord", takainword.ToUpper());
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("challanNo", challanNo);
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    challanNo = challanNo.Replace('/', '_');
                    ReportViewer1.LocalReport.DisplayName = challanNo;

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}