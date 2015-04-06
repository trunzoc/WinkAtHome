<%@ Page Title="Groups" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Groups.aspx.cs" Inherits="WinkAtHome.Groups" %>
<%@ Register Src="~/Controls/Groups.ascx" TagName="ucGroups" TagPrefix="ucG" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <ucG:ucGroups ID="ucGroups" runat="server" />
</asp:Content>

