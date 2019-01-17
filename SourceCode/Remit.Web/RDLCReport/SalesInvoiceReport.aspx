<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SalesInvoiceReport.aspx.cs" Inherits="Remit.Web.RDLCReport.SalesInvoiceReport" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
       
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Height="11.7in" Width="8.3in"  DocumentMapWidth="100%" SizeToReportContent="True" EnableExternalImages="true">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.SalesInvoice.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="SalesInvoiceDataSet" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.SalesInvoiceTableAdapters.SalesDetailTableAdapter"></asp:ObjectDataSource>
       
    </div>
    </form>
</body>
</html>
