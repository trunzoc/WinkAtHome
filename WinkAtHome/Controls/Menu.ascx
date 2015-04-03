<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Menu.ascx.cs" Inherits="WinkAtHome.Controls.Menu" %>
<link rel="stylesheet" href="Styles/custom.css">
<link rel="stylesheet" href="Styles/dashboard.css">

<asp:Table ID="Table1" runat="server" Width="100%" CellPadding="5" CellSpacing="0">
    <asp:TableHeaderRow>
        <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" Height="30">
            <asp:Label ID="Label1" runat="server" Text="&nbsp;Wink Items" ForeColor="White" />
        </asp:TableHeaderCell>
    </asp:TableHeaderRow>
    <asp:TableRow>
        <asp:TableCell>
            <asp:ListBox ID="lbMenu" runat="server" SelectionMode="Single" Width="100%" style="border:hidden" OnSelectedIndexChanged="lbMenu_SelectedIndexChanged" AutoPostBack="true" Font-Size="medium" BackColor="#eeeeee" Rows="6" >
                <asp:ListItem Text="Devices" Value="Devices" />
                <asp:ListItem Text="Groups" Value="Groups" />
                <asp:ListItem Text="Shortcuts" Value="Shortcuts" />
                <asp:ListItem Text="Robots (Coming Soon)" Value="Robots" />
            </asp:ListBox>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>

