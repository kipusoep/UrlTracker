<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UrlTrackerInfo.aspx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerInfo" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Url Tracker</title>

	<link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.bootstrap.min.css") %>" />
	<link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.info.css") %>" />
	<style type="text/css">
		[class^="icon-"], [class*=" icon-"] { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.glyphicons-halflings.png") %>"); }
		.icon-white, .nav-pills > .active > a > [class^="icon-"], .nav-pills > .active > a > [class*=" icon-"], .nav-list > .active > a > [class^="icon-"], .nav-list > .active > a > [class*=" icon-"], .navbar-inverse .nav > .active > a > [class^="icon-"], .navbar-inverse .nav > .active > a > [class*=" icon-"], .dropdown-menu > li > a:hover > [class^="icon-"], .dropdown-menu > li > a:focus > [class^="icon-"], .dropdown-menu > li > a:hover > [class*=" icon-"], .dropdown-menu > li > a:focus > [class*=" icon-"], .dropdown-menu > .active > a > [class^="icon-"], .dropdown-menu > .active > a > [class*=" icon-"], .dropdown-submenu:hover > a > [class^="icon-"], .dropdown-submenu:focus > a > [class^="icon-"], .dropdown-submenu:hover > a > [class*=" icon-"], .dropdown-submenu:focus > a > [class*=" icon-"] { background-image: url("<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.glyphicons-halflings-white.png") %>"); }
	</style>

	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-1.10.1.min.js") %>"></script>
	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-migrate-1.2.1.min.js") %>"></script>
	<script type="text/javascript" src="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.js.bootstrap.min.js") %>"></script>
</head>
<body>
	<form id="form1" runat="server">
		<ul class="nav nav-tabs" id="infoTabs">
			<li class="active"><a href="#info" data-toggle="tab">Info</a></li>
			<li><a href="#settings" data-toggle="tab">Settings</a></li>
			<li><a href="#qa" data-toggle="tab">Q&A</a></li>
			<li><a href="#changeLog" data-toggle="tab">Changelog</a></li>
			<li><a href="#roadmap" data-toggle="tab">Roadmap</a></li>
			<li><a href="#credits" data-toggle="tab">Credits</a></li>
		</ul>
 
		<div class="tab-content">
			<div class="tab-pane active" id="info">
				<p>
					The Url Tracker is used to manage URLs within umbraco. It automatically tracks URL changes, for instance when a node is renamed, and makes sure the old URL will redirect to the new location. This is great for SEO and great for people visiting your website via this old URL. Search engines will update the indexed URL and people won't visit the old, broken URL.<br />
					You can also create your own redirects, based on a simple URL or using a Regex pattern. You can redirect to an existing node or a manually entered URL. This is great for migrating existing indexed URLs to your new website!
				</p>
				<p>
					All features:
				</p>
				<ul>
					<li>Keeps track of <b>URL changes</b> (node gets renamed, moved or the umbracoUrlName property changes)</li>
					<li>Keeps track of <b>not found (404)</b> requests, so you can easily create redirects for them</li>
					<li>Keeps track of <b>removed content</b> and responds with <b>410 Gone</b> when one requests it</li>
					<li><b>Redirects requests</b> for old URLs to their new location</li>
					<li><b>Create</b> your own URL redirects, based on a simple URL or a <b>Regex</b> pattern. You can redirect to an <b>existing node</b> or a manually entered URL.</li>
					<li>Create <b>permanent (301)</b> or <b>temporary (302)</b> redirects</li>
					<li>Supports all extensions, including <b>.php</b> and <b>.html</b></li>
					<li>Supports all kinds of <b>query string</b> options, like matching a query string and pass through the request query string</li>
					<li>Supports <b>multiple websites</b> in a single umbraco instance</li>
				</ul>
			</div>
			<div class="tab-pane" id="settings">
				<p>The Url Tracker supports a few settings, which you can use in the <a href="http://msdn.microsoft.com/en-us/library/aa903313(v=vs.71).aspx" target="_blank">appSettings section of your web.config</a>. Below you'll find these settings, with the type and default value mentioned below the setting name:</p>
				<h4>urlTracker:disabled</h4>
				<h5>boolean (false)</h5>
				<p>Set to true to disable the Http Module of the Url Tracker, so it won't redirect requests anymore.</p>
				<h4>urlTracker:enableLogging</h4>
				<h5>boolean (false)</h5>
				<p>
					Set to true to enable logging debug information of the Url Tracker. Uses umbraco's built-in logging mechanism with LogType set to 'debug'.<br />
					<b>umbracoDebugMode</b> needs to be enabled too.
				</p>
				<h4>urlTracker:404UrlsToIgnore</h4>
				<h5>comma-seperated string (empty)</h5>
				<p>The Url Tracker logs requests resulting in a 404 Not Found status. Some URLs shouldn't be logged and can be set here. One URL is always ignored by default: 'favicon.ico'.</p>
				<h4>urlTracker:trackingDisabled</h4>
				<h5>boolean (false)</h5>
				<p>Set to true to disable tracking URL changes.</p>
			</div>
			<div class="tab-pane" id="qa">
				<p>Some questions and answers. This section will be expanded when people start asking more questions on the forum ;-)</p>
				<h4>Help, the Url Tracker won't redirect any requests?</h4>
				<p>Please enable logging in the <a onclick="$('#infoTabs a[href=\'#settings\']').tab('show');">settings</a> and <a href="http://our.umbraco.org/projects/developer-tools/301-url-tracker/version-2" target="_blank">post the information on the forum</a>.</p>
				<h4>Can I log exceptions and debug info myself?</h4>
				<p>Yes that's possible. By default exceptions/debug info will be logged (depending on configuration) to log4net and/or the umbracoLog table, but it's possible to register your own logger. Just add your implementation of the IUrlTrackerLogger by using InfoCaster.Umbraco.UrlTracker.UrlTrackerLogging.RegisterLogger(logger);</p>
				<h4>I have added hostnames to my root nodes, but I can't select a root node in the Url Tracker?</h4>
				<p>Please ensure the application pool has been recycled after adding new hostnames.</p>
			</div>
			<div class="tab-pane" id="changeLog">
				<ul>
					<li>
						2.1.1 [2013/07/03]
						<ul>
							<li>[Bugfix] Implemented an extra check for the installation of the dashboard</li>
						</ul>
					</li>
					<li>
						2.1.0 [2013/07/02]
						<ul>
							<li>[Feature] Implemented support for SQL CE</li>
							<li>[Feature] Added a setting to disable url tracking (urlTracker:trackingDisabled)</li>
						</ul>
					</li>
					<li>
						2.0.4-beta [2013/07/02]
						<ul>
							<li>[Feature] Added better exception handling for the installer</li>
						</ul>
					</li>
					<li>
						2.0.3-beta [2013/06/21]
						<ul>
							<li>[Bugfix] Enabling logging on umbraco versions including log4net threw ysod</li>
						</ul>
					</li>
					<li>
						2.0.2-beta [2013/06/20]
						<ul>
							<li>[Bugfix] Ports other than 80 didn't work with the Http Module check in the installer</li>
							<li>[Bugfix] String.Format in UrlTrackerDomain.UrlWithDomain was wrongly formatted</li>
						</ul>
					</li>
					<li>
						2.0.1-beta [2013/06/20]
						<ul>
							<li>[Bugfix] Multiple hostnames assigned to a node threw an exception</li>
						</ul>
					</li>
					<li>
						2.0-beta [2013/06/19]
						<ul>
							<li>Initial release, completely rebuilt the package</li>
							<li>Renamed 301 URL Tracker to Url Tracker</li>
							<li>The package is now a single assembly</li>
						</ul>
					</li>
				</ul>
			</div>
			<div class="tab-pane" id="roadmap">
				<p>Features planned for future versions:</p>
				<ul>
					<li>Datatype</li>
					<li>Better validation (already existing etc.)</li>
					<li>Filtering</li>
					<li>Searching</li>
					<li>Regex capturing groups</li>
					<li>Translation support</li>
					<li>Support UrlRewriting if possible</li>
					<li>SQLCE/Azure support (if it doesn't work yet, untested)</li>
				</ul>
			</div>
			<div class="tab-pane" id="credits">
				<p>Special thanks to:</p>
				<ul>
					<li><b>InfoCaster</b> | Being able to combine 'work' with package development and thanks to colleagues for inspiration.</li>
					<li><b>Richard Soeteman</b> | Richard came up with the idea for a package which keeps track of URLs of umbraco nodes.</li>
					<li><b>The uComponents project</b> | For inspiring me to create a single-assembly package solution.</li>
				</ul>
			</div>
		</div>
		<a href="<%= UrlTrackerResources.UrlTrackerManagerUrl %>" class="btn btn-primary"><i class="icon-chevron-left icon-white"></i> Return</a>
	</form>
</body>
</html>
