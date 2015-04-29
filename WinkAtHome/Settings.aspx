<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="WinkAtHome.Settings" MaintainScrollPositionOnPostback="true" %>
<%@ Register Src="~/Controls/winkStatus.ascx" TagName="ucWinkStatus" TagPrefix="ucWS" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Settings - Wink@Home</title>
    
    <style type="text/css">
        *
        {
                font-family: Tahoma;
        }
    </style>

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
</head>

<body style="background-color:#eeeeee;">
    <form id="form2" runat="server">
    <div>
        <asp:Table ID="Table2" runat="server" BorderWidth="0" CellPadding="0" CellSpacing="0" Width="100%">
            <asp:TableRow>
                <asp:TableCell ColumnSpan="2" style="padding-bottom:20px" VerticalAlign="Top">
                    <table style="background-color:#22b9ec;" width="100%" border="0">
                        <tr>
                            <td align="left" width="170px">
                                <asp:Image ID="Image1" runat="server" ImageUrl="~/Images/WinkatHome.png" Height="75px" />
                            </td>
                            <td align="left" width="100">
                                <asp:Label ID="lblVersion" runat="server" Text="" Font-Size="Smaller" ForeColor="White"/>
                            </td>
                            <td align="center" style="vertical-align:middle">
                                <asp:LinkButton ID="lbContol" runat="server" Text="CONTROL" ForeColor="White" Font-Size="X-Large" Font-Bold="true" OnClick="btnDefault_Click" CommandArgument="~/Control.aspx" style="text-decoration: none;" />
                            </td>
                            <td align="center" style="vertical-align:middle">
                                <asp:LinkButton ID="lbMonitor" runat="server" Text="MONITOR" ForeColor="White" Font-Size="X-Large" Font-Bold="true" OnClick="btnDefault_Click" CommandArgument="~/Monitor.aspx" style="text-decoration: none;" />
                            </td>
                            <td align="right" width="280">
                                &nbsp;
                            </td>
                        </tr>
                    </table>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell VerticalAlign="Top" Width="265px" style="padding-right:40px;">
                    <ucWS:ucWinkStatus ID="ucWinkStatus" runat="server" />
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="left"  VerticalAlign="Top">
                    <asp:Table ID="Table1" runat="server">
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
                                            <td>
                                                <asp:TextBox ID="tbValue" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' Width="250" />
                                                <asp:HiddenField ID="hfValue" runat="server" Value='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
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
                        <asp:TableRow>
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
                                        <td align="left" width="150">
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
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </div>
    </form>
</body>
</html>

