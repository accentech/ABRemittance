<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EmpWiseItemUseReport.aspx.cs" Inherits="Remit.Web.RDLCReport.EmpWiseItemUseReport1" %>

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
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" Height="11in" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="8in">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.EmpWiseItemUseReport.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="EmpWiseItemUseReport" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="EmpWiseItemUsereportTableAdapters.EmpWiseItemUseReportTableAdapter"></asp:ObjectDataSource>
    
    </div>
    </form>
</body>
</html>
