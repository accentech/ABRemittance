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
using Remit.Model.Models;
using Remit.Web.Helpers;
using System.Web.Configuration;

namespace Remit.Web.RDLCReport
{
    public partial class WB : System.Web.UI.Page
    {
        string dateFormat = WebConfigurationManager.AppSettings["DateFormat"];
        protected void Page_Load(object sender, EventArgs e)
        {
            long timeZoneOffset = UserSession.GetTimeZoneOffset();
            if (!IsPostBack)
            {               
                using (var _context = new ApplicationEntities())
                {
                    System.Data.DataTable dtItem = new System.Data.DataTable();
                    System.Data.DataTable company = new System.Data.DataTable();
                    ReportViewer2.LocalReport.ReportPath = Server.MapPath("~/RDLCReport/DailyRawMaterialScaleReport.rdlc");
                    ReportViewer2.LocalReport.DataSources.Clear();

                    string conString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                    SqlConnection con = new SqlConnection(conString);
                    con.Open();

                    DateTime Date = new DateTime(0);
                    string Datestring = string.Empty;

                    int groupId = 0;
                    int categoryId = 0;
                    string groupName = string.Empty;
                    string categoryName = string.Empty;

                    if (Request.QueryString["Date"] != null)
                    {
                        Date = Convert.ToDateTime(Request.QueryString["Date"]);
                        Datestring = Date.ToString("yyyy-MM-dd");
                    }

                    string whereText = " where recv.IsDeleted != 1 and CONVERT(date,DATEADD(minute, " + timeZoneOffset + ", recv.ReceivedDate)) = CONVERT(date,'" + Datestring + "')";

                    if (Request.QueryString["groupId"] != null && Request.QueryString["groupId"] != "")
                    {
                        groupId = Convert.ToInt32(Request.QueryString["groupId"]);
                    }

                    var groupObj = _context.ItemGroups.FirstOrDefault(a => a.Id == groupId);
                    if (groupObj != null)
                    {
                        groupName = groupObj.Name;
                        whereText += " and itmcat.ItemGroupId = " + groupId;
                    }
                    else
                    {
                        groupName = "ALL";
                    }

                    if (Request.QueryString["categoryId"] != null && Request.QueryString["categoryId"] != "")
                    {
                        categoryId = Convert.ToInt32(Request.QueryString["categoryId"]);
                    }

                    var categoryObj = _context.ItemCategories.FirstOrDefault(a => a.Id == categoryId);
                    if (categoryObj != null)
                    {
                        categoryName = categoryObj.Name;
                        whereText += " and itm.ItemCategoryId = " + categoryId;
                    }
                    else
                    {
                        categoryName = "ALL";
                    }

                    var query = "Select ROW_NUMBER() Over (Order by itm.Name) As [SN],  itm.Name as RawMaterialsName, lcs.VesselDescription as ShipName, lcs.BillOfEntryNo as BOE,recv.LoadedTruckWeight as InWeight, recv.EmptyTruckWeight as OutWeight, recvDet.ReceivedQuantity as TotalWeight, unt.Name as Unit, recv.VehicleNo as VehicleNo, recv.DriverName as DriverName, recvDet.Remarks as Remarks, cast(recv.ReceivedDate as time) [Time] from [ItemReceiveDetail] as recvDet Inner Join [ItemReceive] as recv On recvDet.ItemReceiveId = recv.Id Inner Join [Item] as itm On recvDet.ItemId = itm.Id Inner Join [ItemCategory] as itmcat ON itm.ItemCategoryId = itmcat.Id Left Join [UnitOfMeasurement] as unt On recvDet.UnitId = unt.Id  Left Join [CommercialInvoice] as ci On recv.CommercialInvoiceId = ci.Id Left Join [LCShipment] as lcs On ci.Id = lcs.CommercialInvoiceId " + whereText + " ";
                    
                    var queryCom = "SELECT  Id, Name, Phone, Fax, Email, ContactPerson, LogoName, CompanyUrl, BaseCurrency, LocalCurrency, Address1, Address2, Address3, ShipmentAddress1, ShipmentAddress2, ShipmentAddress3 FROM Company";

                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlCommand cmdComp = new SqlCommand(queryCom, con);
                    SqlDataAdapter sda = new SqlDataAdapter();

                    using (sda)
                    {
                        sda.SelectCommand = cmd;
                        sda.Fill(dtItem);

                        sda.SelectCommand = cmdComp;
                        sda.Fill(company);
                    }
                    con.Close();

                    ReportDataSource companyDc = new ReportDataSource("CompanyTable", company);
                    ReportViewer2.LocalReport.DataSources.Add(companyDc);
                 
                    ReportDataSource rdc1 = new ReportDataSource("ScaleReport", dtItem);
                    ReportViewer2.LocalReport.DataSources.Add(rdc1);

                    ReportParameter parms = new ReportParameter();
                    parms = new ReportParameter("SearchDate", Date.ToString(dateFormat));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    var titleString = "Daily Raw Materials Scale Report. Group: " + groupName + ", Category: " + categoryName;
                    parms = new ReportParameter("subTitle", titleString);
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    //language base data: show data from resource file

                    ReportUtility utility = new ReportUtility();

                    parms = new ReportParameter("SN", utility.GetResourceValueById("ResourceRDLCWBReport", "SN"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("MaterialsName", utility.GetResourceValueById("ResourceRDLCWBReport", "MaterialsName"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("ShipName", utility.GetResourceValueById("ResourceRDLCWBReport", "ShipName"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("BOE", utility.GetResourceValueById("ResourceRDLCWBReport", "BOE"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("InWeight", utility.GetResourceValueById("ResourceRDLCWBReport", "InWeight"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("OutWeight", utility.GetResourceValueById("ResourceRDLCWBReport", "OutWeight"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("TotalWeight", utility.GetResourceValueById("ResourceRDLCWBReport", "TotalWeight"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Unit", utility.GetResourceValueById("ResourceRDLCWBReport", "Unit"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("VehicleNo", utility.GetResourceValueById("ResourceRDLCWBReport", "VehicleNo"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("DriverName", utility.GetResourceValueById("ResourceRDLCWBReport", "DriverName"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    parms = new ReportParameter("Remarks", utility.GetResourceValueById("ResourceRDLCWBReport", "Remarks"));
                    this.ReportViewer2.LocalReport.SetParameters(parms);

                    ReportViewer2.LocalReport.Refresh();                    
                }
            }
        }
    }
}