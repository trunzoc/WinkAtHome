<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WinkAtHome.Login" %>
<%@ Register Src="~/Controls/winkStatus.ascx" TagName="ucWinkStatus" TagPrefix="ucWS" %>
<%@ Register Src="~/Controls/Footer.ascx" TagName="ucFooter" TagPrefix="ucF" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Login - Wink@Home</title>
    
    <style type="text/css">
        *
        {
                font-family: Tahoma;
        }
    </style>
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
                                <asp:Label ID="ibVersion" runat="server" Font-Size="Smaller" Text="" ForeColor="White" />
                            </td>
                            <td>
                        </tr>
                    </table>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell VerticalAlign="Top" Width="265px" style="padding-right:40px;">
                    <ucWS:ucWinkStatus ID="ucWinkStatus" runat="server" />
                    
                    <script async src="//pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>
                    <!-- WinkAtHome Menu -->
                    <ins class="adsbygoogle"
                            style="display:inline-block;width:300px;height:250px"
                            data-ad-client="ca-pub-9030204945381170"
                            data-ad-slot="1966087641"></ins>
                    <script>
                        (adsbygoogle = window.adsbygoogle || []).push({});
                    </script>
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="left"  VerticalAlign="Top">
                    <asp:Table ID="Table1" runat="server" Width="100%">
                        <asp:TableHeaderRow>
                            <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Left" style="padding:10px;">
                                <asp:Label ID="lblHeader" runat="server" Text="LOGIN" ForeColor="White" />
                            </asp:TableHeaderCell>
                        </asp:TableHeaderRow>
                        <asp:TableRow>
                            <asp:TableCell>
                                <asp:Panel ID="pnlLocalLogin" runat="server">
                                    <asp:Table ID="Table3" runat="server">
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label1" runat="server" Text="Username:" />
                                            </asp:TableCell>
                                            <asp:TableCell>
                                                <asp:TextBox ID="tbUsername" runat="server" Width="200" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label2" runat="server" Text="Password:" />
                                            </asp:TableCell>
                                            <asp:TableCell>
                                                <asp:TextBox ID="tbPassword" runat="server" TextMode="Password" Width="200" />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                                </asp:Panel>
                                
                                <asp:Panel ID="pnlWinkLogin" runat="server">
                                    <asp:Table ID="Table4" runat="server">
                                        <asp:TableRow>
                                            <asp:TableCell>
                                                <asp:Label ID="Label4" runat="server" Text="This site is secured by Wink. Press the login button to be redirected to their site for validation.<br /><br />When successfully validated, Wink will return you to Wink@Home to continue." />
                                            </asp:TableCell>
                                        </asp:TableRow>
                                    </asp:Table>
                                </asp:Panel>

                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>
                                <hr />
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>
                                <asp:Label ID="Label3" runat="server" Text="Remember Me For 30 Days:" />
                                <asp:CheckBox ID="cbRemember" runat="server" />
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>
                                <asp:Button ID="btnLogin" runat="server" Text="Log In" OnClick="btnLogin_Click" CommandArgument="local" />
                            </asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>
                                <asp:Label ID="lblMessage" runat="server" Text="" />
                            </asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow ID="rowFooter">
                <asp:TableCell ColumnSpan="2" HorizontalAlign="Center">
                    <ucF:ucFooter ID="ucFooter" runat="server" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </div>
    </form>
</body>
</html>

