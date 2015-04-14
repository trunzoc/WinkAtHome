<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Groups.ascx.cs" Inherits="WinkAtHome.Controls.Groups" %>

<asp:Table ID="Table1" runat="server" BorderColor="LightGray" BorderWidth="1"  BackColor="#22b9ec" Width="100%">
    <asp:TableHeaderRow BackColor="#22b9ec">
        <asp:TableHeaderCell HorizontalAlign="Left" style="padding:10px;">
            <asp:Label ID="lblHeader" runat="server" Text="Groups" ForeColor="White" />
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
            <asp:DataList ID="dlGroups" runat="server" RepeatColumns='<%# Convert.ToInt32(tbColumns.Text) %>' RepeatDirection="Horizontal" OnItemDataBound="dlGroups_ItemDataBound" Width="100%">
                <ItemStyle Width="200" Height="100px" HorizontalAlign="Center" />
                <ItemTemplate>
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <ContentTemplate>

                            <asp:HiddenField ID="hfMainCommand" runat="server" />
                            <asp:HiddenField ID="hfCurrentStatus" runat="server" />
                            <asp:HiddenField ID="hfLevelCommand" runat="server" />

                            <asp:Table ID="Table3" runat="server">
                                <asp:TableRow>
                                    <asp:TableCell ColumnSpan="2" Height="110" VerticalAlign="Top" HorizontalAlign="Center">
                                        <asp:Table ID="tblDefault" runat="server" Height="100%"> 
                                            <asp:TableRow>
                                                <asp:TableCell HorizontalAlign="Center">
                                                    <asp:ImageButton ID="imgIcon" runat="server" ImageUrl="~/Images/Groups/false.png" Height="100" OnClick="imgIcon_Click" 
                                                        CommandArgument='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).id %>' ToolTip='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).name %>' />
                                                </asp:TableCell>
                                                <asp:TableCell HorizontalAlign="Right">
                                                    <telerik:RadSlider ID="rsBrightness" runat="server" MinimumValue="0" MaximumValue="100" Orientation="Vertical" ToolTip="Off" Height="100"
                                                        ItemType="None" ShowIncreaseHandle="false" ShowDecreaseHandle="false" IsDirectionReversed="true" AutoPostBack="true" LiveDrag="false"
                                                        AnimationDuration="400" ThumbsInteractionMode="Free" OnValueChanged="rsBrightness_ValueChanged" DecreaseText='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).name %>'>
                                                    </telerik:RadSlider>
                                                </asp:TableCell>
                                            </asp:TableRow>
                                        </asp:Table>
                                    </asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow>
                                    <asp:TableCell HorizontalAlign="Center" VerticalAlign="Middle" style="padding-bottom:30px">
                                        <asp:Label ID="lblName" runat="server" Text='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).name %>' Font-Size="small" />
                                    </asp:TableCell>
                                    <asp:TableCell VerticalAlign="Top" HorizontalAlign="Right" Width="23" style="padding-right:3px;">
                                        <asp:ImageButton ID="ibInfo" runat="server" ImageUrl="~/Images/info.png" Height="20" ToolTip="Show Group data" OnClick="ibInfo_Click" />

                                        <asp:button id="btnShowInfo" runat="server" style="display:none;" />

                                        <ajaxtoolkit:ModalPopupExtender ID="mpeInfo" runat="server" PopupControlID="pnlInfo"
                                            TargetControlID="btnShowInfo" BackgroundCssClass="modalBackground" Y="100">
                                        </ajaxtoolkit:ModalPopupExtender>
                                        <asp:Panel ID="pnlInfo" runat="server" BorderWidth="1"  style="display:none">
                                            <asp:Table ID="Table6" runat="server" CellPadding="5" CellSpacing="5" BackColor="#eeeeee">
                                                <asp:TableRow>
                                                    <asp:TableCell ColumnSpan="2">
                                                        <asp:DataList ID="dlProperties" runat="server" RepeatColumns="2" Width="100%" RepeatLayout="Table">
                                                            <HeaderTemplate>
                                                                <table style="width:100%">
                                                            </HeaderTemplate>
                                                            <ItemTemplate>
                                                                    <td align="right" style="width:100px">
                                                                        <asp:Label ID="lblPropertyName" runat="server" Text='<%# Eval("Key") + ": " %>' Font-Size="Small" />
                                                                    </td>
                                                                    <td align="left">
                                                                        <asp:Label ID="lblPropertyValue" runat="server" Text='<%# Eval("Value") %>' Font-Size="Small" />
                                                                    </td>
                                                            </ItemTemplate>
                                                            <FooterTemplate>
                                                                </table>
                                                            </FooterTemplate>
                                                        </asp:DataList>
                                                    </asp:TableCell>
                                                </asp:TableRow>

                                                <asp:TableRow>
                                                    <asp:TableCell HorizontalAlign="Right" VerticalAlign="Top">
                                                        <asp:Label ID="lbl1" runat="server" Text="JSON:" Font-Size="Small" />
                                                    </asp:TableCell>
                                                    <asp:TableCell HorizontalAlign="Left">
                                                        <asp:Textbox ID="tbJSON" runat="server" Text='<%# ((Wink.Group)((IDataItemContainer)Container).DataItem).json %>' TextMode="MultiLine" Height="150" Width="400" ReadOnly="true" />
                                                    </asp:TableCell>
                                                </asp:TableRow>
                                                <asp:TableRow>
                                                    <asp:TableHeaderCell BackColor="#22b9ec" HorizontalAlign="Center" style="padding:10px;" ColumnSpan="2">
                                                        <asp:LinkButton ID="LinkButton1" runat="server" Text="Close" ForeColor="White" style="text-decoration: none;" OnClick="btnClose_Click" />
                                                    </asp:TableHeaderCell>
                                                </asp:TableRow>
                                            </asp:Table>
                                        </asp:Panel>
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