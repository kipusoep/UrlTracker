<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="UrlTrackerInstaller.ascx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.Installer.UrlTrackerInstaller" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>
<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker.Helpers" %>

<%@ Register Assembly="controls" Namespace="umbraco.uicontrols" TagPrefix="umb" %>

<link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.installer.css") %>" />

<umb:PropertyPanel runat="server">
    <div class="installer">
        <div class="dashboardWrapper">
            <h2>Url Tracker</h2>
            <img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.urltracker-icon.png") %>" alt="UrlTracker" class="dashboardIcon" style="<%= InfoCaster.Umbraco.UrlTracker.Helpers.UmbracoHelper.IsVersion7OrNewer ? "left: 3px; top: 5px;" : string.Empty %>" />
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
        </div>
    </div>
</umb:PropertyPanel>

<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-1.11.2.min.js") %>"></script>
<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-migrate-1.2.1.min.js") %>"></script>
<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.installer.js") %>"></script>