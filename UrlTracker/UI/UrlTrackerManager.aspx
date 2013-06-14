<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UrlTrackerManager.aspx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManager" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>

<%@ Register TagPrefix="umbuic" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Url Tracker</title>
	
	<link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.bootstrap.min.css") %>" />
	<link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.urltracker.css") %>" />
	<style type="text/css">
		[class^="icon-"], [class*=" icon-"] { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.glyphicons-halflings.png") %>"); }
		.icon-white, .nav-pills > .active > a > [class^="icon-"], .nav-pills > .active > a > [class*=" icon-"], .nav-list > .active > a > [class^="icon-"], .nav-list > .active > a > [class*=" icon-"], .navbar-inverse .nav > .active > a > [class^="icon-"], .navbar-inverse .nav > .active > a > [class*=" icon-"], .dropdown-menu > li > a:hover > [class^="icon-"], .dropdown-menu > li > a:focus > [class^="icon-"], .dropdown-menu > li > a:hover > [class*=" icon-"], .dropdown-menu > li > a:focus > [class*=" icon-"], .dropdown-menu > .active > a > [class^="icon-"], .dropdown-menu > .active > a > [class*=" icon-"], .dropdown-submenu:hover > a > [class^="icon-"], .dropdown-submenu:focus > a > [class^="icon-"], .dropdown-submenu:hover > a > [class*=" icon-"], .dropdown-submenu:focus > a > [class*=" icon-"] { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.glyphicons-halflings-white.png") %>"); }
		.asc a { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.asc.png") %>"); }
			.asc a:hover { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.asc-hover.png") %>"); }
		.desc a { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.desc.png") %>"); }
			.desc a:hover { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.desc-hover.png") %>"); }
	</style>

	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-1.10.1.min.js") %>"></script>
	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-migrate-1.2.1.min.js") %>"></script>
	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.bootstrap.min.js") %>"></script>
	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.js") %>"></script>
	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.additional-methods.js") %>"></script>
	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.main.js") %>"></script>

	<umbuic:UmbracoClientDependencyLoader runat="server" />
</head>
<body>
	<form id="form1" runat="server" class="form-horizontal">
		<asp:ScriptManager runat="server" ID="scriptManager" LoadScriptsBeforeUI="false" ScriptMode="Debug" />
		<asp:ObjectDataSource runat="server" ID="odsUrlTrackerEntries" DeleteMethod="DeleteUrlTrackerEntry" SelectMethod="GetUrlTrackerEntries" TypeName="InfoCaster.Umbraco.UrlTracker.Repositories.UrlTrackerRepository" SortParameterName="sortExpression">
			<DeleteParameters>
				<asp:Parameter Name="Id" Type="Int32" />
			</DeleteParameters>
		</asp:ObjectDataSource>
		<asp:ObjectDataSource runat="server" ID="odsNotFoundEntries" DeleteMethod="DeleteNotFoundEntriesByOldUrl" SelectMethod="GetNotFoundEntries" TypeName="InfoCaster.Umbraco.UrlTracker.Repositories.UrlTrackerRepository" SortParameterName="sortExpression">
			<DeleteParameters>
				<asp:Parameter Name="oldUrl" Type="String" />
			</DeleteParameters>
		</asp:ObjectDataSource>
		<div>
			<asp:UpdatePanel runat="server" ID="upUrlTracker" UpdateMode="Conditional" ChildrenAsTriggers="true">
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
							<%--<div class="form-inline filters">
								<label class="checkbox">
									<asp:CheckBox runat="server" ID="cbShowAutoEntries" AutoPostBack="true" OnCheckedChanged="cbShowAutoEntries_CheckedChanged" /> Show auto-tracked entries
								</label>
								<label class="checkbox">
									<asp:CheckBox runat="server" ID="cbShowRegexEntries" AutoPostBack="true" OnCheckedChanged="cbShowRegexEntries_CheckedChanged" /> Show custom regex entries
								</label>
								<label class="checkbox">
									<asp:CheckBox runat="server" ID="cbShowNotFoundEntries" AutoPostBack="true" OnCheckedChanged="cbShowNotFoundEntries_CheckedChanged" /> Show not found entries
								</label>
							</div>--%>
							<div class="clearfix">
								<asp:LinkButton runat="server" ID="lbCreate" OnClick="lbCreate_Click" CssClass="btn btn-primary btn-top"><i class="icon-file icon-white"></i> Create</asp:LinkButton>
								<asp:MultiView runat="server" ID="mvSwitchButtons" ActiveViewIndex="0">
									<asp:View runat="server" ID="vwSwitchButtonsNotFound">
										<asp:LinkButton runat="server" ID="lbNotFoundView" OnClick="lbNotFoundView_Click" CssClass="btn btn-switch btn-top"><i class="icon-search"></i> 404 Not Found view</asp:LinkButton>
									</asp:View>
									<asp:View runat="server" ID="vwSwitchButtonsUrlTracker">
										<asp:LinkButton runat="server" ID="lbUrlTrackerView" OnClick="lbUrlTrackerView_Click" CssClass="btn btn-switch btn-top"><i class="icon-home"></i> Url Tracker view</asp:LinkButton>
									</asp:View>
								</asp:MultiView>
							</div>
							<asp:MultiView runat="server" ID="mvGridViews" ActiveViewIndex="0">
								<asp:View runat="server" ID="vwGridViewsUrlTracker">
									<asp:MultiView runat="server" ID="mvUrlTrackerEntries" ActiveViewIndex="0">
										<asp:View runat="server" ID="vwUrlTrackerEntriesTable">
											<asp:GridView runat="server" ID="gvUrlTracker" CssClass="table table-striped table-bordered table-hover" AutoGenerateColumns="False" DataSourceID="odsUrlTrackerEntries" AllowPaging="True" AllowSorting="true" GridLines="None" CellSpacing="-1" DataKeyNames="Id" PageSize="15" OnSelectedIndexChanged="GridView_SelectedIndexChanged" OnRowDataBound="GridView_RowDataBound">
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
										<asp:View runat="server" ID="vwUrlTrackerEntriesNone">
											<p class="info">
												There are no entries yet. You can create an entry with the Create button<asp:Literal runat="server" ID="ltlNotFoundText" Visible="false" Text=", or have a look at the logged 404 Not Found entries with the 404 Not Found view button" />.
											</p>
										</asp:View>
									</asp:MultiView>
								</asp:View>
								<asp:View runat="server" ID="vwGridViewsNotFound">
									<asp:GridView runat="server" ID="gvNotFound" CssClass="table table-striped table-bordered table-hover" AutoGenerateColumns="False" DataSourceID="odsNotFoundEntries" AllowPaging="True" AllowSorting="true" GridLines="None" CellSpacing="-1" DataKeyNames="OldUrl" PageSize="15" OnSelectedIndexChanged="GridView_SelectedIndexChanged" OnRowDataBound="GridView_RowDataBound" OnRowDeleted="gvNotFound_RowDeleted">
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
							</asp:MultiView>
							<asp:LinkButton runat="server" ID="lbDeleteSelected" OnClientClick="return deleteSelectedClick();" OnClick="lbDeleteSelected_Click" CssClass="btn"><i class="icon-trash"></i> Delete selected</asp:LinkButton>
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
		</div>
	</form>
</body>
</html>