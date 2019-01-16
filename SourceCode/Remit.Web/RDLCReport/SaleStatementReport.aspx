<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SaleStatementReport.aspx.cs" Inherits="Remit.Web.RDLCReport.SaleStatementReport" %>
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
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="100%" Height="8.5in" style="padding-left: 0px">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.SalesStatementReport.rdlc">
            </LocalReport>
            
        </rsweb:ReportViewer>
    </div>
    </form>
</body>
</html>
