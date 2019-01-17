<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductInReport.aspx.cs" Inherits="Remit.Web.RDLCReport.ProductInReport" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="18.5in" Height="12.5in" ViewStateMode="Enabled">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.ProductInReport.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource2" Name="ProductInReport" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="GetData" TypeName="ProductInReportTableAdapters.ProductInReportTableAdapter"></asp:ObjectDataSource>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.ProductInReportTableAdapters.ProductInReportTableAdapter"></asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>