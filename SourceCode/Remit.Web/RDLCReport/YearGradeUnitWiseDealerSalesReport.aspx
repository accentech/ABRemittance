﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="YearGradeUnitWiseDealerSalesReport.aspx.cs" Inherits="Remit.Web.RDLCReport.YearGradeUnitWiseDealerSalesReport" %>
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
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="14in" Height="7.52in" style="margin-right: 0px" SizeToReportContent="True" AsyncRendering="False" DocumentMapWidth="100%">
        </rsweb:ReportViewer>
       <%-- <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.CompanyTableAdapters.CompanyTableAdapter"></asp:ObjectDataSource>
        <asp:ObjectDataSource ID="ObjectDataSource2" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.DealerWiseSaleTableAdapters.DWSDataTableTableAdapter">

        </asp:ObjectDataSource>--%>

    </div>
</form>
</body>
</html>