<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="CustomView.ascx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.UserControls.CustomView" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>

<%@ Register Namespace="umbraco.controls" Assembly="umbraco" TagPrefix="umb" %>

<fieldset>
    <legend>Required</legend>
    <asp:Panel runat="server" ID="pnlRootNode" CssClass="control-group">
        <label class="control-label" for="<%= ddlRootNode.ClientID %>"><%= UrlTrackerResources.RootNode %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RootNodeInfo %>"></i></label>
        <div class="controls">
            <asp:DropDownList runat="server" ID="ddlRootNode" AppendDataBoundItems="true" DataTextField="Text" DataValueField="Value" CssClass="input-xlarge">
                <asp:ListItem Value="-1">/</asp:ListItem>
            </asp:DropDownList>
        </div>
    </asp:Panel>
    <asp:MultiView runat="server" ID="mvRedirectFrom" ActiveViewIndex="0">
        <asp:View runat="server" ID="vwRedirectFromUrl">
            <div class="control-group group">
                <label class="control-label" for="<%= tbOldUrl.ClientID %>"><%= UrlTrackerResources.OldUrl %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.OldUrlInfo %>"></i></label>
                <div class="controls">
                    <asp:TextBox runat="server" ID="tbOldUrl" CssClass="input-xlarge old-url" /> <a target="_blank" href="<%= UrlTrackerModel.CalculatedOldUrlWithDomain %>" class="icon-share"></a>
                </div>
            </div>
            <div class="control-group">
                <label class="control-label" for="<%= tbOldUrlQueryString.ClientID %>"><%= UrlTrackerResources.OldUrlQueryString %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.OldUrlQueryStringInfo %>"></i></label>
                <div class="controls input-prepend" style="display: block;">
                    <span class="add-on">?</span>
                    <asp:TextBox runat="server" ID="tbOldUrlQueryString" CssClass="input-xlarge query-string" />
                </div>
            </div>
        </asp:View>
        <asp:View runat="server" ID="vwRedirectFromRegex">
            <div class="control-group">
                <label class="control-label" for="<%= tbOldRegex.ClientID %>"><%= UrlTrackerResources.RegexStandalone %> <a href="http://gskinner.com/RegExr/" target="_blank" data-toggle="tooltip" title="<%= UrlTrackerResources.RegexInfo %>"><i class="icon-question-sign"></i></a></label>
                <div class="controls">
                    <asp:TextBox runat="server" ID="tbOldRegex" CssClass="input-xlarge old-url old-regex" />
                </div>
            </div>
        </asp:View>
    </asp:MultiView>
    <div class="control-group">
        <asp:MultiView runat="server" ID="mvRedirect" ActiveViewIndex="0">
            <asp:View runat="server" ID="vwRedirectNode">
                <label class="control-label"><%= UrlTrackerResources.RedirectNode %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RedirectNodeInfo %>"></i></label>
                <div class="controls">
                    <div class="wrap">
                        <umb:ContentPicker runat="server" ID="cpRedirectNode" />
                    </div>
                </div>
            </asp:View>
            <asp:View runat="server" ID="vwRedirectUrl">
                <label class="control-label" for="<%= tbRedirectUrl.ClientID %>"><%= UrlTrackerResources.RedirectUrlStandalone %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RedirectUrlInfo %>"></i></label>
                <div class="controls">
                    <asp:TextBox runat="server" ID="tbRedirectUrl" CssClass="input-xlarge redirect-to redirect-to-url" />
                </div>
            </asp:View>
        </asp:MultiView>
    </div>
</fieldset>
<fieldset>
    <legend>Optional</legend>
    <div class="control-group">
        <label class="control-label"><%= UrlTrackerResources.RedirectType %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.RedirectTypeInfo %>"></i></label>
        <div class="controls">
            <label class="radio inline">
                <asp:RadioButton runat="server" ID="rbPermanent" GroupName="RedirectType" />
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
                <asp:CheckBox runat="server" ID="cbRedirectPassthroughQueryString" />
            </label>
        </div>
    </div>
    <div class="control-group">
        <label class="control-label" for="<%= cbForceRedirect.ClientID %>"><%= UrlTrackerResources.ForceRedirect %> <i class="icon-question-sign" data-toggle="tooltip" title="<%= UrlTrackerResources.ForceRedirectInfo %>"></i></label>
        <div class="controls">
            <label class="checkbox">
                <asp:CheckBox runat="server" ID="cbForceRedirect" />
            </label>
        </div>
    </div>
    <div class="control-group">
        <label class="control-label" for="<%= tbNotes.ClientID %>"><%= UrlTrackerResources.Notes %></label>
        <div class="controls">
            <asp:TextBox runat="server" ID="tbNotes" CssClass="input-xlarge" />
        </div>
    </div>
    <asp:Panel runat="server" ID="pnlReferrer" CssClass="control-group">
        <label class="control-label">Referrer</label>
        <div class="controls">
            <asp:Label runat="server" ID="lblReferrer" CssClass="input-xlarge uneditable-input" />
        </div>
    </asp:Panel>
    <div class="control-group">
        <label class="control-label">Created</label>
        <div class="controls">
            <asp:Label runat="server" ID="lblInserted" CssClass="simple" />
        </div>
    </div>
</fieldset>