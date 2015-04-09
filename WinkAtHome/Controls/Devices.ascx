<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Devices.ascx.cs" Inherits="WinkAtHome.Controls.Devices" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxtoolkit" %>

<asp:HiddenField ID="hfDeviceType" runat="server" />
<asp:Table ID="Table1" runat="server" BorderColor="LightGray" BorderWidth="1" BorderStyle="Ridge" Width="100%">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
            <asp:Label ID="lblHeader" runat="server" Text="Devices" ForeColor="White" />
        </asp:TableHeaderCell>
        <asp:TableCell BackColor="#22b9ec" HorizontalAlign="right">
            <asp:Label ID="Label1" runat="server" Text="Columns: " ForeColor="White" />
            <asp:TextBox ID="tbColumns" runat="server" Text="5" OnTextChanged="tbColumns_TextChanged" Width="20px" AutoPostBack="true" />
        </asp:TableCell>
    </asp:TableHeaderRow>
    <asp:TableRow>
        <asp:TableCell style="padding:10px;" ColumnSpan="2">
            <asp:DataList ID="dlDevices" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" OnItemDataBound="dlDevices_ItemDataBound" Width="100%">
                <ItemStyle Width="225" Height="100px" />
                <ItemTemplate>
                    <asp:Table ID="Table3" runat="server">
                        <asp:TableRow>
                            <asp:TableCell ColumnSpan="2" Height="110" VerticalAlign="Top">

                                <asp:Table ID="tblDefault" runat="server" Visible="false" Width="100%" Height="100%">
                                    <asp:TableRow>
                                        <asp:TableCell HorizontalAlign="Center">
                                            <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/WinkHouse.png" Height="100" OnClick="imgIcon_Click"  Enabled="false"
                                                CommandArgument='<%# ((Wink.Device)Container.DataItem).id %>' ToolTip='<%# ((Wink.Device)Container.DataItem).name + " : " + ((Wink.Device)Container.DataItem).type %>' />
                                        </asp:TableCell>
                                        <asp:TableCell HorizontalAlign="Right">
                                            <telerik:RadSlider ID="rsBrightness" runat="server" MinimumValue="0" MaximumValue="100" Orientation="Vertical" ToolTip="Off" Height="100" 
                                                ItemType="None" ShowIncreaseHandle="false" ShowDecreaseHandle="false" IsDirectionReversed="true" AutoPostBack="true" LiveDrag="false"
                                                AnimationDuration="400" ThumbsInteractionMode="Free" OnValueChanged="rsBrightness_ValueChanged" DecreaseText='<%# ((Wink.Device)Container.DataItem).name %>'>
                                            </telerik:RadSlider>
                                        </asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>

                                <asp:Table ID="tblThermostat" runat="server" Visible="false" CellPadding="0" CellSpacing="0">
                                    <asp:TableRow>
                                        <asp:TableCell HorizontalAlign="Center">

                                            <asp:Table ID="Table2" runat="server" BackImageUrl="~/Images/Thermostats/thermback.png" Width="100" Height="100" style="background-repeat:no-repeat" CellPadding="0" CellSpacing="0">
                                                <asp:TableRow>
                                                    <asp:TableCell HorizontalAlign="Left" style="padding-left:5px">
                                                        <asp:ImageButton ID="ibThermPower" runat="server" ImageUrl="~/Images/Thermostats/powerfalse.png" Height="20" />
                                                    </asp:TableCell>
                                                    <asp:TableCell HorizontalAlign="Right" style="padding-right:5px">
                                                        <asp:Label ID="lblThermStats" runat="server" Text="" ForeColor="#7698a4" Font-Size="Smaller" />
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow VerticalAlign="Top">
                                                    <asp:TableCell ColumnSpan="2">

                                                        <asp:Table ID="tblThermauto" runat="server" CellPadding="0" CellSpacing="0" Visible="false">
                                                            <asp:TableRow VerticalAlign="Top">
                                                                <asp:TableCell></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:ImageButton ID="ibThermAutoCoolUp" runat="server" ImageUrl="~/Images/Thermostats/coolup.png" />
                                                                </asp:TableCell>
                                                                <asp:TableCell></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:ImageButton ID="ibThermAutoHeatUp" runat="server" ImageUrl="~/Images/Thermostats/heatup.png" />
                                                                </asp:TableCell>
                                                                <asp:TableCell></asp:TableCell>
                                                            </asp:TableRow>
                                                            <asp:TableRow VerticalAlign="Middle">
                                                                <asp:TableCell Width="30"></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    &nbsp;<asp:Label ID="lblTempCoolauto" runat="server" Text="" Font-Size="Large" ForeColor="#22b9ec" Font-Bold="true" />
                                                                </asp:TableCell>
                                                                <asp:TableCell Width="30"></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    &nbsp;<asp:Label ID="lblTempHeatauto" runat="server" Text="" Font-Size="Large" ForeColor="Red" Font-Bold="true" />
                                                                </asp:TableCell>
                                                                <asp:TableCell Width="30"></asp:TableCell>
                                                            </asp:TableRow>
                                                            <asp:TableRow VerticalAlign="Bottom">
                                                                <asp:TableCell></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:ImageButton ID="ibThermAutoCoolDown" runat="server" ImageUrl="~/Images/Thermostats/cooldown.png" />
                                                                </asp:TableCell>
                                                                <asp:TableCell></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:ImageButton ID="ibThermAutoHeatDown" runat="server" ImageUrl="~/Images/Thermostats/heatdown.png" />
                                                                </asp:TableCell>
                                                                <asp:TableCell></asp:TableCell>
                                                            </asp:TableRow>
                                                        </asp:Table>

                                                        <asp:Table ID="tblCoolHeat" runat="server" CellPadding="0" CellSpacing="0" Visible="false">
                                                            <asp:TableRow VerticalAlign="Top">
                                                                <asp:TableCell></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:ImageButton ID="ibThermUp" runat="server" ImageUrl="~/Images/Thermostats/coolup.png" />
                                                                </asp:TableCell>
                                                                <asp:TableCell></asp:TableCell>
                                                            </asp:TableRow>
                                                            <asp:TableRow VerticalAlign="Middle">
                                                                <asp:TableCell Width="30" HorizontalAlign="Right">
                                                                    <asp:Image ID="imgCool" runat="server" ImageUrl="~/Images/Thermostats/coolheat.png" Height="25" />
                                                                </asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    &nbsp;<asp:Label ID="lblTemp" runat="server" Text="" Font-Size="Large" ForeColor="#22b9ec" Font-Bold="true" />
                                                                </asp:TableCell>
                                                                <asp:TableCell Width="30" HorizontalAlign="Left">
                                                                    <asp:Image ID="imgHeat" runat="server" ImageUrl="~/Images/Thermostats/heatcool.png" Height="25" />
                                                                </asp:TableCell>
                                                            </asp:TableRow>
                                                            <asp:TableRow VerticalAlign="Top">
                                                                <asp:TableCell></asp:TableCell>
                                                                <asp:TableCell HorizontalAlign="Center">
                                                                    <asp:ImageButton ID="ibThermDown" runat="server" ImageUrl="~/Images/Thermostats/cooldown.png" />
                                                                </asp:TableCell>
                                                                <asp:TableCell></asp:TableCell>
                                                            </asp:TableRow>
                                                        </asp:Table>

                                                    </asp:TableCell>
                                                </asp:TableRow>
                                            </asp:Table>

                                        </asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow>
                                        <asp:TableCell HorizontalAlign="Center">
                                            <table>
                                                <tr>
                                                    <td>
                                                        <asp:ImageButton ID="ibThermcool" runat="server" ImageUrl="~/Images/Thermostats/coolfalse.png" Height="20" />                                
                                                    </td>
                                                    <td>
                                                        <asp:ImageButton ID="ibThermauto" runat="server" ImageUrl="~/Images/Thermostats/autofalse.png" Height="20" />
                                                    </td>
                                                    <td>
                                                        <asp:ImageButton ID="ibThermheat" runat="server" ImageUrl="~/Images/Thermostats/heatfalse.png" Height="20" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>


                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Center" VerticalAlign="Middle" style="padding-bottom:30px">
                                <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Device)Container.DataItem).name %>' Font-Size="small" />
                            </asp:TableCell>
                            <asp:TableCell VerticalAlign="Top" HorizontalAlign="Right" Width="23" style="padding-right:3px;">
                                <asp:ImageButton ID="ibInfo" runat="server" ImageUrl="~/Images/info.png" Height="20" ToolTip="Show object's Raw data" />
                                <ajaxtoolkit:ModalPopupExtender ID="mpInfo" runat="server" PopupControlID="pnlInfo" 
                                    TargetControlID="ibInfo" CancelControlID="btnClose" BackgroundCssClass="modalBackground" Y="100">
                                </ajaxtoolkit:ModalPopupExtender>
                                <asp:Panel ID="pnlInfo" runat="server" BorderWidth="1"  style="display:none">
                                    <table cellpadding="5" cellspacing="5" style="background-color:#eeeeee;">
                                        <tr>
                                            <td>
                                                <asp:Textbox ID="tbInfo" runat="server" Text='<%# ((Wink.Device)Container.DataItem).json %>' TextMode="MultiLine" Height="200" Width="400" />
                                            </td>
                                        </tr>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Button ID="btnClose" runat="server" Text="Close" />
                                            </td>
                                        </tr>
                                    </table>
                                </asp:Panel>
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>

                    <asp:HiddenField ID="hfMainCommand" runat="server" />
                    <asp:HiddenField ID="hfCurrentStatus" runat="server" />
                    <asp:HiddenField ID="hfLevelCommand" runat="server" />
                    <asp:HiddenField ID="hfJSON" runat="server" />
                </ItemTemplate>
            </asp:DataList>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
