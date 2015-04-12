<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Menu.ascx.cs" Inherits="WinkAtHome.Controls.Menu" %>

<style type="text/css">
    .RadMenu_Default .rmRootGroup
    {
        border:none !important;
        background: Transparent !important;
    }
    .RadMenu_Default a.rmSelected 
    { 
        background-color: #22b9ec; 
        color:white;
        font-weight:bold;
    } 
    .RadMenu_Default a.rmLink:hover
    { 
        background-color: #22b9ec; 
        font-weight:bold;
    } 
</style>

<asp:Table ID="Table1" runat="server" Width="100%" CellPadding="5" CellSpacing="0">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" Height="30">
            <asp:Label ID="Label1" runat="server" Text="&nbsp;WINK ITEMS" ForeColor="White" />
        </asp:TableHeaderCell>
    </asp:TableHeaderRow>
    <asp:TableRow>
        <asp:TableCell>
            <telerik:RadMenu ID="RadMenu1" runat="server" Flow="Vertical" OnItemClick="RadMenu1_ItemClick" >
                <Items>
                    <telerik:RadMenuItem Text="Devices" Value="devices">
                        <Items>
                            <telerik:RadMenuItem Text="All Devices" Value="all" />
                            <telerik:RadMenuItem Text="Controllable Devices" Value="controllable" />
                            <telerik:RadMenuItem Text="Sensors" Value="sensors" />
                            <telerik:RadMenuItem Text="By Type..." Value="devicetype">
                                
                            </telerik:RadMenuItem>
                        </Items>
                    </telerik:RadMenuItem>
                    <telerik:RadMenuItem Text="Groups" Value="groups" />
                    <telerik:RadMenuItem Text="Shortcuts" Value="shortcuts" />
                    <telerik:RadMenuItem Text="Robots" Value="robots" />
                    <telerik:RadMenuItem Text="Settings" Value="settings" />
                </Items>
            </telerik:RadMenu>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>

