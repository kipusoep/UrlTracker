<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UrlTrackerManagerWrapper.ascx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManagerWrapper" %>

<%@ Import Namespace="System.Threading" %>
<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>

<div class="dashboardWrapper">
	<h2>Url Tracker</h2><a style="position: absolute; top: 10px; right: 14px;" target="urltracker" href="<%= UrlTrackerResources.UrlTrackerInfoUrl %>"><img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.info.png") %>" alt="Info" /></a>
	<img src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.urltracker-icon.png") %>" alt="UrlTracker" class="dashboardIcon" />
	<script type="text/javascript">
		//<![CDATA[
		var $urlTrackerIframe;
		$(function () {
			$urlTrackerIframe = $("#urlTrackerIframe");
			$(window).resize(function (e) {
				resizeFrame();
			});
			$(window).trigger("resize");
		});

		function resizeFrame() {
			$urlTrackerIframe.height(parseInt($urlTrackerIframe.parents(".tabpagescrollinglayer").height()) - 93);
		}
		//]]>
	</script>
	<iframe scrolling="auto" frameborder="0" marginheight="0" marginwidth="0" id="urlTrackerIframe" seamless="seamless" name="urltracker" src="<%= UrlTrackerResources.UrlTrackerManagerUrl %>" style="width: 100%;"></iframe>
</div>