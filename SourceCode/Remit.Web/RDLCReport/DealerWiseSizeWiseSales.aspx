﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DealerWiseSizeWiseSales.aspx.cs" Inherits="Remit.Web.RDLCReport.DealerWiseSizeWiseSales" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="100%" Height="11.7in">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.DealerWiseSizeWiseSales.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="DealerWiseSizeWiseSales" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="Remit.Web.RDLCReportDataset.DealerWiseSizeWiseSalesTableAdapters.DealerWiseSizeWiseSalesTableAdapter"></asp:ObjectDataSource>
    
    </div>
    </form>
</body>
</html>
