<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Shortcuts.ascx.cs" Inherits="WinkAtHome.Controls.Shortcuts" %>

<asp:HiddenField ID="hfSettingBase" runat="server" />

<asp:Table ID="Table1" runat="server" BorderColor="#22b9ec" BorderWidth="1" BackColor="#22b9ec" Width="100%" CellPadding="0" CellSpacing="0"> 
    <asp:TableRow>
        <asp:TableCell>
            <asp:Table ID="Table7" runat="server" Width="100%">
                <asp:TableHeaderRow BackColor="#22b9ec">
                    <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
                        <asp:Label ID="lblHeader" runat="server" Text="Shortcuts" ForeColor="White" />
                    </asp:TableHeaderCell>
                    <asp:TableCell Width="40">
                        <asp:ImageButton ID="ibSettings" runat="server" ImageUrl="~/Images/wrench.png" Height="30" ToolTip="Panel Settings" OnClick="ibSettings_Click" />
                        <asp:button id="btnShowSettings" runat="server" style="display:none;" />
                        <ajaxtoolkit:ModalPopupExtender ID="mpeSettings" runat="server" PopupControlID="pnlSettings" 
                            TargetControlID="btnShowSettings" BackgroundCssClass="modalBackground" Y="100">
                        </ajaxtoolkit:ModalPopupExtender>
                        <asp:Panel ID="pnlSettings" runat="server" BorderWidth="1"  style="display:none">
                            <table cellpadding="5" cellspacing="5" style="background-color:#eeeeee;">
                                <tr>
                                    <td colspan="2" style="background-color:#22b9ec;">
                                        <asp:Label ID="Label5" runat="server" Text="Panel Settings " ForeColor="White" Font-Bold="true"/>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="Label2" runat="server" Text="Show Panel: "  Font-Size="Small" />
                                    </td>
                                    <td>
                                        <asp:CheckBox ID="cbShow" runat="server" Checked="true" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Label ID="Label6" runat="server" Font-Size="Small"  Text="Objects Per Line: "  />
                                    </td>
                                    <td>
                                        <asp:TextBox ID="tbColumns" runat="server" Text="5" Width="20px" />
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <asp:Button ID="btnSettingsClose" runat="server" Text="Close" OnClick="btnSettingsClose_Click"  />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </asp:TableCell>
                </asp:TableHeaderRow>
            </asp:Table>
        </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow ID="rowData" BackColor="#eeeeee">
        <asp:TableCell style="padding:10px;">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <asp:DataList ID="dlShortcuts" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" Width="100%" OnItemDataBound="dlShortcuts_ItemDataBound">
                        <ItemStyle Width="200"  HorizontalAlign="Center" VerticalAlign="Top"/>
                        <ItemTemplate>
                            
                                    <asp:HiddenField ID="hfShortcutID" runat="server" Value='<%# ((Wink.Shortcut)((IDataItemContainer)Container).DataItem).id %>' />
                            
                                    <asp:Table ID="Table2" runat="server">
                                        <asp:TableRow>
                                            <asp:TableCell VerticalAlign="Top" HorizontalAlign="Right">
                                                <asp:Label ID="Label1" runat="server" ForeColor="#b5c8cf" Font-Size="Small" Text='<%# ((IDataItemContainer)Container).DisplayIndex + 1 %>' />
                                            </asp:TableCell>
                                            <asp:TableCell HorizontalAlign="Center">
                                                <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/Shortcut.png" Height="75" OnClick="imgIcon_Click" 
                                                    CommandArgument='<%# ((Wink.Shortcut)((IDataItemContainer)Container).DataItem).id %>' ToolTip='<%# ((Wink.Shortcut)((IDataItemContainer)Container).DataItem).name %>' />
                                            </asp:TableCell>
                                            <asp:TableCell HorizontalAlign="Left" VerticalAlign="Bottom">
                                                <asp:ImageButton ID="ibInfo" runat="server" ImageUrl="~/Images/info.png" Height="20" ToolTip="Show Shortcut data" OnClick="ibInfo_Click" />

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
                                                                <asp:Textbox ID="tbJSON" runat="server" Text='<%# ((Wink.Shortcut)((IDataItemContainer)Container).DataItem).json %>' TextMode="MultiLine" Height="80" Width="400" ReadOnly="true" />
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
                                                                <asp:LinkButton ID="LinkButton1" runat="server" Text="Close" ForeColor="White" style="text-decoration: none;" OnClick="btnClose_Click" />
                                                            </asp:TableHeaderCell>
                                                        </asp:TableRow>
                                                    </asp:Table>
                                                </asp:Panel>
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell  HorizontalAlign="Center" ColumnSpan="3">
                                                <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Shortcut)((IDataItemContainer)Container).DataItem).displayName %>' Font-Size="small" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                        </ItemTemplate>
                    </asp:DataList>

                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
