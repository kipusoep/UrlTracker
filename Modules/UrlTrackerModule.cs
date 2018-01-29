using InfoCaster.Umbraco.UrlTracker.Exceptions;
using InfoCaster.Umbraco.UrlTracker.Extensions;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using InfoCaster.Umbraco.UrlTracker.UI.Installer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web;
using Umbraco.Web.Routing;
using UmbracoHelper = InfoCaster.Umbraco.UrlTracker.Helpers.UmbracoHelper;

namespace InfoCaster.Umbraco.UrlTracker.Modules
{
    public class UrlTrackerModule : IHttpModule
    {
        static DatabaseContext _databaseContext { get { return ApplicationContext.Current.DatabaseContext; } }
        static Regex _capturingGroupsRegex = new Regex("\\$\\d+");
        static readonly object _lock = new object();
        static bool _urlTrackerInstalled;
        static bool _execute = true;

        private static bool _urlTrackerSubscribed = false;
        private static readonly object _urlTrackerSubscribeLock = new object();

        #region IHttpModule Members
        public void Dispose() { }

        public void Init(HttpApplication app)
        {
            if (!_urlTrackerSubscribed)
            {
                lock (_urlTrackerSubscribeLock)
                {
                    if (!_urlTrackerSubscribed)
                    {
						// YSOD when running in integrated mode :(
						// app.PostResolveRequestCache += Context_PostResolveRequestCache;

						UmbracoModule.EndRequest += UmbracoModule_EndRequest;

                        LoggingHelper.LogInformation("UrlTracker HttpModule | Subscribed to EndRequest events");

                        _urlTrackerSubscribed = true;
                    }
                }
            }

			// Prevent YSOD crash
			// https://stackoverflow.com/questions/3712598/httpmodule-init-safely-add-httpapplication-beginrequest-handler-in-iis7-integr
			app.PostResolveRequestCache -= Context_PostResolveRequestCache;
			app.PostResolveRequestCache += Context_PostResolveRequestCache;

			LoggingHelper.LogInformation("UrlTracker HttpModule | Subscribed to AcquireRequestState events");

        }

		void Context_PostResolveRequestCache(object sender, EventArgs e)
		{
			CheckUrlTrackerInstalled();

			if (_execute)
				UrlTrackerDo("AcquireRequestState", ignoreHttpStatusCode: true, context: HttpContext.Current);
		}
		#endregion

		void UmbracoModule_EndRequest(object sender, UmbracoRequestEventArgs args)
        {
            CheckUrlTrackerInstalled();

            if (_execute)
                UrlTrackerDo("EndRequest", context: args.HttpContext.ApplicationInstance.Context);
        }

		#region Events

		public static event EventHandler<HttpResponse> PreUrlTracker;

		#endregion

		static void CheckUrlTrackerInstalled()
        {
            try
            {
                if (!_urlTrackerInstalled && Application.SqlHelper != null)
                {
                    lock (_lock)
                    {
                        _urlTrackerInstalled = UrlTrackerRepository.GetUrlTrackerTableExists();
                        if (!_urlTrackerInstalled)
                        {
                            UrlTrackerRepository.CreateUrlTrackerTable();
                            UrlTrackerInstallerService installer = new UrlTrackerInstallerService() { DontWait = true };
                            try
                            {
                                installer.InstallDashboard();
                            }
                            catch (DashboardAlreadyInstalledException) { }
                        }
                        UrlTrackerRepository.UpdateUrlTrackerTable();
                    }
                }
            }
            catch (ArgumentNullException) // Thrown if the umbraco connectionstring is empty
            {
                _execute = false;
            }
        }

        static void UrlTrackerDo(string callingEventName, bool ignoreHttpStatusCode = false, HttpContext context = null)
        {
            if (context == null)
            {
                LoggingHelper.LogInformation("UrlTracker HttpModule | No HttpContext has been passed by {0}", callingEventName);
                context = HttpContext.Current;
            }

            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            if (!string.IsNullOrEmpty(request.QueryString[UrlTrackerSettings.HttpModuleCheck]))
            {
                response.Clear();
                response.Write(UrlTrackerSettings.HttpModuleCheck);
                response.StatusCode = 200;
                response.End();
                return;
            }

            LoggingHelper.LogInformation("UrlTracker HttpModule | {0} start", callingEventName);

            if (UrlTrackerSettings.IsDisabled)
            {
                LoggingHelper.LogInformation("UrlTracker HttpModule | UrlTracker is disabled by config");
                return;
            }

            string url = request.RawUrl;
            if (url.StartsWith("/") && url != "/")
                url = url.Substring(1);

            LoggingHelper.LogInformation("UrlTracker HttpModule | Incoming URL is: {0}", url);

            if (_urlTrackerInstalled && (response.StatusCode == (int)HttpStatusCode.NotFound || ignoreHttpStatusCode))
            {
                if (response.StatusCode == (int)HttpStatusCode.NotFound)
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Response statusCode is 404, continue URL matching");
                else
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Checking for forced redirects (AcquireRequestState), continue URL matching");

                string urlWithoutQueryString = url;
                if (InfoCaster.Umbraco.UrlTracker.Helpers.UmbracoHelper.IsReservedPathOrUrl(url))
                {
                    LoggingHelper.LogInformation("UrlTracker HttpModule | URL is an umbraco reserved path or url, ignore request");
                    return;
                }

                //bool urlHasQueryString = request.QueryString.HasKeys() && url.Contains('?');
                bool urlHasQueryString = url.Contains('?'); // invalid querystring (?xxx) without = sign must also be stripped...

                if (urlHasQueryString)
                    urlWithoutQueryString = url.Substring(0, url.IndexOf('?'));

                string shortestUrl = UrlTrackerHelper.ResolveShortestUrl(urlWithoutQueryString);

                int rootNodeId = -1;
                var domains = UmbracoHelper.GetDomains().ToArray();
                if (domains.Any())
                {
                    string fullRawUrl;
                    string previousFullRawUrlTest;
                    string fullRawUrlTest;
                    fullRawUrl = previousFullRawUrlTest = fullRawUrlTest = string.Format("{0}{1}{2}{3}", request.Url.Scheme, Uri.SchemeDelimiter, request.Url.Host, request.Url.AbsolutePath);

                    UrlTrackerDomain urlTrackerDomain;
                    do
                    {
                        if (previousFullRawUrlTest.EndsWith("/"))
                        {
                            urlTrackerDomain = domains.FirstOrDefault(x => (x.UrlWithDomain == fullRawUrlTest) || (x.UrlWithDomain == fullRawUrlTest + "/"));
                            if (urlTrackerDomain != null)
                            {
                                rootNodeId = urlTrackerDomain.NodeId;
                                urlWithoutQueryString = fullRawUrl.Replace(fullRawUrlTest, string.Empty);
                                if (urlWithoutQueryString.StartsWith("/"))
                                    urlWithoutQueryString = urlWithoutQueryString.Substring(1);
                                if (urlWithoutQueryString.EndsWith("/"))
                                    urlWithoutQueryString = urlWithoutQueryString.Substring(0, urlWithoutQueryString.Length - 1);

                                break;
                            }
                        }
                        previousFullRawUrlTest = fullRawUrlTest;
                        fullRawUrlTest = fullRawUrlTest.Substring(0, fullRawUrlTest.Length - 1);
                    }
                    while (fullRawUrlTest.Length > 0);
                }
                if (rootNodeId == -1)
                {
                    var children = new Node(rootNodeId).ChildrenAsList.ToArray();
                    if (children != null && children.Any())
                        rootNodeId = children.First().Id;
                }
                else
                {
                    var rootUrl = "/";
                    try
                    {
                        rootUrl = new Node(rootNodeId).Url;
                    }
                    catch (ArgumentNullException)
                    {
                        // could not get full url for path, so we keep / as the root... (no other way to check, happens for favicon.ico for example)
                    }
                    var rootFolder = rootUrl != @"/" ? new Uri(HttpContext.Current.Request.Url, rootUrl).AbsolutePath.TrimStart('/') : string.Empty;
                    if (shortestUrl.StartsWith(rootFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        shortestUrl = shortestUrl.Substring(rootFolder.Length);
                    }
                }
                LoggingHelper.LogInformation("UrlTracker HttpModule | Current request's rootNodeId: {0}", rootNodeId);

                string redirectUrl = null;
                int? redirectHttpCode = null;
                bool redirectPassThroughQueryString = true;

                if (!ignoreHttpStatusCode)
                {
                    // Normal matching (database)
                    LoadUrlTrackerMatchesFromDatabase(request, urlWithoutQueryString, urlHasQueryString, shortestUrl, rootNodeId, ref redirectUrl, ref redirectHttpCode, ref redirectPassThroughQueryString);
                }
                else
                {
                    // Forced matching (cache)
                    LoadUrlTrackerMatchesFromCache(request, urlWithoutQueryString, urlHasQueryString, shortestUrl, rootNodeId, ref redirectUrl, ref redirectHttpCode, ref redirectPassThroughQueryString);
                }

                string query;
                if (!redirectHttpCode.HasValue)
                {
                    if (!ignoreHttpStatusCode)
                    {
                        // Normal matching (database)
                        // Regex matching
                        query = "SELECT * FROM icUrlTracker WHERE Is404 = 0 AND ForceRedirect = @forceRedirect AND (RedirectRootNodeId = @redirectRootNodeId OR RedirectRootNodeId = -1) AND OldRegex IS NOT NULL ORDER BY Inserted DESC";
                        UrlTrackerModel result = _databaseContext.Database.FirstOrDefault<UrlTrackerModel>(query, new { forceRedirect = ignoreHttpStatusCode ? 1 : 0, redirectRootNodeId = rootNodeId });
                        Regex regex;
                        if (result != null)
                        {
                            regex = new Regex(result.OldRegex);
                            if (regex.IsMatch(url))
                            {
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Regex match found");
                                if (result.RedirectNodeId.HasValue)
                                {
                                    int redirectNodeId = result.RedirectNodeId.Value;
                                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node id: {0}", redirectNodeId);
                                    Node n = new Node(redirectNodeId);
                                    if (n != null && n.Name != null && n.Id > 0)
                                    {
                                        redirectUrl = UmbracoHelper.GetUrl(redirectNodeId);
                                        LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
                                    }
                                    else
                                        LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node is invalid; node is null, name is null or id <= 0");
                                }
                                else if (!String.IsNullOrWhiteSpace(result.RedirectUrl))
                                {
                                    redirectUrl = result.RedirectUrl;
                                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);

                                    if (_capturingGroupsRegex.IsMatch(redirectUrl))
                                    {
                                        LoggingHelper.LogInformation("UrlTracker HttpModule | Found regex capturing groups in the redirect url");
                                        redirectUrl = regex.Replace(url, redirectUrl);

                                        LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url changed to: {0} (because of regex capturing groups)", redirectUrl);
                                    }
                                }

                                redirectPassThroughQueryString = result.RedirectPassThroughQueryString;
                                LoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is enabled");

                                redirectHttpCode = result.RedirectHttpCode;
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);
                            }
                        }
                    }
                    else
                    {
                        // Forced matching (cache)
                        List<UrlTrackerModel> forcedRedirects = UrlTrackerRepository.GetForcedRedirects().Where(x => !string.IsNullOrEmpty(x.OldRegex)).ToList();
                        if (forcedRedirects == null || !forcedRedirects.Any())
                            return;

                        foreach (var match in forcedRedirects.Where(x => x.RedirectRootNodeId == -1 || x.RedirectRootNodeId == rootNodeId)
                            .Select(x => new { UrlTrackerModel = x, Regex = new Regex(x.OldRegex) })
                            .Where(x => x.Regex.IsMatch(url)))
                        {
                            LoggingHelper.LogInformation("UrlTracker HttpModule | Regex match found");
                            if (match.UrlTrackerModel.RedirectNodeId.HasValue)
                            {
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node id: {0}", match.UrlTrackerModel.RedirectNodeId.Value);
                                Node n = new Node(match.UrlTrackerModel.RedirectNodeId.Value);
                                if (n != null && n.Name != null && n.Id > 0)
                                {
                                    redirectUrl = UmbracoHelper.GetUrl(match.UrlTrackerModel.RedirectNodeId.Value);
                                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
                                }
                                else
                                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node is invalid; node is null, name is null or id <= 0");
                            }
                            else if (!string.IsNullOrEmpty(match.UrlTrackerModel.RedirectUrl))
                            {
                                redirectUrl = match.UrlTrackerModel.RedirectUrl;
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);

                                if (_capturingGroupsRegex.IsMatch(redirectUrl))
                                {
                                    LoggingHelper.LogInformation("UrlTracker HttpModule | Found regex capturing groups in the redirect url");
                                    redirectUrl = match.Regex.Replace(url, redirectUrl);

                                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url changed to: {0} (because of regex capturing groups)", redirectUrl);
                                }
                            }

                            redirectPassThroughQueryString = match.UrlTrackerModel.RedirectPassThroughQueryString;
                            LoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is enabled");

                            redirectHttpCode = match.UrlTrackerModel.RedirectHttpCode;
                            LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);
                        }
                    }
                }

                if (redirectHttpCode.HasValue)
                {
                    string redirectLocation = string.Empty;

                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        if (redirectUrl == "/")
                            redirectUrl = string.Empty;
                        Uri redirectUri = new Uri(redirectUrl.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ? redirectUrl : string.Format("{0}{1}{2}{3}/{4}", request.Url.Scheme, Uri.SchemeDelimiter, request.Url.Host, request.Url.Port != 80 && UrlTrackerSettings.AppendPortNumber ? string.Concat(":", request.Url.Port) : string.Empty, redirectUrl.StartsWith("/") ? redirectUrl.Substring(1) : redirectUrl));
                        if (redirectPassThroughQueryString)
                        {
                            NameValueCollection redirectQueryString = HttpUtility.ParseQueryString(redirectUri.Query);
                            NameValueCollection newQueryString = HttpUtility.ParseQueryString(request.Url.Query);
                            if (redirectQueryString.HasKeys())
                                newQueryString = newQueryString.Merge(redirectQueryString);
                            string pathAndQuery = Uri.UnescapeDataString(redirectUri.PathAndQuery) + redirectUri.Fragment;
                            redirectUri = new Uri(string.Format("{0}{1}{2}{3}/{4}{5}", redirectUri.Scheme, Uri.SchemeDelimiter, redirectUri.Host, redirectUri.Port != 80 && UrlTrackerSettings.AppendPortNumber ? string.Concat(":", redirectUri.Port) : string.Empty, pathAndQuery.Contains('?') ? pathAndQuery.Substring(0, pathAndQuery.IndexOf('?')) : pathAndQuery.StartsWith("/") ? pathAndQuery.Substring(1) : pathAndQuery, newQueryString.HasKeys() ? string.Concat("?", newQueryString.ToQueryString()) : string.Empty));
                        }

                        if (redirectUri == new Uri(string.Format("{0}{1}{2}{3}/{4}", request.Url.Scheme, Uri.SchemeDelimiter, request.Url.Host, request.Url.Port != 80 && UrlTrackerSettings.AppendPortNumber ? string.Concat(":", request.Url.Port) : string.Empty, request.RawUrl.StartsWith("/") ? request.RawUrl.Substring(1) : request.RawUrl)))
                        {
                            LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect URL is the same as Request.RawUrl; don't redirect");
                            return;
                        }
                        if (request.Url.Host.Equals(redirectUri.Host, StringComparison.OrdinalIgnoreCase))
                        {
                            redirectLocation = redirectUri.PathAndQuery + redirectUri.Fragment;
                        }
                        else
                        {
                            redirectLocation = redirectUri.AbsoluteUri;
                        }
                        LoggingHelper.LogInformation("UrlTracker HttpModule | Response redirectlocation set to: {0}", redirectLocation);
                    }

                    response.Clear();
                    response.StatusCode = redirectHttpCode.Value;
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Response statuscode set to: {0}", response.StatusCode);

					if (PreUrlTracker != null)
					{
						PreUrlTracker(null, response);
						LoggingHelper.LogInformation("UrlTracker HttpModule | Custom event has been called: {0}", PreUrlTracker.Method.Name);
					}

                    if (!string.IsNullOrEmpty(redirectLocation))
                    {
                        response.RedirectLocation = redirectLocation;
                    }

                    response.End();
                }
                else if (!ignoreHttpStatusCode)
                {
                    // Log 404
                    if (!UrlTrackerSettings.IsNotFoundTrackingDisabled && !UrlTrackerSettings.NotFoundUrlsToIgnore.Contains(urlWithoutQueryString) && !UmbracoHelper.IsReservedPathOrUrl(urlWithoutQueryString) && request.Headers["X-UrlTracker-Ignore404"] != "1")
                    {
                        bool ignoreNotFoundBasedOnRegexPatterns = false;
                        foreach (Regex regexNotFoundUrlToIgnore in UrlTrackerSettings.RegexNotFoundUrlsToIgnore)
                        {
                            if (regexNotFoundUrlToIgnore.IsMatch(urlWithoutQueryString))
                            {
                                ignoreNotFoundBasedOnRegexPatterns = true;
                                break;
                            }
                        }

                        if (!ignoreNotFoundBasedOnRegexPatterns)
                        {
                            LoggingHelper.LogInformation("UrlTracker HttpModule | No match found, logging as 404 not found");
                            query = "INSERT INTO icUrlTracker (OldUrl, RedirectRootNodeId, ";
                            if (urlHasQueryString)
                                query += "OldUrlQueryString, ";
                            query += "Is404, Referrer) VALUES (@oldUrl, @redirectRootNodeId, ";
                            if (urlHasQueryString)
                                query += "@oldUrlQueryString, ";
                            query += "1, @referrer)";
                            _databaseContext.Database.Execute(query, new { oldUrl = urlWithoutQueryString, redirectRootNodeId = rootNodeId, oldUrlQueryString = request.QueryString.ToString(), referrer = request.UrlReferrer != null && !request.UrlReferrer.ToString().Contains(UrlTrackerSettings.ReferrerToIgnore) ? (object)request.UrlReferrer.ToString() : DBNull.Value });
                        }
                    }
                    if (UrlTrackerSettings.IsNotFoundTrackingDisabled)
                        LoggingHelper.LogInformation("UrlTracker HttpModule | No match found and not found (404) tracking is disabled");
                    if (UrlTrackerSettings.NotFoundUrlsToIgnore.Contains(urlWithoutQueryString))
                        LoggingHelper.LogInformation("UrlTracker HttpModule | No match found, url is configured to be ignored: {0}", urlWithoutQueryString);
                    else if (UmbracoHelper.IsReservedPathOrUrl(urlWithoutQueryString))
                        LoggingHelper.LogInformation("UrlTracker HttpModule | No match found, url is ignored because it's an umbraco reserved URL or path: {0}", urlWithoutQueryString);
                    else if (request.Headers["X-UrlTracker-Ignore404"] == "1")
                        LoggingHelper.LogInformation("UrlTracker HttpModule | No match found, url is ignored because the 'X-UrlTracker-Ignore404' header was set to '1'. URL: {0}", urlWithoutQueryString);
                }
                else
                    LoggingHelper.LogInformation("UrlTracker HttpModule | No match found in {0}", callingEventName);
            }
            else
                LoggingHelper.LogInformation("UrlTracker HttpModule | Response statuscode is not 404, UrlTracker won't do anything");

            LoggingHelper.LogInformation("UrlTracker HttpModule | {0} end", callingEventName);
        }

        static void LoadUrlTrackerMatchesFromDatabase(HttpRequest request, string urlWithoutQueryString, bool urlHasQueryString, string shortestUrl, int rootNodeId, ref string redirectUrl, ref int? redirectHttpCode, ref bool redirectPassThroughQueryString)
        {
            string query = "SELECT * FROM icUrlTracker WHERE Is404 = 0 AND ForceRedirect = 0 AND (RedirectRootNodeId = @redirectRootNodeId OR RedirectRootNodeId IS NULL OR RedirectRootNodeId = -1) AND (OldUrl = @url OR OldUrl = @shortestUrl) ORDER BY CASE WHEN RedirectHttpCode = 410 THEN 2 ELSE 1 END, OldUrlQueryString DESC";
            UrlTrackerModel result = _databaseContext.Database.FirstOrDefault<UrlTrackerModel>(query, new { redirectRootNodeId = rootNodeId, url = urlWithoutQueryString, shortestUrl = shortestUrl });

            if (result == null)
            {
                return;
            }
            LoggingHelper.LogInformation("UrlTracker HttpModule | URL match found");
            if (result.RedirectNodeId.HasValue && result.RedirectHttpCode != (int)HttpStatusCode.Gone)
            {
                int redirectNodeId = result.RedirectNodeId.Value;
                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node id: {0}", redirectNodeId);
                Node n = new Node(redirectNodeId);
                if (n != null && n.Name != null && n.Id > 0)
                {
                    string tempUrl = UmbracoHelper.GetUrl(redirectNodeId);
                    redirectUrl = tempUrl.StartsWith(Uri.UriSchemeHttp) ? tempUrl : string.Format("{0}{1}{2}{3}{4}", HttpContext.Current.Request.Url.Scheme, Uri.SchemeDelimiter, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port != 80 && UrlTrackerSettings.AppendPortNumber ? string.Concat(":", HttpContext.Current.Request.Url.Port) : string.Empty, tempUrl);
                    if (redirectUrl.StartsWith(Uri.UriSchemeHttp))
                    {
                        Uri redirectUri = new Uri(redirectUrl);
                        string pathAndQuery = Uri.UnescapeDataString(redirectUri.PathAndQuery) + redirectUri.Fragment;

                        redirectUrl = GetCorrectedUrl(redirectUri, rootNodeId, pathAndQuery);
                    }
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
                }
                else
                {
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node is invalid; node is null, name is null or id <= 0");
                    return;
                }
            }
            else if (!String.IsNullOrWhiteSpace(result.RedirectUrl))
            {
                redirectUrl = result.RedirectUrl;
                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
            }

            redirectPassThroughQueryString = result.RedirectPassThroughQueryString;
            LoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is {0}", redirectPassThroughQueryString ? "enabled" : "disabled");

            NameValueCollection oldUrlQueryString = null;
            if (!String.IsNullOrWhiteSpace(result.OldUrlQueryString))
            {
                oldUrlQueryString = HttpUtility.ParseQueryString(result.OldUrlQueryString);
                LoggingHelper.LogInformation("UrlTracker HttpModule | Old URL query string set to: {0}", oldUrlQueryString.ToQueryString());
            }

            if ((urlHasQueryString || oldUrlQueryString != null) && (oldUrlQueryString != null && !request.QueryString.CollectionEquals(oldUrlQueryString)))
            {
                LoggingHelper.LogInformation("UrlTracker HttpModule | Aborting; query strings don't match");
                return;
            }

            redirectHttpCode = result.RedirectHttpCode;
            LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);
        }

        private static string GetCorrectedUrl(Uri redirectUri, int rootNodeId, string pathAndQuery)
        {
            string redirectUrl = pathAndQuery;
            if (redirectUri.Host != HttpContext.Current.Request.Url.Host)
            {
                // if site runs on other domain then current, check if the current domain is already a domain for that site (prevent unnessecary redirect to primary domain)
                List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();
                List<UrlTrackerDomain> siteDomains = domains.Where(x => x.NodeId == rootNodeId).ToList();
                List<string> hosts =
                    siteDomains
                    .Select(x => new Uri(x.UrlWithDomain).Host)
                    .ToList();
                if (!hosts.Contains(redirectUri.Host))
                {
                    // if current domain is not related to target domain, do absoluteUri redirect
                    redirectUrl = new Uri(redirectUri, '/' + pathAndQuery.TrimStart('/')).AbsoluteUri;
                }
            }
            return redirectUrl;
        }

        static void LoadUrlTrackerMatchesFromCache(HttpRequest request, string urlWithoutQueryString, bool urlHasQueryString, string shortestUrl, int rootNodeId, ref string redirectUrl, ref int? redirectHttpCode, ref bool redirectPassThroughQueryString)
        {
            var forcedRedirects = UrlTrackerRepository.GetForcedRedirects();
            if (forcedRedirects == null || !forcedRedirects.Any())
                return;

            var redirects =
                forcedRedirects.Where(
                    x =>
                    !x.Is404 && (x.RedirectRootNodeId == rootNodeId || x.RedirectRootNodeId == -1)
                    && (string.Equals(x.OldUrl, urlWithoutQueryString, StringComparison.CurrentCultureIgnoreCase)
                        || string.Equals(x.OldUrl, shortestUrl, StringComparison.CurrentCultureIgnoreCase)))
                               .OrderBy(x => x.RedirectHttpCode == 410 ? 2 : 1)
                               .ThenByDescending(x => x.OldUrlQueryString)
                               .ToArray();
            foreach (UrlTrackerModel forcedRedirect in redirects)
            {
                LoggingHelper.LogInformation("UrlTracker HttpModule | URL match found");
                if (forcedRedirect.RedirectNodeId.HasValue && forcedRedirect.RedirectHttpCode != (int)HttpStatusCode.Gone)
                {
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node id: {0}", forcedRedirect.RedirectNodeId.Value);

                    Node n = new Node(forcedRedirect.RedirectNodeId.Value);
                    if (n != null && n.Name != null && n.Id > 0)
                    {
                        string tempUrl = UmbracoHelper.GetUrl(forcedRedirect.RedirectNodeId.Value);
                        redirectUrl = tempUrl.StartsWith(Uri.UriSchemeHttp) ? tempUrl : string.Format("{0}{1}{2}{3}{4}", HttpContext.Current.Request.Url.Scheme, Uri.SchemeDelimiter, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port != 80 && UrlTrackerSettings.AppendPortNumber ? string.Concat(":", HttpContext.Current.Request.Url.Port) : string.Empty, tempUrl);
                        if (redirectUrl.StartsWith(Uri.UriSchemeHttp))
                        {
                            Uri redirectUri = new Uri(redirectUrl);
                            string pathAndQuery = Uri.UnescapeDataString(redirectUri.PathAndQuery);
                            //redirectUrl = pathAndQuery.StartsWith("/") && pathAndQuery != "/" ? pathAndQuery.Substring(1) : pathAndQuery;
                            redirectUrl = GetCorrectedUrl(redirectUri, forcedRedirect.RedirectRootNodeId, pathAndQuery);

                        }
                        LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
                    }
                    else
                    {
                        LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node is invalid; node is null, name is null or id <= 0");
                        continue;
                    }
                }
                else if (!string.IsNullOrEmpty(forcedRedirect.RedirectUrl))
                {
                    redirectUrl = forcedRedirect.RedirectUrl;
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
                }

                redirectPassThroughQueryString = forcedRedirect.RedirectPassThroughQueryString;
                LoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is {0}", redirectPassThroughQueryString ? "enabled" : "disabled");

                NameValueCollection oldUrlQueryString = null;
                if (!string.IsNullOrEmpty(forcedRedirect.OldUrlQueryString))
                {
                    oldUrlQueryString = HttpUtility.ParseQueryString(forcedRedirect.OldUrlQueryString);
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Old URL query string set to: {0}", oldUrlQueryString.ToQueryString());
                }

                if ((urlHasQueryString || oldUrlQueryString != null) && (oldUrlQueryString != null && !request.QueryString.CollectionEquals(oldUrlQueryString)))
                {
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Aborting; query strings don't match");
                    continue;
                }

                redirectHttpCode = forcedRedirect.RedirectHttpCode;
                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);

                break;
            }
        }
    }
}