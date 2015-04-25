<%@ Page Title="MONITOR" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Monitor.aspx.cs" Inherits="WinkAtHome.Monitor" MaintainScrollPositionOnPostback="true" %>
<%@ Register Src="~/Controls/Devices.ascx" TagName="ucDevices" TagPrefix="ucD" %>
<%@ Register Src="~/Controls/Groups.ascx" TagName="ucGroups" TagPrefix="ucG" %>
<%@ Register Src="~/Controls/Robots.ascx" TagName="ucRobots" TagPrefix="ucR" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server" BorderWidth="0" CellPadding="0" CellSpacing="0">
    <asp:Table ID="Table1" runat="server" Width="100%" CellPadding="0" CellSpacing="0">
        <asp:TableRow>
            <asp:TableCell style="padding-bottom:20px;">
                <ucD:ucDevices ID="ucSesnors" runat="server" ControllableOnly="true" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell style="padding-bottom:20px;">
                <ucR:ucRobots ID="ucRobots" runat="server" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell style="padding-bottom:20px;">
                <ucG:ucGroups ID="ucGroups" runat="server" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <ucD:ucDevices ID="ucDevices" runat="server" ControllableOnly="true" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
</asp:Content>

