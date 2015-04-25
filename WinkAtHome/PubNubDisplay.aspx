<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PubNubDisplay.aspx.cs" Inherits="WinkAtHome.PubNubDisplay" %>

<%@ Register TagPrefix="asp" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>PubNub</title>
</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" EnablePartialRendering="true" runat="server">
    </asp:ScriptManager>
    <div>
        <asp:UpdatePanel ID="UpdatePanelRight" runat="server" ChildrenAsTriggers="false"
            UpdateMode="Conditional">
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="txtMessage" />
                <asp:AsyncPostBackTrigger ControlID="UpdateTimer" EventName="Tick" />
            </Triggers>
            <ContentTemplate>
                <fieldset>
                    <asp:TextBox ID="txtMessage" runat="server" ReadOnly="true" BackColor="Black" ForeColor="Aqua"
                        Height="500px" TextMode="MultiLine" Wrap="true" AutoPostBack="false" Width="100%"></asp:TextBox>
                    <asp:Timer runat="server" ID="UpdateTimer" Interval="500" Enabled="false" OnTick="UpdateTimer_Tick" />
                </fieldset>
            </ContentTemplate>
        </asp:UpdatePanel>
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
        <br />
    </div>
    </form>
</body>
</html>
