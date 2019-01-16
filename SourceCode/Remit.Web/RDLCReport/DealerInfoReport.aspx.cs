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
    public partial class DealerInfoReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string whereText = string.Empty;

                if (Request.QueryString["zoneId"] != null)
                {
                    whereText = " where FGDealer.DealersZoneId=" + Request.QueryString["zoneId"];
                }
               

                using (var _context = new ApplicationEntities())
                {
                   
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();
                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/DealerInfoReport.rdlc");
                    ReportViewer1.LocalReport.DataSources.Clear();

                    var query = "SELECT FGDealer.Name, FGDealer.Address, FGDealer.ContactEmail, " +
                                " FGDealerZone.ZoneName, FGDealer.OwnerPhone, FGDealer.OwnerName, " +
                                " FGDealer.ContactPersonName, FGDealer.ContactPhone," +
                                " FGDealer.ContactPersonDesignation, FGDealer.DefaultDeliverySite " +
                                " FROM  FGDealer INNER JOIN " +
                                " FGDealerZone ON FGDealer.DealersZoneId = FGDealerZone.Id " + whereText ;

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
                            sda.Fill(dtItem);

                            cmdComp.Connection = con;
                            sda.SelectCommand = cmdComp;
                            sda.Fill(company);
                        }
                    }



                    ReportDataSource rdc1 = new ReportDataSource("ZoneWiseDealerInfo", dtItem);
                    ReportDataSource companyDc = new ReportDataSource("CompanyDataSet", company);
                    ReportViewer1.LocalReport.DataSources.Add(rdc1);
                    ReportViewer1.LocalReport.DataSources.Add(companyDc);



                    //language base data: show data from resource file
                    ReportParameter parms = new ReportParameter();
                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("Zone", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "Zone"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("SL", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "SL"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DealersName", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "DealersName"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Address", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "Address"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Owner", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "Owner"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ContactPerson", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "ContactPerson"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DefaultDeliverySite", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "DefaultDeliverySite"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Email", utility.GetResourceValueById("ResourceRDLCDealerInfoReport", "Email"));
                    this.ReportViewer1.LocalReport.SetParameters(parms);

                    ReportViewer1.LocalReport.Refresh();
                }
            }
        }
    }
}