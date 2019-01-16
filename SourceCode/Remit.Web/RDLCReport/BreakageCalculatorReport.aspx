<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BreakageCalculatorReport.aspx.cs" Inherits="Remit.Web.RDLCReport.BreakageCalculatorReport" %>
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
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="542px" DocumentMapWidth="80%" SizeToReportContent="True" Height="11.7in">
                <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.BreakageCalculatorReport.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="commissionCalculator" />
                        <rsweb:ReportDataSource DataSourceId="ObjectDataSource2" Name="CompanyDataSet" />
                    </DataSources>
                </LocalReport>

            </rsweb:ReportViewer>
            <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.CompanyTableAdapters.CompanyTableAdapter"></asp:ObjectDataSource>
        </div>
    </form>
</body>
</html>
