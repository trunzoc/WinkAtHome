<%@ Page Title="Monitor" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Monitor.aspx.cs" Inherits="WinkAtHome.Monitor" %>
<%@ Register Src="~/Controls/Devices.ascx" TagName="ucDevices" TagPrefix="ucD" %>
<%@ Register Src="~/Controls/Groups.ascx" TagName="ucGroups" TagPrefix="ucG" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <asp:Table ID="Table1" runat="server">
        <asp:TableRow style="padding-bottom:30px;">
            <asp:TableCell Width="50%">
                Sensors will go here
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <ucG:ucGroups ID="ucGroups" runat="server" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="2">
                <ucD:ucDevices ID="ucDevices" runat="server" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
</asp:Content>

