<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UrlTrackerInfo.aspx.cs" Inherits="InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerInfo" %>

<%@ Import Namespace="InfoCaster.Umbraco.UrlTracker" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Url Tracker</title>

    <link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.bootstrap.min.css") %>" />
    <link rel="stylesheet" type="text/css" href="<%= Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.css.info.css") %>" />

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
                <p>Set to true to disable the HTTP Module of the Url Tracker, so it won't redirect requests anymore.</p>
                <h4>urlTracker:enableLogging</h4>
                <h5>boolean (false)</h5>
                <p>
                    Set to true to enable logging debug information of the Url Tracker. Uses umbraco's built-in logging mechanism with LogType set to 'debug'.<br />
                    <b>umbracoDebugMode</b> needs to be enabled too.
                </p>
                <h4>urlTracker:404UrlsToIgnore</h4>
                <h5>comma-seperated string (empty)</h5>
                <p>The Url Tracker logs requests resulting in a 404 Not Found status. Some URLs shouldn't be logged and can be set here. One URL is always ignored by default: 'favicon.ico'.</p>
                <p>You can also ignore logging a 404 Not Found entry for certain requests, by adding an HTTP header 'X-UrlTracker-Ignore404' with value '1'</p>
                <h4>urlTracker:trackingDisabled</h4>
                <h5>boolean (false)</h5>
                <p>Set to true to disable tracking URL changes.</p>
                <h4>urlTracker:notFoundTrackingDisabled</h4>
                <h5>boolean (false)</h5>
                <p>Set to true to disable tracking not found (404) requests.</p>
                <h4>urlTracker:appendPortNumber</h4>
                <h5>boolean (true)</h5>
                <p>Set to false to disable appending a port number to redirect URLs</p>
                <h4>urlTracker:hasDomainOnChildNode</h4>
                <h5>boolean (false)</h5>
                <p>Set to true if a childnode has a domain</p>
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
                        3.2 [2014/11/14]
                        <ul>
                            <li>[Feature] Added new setting for appending the port number. Useful for when the site is running on a non-standard port number thanks to Greg Fyans</li>
                            <li>[BugFix] Fixed paths to relative paths, so it works in root and virtual folders thanks to Sandro Mastronardi</li>
                            <li>[BugFix] 410 Gone response wasn't working anymore</li>
                        </ul>
                    </li>
                    <li>
                        3.1 [2014/11/13]
                        <ul>
                            <li>[Improvement] SQL Compact Edition is now supported thanks to Ornella Geboers and Sandro Mastronardi</li>
                            <li>[Improvement] Versioning is now based on major.minor.build.revision (<a href="http://stackoverflow.com/questions/356543/can-i-automatically-increment-the-file-build-version-when-using-visual-studio">source</a>)</li>
                        </ul>
                    </li>
                    <li>
                        3.0.2 [2014/09/11]
                        <ul>
                            <li>[BugFix] Added exception handling if Application.SqlHelper throws an ArgumentNullException</li>
                        </ul>
                    </li>
                    <li>
                        3.0.1 [2014/07/09]
                        <ul>
                            <li>[BugFix] Fixed an issue with the installer (table does not exist: <a href="https://github.com/kipusoep/UrlTracker/issues/14">#14</a>)</li>
                        </ul>
                    </li>
                    <li>
                        3.0.0 [2014/06/27]
                        <ul>
                            <li>[Upgrade] All code has been rewritten to use the new services and models of umbraco v6.1</li>
                            <li>[BugFix] URL changes are tracked again (<a href="https://github.com/kipusoep/UrlTracker/issues/9" target="_blank">#9</a>)</li>
                            <li>[BugFix] 410 Gone response now gets lower priority than other response codes</li>
                            <li>[Improvement] Improved performance, removed hitting the DB for every single request, added caching instead</li>
                            <li>[Dropped] Dropped support for pre-6.1 umbraco versions</li>
                        </ul>
                    </li>
                    <li>
                        2.6.2 <b>EOL</b> [2014/06/23]
                        <ul>
                            <li>[BugFix] The 'if not exists'-check for the table index wasn't working correctly</li>
                        </ul>
                    </li>
                    <li>
                        2.6.1 [2014/06/23]
                        <ul>
                            <li>[BugFix] Added 'if not exists'-check for table index</li>
                        </ul>
                    </li>
                    <li>
                        2.6.0 [2014/06/20]
                        <ul>
                            <li>[BugFix] Huge performance increase</li>
                            <li>[BugFix] URL was being wrongly decoded when redirecting</li>
                        </ul>
                    </li>
                    <li>
                        2.5.5 [2014/03/03]
                        <ul>
                            <li>[BugFix] Nodes without a template assigned are now ignored if the URL changes</li>
                        </ul>
                    </li>
                    <li>
                        2.5.4 [2014/01/31]
                        <ul>
                            <li>[BugFix] The UrlTracker won't redirect to the exact same URL as the requested URL anymore</li>
                        </ul>
                    </li>
                    <li>
                        2.5.3 [2014/01/30]
                        <ul>
                            <li>[BugFix] Fixed a small bug with the automatic comments</li>
                            <li>[Feature] Added a button to the not found (404) view to clear all not found entries</li>
                            <li>[Feature] Added a config setting to disable tracking not found (404) requests</li>
                        </ul>
                    </li>
                    <li>
                        2.5.2 [2014/01/24]
                        <ul>
                            <li>[BugFix] Fixed an issue with path hostnames</li>
                        </ul>
                    </li>
                    <li>
                        2.5.1 [2014/01/22]
                        <ul>
                            <li>[BugFix] Fixed an issue with domains with only a language and no hostname attached to it (GertyEngrie)</li>
                            <li>[BugFix] Replacing a redirect node with a redirect URL didn't work (GertyEngrie)</li>
                        </ul>
                    </li>
                    <li>
                        2.5.0 [2014/01/15]
                        <ul>
                            <li>[Feature] Added support for Umbraco Belle (v7) thanks to Tim Geyssens (there's only an issue with scrollbars, but it's umbraco related: http://issues.umbraco.org/issue/U4-3940)</li>
                        </ul>
                    </li>
                    <li>
                        2.4.7 [2014/01/15]
                        <ul>
                            <li>[Bugfix] Fixed an issue with the Old URL field in the Not Found create redirect view</li>
                            <li>[Feature] Added the option to disable logging 404 Not Found to the UrlTracker by adding an HTTP header 'X-UrlTracker-Ignore404' with value '1' for a certain request</li>
                        </ul>
                    </li>
                    <li>
                        2.4.6 [2014/01/15]
                        <ul>
                            <li>[Bugfix] Fixed a small issue which occurred if the umbracoDbDSN isn't set</li>
                        </ul>
                    </li>
                    <li>
                        2.4.5 [2014/01/10]
                        <ul>
                            <li>[Bugfix] Domains list wasn't getting updated when domains where added or removed</li>
                            <li>[Bugfix] Fixed an encoding issue (http://our.umbraco.org/projects/developer-tools/301-url-tracker/version-2/46914-Redirect-with-url-encoded-chars-issue)</li>
                        </ul>
                    </li>
                    <li>
                        2.4.4 [2014/01/03]
                        <ul>
                            <li>[Bugfix] Fixed a silly bug which caused the 'Delete selected' button to be hidden</li>
                        </ul>
                    </li>
                    <li>
                        2.4.3 [2013/12/19]
                        <ul>
                            <li>[Bugfix] UI handles unpublished redirect nodes again (hide)</li>
                            <li>[Feature] Added the option to change the pagesize of the entries and not-found tables (saved in cookie)</li>
                            <li>[Improvement] Decreased the general font-size of the UI to 12px to match umbraco</li>
                            <li>[Improvement] Performance improvements</li>
                        </ul>
                    </li>
                    <li>
                        2.4.2 [2013/12/12]
                        <ul>
                            <li>[Bugfix] Fixed a bug when doing a clean install of the Url Tracker</li>
                            <li>[Bugfix] Fixed a bug which occurred when umbraco is running on a different port than 80</li>
                        </ul>
                    </li>
                    <li>
                        2.4.1 [2013/12/11]
                        <ul>
                            <li>[Bugfix] Fixed a bug when doing a clean install of the Url Tracker</li>
                            <li>[Bugfix] The database columns were too small in some situations</li>
                        </ul>
                    </li>
                    <li>
                        2.4.0 [2013/12/11]
                        <ul>
                            <li>[Bugfix] In some situations an exception would occur with umbraco v6 (Value cannot be null. Parameter name: umbracoContext)</li>
                            <li>[Feature] Added the option to force a redirect (will allways perform a redirect, even if a page at the specified old URL exists)</li>
                            <li>[Improvement] Massive performance improvements compared to v2.2.8 and newer</li>
                            <li>[Improvement] Better 404 handling; ignores 404 entry if the URL is an arterySignalR (BrowserLink) URL (VS 2013)</li>
                        </ul>
                    </li>
                    <li>
                        2.3.2 [2013/11/27]
                        <ul>
                            <li>[Bugfix] Fixed a hostnames issue</li>
                        </ul>
                    </li>
                    <li>
                        2.3.1 [2013/11/27]
                        <ul>
                            <li>[Bugfix] There was an issue with the HttpModule (http://our.umbraco.org/projects/developer-tools/301-url-tracker/version-2/46447-404-page-generates-error-once-URL-tracker-installed)</li>
                        </ul>
                    </li>
                    <li>
                        2.3.0 [2013/11/26]
                        <ul>
                            <li>[Bugfix] The old URL shown in the UI was sometimes wrong when a domain with path was configured</li>
                            <li>[Bugfix] Fixed a bug which occured when overlapping hostnames were configured, like 'domain.org' and 'domain.org/path'</li>
                        </ul>
                    </li>
                    <li>
                        2.2.9 [2013/11/25]
                        <ul>
                            <li>[Bugfix] Some automatic entries were not recognized as automatic entry</li>
                        </ul>
                    </li>
                    <li>
                        2.2.8 [2013/11/11]
                        <ul>
                            <li>[Bugfix] Improved the 'rootnode is 0' bugfix from 2.2.7</li>
                            <li>[Bugfix] An issue was introduced in version 2.2.6 for 'older' umbraco versions</li>
                            <li>[Improvement] UI handles unpublished nodes better</li>
                        </ul>
                    </li>
                    <li>
                        2.2.7 [2013/11/08]
                        <ul>
                            <li>[Bugfix] Fixed a UI issue when the rootnode is 0</li>
                            <li>[Improvement] Better 404 handling; ignores referrer if it's value is the Url Tracker UI, ignores 404 entry if the URL is a BrowserLink URL (VS 2013)</li>
                        </ul>
                    </li>
                    <li>
                        2.2.6 [2013/10/21]
                        <ul>
                            <li>[Bugfix] There were some issues with umbraco 6.1.x and static file extensions (like .html)</li>
                        </ul>
                    </li>
                    <li>
                        2.2.5 [2013/10/16]
                        <ul>
                            <li>[Bugfix] There was an issue with multiple entries with the same old url, but different querystrings</li>
                            <li>[Bugfix] When multiple versions of log4net exist in the bin folder, the UrlTracker would crash</li>
                        </ul>
                    </li>
                    <li>
                        2.2.3 [2013/08/30]
                        <ul>
                            <li>[Bugfix] Sometimes the installer was stuck at "Installing database table" (VirtualPathProvider issue)</li>
                        </ul>
                    </li>
                    <li>
                        2.2.2 [2013/08/23]
                        <ul>
                            <li>[Feature] Added regex capturing groups support (use '$n', where n is the capturing group number starting from 1)</li>
                            <li>[Feature] Added response from HttpModule if it 'fails' in the installer (debugging purposes)</li>
                        </ul>
                    </li>
                    <li>
                        2.2.0 [2013/08/22]
                        <ul>
                            <li>[Feature] Added filtering and searching</li>
                            <li>[Feature] Improved error handling</li>
                        </ul>
                    </li>
                    <li>
                        2.1.6 [2013/08/16]
                        <ul>
                            <li>[Bugfix] Fixed two small bugs (http module)</li>
                        </ul>
                    </li>
                    <li>
                        2.1.4 [2013/08/07]
                        <ul>
                            <li>[Bugfix] Migrating data from v1 to v2 resulted in an error when there were absolute OldUrls in the DB table</li>
                        </ul>
                    </li>
                    <li>
                        2.1.3 [2013/08/05]
                        <ul>
                            <li>[Bugfix] Redirects from URLs with non-aspx extensions were displayed with '.aspx' appended at the end</li>
                            <li>[Bugfix] Redirects with querystring passthrough failed sometimes</li>
                        </ul>
                    </li>
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
                    <li>Support UrlRewriting if possible</li>
                    <li>Azure support (if it doesn't work yet, untested)</li>
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
