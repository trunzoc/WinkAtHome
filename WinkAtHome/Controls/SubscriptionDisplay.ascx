<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SubscriptionDisplay.ascx.cs" Inherits="WinkAtHome.Controls.SubscriptionDisplay" %>

<script type="text/javascript">
    function setHeight() {
        var hfMainHeight = document.getElementById('<%= hfMainHeight.ClientID%>');

        if (hfMainHeight.value == 0) {
            var mainHeight = getMainHeight();
            var pnlEvents = document.getElementById('<%= pnlEvents.ClientID%>');
            pnlEvents.style.height = mainHeight - 48 + "px";
        }
    }
</script>

<asp:HiddenField ID="hfSettingBase" runat="server" />
<asp:HiddenField ID="hfMainHeight" runat="server" />

<asp:Table ID="tblData" runat="server" BorderColor="#22b9ec" BorderWidth="1" Width="100%" Height="100%" BackColor="#22b9ec" CellPadding="0" CellSpacing="0">
    <asp:TableRow ID="rowFullHeader">
        <asp:TableCell>
            <asp:Table ID="Table7" runat="server" Width="100%">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
                        <asp:Label ID="lblHeader" runat="server" Text="Log Events" ForeColor="White" />
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
    <asp:TableRow ID="rowShortHeader">
        <asp:TableCell>
            <asp:Table ID="Table3" runat="server" Width="100%">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
                        <asp:Label ID="Label3" runat="server" Text="Log Events" ForeColor="White" />
                    </asp:TableHeaderCell>
                    <asp:TableCell Width="30">
                        <asp:ImageButton ID="ibPauseShort" runat="server" ImageUrl="~/Images/pause.png" Height="27" ToolTip="Pause Subscription Messages" OnClick="ibPause_Click" />
                    </asp:TableCell>
                </asp:TableHeaderRow>
            </asp:Table>
        </asp:TableCell>
    </asp:TableRow>

    <asp:TableRow ID="rowData" BackColor="#eeeeee">
        <asp:TableCell style="padding:3px;">
            <asp:UpdatePanel ID="upData" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Panel ID="pnlEvents" runat="server" ScrollBars="Auto" Height="800" Width="100%" >
                        <asp:DataList ID="dlEvents" runat="server" ItemStyle-Wrap="true" Width="100%" ShowHeader="false">
                            <SeparatorTemplate>
                                <hr Color="#cccccc" />
                            </SeparatorTemplate>
                            <HeaderTemplate>
                                <asp:Label ID="lblLatest" runat="server" Text='<%# "Showing " + logLength.ToString() + " Latest Messages" %>' Font-Size="x-Small" Font-Bold="true" />
                                <hr Color="#cccccc" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:Table ID="Table2" runat="server" Width="100%" CellPadding="1" CellSpacing="0">
                                    <asp:TableRow>
                                        <asp:TableCell>
                                            <asp:Label ID="lblDevice" runat="server" Text='<%# ((WinkAtHome.WinkEvent)((IDataItemContainer)Container).DataItem).objectType %>' Font-Size="Small" Font-Bold="true" />
                                        </asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow>
                                        <asp:TableCell>
                                            <asp:Label ID="lblTimestamp" runat="server" Text='<%# WinkAtHome.Common.getLocalTime(((WinkAtHome.WinkEvent)((IDataItemContainer)Container).DataItem).messageReceived).ToString() %>' Font-Size="X-Small" />
                                        </asp:TableCell>
                                    </asp:TableRow>
                                    <asp:TableRow>
                                        <asp:TableCell>
                                            <span style="font-size:x-small">
                                                <%# ((WinkAtHome.WinkEvent)((IDataItemContainer)Container).DataItem).text.Replace(",", ", \r\n") %>
                                            </span>
                                        </asp:TableCell>
                                    </asp:TableRow>
                                </asp:Table>
                            </ItemTemplate>
                        </asp:DataList>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>