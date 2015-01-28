<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UrlTrackerManager.aspx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManager" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>
<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker.Helpers" %>

<%@ Register TagPrefix="umbuic" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cdc" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Url Tracker</title>
    
    <link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.bootstrap.min.css") %>" />
    <link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.urltracker.css") %>" />

    <script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-1.10.1.min.js") %>"></script>
    <script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-migrate-1.2.1.min.js") %>"></script>
    <script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.bootstrap.min.js") %>"></script>
    <script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.js") %>"></script>
    <script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.additional-methods.js") %>"></script>
    <script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.main.js") %>"></script>

    <umbuic:UmbracoClientDependencyLoader runat="server" />

    <% if (UmbracoHelper.IsVersion7OrNewer) { %>
    <cdc:JsInclude runat="server" FilePath="Application/NamespaceManager.js" PathNameAlias="UmbracoClient" Priority="0" />
    <cdc:JsInclude runat="server" FilePath="Application/jQuery/jquery.cookie.js" PathNameAlias="UmbracoClient" Priority="1" />
    <cdc:JsInclude runat="server" FilePath="ui/base2.js" PathNameAlias="UmbracoClient" Priority="1"  /> 
    <cdc:JsInclude runat="server" FilePath="Application/UmbracoApplicationActions.js" PathNameAlias="UmbracoClient" Priority="2" />
    <cdc:JsInclude runat="server" FilePath="Application/UmbracoUtils.js" PathNameAlias="UmbracoClient" Priority="2" />
    <cdc:JsInclude runat="server" FilePath="Application/UmbracoClientManager.js" PathNameAlias="UmbracoClient" Priority="3" />
    <cdc:JsInclude runat="server" FilePath="UI/knockout.js" PathNameAlias="UmbracoClient" Priority="3" />
    <cdc:JsInclude runat="server" FilePath="UI/knockout.mapping.js" PathNameAlias="UmbracoClient" Priority="4" />
    <cdc:JsInclude runat="server" FilePath="modal/modal.js" PathNameAlias="UmbracoClient" Priority="10" />
    <cdc:JsInclude runat="server" FilePath="ui/default.js" PathNameAlias="UmbracoClient" Priority="10" />
    <cdc:JsInclude runat="server" FilePath="Application/jQuery/jquery.hotkeys.js" PathNameAlias="UmbracoClient" Priority="10" />
    <% } %>
</head>
<body>
    <form id="form1" runat="server" class="form-horizontal">
        <asp:ScriptManager runat="server" ID="scriptManager" LoadScriptsBeforeUI="false" ScriptMode="Debug" OnAsyncPostBackError="scriptManager_AsyncPostBackError" />
        <asp:ObjectDataSource runat="server" ID="odsUrlTrackerEntries" DeleteMethod="DeleteUrlTrackerEntry" SelectMethod="GetUrlTrackerEntries" TypeName="InfoCaster.Umbraco.UrlTracker.Repositories.UrlTrackerRepository" SortParameterName="sortExpression">
            <DeleteParameters>
                <asp:Parameter Name="Id" Type="Int32" />
            </DeleteParameters>
        </asp:ObjectDataSource>
        <asp:ObjectDataSource runat="server" ID="odsNotFoundEntries" DeleteMethod="DeleteNotFoundEntriesByRootAndOldUrl" SelectMethod="GetNotFoundEntries" TypeName="InfoCaster.Umbraco.UrlTracker.Repositories.UrlTrackerRepository" SortParameterName="sortExpression">
            <DeleteParameters>
                <asp:Parameter Name="redirectRootNodeId" Type="Int32" />
                <asp:Parameter Name="oldUrl" Type="String" />
            </DeleteParameters>
        </asp:ObjectDataSource>
        <div>
            <asp:MultiView runat="server" ID="mvUrlTrackerError" ActiveViewIndex="0">
                <asp:View runat="server" ID="vwUrlTrackerErrorNormal">
                    <asp:UpdatePanel runat="server" ID="upUrlTracker" UpdateMode="Conditional" ChildrenAsTriggers="false">
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbUrlTracker" />
                            <asp:AsyncPostBackTrigger ControlID="lbCreate" />
                            <asp:AsyncPostBackTrigger ControlID="lbClearNotFound" />
                            <asp:AsyncPostBackTrigger ControlID="lbNotFoundView" />
                            <asp:AsyncPostBackTrigger ControlID="lbUrlTrackerView" />
                            <asp:AsyncPostBackTrigger ControlID="gvUrlTracker" />
                            <asp:AsyncPostBackTrigger ControlID="gvNotFound" />
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteSelected" />
                            <asp:AsyncPostBackTrigger ControlID="lbSwitchToAdvancedView" />
                            <asp:AsyncPostBackTrigger ControlID="lbSwitchToNormalView" />
                            <asp:AsyncPostBackTrigger ControlID="lbReturnFromDetail" />
                            <asp:AsyncPostBackTrigger ControlID="lbSave" />
                            <asp:AsyncPostBackTrigger ControlID="btnSaveDummy" />
                            <asp:AsyncPostBackTrigger ControlID="lbReturnFromNew" />
                            <asp:AsyncPostBackTrigger ControlID="lbCreateNew" />
                            <asp:AsyncPostBackTrigger ControlID="btnCreateDummy" />
                        </Triggers>
                        <ContentTemplate>
                            <asp:Panel runat="server" ID="pnlBreadcrumb" EnableViewState="false">
                                <ul class="breadcrumb">
                                    <li><asp:LinkButton runat="server" ID="lbUrlTracker" OnClick="ShowOverview">Overview</asp:LinkButton> </li>
                                    <li runat="server" id="liDetails"><span class="divider">/</span> <span class="active">Details <%= UrlTrackerModel != null ? string.Concat("#", UrlTrackerModel.Id) : string.Empty %></span></li>
                                    <li runat="server" id="liNew"><span class="divider">/</span> <span class="active">Create</span></li>
                                </ul>
                            </asp:Panel>
                            <asp:MultiView runat="server" ID="mvUrlTracker" ActiveViewIndex="0">
                                <asp:View runat="server" ID="vwUrlTrackerOverview">
                                    <div class="clearfix">
                                        <asp:MultiView runat="server" ID="mvMainButtons" ActiveViewIndex="0">
                                            <asp:View runat="server" ID="vwMainButtonsCreate">
                                                <asp:LinkButton runat="server" ID="lbCreate" OnClick="lbCreate_Click" CssClass="btn btn-primary btn-top"><i class="icon-file icon-white"></i> Create</asp:LinkButton>
                                            </asp:View>
                                            <asp:View runat="server" ID="vwMainButtonsClearNotFound">
                                                <asp:LinkButton runat="server" ID="lbClearNotFound" OnClientClick="return clearNotFoundClick();" OnClick="lbClearNotFound_Click" CssClass="btn btn-danger btn-top"><i class="icon-trash icon-white"></i> Clear not found list</asp:LinkButton>
                                            </asp:View>
                                        </asp:MultiView>
                                        <asp:MultiView runat="server" ID="mvSwitchButtons" ActiveViewIndex="0">
                                            <asp:View runat="server" ID="vwSwitchButtonsNotFound">
                                                <asp:LinkButton runat="server" ID="lbNotFoundView" OnClick="lbNotFoundView_Click" CssClass="btn btn-switch btn-top"><i class="icon-search"></i> 404 Not Found view</asp:LinkButton>
                                            </asp:View>
                                            <asp:View runat="server" ID="vwSwitchButtonsUrlTracker">
                                                <asp:LinkButton runat="server" ID="lbUrlTrackerView" OnClick="lbUrlTrackerView_Click" CssClass="btn btn-switch btn-top"><i class="icon-home"></i> Url Tracker view</asp:LinkButton>
                                            </asp:View>
                                        </asp:MultiView>
                                    </div>
                                    <asp:Panel runat="server" ID="pnlFilter" CssClass="form-inline filters clearfix">
                                        <label class="search">
                                            <asp:TextBox runat="server" ID="tbSearch" ClientIDMode="Static" AutoPostBack="true" OnTextChanged="tbSearch_TextChanged" placeholder="Search" AutoCompleteType="None" autocomplete="off" />
                                        </label>
                                        <asp:Panel runat="server" ID="pnlFilters" CssClass="right">
                                            Filter: 
                                            <label class="checkbox">
                                                <asp:CheckBox runat="server" ID="cbShowAutoEntries" Checked="true" AutoPostBack="true" OnCheckedChanged="cbFilters_CheckedChanged" /> Automatic entries
                                            </label>
                                            <label class="checkbox">
                                                <asp:CheckBox runat="server" ID="cbShowCustomEntries" Checked="true" AutoPostBack="true" OnCheckedChanged="cbFilters_CheckedChanged" /> Custom URL entries
                                            </label>
                                            <label class="checkbox">
                                                <asp:CheckBox runat="server" ID="cbShowRegexEntries" Checked="true" AutoPostBack="true" OnCheckedChanged="cbFilters_CheckedChanged" /> Custom regex entries
                                            </label>
                                        </asp:Panel>
                                    </asp:Panel>
                                    <asp:UpdatePanel runat="server" ID="upUrlTrackerGrid" UpdateMode="Conditional" ChildrenAsTriggers="false">
                                        <Triggers>
                                            <asp:AsyncPostBackTrigger ControlID="tbSearch" />
                                            <asp:AsyncPostBackTrigger ControlID="cbShowAutoEntries" />
                                            <asp:AsyncPostBackTrigger ControlID="cbShowCustomEntries" />
                                            <asp:AsyncPostBackTrigger ControlID="cbShowRegexEntries" />
                                            <asp:AsyncPostBackTrigger ControlID="ddlPageSize" />
                                        </Triggers>
                                        <ContentTemplate>
                                            <asp:MultiView runat="server" ID="mvGridViews" ActiveViewIndex="0">
                                                <asp:View runat="server" ID="vwGridViewsUrlTracker">
                                                    <asp:MultiView runat="server" ID="mvUrlTrackerEntries" ActiveViewIndex="0">
                                                        <asp:View runat="server" ID="vwUrlTrackerEntriesTable">
                                                            <asp:MultiView runat="server" ID="mvUrlTrackerFilter" ActiveViewIndex="0">
                                                                <asp:View runat="server" ID="vwUrlTrackerFilterGrid">
                                                                    <asp:GridView runat="server" ID="gvUrlTracker" CssClass="table table-striped table-bordered table-hover" AutoGenerateColumns="False" DataSourceID="odsUrlTrackerEntries" AllowPaging="True" PagerSettings-Mode="NumericFirstLast" AllowSorting="true" GridLines="None" CellSpacing="-1" DataKeyNames="Id" OnSelectedIndexChanged="GridView_SelectedIndexChanged" OnRowDataBound="GridView_RowDataBound">
                                                                        <SortedAscendingHeaderStyle CssClass="asc" />
                                                                        <SortedDescendingHeaderStyle CssClass="desc" />
                                                                        <Columns>
                                                                            <asp:TemplateField HeaderStyle-CssClass="checkboxes" ItemStyle-CssClass="checkboxes" HeaderStyle-Width="13">
                                                                                <HeaderTemplate>
                                                                                    <input type="checkbox" id="cbSelectAll" />
                                                                                </HeaderTemplate>
                                                                                <ItemTemplate>
                                                                                    <asp:CheckBox ID="cbSelect" runat="server" />
                                                                                    <asp:HiddenField runat="server" ID="hfId" />
                                                                                </ItemTemplate>
                                                                            </asp:TemplateField>
                                                                            <asp:BoundField DataField="RedirectRootNodeName" HeaderText="Site" SortExpression="RedirectRootNodeName" />
                                                                            <asp:BoundField DataField="CalculatedOldUrl" HeaderText="Old URL / Regex" SortExpression="CalculatedOldUrl" />
                                                                            <asp:BoundField DataField="CalculatedRedirectUrl" HeaderText="New URL" SortExpression="CalculatedRedirectUrl" />
                                                                            <asp:BoundField DataField="Notes" HeaderText="Notes" SortExpression="Notes" />
                                                                            <asp:BoundField DataField="Inserted" HeaderText="Created" SortExpression="Inserted" />
                                                                            <asp:TemplateField HeaderStyle-Width="36">
                                                                                <ItemTemplate>
                                                                                    <asp:ImageButton runat="server" ID="lbSelect" CommandName="Select" AlternateText="Edit" />
                                                                                    <asp:ImageButton runat="server" ID="lbDelete" CommandName="Delete" AlternateText="Delete" OnClientClick="return confirm('Are you sure you want to delete this entry?');" />
                                                                                </ItemTemplate>
                                                                            </asp:TemplateField>
                                                                        </Columns>
                                                                    </asp:GridView>
                                                                </asp:View>
                                                                <asp:View runat="server" ID="vwUrlTrackerFilterNoResults">
                                                                    <p class="warning">No results</p>
                                                                </asp:View>
                                                            </asp:MultiView>
                                                        </asp:View>
                                                        <asp:View runat="server" ID="vwUrlTrackerEntriesNone">
                                                            <p class="info">
                                                                There are no entries yet. You can create an entry with the Create button<asp:Literal runat="server" ID="ltlNotFoundText" Visible="false" Text=", or have a look at the logged 404 Not Found entries with the 404 Not Found view button" />.
                                                            </p>
                                                        </asp:View>
                                                    </asp:MultiView>
                                                </asp:View>
                                                <asp:View runat="server" ID="vwGridViewsNotFound">
                                                    <asp:MultiView runat="server" ID="mvNotFoundFilter" ActiveViewIndex="0">
                                                        <asp:View runat="server" ID="vwNotFoundFilterGrid">
                                                            <asp:GridView runat="server" ID="gvNotFound" CssClass="table table-striped table-bordered table-hover" AutoGenerateColumns="False" DataSourceID="odsNotFoundEntries" AllowPaging="True" PagerSettings-Mode="NumericFirstLast" AllowSorting="true" GridLines="None" CellSpacing="-1" DataKeyNames="RedirectRootNodeId, OldUrl"  OnSelectedIndexChanged="GridView_SelectedIndexChanged" OnRowDataBound="GridView_RowDataBound" OnRowDeleted="gvNotFound_RowDeleted">
                                                                <SortedAscendingHeaderStyle CssClass="asc" />
                                                                <SortedDescendingHeaderStyle CssClass="desc" />
                                                                <Columns>
                                                                    <asp:TemplateField HeaderStyle-CssClass="checkboxes" ItemStyle-CssClass="checkboxes" HeaderStyle-Width="13">
                                                                        <HeaderTemplate>
                                                                            <input type="checkbox" id="cbSelectAll" />
                                                                        </HeaderTemplate>
                                                                        <ItemTemplate>
                                                                            <asp:CheckBox ID="cbSelect" runat="server" />
                                                                            <asp:HiddenField runat="server" ID="hfOldUrl" />
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:BoundField DataField="RedirectRootNodeName" HeaderText="Site" SortExpression="RedirectRootNodeName" />
                                                                    <asp:BoundField DataField="OldUrl" HeaderText="URL" SortExpression="OldUrl" />
                                                                    <asp:BoundField DataField="NotFoundCount" HeaderText="Occurrences" SortExpression="NotFoundCount" />
                                                                    <asp:BoundField DataField="Referrer" HeaderText="Most important referrer" SortExpression="Referrer" />
                                                                    <asp:BoundField DataField="Inserted" HeaderText="Last occurred" SortExpression="Inserted" />
                                                                    <asp:TemplateField HeaderStyle-Width="36">
                                                                        <ItemTemplate>
                                                                            <asp:ImageButton runat="server" ID="lbSelect" CommandName="Select" AlternateText="Edit" />
                                                                            <asp:ImageButton runat="server" ID="lbDelete" CommandName="Delete" AlternateText="Delete" OnClientClick="return confirm('Are you sure you want to delete this entry?');" />
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                            </asp:GridView>
                                                        </asp:View>
                                                        <asp:View runat="server" ID="vwNotFoundFilterNoResults">
                                                            <p class="warning">No results</p>
                                                        </asp:View>
                                                    </asp:MultiView>
                                                </asp:View>
                                            </asp:MultiView>
                                            <asp:LinkButton runat="server" ID="lbDeleteSelected" OnClientClick="return deleteSelectedClick();" OnClick="lbDeleteSelected_Click" CssClass="btn"><i class="icon-trash"></i> Delete selected</asp:LinkButton>
                                            <div class="page-size">
                                                Pagesize: 
                                                <asp:DropDownList runat="server" ID="ddlPageSize" OnSelectedIndexChanged="ddlPageSize_SelectedIndexChanged" AutoPostBack="true">
                                                    <asp:ListItem Text="10" />
                                                    <asp:ListItem Text="20" />
                                                    <asp:ListItem Text="30" />
                                                    <asp:ListItem Text="50" />
                                                    <asp:ListItem Text="100" />
                                                    <asp:ListItem Text="200" />
                                                </asp:DropDownList>
                                            </div>
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                    <asp:UpdateProgress runat="server" AssociatedUpdatePanelID="upUrlTrackerGrid" DisplayAfter="0">
                                        <ProgressTemplate>
                                            <div class="loading-overlay"></div>
                                            <div id="circularG">
                                                <div id="circularG_1" class="circularG">
                                                </div>
                                                <div id="circularG_2" class="circularG">
                                                </div>
                                                <div id="circularG_3" class="circularG">
                                                </div>
                                                <div id="circularG_4" class="circularG">
                                                </div>
                                                <div id="circularG_5" class="circularG">
                                                </div>
                                                <div id="circularG_6" class="circularG">
                                                </div>
                                                <div id="circularG_7" class="circularG">
                                                </div>
                                                <div id="circularG_8" class="circularG">
                                                </div>
                                            </div>
                                        </ProgressTemplate>
                                    </asp:UpdateProgress>
                                </asp:View>
                                <asp:View runat="server" ID="vwUrlTrackerDetail">
                                    <asp:MultiView runat="server" ID="mvViewSwitcher" ActiveViewIndex="0">
                                        <asp:View runat="server" ID="vwViewSwitcherAdvanced">
                                            <asp:LinkButton runat="server" ID="lbSwitchToAdvancedView" OnClick="lbSwitchToAdvancedView_Click" CssClass="btn view-switch"><i class="icon-cog"></i> Advanced view</asp:LinkButton>
                                        </asp:View>
                                        <asp:View runat="server" ID="vwViewSwitcherBack">
                                            <asp:LinkButton runat="server" ID="lbSwitchToNormalView" OnClick="lbSwitchToNormalView_Click" CssClass="btn view-switch"><i class="icon-chevron-left"></i> Return to default view</asp:LinkButton>
                                        </asp:View>
                                    </asp:MultiView>
                                    <asp:Panel runat="server" ID="pnlEditValidationGroup" CssClass="validationGroup">
                                        <asp:LinkButton runat="server" ID="lbReturnFromDetail" OnClick="ShowOverview" CssClass="btn"><i class="icon-chevron-left"></i> Return to overview</asp:LinkButton>
                                        <asp:LinkButton runat="server" ID="lbSave" OnClick="lbSave_Click" CssClass="btn btn-primary causesValidation"><i class="icon-download-alt icon-white"></i> Save changes</asp:LinkButton>
                                        <asp:Button runat="server" ID="btnSaveDummy" OnClick="lbSave_Click" CssClass="hide" />
                                    </asp:Panel>
                                </asp:View>
                                <asp:View runat="server" ID="vwUrlTrackerNew">
                                    <asp:Panel runat="server" ID="pnlCreateValidationGroup" CssClass="validationGroup">
                                        <asp:LinkButton runat="server" ID="lbReturnFromNew" OnClick="ShowOverview" CssClass="btn"><i class="icon-chevron-left"></i> Return to overview</asp:LinkButton>
                                        <asp:LinkButton runat="server" ID="lbCreateNew" OnClick="lbCreateNew_Click" CssClass="btn btn-primary causesValidation"><i class="icon-file icon-white"></i> Create</asp:LinkButton>
                                        <asp:Button runat="server" ID="btnCreateDummy" OnClick="lbCreateNew_Click" CssClass="hide" />
                                    </asp:Panel>
                                </asp:View>
                            </asp:MultiView>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <asp:UpdateProgress runat="server" AssociatedUpdatePanelID="upUrlTracker" DisplayAfter="0">
                        <ProgressTemplate>
                            <div class="loading-overlay"></div>
                            <div id="circularG">
                                <div id="circularG_1" class="circularG">
                                </div>
                                <div id="circularG_2" class="circularG">
                                </div>
                                <div id="circularG_3" class="circularG">
                                </div>
                                <div id="circularG_4" class="circularG">
                                </div>
                                <div id="circularG_5" class="circularG">
                                </div>
                                <div id="circularG_6" class="circularG">
                                </div>
                                <div id="circularG_7" class="circularG">
                                </div>
                                <div id="circularG_8" class="circularG">
                                </div>
                            </div>
                        </ProgressTemplate>
                    </asp:UpdateProgress>
                </asp:View>
                <asp:View runat="server" ID="vwUrlTrackerErrorMessage">
                    <p class="error">
                        <asp:Literal runat="server" ID="ltlError" />
                    </p>
                    <asp:LinkButton runat="server" ID="lbDeleteErrorRows" OnClick="lbDeleteErrorRows_Click" CssClass="btn btn-danger" />
                </asp:View>
            </asp:MultiView>
        </div>
    </form>
</body>
</html>