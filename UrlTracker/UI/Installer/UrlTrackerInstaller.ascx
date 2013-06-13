<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="UrlTrackerInstaller.ascx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.Installer.UrlTrackerInstaller" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>

<%@ Register Assembly="controls" Namespace="umbraco.uicontrols" TagPrefix="umb" %>

<link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.installer.css") %>" />

<umb:PropertyPanel runat="server">
	<div class="installer">
		<div class="dashboardWrapper">
			<h2>Url Tracker</h2>
			<img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.logo.png") %>" alt="UrlTracker" class="dashboardIcon" />
			<p>Please wait while the Url Tracker executes the final installation tasks...</p>
			<div id="urlTrackerLoader">
			</div>
			<img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.loading-orange.gif") %>" alt="Loading" id="loadingIcon" />
		</div>
	</div>
</umb:PropertyPanel>

<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.installer.js") %>"></script>