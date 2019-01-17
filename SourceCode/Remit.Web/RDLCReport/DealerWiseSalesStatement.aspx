<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DealerWiseSalesStatement.aspx.cs" Inherits="Remit.Web.RDLCReport.DealerWiseSalesStatement" %>

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
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="14in" Height="8.5in" style="padding-left: 0px" AsyncRendering="False" DocumentMapCollapsed="True" DocumentMapWidth="100%" SizeToReportContent="True">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.DealerWiseSales.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="ConpanyInfo" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
    </div>
    </form>
</body>
</html>
