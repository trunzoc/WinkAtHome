<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Groups.ascx.cs" Inherits="WinkAtHome.Controls.Groups" %>

<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="upData">
    <ProgressTemplate>
        <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7;">
            <asp:Image ID="imgUpdateProgress" runat="server" ImageUrl="~/images/loading.gif" AlternateText="Loading ..." ToolTip="Loading ..." style="padding: 10px;" />
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>

<asp:HiddenField ID="hfSettingBase" runat="server" />

<asp:UpdatePanel ID="upData" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
    <ContentTemplate>

        <asp:Table ID="Table1" runat="server" BorderColor="#22b9ec" BorderWidth="1"  BackColor="#22b9ec" Width="100%" CellPadding="0" CellSpacing="0">
            <asp:TableRow>
                <asp:TableCell>
                    <asp:Table ID="Table7" runat="server" Width="100%">
                        <asp:TableHeaderRow BackColor="#22b9ec">
                            <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
                                <asp:Label ID="lblHeader" runat="server" Text="Groups" ForeColor="White" />
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
                                                <asp:Label ID="Label3" runat="server" Text="Section Settings " ForeColor="White" Font-Bold="true"/>
                                            </asp:TableHeaderCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label7" runat="server" Text="Show Panel: "  Font-Size="Small" />
                                            </asp:TableCell>
                                            <asp:TableCell>
                                                <asp:CheckBox ID="cbShow" runat="server" Checked="true" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label1" runat="server" Text="Hide Empty Groups: "  Font-Size="Small" />
                                            </asp:TableCell>
                                            <asp:TableCell>
                                                <asp:CheckBox ID="cbHideEmpty" runat="server" Checked="true" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label8" runat="server" Font-Size="Small"  Text="Objects Per Line: "  />
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
                    <asp:DataList ID="dlGroups" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" OnItemDataBound="dlGroups_ItemDataBound" Width="100%">
                        <ItemStyle Width="200" Height="100px" HorizontalAlign="Center" />
                        <ItemTemplate>

                            <asp:HiddenField ID="hfMainCommand" runat="server" />
                            <asp:HiddenField ID="hfCurrentStatus" runat="server" />
                            <asp:HiddenField ID="hfLevelCommand" runat="server" />
                            <asp:HiddenField ID="hfGroupID" runat="server" Value='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).id %>' />

                            <asp:Table ID="Table3" runat="server">
                                <asp:TableRow>
                                    <asp:TableCell VerticalAlign="Top">
                                        <asp:Label ID="Label1" runat="server" ForeColor="#b5c8cf" Font-Size="Small" Text='<%# ((IDataItemContainer)Container).DisplayIndex + 1 %>' />
                                    </asp:TableCell>
                                    <asp:TableCell ColumnSpan="2" Height="110" VerticalAlign="Top" HorizontalAlign="Center">
                                        <asp:Table ID="tblDefault" runat="server" Height="100%"> 
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="Center">
                                                    <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/Groups/false.png" Height="100" OnClick="imgIcon_Click" 
                                                        CommandArgument='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).id %>' ToolTip='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).name %>' />
                                                </asp:TableCell>
                                                <asp:TableCell HorizontalAlign="Right">
                                                    <telerik:RadSlider ID="rsBrightness" runat="server" MinimumValue="0" MaximumValue="100" Orientation="Vertical" ToolTip="Off" Height="100"
                                                        ItemType="None" ShowIncreaseHandle="false" ShowDecreaseHandle="false" IsDirectionReversed="true" AutoPostBack="true" LiveDrag="false"
                                                        AnimationDuration="400" ThumbsInteractionMode="Free" OnValueChanged="rsBrightness_ValueChanged" DecreaseText='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).name %>'>
                                                    </telerik:RadSlider>
                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Center" VerticalAlign="Middle" style="padding-bottom:30px" ColumnSpan="2">
                                        <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).displayName %>' Font-Size="small" />
                                    </asp:TableCell>
                                    <asp:TableCell VerticalAlign="Top" HorizontalAlign="Right" Width="23" style="padding-right:3px;">
                                        <asp:ImageButton ID="ibInfo" runat="server" ImageUrl="~/Images/info.png" Height="20" ToolTip="Show Group data" OnClick="ibInfo_Click" />

                                        <asp:button id="btnShowInfo" runat="server" style="display:none;" />

                                        <ajaxtoolkit:ModalPopupExtender ID="mpeInfo" runat="server" PopupControlID="pnlInfo"
                                            TargetControlID="btnShowInfo" BackgroundCssClass="modalBackground" Y="100">
                                        </ajaxtoolkit:ModalPopupExtender>
                                        <asp:Panel ID="pnlInfo" runat="server" BorderWidth="1"  style="display:none">
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

                                                <asp:TableRow>
                                                    <asp:TableCell HorizontalAlign="Right" VerticalAlign="Top">
                                                        <asp:Label ID="lbl1" runat="server" Text="JSON:" Font-Size="Small" />
                                                    </asp:TableCell>
                                                    <asp:TableCell HorizontalAlign="Left">
                                                        <asp:Textbox ID="tbJSON" runat="server" Text='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).json %>' TextMode="MultiLine" Height="80" Width="400" ReadOnly="true" />
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
                                                        <asp:LinkButton ID="lbInfoClose" runat="server" Text="Save & Close" ForeColor="White" style="text-decoration: none;" OnClick="btnClose_Click" CommandArgument='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).id %>' />
                                                    </asp:TableHeaderCell>
                                                </asp:TableRow>
                                            </asp:Table>
                                        </asp:Panel>
                                    </asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>

                        </ItemTemplate>
                        <FooterTemplate>
                            <asp:Label ID="lblEmpty" Text="No Groups To Display" runat="server" Visible='<%#bool.Parse((dlGroups.Items.Count==0).ToString())%>' />
                        </FooterTemplate>
                    </asp:DataList>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </ContentTemplate>
</asp:UpdatePanel>
