<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductStock.aspx.cs" Inherits="Remit.Web.RDLCReport.ProductStock" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

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
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="8.3in" Height="600px" DocumentMapCollapsed="True" EnableTheming="True" SizeToReportContent="True">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.ProductStock.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="ProductStock" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="ProductStockTableAdapters.ProductStockTableAdapter"></asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
