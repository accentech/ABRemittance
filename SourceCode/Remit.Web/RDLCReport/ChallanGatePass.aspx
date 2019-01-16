<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChallanGatePass.aspx.cs" Inherits="Remit.Web.RDLCReport.ChallanGatePass" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server" submitdisabledcontrols="false">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="8.3in" DocumentMapWidth="2000px" SizeToReportContent="True" Height="11.7in" style="margin-right: 0px" EnableTheming="True" PromptAreaCollapsed="True" ShowPageNavigationControls="False">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.Challan.rdlc">
            </LocalReport>

        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.DeliveryChallanReportTableAdapters.FGDeliveryDataSetTableAdapter"></asp:ObjectDataSource>
        
    </div>
    </form>
   
</body>
</html>
