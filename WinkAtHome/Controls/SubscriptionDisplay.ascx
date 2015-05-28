<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SubscriptionDisplay.ascx.cs" Inherits="WinkAtHome.Controls.SubscriptionDisplay" %>

<asp:HiddenField ID="hfSettingBase" runat="server" />
<asp:HiddenField ID="hfLogLength" runat="server" />

<asp:Table ID="Table1" runat="server" BorderColor="#22b9ec" BorderWidth="1" Width="100%" BackColor="#22b9ec"  CellPadding="0" CellSpacing="0">
    <asp:TableRow>
        <asp:TableCell>
            <asp:Table ID="Table7" runat="server" Width="100%">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
                        <asp:Label ID="lblHeader" runat="server" Text="Subscription Feed" ForeColor="White" />
                    </asp:TableHeaderCell>
                    <asp:TableCell Width="40">
                        <asp:ImageButton ID="ibPause" runat="server" ImageUrl="~/Images/pause.png" Height="27" ToolTip="Pause Subscription Messages" OnClick="ibPause_Click" />
                    </asp:TableCell>
                    <asp:TableCell Width="40">
                        <asp:ImageButton ID="ibErase" runat="server" ImageUrl="~/Images/erase.png" Height="27" ToolTip="Clear Subscription Messages" OnClick="ibErase_Click" />
                    </asp:TableCell>
                    <asp:TableCell Width="40">
                        <asp:ImageButton ID="ibReconnect" runat="server" ImageUrl="~/Images/arrows.png" Height="27" ToolTip="Reconnect to Subscription Service" OnClick="ibReconnect_Click" />
                    </asp:TableCell>
                    <asp:TableCell Width="40">
                        <asp:ImageButton ID="ibSettings" runat="server" ImageUrl="~/Images/wrench.png" Height="30" ToolTip="Panel Settings" OnClick="ibSettings_Click" />
                        <asp:button id="btnShowSettings" runat="server" style="display:none;" />
                        <ajaxtoolkit:ModalPopupExtender ID="mpeSettings" runat="server" PopupControlID="pnlSettings" 
                            TargetControlID="btnShowSettings" CancelControlID="imgInfoClose" BackgroundCssClass="modalBackground" Y="50">
                        </ajaxtoolkit:ModalPopupExtender>
                        <asp:Panel ID="pnlSettings" runat="server" BorderWidth="1"  style="display:none">
                            <asp:Table ID="Table9" runat="server" CellPadding="5" CellSpacing="5" BackColor="#eeeeee" BorderWidth="1" Width="300">
                                <asp:TableHeaderRow BackColor="#22b9ec">
                                    <asp:TableCell ColumnSpan="2">
                                        <asp:Table ID="Table10" runat="server" CellPadding="0" CellSpacing="0" Width="100%">
                                            <asp:TableRow>
                                                <asp:TableHeaderCell HorizontalAlign="Center">
                                                    <asp:Label ID="Label7" runat="server" Text="Section Settings" ForeColor="White" />
                                                </asp:TableHeaderCell>
                                                <asp:TableHeaderCell Width="20" HorizontalAlign="Right">
                                                    <asp:Image ID="imgInfoClose" runat="server" ImageUrl="~/Images/close.png" Width="20" />
                                                </asp:TableHeaderCell>
                                                <asp:TableCell Width="10"></asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>
                                    </asp:TableCell>
                                </asp:TableHeaderRow>
                                <asp:TableRow>
                                    <asp:TableCell>
                                        <asp:Label ID="Label1" runat="server" Text="Show Panel: "  Font-Size="Small" />
                                    </asp:TableCell>
                                    <asp:TableCell>
                                        <asp:CheckBox ID="cbShow" runat="server" Checked="true" />
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell>
                                        <asp:Label ID="Label2" runat="server" Text="Records To Show: "  Font-Size="Small" />
                                    </asp:TableCell>
                                    <asp:TableCell>
                                        <asp:TextBox ID="tbLogLength" runat="server" />
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow BackColor="#22b9ec">
                                    <asp:TableHeaderCell HorizontalAlign="Center" style="padding:10px;" ColumnSpan="2">
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
            <asp:UpdatePanel ID="upData" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:TextBox ID="txtMessage" runat="server" ReadOnly="true" BackColor="Black" ForeColor="#22b9ec" Font-Size="Smaller"
                        Height="500" TextMode="MultiLine" Wrap="true" AutoPostBack="false" Width="100%" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>