<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Robots.ascx.cs" Inherits="WinkAtHome.Controls.Robots" %>

<asp:Table ID="Table1" runat="server" BorderColor="LightGray" BorderWidth="1"  BackColor="#22b9ec" Width="100%" >
    <asp:TableHeaderRow BackColor="#22b9ec">
        <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
            <asp:Label ID="lblHeader" runat="server" Text="Robots" ForeColor="White" />
        </asp:TableHeaderCell>
        <asp:TableCell HorizontalAlign="right">
            <asp:Label ID="Label1" runat="server" Text="Columns: " ForeColor="White" />
            <asp:TextBox ID="tbColumns" runat="server" Text="5" OnTextChanged="tbColumns_TextChanged" Width="20px" AutoPostBack="true" />
        </asp:TableCell>
        <asp:TableCell Width="1" style="padding-left:10px; padding-right:10px;">
            <asp:ImageButton ID="ibExpand" runat="server" ImageUrl="~/Images/expand.png" Height="20" OnClick="ibExpand_Click" ToolTip="Show/Hide This Panel"/>
        </asp:TableCell>
    </asp:TableHeaderRow>
    <asp:TableRow ID="rowData" BackColor="#eeeeee">
        <asp:TableCell style="padding:10px;" ColumnSpan="3">
            <asp:DataList ID="dlRobots" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" OnItemDataBound="dlRobots_ItemDataBound" Width="100%">
                <ItemStyle Height="175" HorizontalAlign="Center" />
                <ItemTemplate>
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <ContentTemplate>

                            <asp:Table ID="Table2" runat="server" Width="100%">
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Center">
                                        <asp:Table ID="Table3" runat="server">
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="Right">
                                                    <asp:ImageButton ID="imgIcon" runat="server" ImageUrl='<%# "~/Images/Robots/" + ((Wink.Robot)((IDataItemContainer)Container).DataItem).enabled + ".png" %>' Height="75" OnClick="imgIcon_Click" 
                                                        CommandArgument='<%# ((Wink.Robot)((IDataItemContainer)Container).DataItem).id %>' CommandName='<%# ((Wink.Robot)((IDataItemContainer)Container).DataItem).enabled %>' ToolTip='<%# ((Wink.Robot)((IDataItemContainer)Container).DataItem).name %>' />
                                                </asp:TableCell>
                                                <asp:TableCell HorizontalAlign="Left" VerticalAlign="Bottom">
                                                    <asp:ImageButton ID="ibInfo" runat="server" ImageUrl="~/Images/info.png" Height="20" ToolTip="Show Device data" />
                                                    <ajaxtoolkit:ModalPopupExtender ID="ModalPopupExtender1" runat="server" PopupControlID="pnlInfo" 
                                                        TargetControlID="ibInfo" CancelControlID="btnClose" BackgroundCssClass="modalBackground" Y="200">
                                                    </ajaxtoolkit:ModalPopupExtender>
                                                    <asp:Panel ID="pnlInfo" runat="server" BorderWidth="1"  style="display:none">
                                                        <table cellpadding="5" cellspacing="5" style="background-color:#eeeeee;">
                                                            <tr>
                                                                <td>
                                                                    <asp:DataList ID="dlProperties" runat="server">
                                                                        <HeaderTemplate>
                                                                            <table>
                                                                        </HeaderTemplate>
                                                                        <ItemTemplate>
                                                                            <tr>
                                                                                <td align="right">
                                                                                    <asp:Label ID="lblPropertyName" runat="server" Text='<%# Eval("Key") + ": " %>' />
                                                                                </td>
                                                                                <td align="left">
                                                                                    <asp:Label ID="lblPropertyValue" runat="server" Text='<%# Eval("Value") %>' />
                                                                                </td>
                                                                            </tr>
                                                                        </ItemTemplate>
                                                                        <FooterTemplate>
                                                                            </table>
                                                                        </FooterTemplate>
                                                                    </asp:DataList>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td align="left">
                                                                    <asp:Label ID="Label4" runat="server" Text="JSON:" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Textbox ID="tbJSON" runat="server" Text='<%# ((Wink.Robot)((IDataItemContainer)Container).DataItem).json %>' TextMode="MultiLine" Height="200" Width="400" ReadOnly="true" />
                                                                </td>
                                                            </tr>
                                                            </tr>
                                                            <tr>
                                                                <td>
                                                                    <asp:Button ID="btnClose" runat="server" Text="Close" />
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </asp:Panel>
                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Center" ColumnSpan="2">
                                        <asp:TextBox ID="tbName" runat="server" Text='<%# ((Wink.Robot)((IDataItemContainer)Container).DataItem).name %>' Font-Size="small" TextMode="MultiLine" Wrap="true"
                                            style="border:0px; text-align:center; overflow:hidden" BackColor="Transparent" />
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Center" ColumnSpan="2">
                                        <asp:Label ID="Label2" runat="server" Text="Last Triggered:" Font-Size="small" />
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Center" ColumnSpan="2">
                                        <asp:Label ID="Label3" runat="server" Text='<%# ((Wink.Robot)((IDataItemContainer)Container).DataItem).last_fired.ToString().Contains("1/1/1970") ? "Never" : ((Wink.Robot)((IDataItemContainer)Container).DataItem).last_fired.ToString() %>' Font-Size="small" />
                                    </asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>

                         </ContentTemplate>
                    </asp:UpdatePanel>
                </ItemTemplate>
            </asp:DataList>
        </asp:TableCell>
    </asp:TableRow>
</asp:Table>
