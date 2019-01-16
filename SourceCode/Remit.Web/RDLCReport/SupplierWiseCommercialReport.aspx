<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SupplierWiseCommercialReport.aspx.cs" Inherits="Remit.Web.RDLCReport.SupplierWiseCommercialReport" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
<form id="form2" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="14in" Height="7.52in" style="margin-right: 0px" SizeToReportContent="True" AsyncRendering="False" DocumentMapWidth="100%">
        </rsweb:ReportViewer>
    </div>
</form>
</body>
</html>

