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
			<%--<img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.loading-orange.gif") %>" alt="Loading" id="loadingIcon" />--%>
		</div>
	</div>
</umb:PropertyPanel>

<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.installer.js") %>"></script>