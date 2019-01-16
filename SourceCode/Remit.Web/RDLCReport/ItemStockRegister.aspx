<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ItemStockRegister.aspx.cs" Inherits="Remit.Web.RDLCReport.ItemStockRegister" %>

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
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="11.7in" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="8.3in">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.ItemStockRegister.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="ItemStockRegister" />
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource2" Name="CompanyTable" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="GetData" TypeName="CompanyTableAdapters.CompanyTableAdapter"></asp:ObjectDataSource>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="ItemStockRegisterTableAdapters.ItemStockRegisterTableAdapter"></asp:ObjectDataSource>
    
    </div>
    </form>
</body>
</html>
