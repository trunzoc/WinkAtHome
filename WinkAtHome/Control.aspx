<%@ Page Title="CONTROL" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Control.aspx.cs" Inherits="WinkAtHome.Control" %>
<%@ Register Src="~/Controls/Devices.ascx" TagName="ucDevices" TagPrefix="ucD" %>
<%@ Register Src="~/Controls/Shortcuts.ascx" TagName="ucShortcuts" TagPrefix="ucS" %>
<%@ Register Src="~/Controls/Groups.ascx" TagName="ucGroups" TagPrefix="ucG" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <asp:Table ID="Table1" runat="server" Width="100%" BorderWidth="0" CellPadding="0" CellSpacing="0">
        <asp:TableRow style="padding-bottom:30px;">
            <asp:TableCell>
                <ucS:ucShortcuts ID="ucShortcuts" runat="server" />
                <br />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <ucG:ucGroups ID="ucGroups" runat="server" />
                <br />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <ucD:ucDevices ID="ucDevices" runat="server" ControllableOnly="true" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
</asp:Content>
