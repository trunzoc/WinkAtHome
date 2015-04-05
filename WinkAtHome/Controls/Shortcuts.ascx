<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Shortcuts.ascx.cs" Inherits="WinkAtHome.Controls.Shortcuts" %>

<asp:Table ID="Table1" runat="server" BorderColor="LightGray" BorderWidth="1" BorderStyle="Ridge" Width="100%">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
            <asp:Label ID="lblHeader" runat="server" Text="Shortcuts" ForeColor="White" />
        </asp:TableHeaderCell>
        <asp:TableCell BackColor="#22b9ec" HorizontalAlign="right">
            <asp:Label ID="Label1" runat="server" Text="Columns: " ForeColor="White" />
            <asp:TextBox ID="tbColumns" runat="server" Text="8" OnTextChanged="tbColumns_TextChanged" Width="20px" />
        </asp:TableCell>
    </asp:TableHeaderRow>
    <asp:TableRow>
        <asp:TableCell style="padding:10px;" ColumnSpan="2">
            <asp:DataList ID="dlShortcuts" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" Width="100%">
                <ItemStyle Width="100" />
                <ItemTemplate>
                    <table>
                        <tr>
                            <td align="center">
                                <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/Shortcut.png" Height="75" OnClick="imgIcon_Click" 
                                    CommandArgument='<%# ((Wink.Shortcut)Container.DataItem).id %>' ToolTip='<%# ((Wink.Shortcut)Container.DataItem).name %>' />
                            </td>
                        </tr>
                        <tr>
                            <td align="center" nowrap="true">
                                <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Shortcut)Container.DataItem).name %>' Font-Size="small" />
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
            </asp:DataList>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
