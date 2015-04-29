<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PubNubDisplay.ascx.cs" Inherits="WinkAtHome.Controls.PubNubDisplay" %>

<script type="text/javascript" language="javascript">

    var xPos, yPos;
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_pageLoaded(PageLoadedEventHandler);
    function PageLoadedEventHandler() {
        //alert("page loaded event handler");
    }

    prm.add_beginRequest(BeginRequestEventHandler);
    function BeginRequestEventHandler() {
        if ($get('<%= txtMessage.ClientID %>') != null) {
            xPos = $get('<%= txtMessage.ClientID %>').scrollLeft;
            yPos = $get('<%= txtMessage.ClientID %>').scrollTop;
        }
    }

    prm.add_endRequest(EndRequestEventHandler);
    function EndRequestEventHandler() {
        if ($get('<%= txtMessage.ClientID %>') != null) {
            $get('<%= txtMessage.ClientID %>').scrollLeft = xPos;
            $get('<%= txtMessage.ClientID %>').scrollTop = $get('<%= txtMessage.ClientID %>').scrollHeight;
        }
    }
</script>

<asp:HiddenField ID="hfSettingBase" runat="server" />

<asp:Table ID="Table1" runat="server" BorderColor="#22b9ec" BorderWidth="1" Width="100%" BackColor="#22b9ec"  CellPadding="0" CellSpacing="0">
    <asp:TableRow>
        <asp:TableCell>
            <asp:Table ID="Table7" runat="server" Width="100%">
                <asp:TableHeaderRow>
                    <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
                        <asp:Label ID="lblHeader" runat="server" Text="PubNub Subscription Feed" ForeColor="White" />
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
            <asp:UpdatePanel ID="UpdatePanelPubNub" runat="server" ChildrenAsTriggers="false"
                UpdateMode="Conditional">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="txtMessage" />
                </Triggers>
                <ContentTemplate>
                    <fieldset>
                        <asp:TextBox ID="txtMessage" runat="server" ReadOnly="true" BackColor="Black" ForeColor="#22b9ec"
                            Height="500px" TextMode="MultiLine" Wrap="true" AutoPostBack="false" Width="100%"></asp:TextBox>
                    </fieldset>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>