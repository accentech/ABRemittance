<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WB.aspx.cs" Inherits="Remit.Web.RDLCReport.WB" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
          <%--  <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt">
                <LocalReport ReportEmbeddedResource="Remit.Web.Report1.rdlc">
                </LocalReport>
            </rsweb:ReportViewer>--%>
            <asp:ScriptManager ID="ScriptManager2" runat="server"></asp:ScriptManager>
            <rsweb:ReportViewer ID="ReportViewer2" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="8.3in" DocumentMapWidth="100%" SizeToReportContent="True" Height="11.7in" style="margin-right: 0px">
                <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.DailyRawMaterialScaleReport.rdlc">
                </LocalReport>
            </rsweb:ReportViewer>
        </div>
    </form>
</body>
</html>
