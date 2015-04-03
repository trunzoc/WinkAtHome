<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WinkAtHome.Default" %>
<%@ Register Src="~/Controls/Devices.ascx" TagName="ucDevices" TagPrefix="ucD" %>
<%@ Register Src="~/Controls/Shortcuts.ascx" TagName="ucShortcuts" TagPrefix="ucS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <asp:Table ID="Table1" runat="server">
        <asp:TableRow style="padding-bottom:30px;">
            <asp:TableCell Width="50%">
                <ucS:ucShortcuts ID="ucShortcuts" runat="server" />
            </asp:TableCell>
            <asp:TableCell>

            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="2">
                <ucD:ucDevices ID="ucDevices" runat="server" ControllableOnly="true" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
</asp:Content>

