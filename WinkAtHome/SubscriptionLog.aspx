<%@ Page Title="Subscription Log" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="SubscriptionLog.aspx.cs" Inherits="WinkAtHome.SubscriptionLog" %>
<%@ Register Src="~/Controls/SubscriptionDisplay.ascx" TagName="ucSubscription" TagPrefix="ucP" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <ucP:ucSubscription ID="ucSubscription" runat="server" DisplayHeight="1000"  />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cphLeft" runat="server">
</asp:Content>