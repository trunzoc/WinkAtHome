<%@ Page Title="" Language="C#" MasterPageFile="~/WinkAtHome.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="WinkAtHome.Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphMain" runat="server">
    <asp:Repeater ID="rptSettings" runat="server">
        <HeaderTemplate>
            <table>
        </HeaderTemplate>
        <ItemTemplate>
            <tr>
                <td>
                    <asp:Label ID="lblKey" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).key %>' />
                </td>
                <td>
                    <asp:TextBox ID="tbValue" runat="server" Text='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' Width="400" />
                    <asp:HiddenField ID="hfValue" runat="server" Value='<%# ((WinkAtHome.SettingMgmt.Setting)Container.DataItem).value %>' />
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </table>
        </FooterTemplate>
    </asp:Repeater>
    <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
</asp:Content>
