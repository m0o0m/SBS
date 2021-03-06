﻿<%@ Page Language="VB" MasterPageFile="Player.master" AutoEventWireup="false"
    CodeFile="WeekBalance.aspx.vb" Inherits="SBSPlayer.WeekBalance" Title="Untitled Page" %>

<%@ Register Assembly="WebsiteLibrary" Namespace="WebsiteLibrary" TagPrefix="wlb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cphBody" runat="Server">
    
    <div class="row">
        <div class="col-lg-12">
            <div class="page-title-breadcrumb">
                <div class="page-header pull-left">
                    <div class="page-title pull-left mrm">
                        Section
                    </div>
                    <span class="label label-grey pull-left"  style="font-size: large">Weekly Balance</span>
                </div>
                <div class="clearfix">
                </div>
            </div>
        </div>
    </div>

    <div class="mbxl"></div>

    <div class="panel panel-grey">
        <div class="panel-heading">Filter</div>
        <div class="panel-body">
            <div class="form-group">
                <label class="col-md-2 control-label">Date Range</label>
                <div class="col-md-4">
                    <wlb:CDropDownList ID="ddlWeeks" runat="server" CssClass="form-control" hasOptionalItem="false"
                        AutoPostBack="true" OnSelectedIndexChanged="SelectedIndexChanged" />
                </div>
            </div>
        </div>
    </div>

    <div class="panel panel-grey">
        <div class="panel-heading">Result</div>
        <div class="panel-body">
            <asp:DataGrid ID="dgPlayers" runat="server" AutoGenerateColumns="false"
                CssClass="table table-hover table-bordered">
                <HeaderStyle CssClass="tableheading" HorizontalAlign="Center" />
                <ItemStyle HorizontalAlign="Center" />
                <AlternatingItemStyle HorizontalAlign="Center" />
                <Columns>
                    <asp:TemplateColumn HeaderText="Week Of" ItemStyle-HorizontalAlign="Left" ItemStyle-Width="210px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnWeekOf" runat="server" CommandName="VIEW_WEEK_HISTORY" 
                                CommandArgument='<%# Container.DataItem("Title") %>' Text='<%# Container.DataItem("Title") %>'
                                ToolTip="View History" />
                            <asp:HiddenField ID="HFMonday" Value='<%# Container.DataItem("ThisMonday") %>' runat="server" />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Mon" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnMon" runat="server" CommandName="VIEW_HISTORY" Font-Underline="false"
                                CommandArgument='<%# Container.DataItem("ThisMonday")%>' Text='<%#Container.DataItem("Mon")%>' ForeColor='<%# If(Container.DataItem("Mon") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Tues" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnTues" runat="server" CommandName="VIEW_HISTORY"
                                CommandArgument='<%# SBCBL.std.SafeDate( Container.DataItem("ThisMonday")).AddDays(1) %>'
                                Text='<%#Container.DataItem("Tues")%>'  ForeColor='<%# If(Container.DataItem("Tues") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Wed" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnWed" runat="server" CommandName="VIEW_HISTORY"
                                CommandArgument='<%# SBCBL.std.SafeDate( Container.DataItem("ThisMonday")).AddDays(2) %>'
                                Text='<%#Container.DataItem("Wed") %>' ForeColor='<%# If(Container.DataItem("Wed") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Thurs" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnThurs" runat="server" CommandName="VIEW_HISTORY" Font-Underline="false"
                                CommandArgument='<%# SBCBL.std.SafeDate( Container.DataItem("ThisMonday")).AddDays(3) %>'
                                Text='<%#Container.DataItem("Thurs")%>' ForeColor='<%# If(Container.DataItem("Thurs") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Fri" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnFri" runat="server" CommandName="VIEW_HISTORY" 
                                CommandArgument='<%# SBCBL.std.SafeDate( Container.DataItem("ThisMonday")).AddDays(4) %>'
                                Text='<%#Container.DataItem("Fri")%>' ForeColor='<%# If(Container.DataItem("Fri") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Sat" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnSat" runat="server" CommandName="VIEW_HISTORY" 
                                CommandArgument='<%# SBCBL.std.SafeDate( Container.DataItem("ThisMonday")).AddDays(5) %>'
                                Text='<%#Container.DataItem("Sat")%>' ForeColor='<%# If(Container.DataItem("Sat") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Sun" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:LinkButton ID="lbnSun" runat="server" CommandName="VIEW_HISTORY" 
                                CommandArgument='<%# SBCBL.std.SafeDate( Container.DataItem("ThisMonday")).AddDays(6) %>'
                                Text='<%#Container.DataItem("Sun")%>' ForeColor='<%# If(Container.DataItem("Sun") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' />
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Total" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:Label ID="lblTotal" runat="server" Text='<%#SBCBL.std.SafeInteger(Container.DataItem("Net"))%>'
                                 ForeColor='<%# If(Container.DataItem("Net") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' ></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Pending" ItemStyle-Width="75px">
                        <ItemTemplate>
                            <asp:Label ID="lblPending" runat="server" Text='<%# FormatNumber(SBCBL.std.SafeDouble(Container.DataItem("Pending")), SBCBL.std.GetRoundMidPoint) %>'
                                 ForeColor='<%# If(Container.DataItem("Pending") < 0, Drawing.Color.Red, Drawing.Color.Blue)%>' ></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
        </div>
    </div>

    <div id="trchart" runat="server" class="panel panel-grey">
        <div class="panel-heading"></div>
        <div class="panel-body">
            <object classid="clsid:d27cdb6e-ae6d-11cf-96b8-444553540000" codebase="/FusionCharts_Enterprise/swflash.cab"
                width="490" height="350" id="chart1">
                <param name="movie" value="/FusionCharts_Enterprise/Charts/MSLine.swf" />
                <%="<param name=""FlashVars"" value=""&dataXML="%><asp:Literal ID="lblXML1" runat="server"></asp:Literal>
                <%="""/>"%>
                <param name="wmode" value="opaque" />
                <param name="quality" value="high" />
                <embed src="/FusionCharts_Enterprise/Charts/MSLine.swf" flashvars="&dataXML=<%= lblXML1.Text%>"
                    quality="high" bgcolor="#ffffff" wmode="opaque" width="500" height="350" name="MSLine"
                    align="middle" type="application/x-shockwave-flash" pluginspage="http://www.macromedia.com/go/getflashplayer" />
            </object>
        </div>
    </div>

</asp:Content>
