<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="WinkAtHome.Settings" %>
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
                            <td align="center" style="vertical-align:middle">
                                <asp:LinkButton ID="lbDashboard" runat="server" Text="Dashboard" ForeColor="White" Font-Size="XX-Large" OnClick="lbDashboard_Click" OnClientClick="if ( ! cancelConfirmation()) return false;" style="text-decoration: none;" />
                            </td>
                            <td align="center" style="vertical-align:middle">
                                <asp:LinkButton ID="lbSettings" runat="server" Text="Settings" ForeColor="White" Font-Size="XX-Large" style="text-decoration: none;" />
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
                            <asp:TableHeaderCell ColumnSpan="2" BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
                                <asp:Label ID="lblHeader" runat="server" Text="Settings" ForeColor="White" />
                            </asp:TableHeaderCell>
                        </asp:TableHeaderRow>
                        <asp:TableRow>
                            <asp:TableCell ColumnSpan="2">
                                <asp:DataList ID="dlSettings" runat="server">
                                    <HeaderTemplate>
                                        <table>
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
                            <asp:TableCell HorizontalAlign="Left">
                                <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" OnClientClick="if ( ! saveConfirmation()) return false;" />
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Right">
                                <asp:Button ID="btnDashboard" runat="server" Text="return to Dashboard" OnClick="lbDashboard_Click" OnClientClick="if ( ! cancelConfirmation()) return false;" />
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell ColumnSpan="2">
                                &nbsp;
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell ColumnSpan="2">
                                <asp:Label ID="Label1" runat="server" Text="Danger Ahead!" ForeColor="Red" Font-Bold="true" />
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell HorizontalAlign="Left" Width="100px">
                                <asp:Button ID="btnManualEdit" runat="server" Text="Manual Edit" OnClick="btnManualEdit_Click" />
                            </asp:TableCell>
                            <asp:TableCell HorizontalAlign="Left">
                                <asp:Button ID="btnWipe" runat="server" Text="Wipe Settings" OnClick="btnWipe_Click" OnClientClick="if ( ! deleteConfirmation()) return false;" />
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow ID="rowEdit" Visible="false">
                            <asp:TableCell ColumnSpan="2">
                                <asp:TextBox ID="tbEdit" runat="server" TextMode="MultiLine" Height="250px" Width="100%" />
                                <br />
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

