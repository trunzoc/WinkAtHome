<%@ Page Title="Shortcuts" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Shortcuts.aspx.cs" Inherits="WinkAtHome.Shortcuts" %>
<%@ Register Src="~/Controls/Shortcuts.ascx" TagName="ucShortcuts" TagPrefix="ucS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <ucS:ucShortcuts ID="ucShortcuts" runat="server" />
</asp:Content>
