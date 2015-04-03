<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Devices.ascx.cs" Inherits="WinkAtHome.Controls.Devices" %>

<asp:Table ID="Table1" runat="server" BorderColor="LightGray" BorderWidth="1" BorderStyle="Ridge" Width="100%">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
            <asp:Label ID="lblHeader" runat="server" Text="Devices" ForeColor="White" />
        </asp:TableHeaderCell>
    </asp:TableHeaderRow>
    <asp:TableRow>
        <asp:TableCell style="padding:10px;">
            <asp:DataList ID="dlDevices" runat="server" RepeatColumns="4" RepeatDirection="Horizontal" OnItemDataBound="dlDevices_ItemDataBound" Width="100%">
                <ItemStyle Width="225" Height="100px" />
                <ItemTemplate>
                    <table>
                        <tr>
                            <td align="center">
                                <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/WinkHouse.png" Height="100" OnClick="imgIcon_Click" 
                                    CommandArgument='<%# ((Wink.Device)Container.DataItem).id %>' ToolTip='<%# ((Wink.Device)Container.DataItem).name %>' />
                            </td>
                            <td>
                                <telerik:RadSlider ID="rsBrightness" runat="server" MinimumValue="0" MaximumValue="100" Orientation="Vertical" ToolTip="Off" Height="100"
                                    ItemType="None" ShowIncreaseHandle="false" ShowDecreaseHandle="false" IsDirectionReversed="true" AutoPostBack="true" LiveDrag="false"
                                    AnimationDuration="400" ThumbsInteractionMode="Free" OnValueChanged="rsBrightness_ValueChanged" DecreaseText='<%# ((Wink.Device)Container.DataItem).name %>'>
                                </telerik:RadSlider>
                            </td>
                        </tr>
                        <tr>
                            <td align="center" style="padding-bottom:30px">
                                <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Device)Container.DataItem).name %>' Font-Size="small" />
                                <%--<br /><asp:Label ID="lblType" runat="server" Font-Size="small" />--%>
                            </td>
                            <td></td>
                        </tr>
                    </table>
                    <asp:HiddenField ID="hfMainCommand" runat="server" />
                    <asp:HiddenField ID="hfCurrentStatus" runat="server" />
                    <asp:HiddenField ID="hfLevelCommand" runat="server" />
                </ItemTemplate>
            </asp:DataList>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
