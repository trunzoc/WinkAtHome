<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Robots.ascx.cs" Inherits="WinkAtHome.Controls.Robots" %>

<asp:Table ID="Table1" runat="server" BorderColor="LightGray" BorderWidth="1" BorderStyle="Ridge" Width="100%" rules="none">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
            <asp:Label ID="lblHeader" runat="server" Text="Robots" ForeColor="White" />
        </asp:TableHeaderCell>
        <asp:TableCell BackColor="#22b9ec" HorizontalAlign="right">
            <asp:Label ID="Label1" runat="server" Text="Columns: " ForeColor="White" />
            <asp:TextBox ID="tbColumns" runat="server" Text="8" OnTextChanged="tbColumns_TextChanged" Width="20px" AutoPostBack="true" />
        </asp:TableCell>
    </asp:TableHeaderRow>
    <asp:TableRow>
        <asp:TableCell style="padding-bottom:10px;" ColumnSpan="2">
            <asp:DataList ID="dlRobots" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" Width="100%">
                <ItemStyle Height="175" />
                <ItemTemplate>
                    <table width="100%">
                        <tr>
                            <td align="center">
                                <asp:ImageButton ID="imgIcon" runat="server" ImageUrl='<%# "~/Images/Robots/" + ((Wink.Robot)Container.DataItem).enabled + ".png" %>' Height="75" OnClick="imgIcon_Click" 
                                    CommandArgument='<%# ((Wink.Robot)Container.DataItem).id %>' CommandName='<%# ((Wink.Robot)Container.DataItem).enabled %>' ToolTip='<%# ((Wink.Robot)Container.DataItem).name %>' />
                            </td>
                        </tr>
                        <tr>
                            <td align="center" nowrap="true">
                                <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Robot)Container.DataItem).name %>' Font-Size="small" />
                            </td>
                        </tr>
                        <tr>
                            <td align="center" nowrap="true">
                                <asp:Label ID="Label2" runat="server" Text="Last Triggered:" Font-Size="small" />
                            </td>
                        </tr>
                        <tr>
                            <td align="center" nowrap="true">
                                <asp:Label ID="Label3" runat="server" Text='<%# ((Wink.Robot)Container.DataItem).last_fired.ToString().Contains("1/1/1970") ? "Never" : ((Wink.Robot)Container.DataItem).last_fired.ToString() %>' Font-Size="small" />
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
            </asp:DataList>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
