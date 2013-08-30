<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="NotFoundView.ascx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.UserControls.NotFoundView" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>

<%@ Register Namespace="umbraco.controls" Assembly="umbraco" TagPrefix="umb" %>

<fieldset>
	<legend>Required</legend>
	<asp:Panel runat="server" ID="pnlRootNode" CssClass="control-group">
		<label class="control-label"><%= UrlTrackerResources.RootNode %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RootNodeInfo %>"></i></label>
		<div class="controls">
			<asp:HyperLink runat="server" ID="lnkRootNode" CssClass="simple" data-toggle="tooltip" />
		</div>
	</asp:Panel>
	<div class="control-group">
		<label class="control-label"><%= UrlTrackerResources.OldUrl %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.OldUrlInfo %>"></i></label>
		<div class="controls">
			<asp:HyperLink runat="server" ID="lnkOldUrl" Target="_blank" CssClass="simple" data-toggle="tooltip" />
		</div>
	</div>
	<div class="clearfix">
		<div class="control-group">
			<label class="control-label"><%= UrlTrackerResources.RedirectNode %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RedirectNodeInfo %>"></i></label>
			<div class="controls">
				<div class="wrap">
					<umb:ContentPicker runat="server" ID="cpRedirectNode" />
				</div>
			</div>
		</div>
		<div class="control-group">
			<label class="control-label" for="<%= tbRedirectUrl.ClientID %>"><%= UrlTrackerResources.RedirectUrl %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RedirectUrlInfo %>"></i></label>
			<div class="controls">
				<asp:TextBox runat="server" ID="tbRedirectUrl" CssClass="input-xlarge redirect-to redirect-to-url" />
			</div>
		</div>
	</div>
</fieldset>
<fieldset>
	<legend>Optional</legend>
	<div class="control-group">
		<label class="control-label"><%= UrlTrackerResources.RedirectType %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RedirectTypeInfo %>"></i></label>
		<div class="controls">
			<label class="radio inline">
				<asp:RadioButton runat="server" ID="rbPermanent" GroupName="RedirectType" Checked="true" />
			</label>
			<label class="radio inline">
				<asp:RadioButton runat="server" ID="rbTemporary" GroupName="RedirectType" />
			</label>
		</div>
	</div>
	<div class="control-group">
		<label class="control-label" for="<%= cbRedirectPassthroughQueryString.ClientID %>"><%= UrlTrackerResources.PassthroughQueryString %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.PassthroughQueryStringInfo %>"></i></label>
		<div class="controls">
			<label class="checkbox">
				<asp:CheckBox runat="server" ID="cbRedirectPassthroughQueryString" Checked="true" />
			</label>
		</div>
	</div>
	<div class="control-group">
		<label class="control-label" for="<%= tbNotes.ClientID %>"><%= UrlTrackerResources.Notes %></label>
		<div class="controls">
			<asp:TextBox runat="server" ID="tbNotes" CssClass="input-xlarge" />
		</div>
	</div>
</fieldset>