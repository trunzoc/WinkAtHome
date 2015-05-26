<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Footer.ascx.cs" Inherits="WinkAtHome.Controls.Footer" %>

<asp:Table ID="Table1" runat="server" Width="100%">
    <asp:TableRow ID="rowFooter">
        <asp:TableCell HorizontalAlign="Center">
            <asp:Label ID="Label8" runat="server" Font-Size="Small" Text="© 2015 Craig Trunzo. All rights reserved.  All images are the property of their respective owners. This website is not a product of Quirky or Wink, and they hold no responsibility over it's functionality." />
        </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow ID="rowEmailSupport">
        <asp:TableCell HorizontalAlign="Center">
            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="mailto:support@winkathome.net" Font-Size="Small">support@winkathome.net</asp:HyperLink>
        </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow ID="rowDevBegging">
        <asp:TableCell HorizontalAlign="Center">
            <asp:Table ID="Table2" runat="server">
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="Center" ColumnSpan="2">
                        <asp:Label ID="Label1" runat="server" Text="Help Support The Developer" Font-Size="Small" Font-Bold="true" />
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell HorizontalAlign="Center">
                        <asp:ImageButton ID="ibPayPal" runat="server" Height="30" ImageUrl="https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif" OnClientClick="javascript:window.open('https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5NLDWXRPQXSN6', '_blank');" AlternateText="PayPal - The safer, easier way to pay online!" />
                    </asp:TableCell>
                    <asp:TableCell VerticalAlign="Top">
                        <asp:ImageButton ID="ibYouTube" runat="server" Height="80" ImageUrl="http://www.youtube.com/yt/brand/media/image/YouTube-logo-full_color.png" OnClientClick="javascript:window.open('https://www.youtube.com/user/trunzoc/videos', '_blank');" AlternateText="My YouTube Channel" />
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
        </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow ID="rowBottomAds">
        <asp:TableCell>
            <script async src="//pagead2.googlesyndication.com/pagead/js/adsbygoogle.js"></script>
            <!-- WinkAtHome bottom Leaderboard -->
            <ins class="adsbygoogle"
                    style="display:inline-block;width:728px;height:90px"
                    data-ad-client="ca-pub-9030204945381170"
                    data-ad-slot="1047757646"></ins>
            <script>
                (adsbygoogle = window.adsbygoogle || []).push({});
            </script>
        </asp:TableCell>
    </asp:TableRow>
    <asp:TableRow ID="rowTracking">
        <asp:TableCell>
            <script>
                (function (i, s, o, g, r, a, m) {
                    i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                        (i[r].q = i[r].q || []).push(arguments)
                    }, i[r].l = 1 * new Date(); a = s.createElement(o),
                    m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
                })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

                ga('create', 'UA-63069282-1', 'auto');
                ga('send', 'pageview');

            </script>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
