<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Devices.ascx.cs" Inherits="WinkAtHome.Controls.Devices" %>

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
                                <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                                    <Triggers>
                                        <asp:PostBackTrigger ControlID="imgIcon" />
                                        <asp:PostBackTrigger ControlID="rsBrightness" />
                                    </Triggers>
                                    <ContentTemplate>

                                        <asp:Table ID="tblDefault" runat="server" Visible="false" Width="100%" Height="100%">
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="Center">
                                                    <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/WinkHouse.png" Height="100" OnClick="imgIcon_Click"  Enabled="false"
                                                        CommandArgument='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).id %>' ToolTip='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).name + " : " + ((Wink.Device)((IDataItemContainer)Container).DataItem).type %>' />
                                                </asp:TableCell>
                                                <asp:TableCell HorizontalAlign="Right">
                                                    <telerik:RadSlider ID="rsBrightness" runat="server" MinimumValue="0" MaximumValue="100" Orientation="Vertical" ToolTip="Off" Height="100" 
                                                        ItemType="None" ShowIncreaseHandle="false" ShowDecreaseHandle="false" IsDirectionReversed="true" AutoPostBack="true" LiveDrag="false"
                                                        AnimationDuration="400" ThumbsInteractionMode="Free" OnValueChanged="rsBrightness_ValueChanged" DecreaseText='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).name %>'>
                                                    </telerik:RadSlider>
                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>
 
                                        <asp:Table ID="tblThermostat" runat="server" Visible="false" CellPadding="0" CellSpacing="0">
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="Center">
                                                    <asp:LinkButton ID="lbThermostat" runat="server" style="text-decoration:none;">

                                                    <asp:Table ID="Table2" runat="server" BackImageUrl="~/Images/Thermostats/thermback.png" Width="100" Height="100" style="background-repeat:no-repeat" CellPadding="0" CellSpacing="0">
                                                        <asp:TableRow>
                                                            <asp:TableCell HorizontalAlign="Left" style="padding-left:5px" VerticalAlign="Top">
                                                                <asp:ImageButton ID="ibThermPower" runat="server" ImageUrl="~/Images/Thermostats/powerfalse.png" Height="20" />
                                                            </asp:TableCell>
                                                            <asp:TableCell HorizontalAlign="Right" style="padding-right:5px" VerticalAlign="Top">
                                                                <asp:Label ID="lblThermStats" runat="server" Text="" ForeColor="#7698a4" Font-Size="Smaller" />
                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                        <asp:TableRow VerticalAlign="Top">
                                                            <asp:TableCell ColumnSpan="2">

                                                                <asp:Table ID="tblThermauto" runat="server" CellPadding="0" CellSpacing="0" Visible="false" Width="100%" Height="100%">
                                                                    <asp:TableRow VerticalAlign="Middle">
                                                                        <asp:TableCell HorizontalAlign="Center">
                                                                            <asp:Label ID="lblTempCoolauto" runat="server" Text="" Font-Size="X-Large" ForeColor="#22b9ec" />
                                                                        </asp:TableCell>
                                                                        <asp:TableCell HorizontalAlign="Center">
                                                                            <asp:Label ID="lblTempHeatauto" runat="server" Text="" Font-Size="X-Large" ForeColor="Red" />
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell ColumnSpan="2" HorizontalAlign="Center" VerticalAlign="Bottom">
                                                                            <asp:Image ID="imgThermostatModeAuto" runat="server" Height="25" />
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                </asp:Table>

                                                                <asp:Table ID="tblCoolHeat" runat="server" CellPadding="0" CellSpacing="0" Visible="false" Width="100%" Height="100%">
                                                                    <asp:TableRow VerticalAlign="Middle">
                                                                            <asp:TableCell HorizontalAlign="Center">
                                                                            &nbsp;<asp:Label ID="lblTemp" runat="server" Text="" Font-Size="X-Large" ForeColor="#22b9ec" />
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell HorizontalAlign="Center" VerticalAlign="Bottom">
                                                                            <asp:Image ID="imgThermostatModeHeatCool" runat="server" Height="25" />
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                </asp:Table>


                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                    </asp:Table>

                                                    </asp:LinkButton>

                                                    <ajaxtoolkit:ModalPopupExtender ID="mdeThermostats" runat="server" PopupControlID="pnlThermostats" TargetControlID="lbThermostat"
                                                        CancelControlID="lbCancelThermostat" OkControlID="lbApplyThermostat" BackgroundCssClass="modalBackground" Y="200">
                                                    </ajaxtoolkit:ModalPopupExtender>

                                                    <asp:Panel ID="pnlThermostats" runat="server" Width="210" Height="300" BorderWidth="1"  style="display:none" BackColor="#eeeeee">
                                                        <br /><br />
                                                        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
                                                            <ContentTemplate>
                                                                <asp:Table ID="Table5" runat="server" BackImageUrl="~/Images/Thermostats/thermbackbig.png" Width="200" Height="200" style="background-repeat:no-repeat" CellPadding="0" CellSpacing="0">
                                                                    <asp:TableRow>
                                                                        <asp:TableCell HorizontalAlign="Left" style="padding-left:5px" VerticalAlign="Top">
                                                                            <asp:ImageButton ID="ibThermPowerSet" runat="server" ImageUrl="~/Images/Thermostats/powerfalse.png" Height="40" OnClick="ibThermPower_Click" />
                                                                        </asp:TableCell>
                                                                        <asp:TableCell></asp:TableCell>
                                                                        <asp:TableCell HorizontalAlign="Right" style="padding-right:10px" VerticalAlign="Top">
                                                                            <asp:Label ID="lblThermStatsSet" runat="server" Text="" ForeColor="#7698a4" Font-Size="XX-Large" />
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                    <asp:TableRow VerticalAlign="Top">
                                                                        <asp:TableCell ColumnSpan="3" HorizontalAlign="Center" VerticalAlign="Middle">

                                                                            <asp:Table ID="tblThermautoSet" runat="server" CellPadding="0" CellSpacing="0" Visible="false">
                                                                                <asp:TableRow VerticalAlign="Top">
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        <asp:ImageButton ID="ibThermAutoCoolUp" runat="server" ImageUrl="~/Images/Thermostats/coolup.png" OnClick="ibThermChange_Click" CommandArgument="coolup" Height="16" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        <asp:ImageButton ID="ibThermAutoHeatUp" runat="server" ImageUrl="~/Images/Thermostats/heatup.png" OnClick="ibThermChange_Click" CommandArgument="heatup" Height="16" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                </asp:TableRow>
                                                                                <asp:TableRow VerticalAlign="Middle">
                                                                                    <asp:TableCell Width="30"></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        &nbsp;<asp:Label ID="lblTempCoolSetauto" runat="server" Text="" Font-Size="36" ForeColor="#22b9ec" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell Width="30"></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        &nbsp;<asp:Label ID="lblTempHeatSetauto" runat="server" Text="" Font-Size="36" ForeColor="Red" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell Width="30"></asp:TableCell>
                                                                                </asp:TableRow>
                                                                                <asp:TableRow VerticalAlign="Bottom">
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        <asp:ImageButton ID="ibThermAutoCoolDown" runat="server" ImageUrl="~/Images/Thermostats/cooldown.png" OnClick="ibThermChange_Click" CommandArgument="cooldown" Height="16" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        <asp:ImageButton ID="ibThermAutoHeatDown" runat="server" ImageUrl="~/Images/Thermostats/heatdown.png" OnClick="ibThermChange_Click" CommandArgument="heatdown" Height="16" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                </asp:TableRow>
                                                                            </asp:Table>

                                                                            <asp:Table ID="tblCoolHeatSet" runat="server" CellPadding="0" CellSpacing="0" Visible="false">
                                                                                <asp:TableRow VerticalAlign="Top">
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        <asp:ImageButton ID="ibThermUp" runat="server" ImageUrl="~/Images/Thermostats/coolup.png" OnClick="ibThermChange_Click" CommandArgument="up" Height="16" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                </asp:TableRow>
                                                                                <asp:TableRow VerticalAlign="Middle">
                                                                                    <asp:TableCell Width="30" HorizontalAlign="Right">
                                                                                        <asp:Image ID="imgCool" runat="server" ImageUrl="~/Images/Thermostats/coolheat.png" Height="50" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        &nbsp;<asp:Label ID="lblTempSet" runat="server" Text="" Font-Size="36" ForeColor="#22b9ec" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell Width="30" HorizontalAlign="Left">
                                                                                        <asp:Image ID="imgHeat" runat="server" ImageUrl="~/Images/Thermostats/heatcool.png" Height="50" />
                                                                                    </asp:TableCell>
                                                                                </asp:TableRow>
                                                                                <asp:TableRow VerticalAlign="Top">
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                    <asp:TableCell HorizontalAlign="Center">
                                                                                        <asp:ImageButton ID="ibThermDown" runat="server" ImageUrl="~/Images/Thermostats/cooldown.png" OnClick="ibThermChange_Click" CommandArgument="down" Height="16" />
                                                                                    </asp:TableCell>
                                                                                    <asp:TableCell></asp:TableCell>
                                                                                </asp:TableRow>
                                                                            </asp:Table>
														
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell HorizontalAlign="Center">
                                                                            <asp:ImageButton ID="ibThermcool" runat="server" ImageUrl="~/Images/Thermostats/coolfalse.png" Width="60" OnClick="ibThermModeChange_Click" CommandArgument="cool" />                                
                                                                        </asp:TableCell>
                                                                        <asp:TableCell>
                                                                            <asp:ImageButton ID="ibThermauto" runat="server" ImageUrl="~/Images/Thermostats/autofalse.png" Width="60" OnClick="ibThermModeChange_Click" CommandArgument="auto" />
                                                                        </asp:TableCell>
                                                                        <asp:TableCell>
                                                                            <asp:ImageButton ID="ibThermheat" runat="server" ImageUrl="~/Images/Thermostats/heatfalse.png" Width="60" OnClick="ibThermModeChange_Click" CommandArgument="heat" />
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                </asp:Table>
                        
                                                                <asp:HiddenField ID="hfSetHighTemp" runat="server" />
                                                                <asp:HiddenField ID="hfSetLowTemp" runat="server" />
                                                                <asp:HiddenField ID="hfSetTemp" runat="server" />
                                                                <asp:HiddenField ID="hfSetMode" runat="server" />
                                                                <asp:HiddenField ID="hfSetPower" runat="server" />


                                                                <br />
                                                                <asp:Table ID="Table4" runat="server" BorderColor="LightGray" BorderWidth="0" Width="100%">
                                                                    <asp:TableHeaderRow>
                                                                        <asp:TableHeaderCell ID="cellApply" BackColor="#22b9ec" HorizontalAlign="Center" style="padding:10px;">
                                                                            <asp:LinkButton ID="lbApplyThermostat" runat="server" OnClick="lbApplyThermostat_Click" Text="APPLY CHANGES" CommandArgument='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).id %>' ForeColor="White" style="text-decoration: none;" />
                                                                        </asp:TableHeaderCell>
                                                                    </asp:TableHeaderRow>
                                                                    <asp:TableHeaderRow>
                                                                        <asp:TableHeaderCell BackColor="red" HorizontalAlign="Center" style="padding:10px;">
                                                                            <asp:LinkButton ID="lbCancelThermostat" runat="server" Text="CANCEL & EXIT" ForeColor="White" style="text-decoration: none;" />
                                                                        </asp:TableHeaderCell>
                                                                    </asp:TableHeaderRow>
                                                                </asp:Table>
                                                            </ContentTemplate>
                                                            <Triggers>
                                                                <asp:PostBackTrigger ControlID="lbCancelThermostat" />
                                                                <asp:PostBackTrigger ControlID="lbApplyThermostat" />
                                                            </Triggers>
                                                        </asp:UpdatePanel>
                                                    </asp:Panel>

                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>

                                        <asp:HiddenField ID="hfMainCommand" runat="server" />
                                        <asp:HiddenField ID="hfCurrentStatus" runat="server" />
                                        <asp:HiddenField ID="hfLevelCommand" runat="server" />
                                        <asp:HiddenField ID="hfJSON" runat="server" />
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Center" VerticalAlign="Middle" style="padding-bottom:30px">
                                <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Device)Container.DataItem).name %>' Font-Size="small" />
                            </asp:TableCell>
                            <asp:TableCell VerticalAlign="Top" HorizontalAlign="Right" Width="23" style="padding-right:3px;">
                                <asp:ImageButton ID="ibInfo" runat="server" ImageUrl="~/Images/info.png" Height="20" ToolTip="Show Device data" />
                                <ajaxtoolkit:ModalPopupExtender ID="mpInfo" runat="server" PopupControlID="pnlInfo" 
                                    TargetControlID="ibInfo" CancelControlID="btnClose" BackgroundCssClass="modalBackground" Y="200">
                                </ajaxtoolkit:ModalPopupExtender>
                                <asp:Panel ID="pnlInfo" runat="server" BorderWidth="1"  style="display:none">
                                    <table cellpadding="5" cellspacing="5" style="background-color:#eeeeee;">
                                        <tr>
                                            <td>
                                                <asp:DataList ID="dlProperties" runat="server">
                                                    <HeaderTemplate>
                                                        <table>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <tr>
                                                            <td align="right">
                                                                <asp:Label ID="lblPropertyName" runat="server" Text='<%# Eval("Key") + ": " %>' />
                                                            </td>
                                                            <td align="left">
                                                                <asp:Label ID="lblPropertyValue" runat="server" Text='<%# Eval("Value") %>' />
                                                            </td>
                                                        </tr>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        </table>
                                                    </FooterTemplate>
                                                </asp:DataList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td align="left">
                                                <asp:Label ID="lbl1" runat="server" Text="JSON:" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                                <asp:Textbox ID="tbJSON" runat="server" Text='<%# ((Wink.Device)Container.DataItem).json %>' TextMode="MultiLine" Height="200" Width="400" ReadOnly="true" />
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
                 </ItemTemplate>
            </asp:DataList>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
