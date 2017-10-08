<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style>
    .txtCertificate { width:500px;height:500px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="SpidButton" runat="server" Text="Accedi con SPID" OnCommand="SpidButton_Command" />
    </div>
    </form>
</body>
</html>
