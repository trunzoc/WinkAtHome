<%@ Page Title="Settings" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="WinkAtHome.Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <script type="text/javascript">
        function saveConfirmation() {
            return confirm("Are you sure you want to save these changes?\n \n This cannot be undone");
        }

        function deleteConfirmation() {
            return confirm("Are you sure you want to wipe your settings\n \n This cannot be undone?");
        }
    </script>
    <asp:Table ID="Table1" runat="server">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell ColumnSpan="2" BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
            <asp:Label ID="lblHeader" runat="server" Text="Settings" ForeColor="White" />
        </asp:TableHeaderCell>
    </asp:TableHeaderRow>
        <asp:TableRow>
            <asp:TableCell ColumnSpan="2">
                <asp:DataList ID="rptSettings" runat="server">
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
            <asp:TableCell HorizontalAlign="Left" ColumnSpan="2">
                <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" OnClientClick="if ( ! saveConfirmation()) return false;" />
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
            <asp:TableCell ColumnSpan="3">
                <asp:TextBox ID="tbEdit" runat="server" TextMode="MultiLine" Height="250px" Width="100%" />
                <br />
                <asp:Button ID="btnSaveEdit" runat="server" Text="Save Edit" OnClick="btnSaveEdit_Click" OnClientClick="if ( ! saveConfirmation()) return false;" />
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
</asp:Content>
