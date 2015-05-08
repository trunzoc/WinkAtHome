<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Devices.ascx.cs" Inherits="WinkAtHome.Controls.Devices" %>

<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upData">
    <ProgressTemplate>
        <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7;">
            <asp:Image ID="imgUpdateProgress" runat="server" ImageUrl="~/images/loading.gif" AlternateText="Loading ..." ToolTip="Loading ..." style="padding: 10px;" />
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>

<asp:HiddenField ID="hfDeviceType" runat="server" />
<asp:HiddenField ID="hfSettingBase" runat="server" />

<asp:UpdatePanel ID="upData" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>
        <asp:Table ID="Table1" runat="server" BorderColor="#22b9ec" BorderWidth="1" Width="100%" BackColor="#22b9ec"  CellPadding="0" CellSpacing="0">
            <asp:TableRow>
                <asp:TableCell>
                    <asp:Table ID="Table7" runat="server" Width="100%">
                        <asp:TableHeaderRow>
                            <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
                                <asp:Label ID="lblHeader" runat="server" Text="Devices" ForeColor="White" />
                            </asp:TableHeaderCell>
                            <asp:TableCell Width="40">
                                <asp:ImageButton ID="ibSettings" runat="server" ImageUrl="~/Images/wrench.png" Height="30" ToolTip="Panel Settings" OnClick="ibSettings_Click" />
                                <asp:button id="btnShowSettings" runat="server" style="display:none;" />
                                <ajaxtoolkit:ModalPopupExtender ID="mpeSettings" runat="server" PopupControlID="pnlSettings" 
                                    TargetControlID="btnShowSettings" BackgroundCssClass="modalBackground" Y="100">
                                </ajaxtoolkit:ModalPopupExtender>
                                <asp:Panel ID="pnlSettings" runat="server" style="display:none">
                                    <asp:Table ID="Table9" runat="server" CellPadding="5" CellSpacing="5" BackColor="#eeeeee">
                                        <asp:TableRow>
                                            <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Center" style="padding:10px;" ColumnSpan="2">
                                                <asp:Label ID="Label5" runat="server" Text="Section Settings " ForeColor="White" Font-Bold="true"/>
                                            </asp:TableHeaderCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label2" runat="server" Text="Show Panel: "  Font-Size="Small" />
                                            </asp:TableCell>
                                            <asp:TableCell>
                                                <asp:CheckBox ID="cbShow" runat="server" Checked="true" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label6" runat="server" Font-Size="Small"  Text="Objects Per Line: "  />
                                            </asp:TableCell>
                                            <asp:TableCell>
                                                <asp:TextBox ID="tbColumns" runat="server" Text="5" Width="30px" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Center" style="padding:10px;" ColumnSpan="2">
                                                <asp:LinkButton ID="ibSettingsClose" runat="server" Text="Save & Close" ForeColor="White" style="text-decoration: none;" OnClick="ibSettingsClose_Click" />
                                            </asp:TableHeaderCell>
                                        </asp:TableRow>
                                    </asp:Table>                            
                                </asp:Panel>
                            </asp:TableCell>
                        </asp:TableHeaderRow>
                    </asp:Table>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="rowData" BackColor="#eeeeee">
                <asp:TableCell style="padding:10px;">
                    <asp:DataList ID="dlDevices" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" OnItemDataBound="dlDevices_ItemDataBound" Width="100%">
                        <ItemStyle Width="200" Height="100px" HorizontalAlign="Center" VerticalAlign="Top" />
                        <ItemTemplate>
                            <asp:HiddenField ID="hfMainCommand" runat="server" />
                            <asp:HiddenField ID="hfCurrentStatus" runat="server" />
                            <asp:HiddenField ID="hfLevelCommand" runat="server" />
                            <asp:HiddenField ID="hfDeviceID" runat="server" Value='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).id %>' />

                            <asp:Table ID="Table3" runat="server">
                                <asp:TableRow>
                                    <asp:TableCell ColumnSpan="3" Height="110" VerticalAlign="Top" HorizontalAlign="Center">

                                        <asp:Table ID="tblDefault" runat="server" Visible="false" Height="100%">
                                            <asp:TableRow>
                                                <asp:TableCell VerticalAlign="Bottom" Width="1">
                                                    <asp:Image ID="imgBattery" runat="server" ImageUrl="~/Images/Battery/Battery0.png" Width="15" Visible="false" style="position:relative; left:20px;" onmouseover="this.style.cursor='Help'" onmouseout="this.style.cursor='default'" />
                                                </asp:TableCell>
                                                <asp:TableCell VerticalAlign="Top" Width="1">
                                                    <asp:Label ID="Label1" runat="server" ForeColor="#b5c8cf" Font-Size="Small" Text='<%# ((IDataItemContainer)Container).DisplayIndex + 1 %>' />
                                                </asp:TableCell>
                                                <asp:TableCell HorizontalAlign="Center">
                                                    <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/WinkHouse.png" Height="100" OnClick="imgIcon_Click"  Enabled="false"
                                                        CommandArgument='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).id %>' ToolTip='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).name %>' />
                                                </asp:TableCell>
                                                <asp:TableCell HorizontalAlign="Right">
                                                    <telerik:RadSlider ID="rsBrightness" runat="server" MinimumValue="0" MaximumValue="100" Orientation="Vertical" ToolTip="Off" Height="100" Visible="false"
                                                        ItemType="None" ShowIncreaseHandle="false" ShowDecreaseHandle="false" IsDirectionReversed="true" AutoPostBack="true" LiveDrag="false"
                                                        AnimationDuration="400" ThumbsInteractionMode="Free" OnValueChanged="rsBrightness_ValueChanged" DecreaseText='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).name %>'>
                                                    </telerik:RadSlider>
                                                </asp:TableCell>
                                                <asp:TableCell VerticalAlign="Top" Width="1">
                                                    <asp:Image ID="imgAlert" runat="server" ImageUrl="~/Images/alert.png" Width="25" Visible="false" style="position:relative; right:20px;" />
                                                    <asp:Image ID="imgUpdate" runat="server" ImageUrl="~/Images/update.png" Width="25" Visible='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).update_needed %>' style="position:relative; right:20px;" />
                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>

                                        <asp:Table ID="tblThermostat" runat="server" Visible="false" CellPadding="0" CellSpacing="0">
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="Center">
                                                    <asp:LinkButton ID="lbThermostat" runat="server"  ToolTip='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).name %>' style="text-decoration:none;" OnClick="ibThermostat_Click">

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

                                                    <asp:button id="btnShowThermostat" runat="server" style="display:none;" />

                                                    <ajaxtoolkit:ModalPopupExtender ID="mdeThermostats" runat="server" PopupControlID="pnlThermostats" TargetControlID="btnShowThermostat"
                                                        BackgroundCssClass="modalBackground" Y="200">
                                                    </ajaxtoolkit:ModalPopupExtender>

                                                    <asp:Panel ID="pnlThermostats" runat="server" Width="250" Height="350" BorderWidth="1"  style="display:none" BackColor="#eeeeee">
                                                        <br />
                                                        <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                                                            <Triggers>
                                                                <asp:PostBackTrigger ControlID="lbCancelThermostat"/>
                                                            </Triggers>
                                                            <ContentTemplate>
                                                                
                                                                <asp:HiddenField ID="hfDeadband" runat="server" />
                                                                <asp:HiddenField ID="hfSetHighTemp" runat="server" />
                                                                <asp:HiddenField ID="hfSetLowTemp" runat="server" />
                                                                <asp:HiddenField ID="hfSetTemp" runat="server" />
                                                                <asp:HiddenField ID="hfSetMode" runat="server" />
                                                                <asp:HiddenField ID="hfSetPower" runat="server" />

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

                                                                <br />

                                                                <asp:Table ID="Table4" runat="server" BorderColor="LightGray" CellPadding="10" BorderWidth="0" Width="100%">
                                                                    <asp:TableHeaderRow>
                                                                        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Center" style="padding:10px;">
                                                                            <asp:LinkButton ID="lbCancelThermostat" runat="server" Text="Close" ForeColor="White" style="text-decoration: none;" OnClick="btnClose_Click" />
                                                                        </asp:TableHeaderCell>
                                                                    </asp:TableHeaderRow>
                                                                    <asp:TableRow>
                                                                        <asp:TableCell HorizontalAlign="Center">
                                                                            <asp:Label ID="lblNotes" runat="server" Text="" ForeColor="Green" />
                                                                        </asp:TableCell>
                                                                    </asp:TableRow>
                                                                </asp:Table>
                                                            </ContentTemplate>
                                                        </asp:UpdatePanel>
                                                    </asp:Panel>

                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>

                                        </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Center" VerticalAlign="Middle" style="padding-bottom:30px">
                                        <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).displayName %>' Font-Size="small" />
                                    </asp:TableCell>
                                    <asp:TableCell Width="1" VerticalAlign="Top">
                                        <asp:ImageButton ID="ibHubDevices" runat="server" ImageUrl="~/Images/dots.png" Height="20" OnClick="ibHubDevices_Click" 
                                            CommandArgument='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).id %>' 
                                            ToolTip='<%# "Show Linked Devices for " + ((Wink.Device)((IDataItemContainer)Container).DataItem).displayName %>' 
                                            Visible='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).menu_type.ToLower() == "hubs" %>' />
                                                
                                        <asp:button id="btnShowHubDevices" runat="server" style="display:none;" />
                                        <ajaxtoolkit:ModalPopupExtender ID="mpeHubDevices" runat="server" PopupControlID="pnlHubDevices"
                                            TargetControlID="btnShowHubDevices" BackgroundCssClass="modalBackground" Y="100">
                                        </ajaxtoolkit:ModalPopupExtender>
                                        <asp:Panel ID="pnlHubDevices" runat="server" Height="500" style="display:none">
                                            <asp:Table ID="Table8" runat="server" CellPadding="5" CellSpacing="5" BackColor="#eeeeee">
                                                <asp:TableRow>
                                                    <asp:TableCell>
                                                        <asp:GridView ID="gvHubDevices" runat="server" AutoGenerateColumns="false" Height="490" AllowSorting="false">
                                                            <HeaderStyle Font-Size="Small" />
                                                            <RowStyle Font-Size="Small" />
                                                            <Columns>
                                                                <asp:BoundField HeaderText="Name" DataField="name" SortExpression="name" />
                                                                <asp:BoundField HeaderText="Item Type" DataField="type" SortExpression="type" />
                                                                <asp:BoundField HeaderText="Manufacturer" DataField="manufacturer" SortExpression="manufacturer" />
                                                                <asp:BoundField HeaderText="Model" DataField="Model" SortExpression="Model" />
                                                                <asp:BoundField HeaderText="Device ID" DataField="id" SortExpression="id" />
                                                            </Columns>
                                                        </asp:GridView>
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Center" style="padding:10px;" ColumnSpan="2">
                                                        <asp:LinkButton ID="lbCloseHubDevices" runat="server" Text="Close" ForeColor="White" OnClick="lbCloseHubDevices_Click" style="text-decoration: none;" />
                                                    </asp:TableHeaderCell>
                                                </asp:TableRow>
                                            </asp:Table>
                                        </asp:Panel>
                                    </asp:TableCell>
                                    <asp:TableCell VerticalAlign="Top" HorizontalAlign="Right" Width="1" style="padding-right:3px;">
                                        <asp:ImageButton ID="ibInfo" runat="server" ImageUrl="~/Images/info.png" Height="20" ToolTip='<%# "Show Device data for " + ((Wink.Device)((IDataItemContainer)Container).DataItem).id %>' OnClick="ibInfo_Click" />

                                        <asp:button id="btnShowInfo" runat="server" style="display:none;" />
                                        <ajaxtoolkit:ModalPopupExtender ID="mpeInfo" runat="server" PopupControlID="pnlInfo"
                                            TargetControlID="btnShowInfo" BackgroundCssClass="modalBackground" Y="100">
                                        </ajaxtoolkit:ModalPopupExtender>
                                        <asp:Panel ID="pnlInfo" runat="server" Height="500" style="display:none">
                                            <asp:Table ID="Table6" runat="server" CellPadding="5" CellSpacing="5" BackColor="#eeeeee">
                                                <asp:TableRow>
                                                    <asp:TableCell ColumnSpan="2">
                                                        <asp:DataList ID="dlProperties" runat="server" RepeatColumns="2" Width="100%" RepeatLayout="Table">
                                                            <HeaderTemplate>
                                                                <table style="width:100%">
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                    <td align="right" style="width:100px">
                                                                        <asp:Label ID="lblPropertyName" runat="server" Text='<%# Eval("Key") + ": " %>' Font-Size="Small" />
                                                                    </td>
                                                                    <td align="left">
                                                                        <asp:Label ID="lblPropertyValue" runat="server" Text='<%# Eval("Value") %>' Font-Size="Small" />
                                                                    </td>
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                </table>
                                                            </FooterTemplate>
                                                        </asp:DataList>
                                                    </asp:TableCell>
                                                </asp:TableRow>

                                                <asp:TableRow ID="rowSensorStates" Visible='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).sensor_states.Count > 0 %>'>
                                                    <asp:TableCell VerticalAlign="Top">
                                                        <asp:Label ID="Label5" runat="server" Text="Sensor States:" Font-Size="Small" />
                                                    </asp:TableCell>
                                                    <asp:TableCell>
                                                        <asp:GridView ID="gvSensorStates" runat="server">
                                                            <HeaderStyle Font-Size="Small" />
                                                            <RowStyle Font-Size="Small" />
                                                        </asp:GridView>
                                                    </asp:TableCell>
                                                </asp:TableRow>

                                                <asp:TableRow ID="rowLastReadings" Visible="false">
                                                    <asp:TableCell VerticalAlign="Top">
                                                        <asp:Label ID="Label3" runat="server" Text="Last Readings:" Font-Size="Small" />
                                                    </asp:TableCell>
                                                    <asp:TableCell>
                                                        <asp:Panel ID="Panel1" runat="server" ScrollBars="Vertical" BorderWidth="1" Height="80">
                                                            <asp:GridView ID="gvLastReadings" runat="server">
                                                                <HeaderStyle Font-Size="Small" />
                                                                <RowStyle Font-Size="Small" />
                                                            </asp:GridView>
                                                        </asp:Panel>                                                        
                                                    </asp:TableCell>
                                                </asp:TableRow>

                                                <asp:TableRow ID="rowDesiredStates" Visible="false">
                                                    <asp:TableCell HorizontalAlign="Right" VerticalAlign="Top">
                                                        <asp:Label ID="Label2" runat="server" Text="Desired States:" Font-Size="Small" />
                                                    </asp:TableCell>
                                                    <asp:TableCell HorizontalAlign="Left">
                                                        <asp:Image ID="imgDSExpand" runat="server" ImageUrl="~/Images/plus.png" Height="20" />                                                        
                                                        <asp:ListBox ID="lbDesiredStates" runat="server" Height="150"></asp:ListBox>
                                                    </asp:TableCell>
                                                </asp:TableRow>

                                                <asp:TableRow>
                                                    <asp:TableCell HorizontalAlign="Right" VerticalAlign="Top">
                                                        <asp:Label ID="lbl1" runat="server" Text="JSON:" Font-Size="Small" />
                                                    </asp:TableCell>
                                                    <asp:TableCell HorizontalAlign="Left">
                                                        <asp:Textbox ID="tbJSON" runat="server" Text='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).json %>' TextMode="MultiLine" Height="100" Width="400" ReadOnly="true" />
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow ID="rowDisplayName">
                                                    <asp:TableCell>
                                                        <asp:Label ID="lblDisplayName" runat="server" Text="Display Name:" Font-Size="Small"/>
                                                    </asp:TableCell>
                                                    <asp:TableCell>
                                                        <asp:TextBox ID="tbDisplayName" runat="server" Width="300" />
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow ID="rowPosition">
                                                    <asp:TableCell>
                                                        <asp:Label ID="Label4" runat="server" Text="Item Position:" Font-Size="Small" />
                                                    </asp:TableCell>
                                                    <asp:TableCell>
                                                        <asp:TextBox ID="tbPosition" runat="server" Width="50"  />
                                                        <asp:Label ID="lblPositionBad" runat="server" Text="Please enter a whole number between 1 and 1000" ForeColor="Red" Visible="false" />
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Center" style="padding:10px;" ColumnSpan="2">
                                                        <asp:LinkButton ID="lbInfoClose" runat="server" Text="Save & Close" ForeColor="White" style="text-decoration: none;" OnClick="btnClose_Click" CommandArgument='<%# ((Wink.Device)((IDataItemContainer)Container).DataItem).id %>' />
                                                    </asp:TableHeaderCell>
                                                </asp:TableRow>
                                            </asp:Table>
                                        </asp:Panel>
                                    </asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>

                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblEmpty" Text="No Devices To Display" runat="server" Visible='<%#bool.Parse((dlDevices.Items.Count==0).ToString())%>' />
                        </FooterTemplate>
                    </asp:DataList>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </ContentTemplate>
</asp:UpdatePanel>
