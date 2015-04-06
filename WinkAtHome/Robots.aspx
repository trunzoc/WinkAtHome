<%@ Page Title="Robots" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Robots.aspx.cs" Inherits="WinkAtHome.Robots" %>
<%@ Register Src="~/Controls/Robots.ascx" TagName="ucRobots" TagPrefix="ucR" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <ucR:ucRobots ID="ucRobots" runat="server" />
</asp:Content>
