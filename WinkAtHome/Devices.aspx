<%@ Page Title="Controllable Devices" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Devices.aspx.cs" Inherits="WinkAtHome.Devices" %>
<%@ Register Src="~/Controls/Devices.ascx" TagName="ucDevices" TagPrefix="ucD" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <ucD:ucDevices ID="ucDevices" runat="server" ControllableOnly="false" />
</asp:Content>
