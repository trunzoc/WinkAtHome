<%@ Page Title="Settings" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="WinkAtHome.Settings" %>
<%@ Register Src="~/Controls/winkStatus.ascx" TagName="ucWinkStatus" TagPrefix="ucWS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">

    <script type="text/javascript">
        function saveConfirmation() {
            return confirm("Are you sure you want to save these changes?\n \n This cannot be undone");
        }

        function deleteConfirmation() {
            return confirm("Are you sure you want to wipe your settings?\n \n This cannot be undone?");
        }

        function cancelConfirmation() {
            return confirm("Any unsaved chances will be lost you made.\n \n Are you sure you want to leave the Settings page?");
        }
    </script>

    <asp:Table ID="Table1" runat="server" Width="800" BorderWidth="0" CellPadding="0" CellSpacing="0">
        <asp:TableHeaderRow>
            <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
                <asp:Label ID="lblHeader" runat="server" Text="Settings" ForeColor="White" />
            </asp:TableHeaderCell>
        </asp:TableHeaderRow>
        <asp:TableRow ID="rowWarning">
            <asp:TableCell>
                <asp:Label ID="lblMessage" runat="server" Text="" Font-Bold="true" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <asp:DataList ID="dlRequiredSettings" runat="server" Width="100%">
                    <HeaderTemplate>
                        <table width="100%">
                        <tr>
                            <td colspan="2" align="center">
                                <asp:Label ID="lblRequiredSettings" runat="server" Text="Required Settings" />
                            </td>
                        </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <asp:Label ID="lblKey" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).key %>' />
                            </td>
                            <td width="255">
                                <asp:TextBox ID="tbValue" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' Width="250" />
                                <asp:HiddenField ID="hfValue" runat="server" Value='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <AlternatingItemTemplate>
                        <tr style="background-color:#dddddd">
                            <td>
                                <asp:Label ID="lblKey" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).key %>' />
                            </td>
                            <td width="255">
                                <asp:TextBox ID="tbValue" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' Width="250" />
                                <asp:HiddenField ID="hfValue" runat="server" Value='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' />
                            </td>
                        </tr>
                    </AlternatingItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:DataList>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <hr />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <asp:DataList ID="dlAdditionalSettings" runat="server" width="100%">
                    <HeaderTemplate>
                        <table width="100%">
                        <tr>
                            <td colspan="2" align="center">
                                <asp:Label ID="lblAdditionalSettings" runat="server" Text="Additional Settings" />
                            </td>
                        </tr>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <asp:Label ID="lblKey" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).key %>' />
                            </td>
                            <td width="255">
                                <asp:TextBox ID="tbValue" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' Width="250" />
                                <asp:HiddenField ID="hfValue" runat="server" Value='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <AlternatingItemTemplate>
                        <tr style="background-color:#dddddd">
                            <td>
                                <asp:Label ID="lblKey" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).key %>' />
                            </td>
                            <td width="255">
                                <asp:TextBox ID="tbValue" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' Width="250" />
                                <asp:HiddenField ID="hfValue" runat="server" Value='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' />
                            </td>
                        </tr>
                    </AlternatingItemTemplate>
                    <FooterTemplate>
                        </table>
                    </FooterTemplate>
                </asp:DataList>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <table width="100%">
                    <tr>
                        <td align="left">
                            <asp:Button ID="btnSave" runat="server" Text="Save Settings" OnClick="btnSave_Click" OnClientClick="if ( ! saveConfirmation()) return false;" />
                        </td>
                        <td align="right">
                            <asp:Button ID="btnDefault" runat="server" Text="Exit Settings" OnClick="btnDefault_Click" CommandArgument="~/Default.aspx" OnClientClick="if ( ! cancelConfirmation()) return false;" />
                        </td>
                    </tr>
                </table>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow ID="rowInfo">
            <asp:TableCell>
                <table width="100%">
                    <tr>
                        <td colspan="2">
                            <hr />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2" align="center">
                            <asp:Label ID="Label3" runat="server" Text="Informational Only" />
                        </td>
                    </tr>
                    <tr>
                        <td align="left" width="200">
                            <asp:Label ID="Label2" runat="server" Text="App Version" />
                        </td>
                        <td align="left">
                            <asp:TextBox ID="tbVersion" runat="server" ReadOnly="true" Width="100%" />
                        </td>
                    </tr>
                    <tr>
                        <td align="left">
                            <asp:Label ID="lblDBPath" runat="server" Text="Database Path" />
                        </td>
                        <td align="left">
                            <asp:TextBox ID="tbDBPath" runat="server" ReadOnly="true" Width="100%" />
                        </td>
                    </tr>
                    <tr>
                        <td align="left">
                            <asp:Label ID="Label4" runat="server" Text="Current Access Token" />
                        </td>
                        <td align="left">
                            <asp:TextBox ID="tbAccessToken" runat="server" ReadOnly="true" Width="100%" />
                        </td>
                    </tr>
                </table>
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell>
                <hr />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell HorizontalAlign="Center">
                <asp:Label ID="Label1" runat="server" Text="Danger Ahead!" ForeColor="Red" Font-Bold="true" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow>
            <asp:TableCell HorizontalAlign="Justify">
                <asp:Button ID="btnManualEdit" runat="server" Text="Manually Edit Settings" OnClick="btnManualEdit_Click" />&nbsp;
                <asp:Button ID="btnDeviceJSON" runat="server" Text="Show Device JSON" OnClick="btnRawDevData_Click" CommandArgument="devices" />&nbsp;
                <asp:Button ID="btnRobotJSON" runat="server" Text="Show Robot JSON" OnClick="btnRawDevData_Click" CommandArgument="robots" />&nbsp;
                <asp:Button ID="btnWipe" runat="server" Text="Wipe Settings" OnClick="btnWipe_Click" OnClientClick="if ( ! deleteConfirmation()) return false;" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow ID="rowEditText" Visible="false">
            <asp:TableCell>
                <asp:TextBox ID="tbEdit" runat="server" TextMode="MultiLine" Height="250px" Width="100%" />
            </asp:TableCell>
        </asp:TableRow>
        <asp:TableRow ID="rowEditButton" Visible="false">
            <asp:TableCell>
                <asp:Button ID="btnSaveEdit" runat="server" Text="Save Edit" OnClick="btnSaveEdit_Click" OnClientClick="if ( ! saveConfirmation()) return false;" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cphLeft" runat="server">
</asp:Content>