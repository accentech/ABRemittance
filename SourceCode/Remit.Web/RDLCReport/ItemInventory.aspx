<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ItemInventory.aspx.cs" Inherits="Remit.Web.RDLCReport.InemInventory" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="12in" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="8.3in">
                <localreport reportembeddedresource="Remit.Web.RDLCReport.ItemInventory.rdlc">
                    <datasources>
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource2" Name="ItemInventory" />
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource3" Name="CompanyTable" />
                    </datasources>
                </localreport>
            </rsweb:ReportViewer>
            <asp:ObjectDataSource ID="ObjectDataSource3" runat="server" SelectMethod="GetData" TypeName="CompanyTableAdapters.CompanyTableAdapter"></asp:ObjectDataSource>
            <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="GetData" TypeName="ItemInventoryTableAdapters.ItemInventoryTableAdapter"></asp:ObjectDataSource>
            <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.ItemInventoryTableAdapters.ItemInventoryTableAdapter"></asp:ObjectDataSource>
        </div>
    </form>
</body>
</html>