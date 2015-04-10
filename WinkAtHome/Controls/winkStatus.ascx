<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="winkStatus.ascx.cs" Inherits="WinkAtHome.Controls.winkStatus" %>

<asp:Table ID="Table1" runat="server" Width="100%" CellPadding="5" CellSpacing="0">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" Height="30">
            <asp:Label ID="Label1" runat="server" Text="&nbsp;WINK SYSTEM STATUS" ForeColor="White" />
        </asp:TableHeaderCell>
    </asp:TableHeaderRow>
    <asp:TableRow>
        <asp:TableCell>
            <asp:Label ID="lblStatusLastUpdate" runat="server" Text="Last Updated:" Font-Size="Smaller" />
        </asp:TableCell>
    </asp:TableRow>
    
    <asp:TableRow>
        <asp:TableCell>
            <asp:Repeater ID="rptStatus" runat="server" OnItemDataBound="rptStatus_ItemDataBound">
                <HeaderTemplate>
                    <table width="100%">
                        <tr>
                            <td>
                                <asp:Label ID="Label4" runat="server" Text="Services" Font-Bold="true" />
                            </td>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td align="left">
                            <asp:Label ID="Label2" runat="server" Text='<%# Eval("Key") %>' Font-Size="Small" />
                        </td>
                        <td align="right">
                            <asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Value") %>' Font-Size="Small"  />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </asp:TableCell>
    </asp:TableRow>

    <asp:TableRow>
        <asp:TableCell>
            <asp:Repeater ID="rptIncident" runat="server" OnItemDataBound="rptStatus_ItemDataBound">
                <HeaderTemplate>
                    <table>
                        <tr>
                            <td>
                                <asp:Label ID="Label4" runat="server" Text="Incidents" Font-Bold="true" />
                            </td>
                        </tr>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td align="left" width="250px">
                            <asp:Label ID="Label2" runat="server" Text='<%# Eval("Key") %>' Font-Size="Small" />
                        </td>
                    </tr>
                    <tr>
                        <td align="right">
                            <asp:Label ID="Label3" runat="server" Text="Status: " Font-Size="Small"  /><asp:Label ID="lblStatus" runat="server" Text='<%# Eval("Value") %>' Font-Size="Small"  />
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
