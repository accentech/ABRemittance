<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SizeWiseGradeWizeDelivery.aspx.cs" Inherits="Remit.Web.RDLCReport.SizeWiseGradeWizeDelivery" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>    
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Height="8.27in" Width="11.7in">
            <LocalReport ReportEmbeddedResource="Remit.Web.RDLCReport.SizeWiseGradeWizeDelivery.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="SizeWiseGradeWizeDelivery" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" SelectMethod="GetData" TypeName="SizeWiseGradeWizeDeliveryTableAdapters.SizeWiseGradeWizeDeliveryTableAdapter"></asp:ObjectDataSource>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>    
    </div>
    </form>
</body>
</html>
