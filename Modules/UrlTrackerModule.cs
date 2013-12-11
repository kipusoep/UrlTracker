using InfoCaster.Umbraco.UrlTracker.Extensions;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.NodeFactory;
using Umbraco.Web;
using UmbracoHelper = InfoCaster.Umbraco.UrlTracker.Helpers.UmbracoHelper;

namespace InfoCaster.Umbraco.UrlTracker.Modules
{
    public class UrlTrackerModule : IHttpModule
    {
        static ISqlHelper _sqlHelper { get { return Application.SqlHelper; } }
        static Regex _capturingGroupsRegex = new Regex("\\$\\d+");
        static bool _ranUpdates = false;
        static readonly object _lock = new object();

        #region IHttpModule Members
        public void Dispose() { }

        public void Init(HttpApplication context)
        {
            if (!_ranUpdates)
            {
                lock(_lock)
                {
                    UrlTrackerRepository.UpdateUrlTrackerTable();
                    _ranUpdates = true;
                }
            }

            context.AcquireRequestState += context_AcquireRequestState;
            context.EndRequest += context_EndRequest;

            LoggingHelper.LogInformation("UrlTracker HttpModule | Subscribed to PostReleaseRequestState event");
        }
        #endregion

        void context_AcquireRequestState(object sender, EventArgs e)
        {
            UrlTrackerDo(ignoreHttpStatusCode: true);
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            UrlTrackerDo();
        }

        private static void UrlTrackerDo(bool ignoreHttpStatusCode = false)
        {
            HttpContext context = HttpContext.Current;
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

            LoggingHelper.LogInformation("UrlTracker HttpModule | PostReleaseRequestState start");

            if (UrlTrackerSettings.IsDisabled)
            {
                LoggingHelper.LogInformation("UrlTracker HttpModule | UrlTracker is disabled by config");
                return;
            }

            string url = request.RawUrl;
            if (url.StartsWith("/"))
                url = url.Substring(1);

            LoggingHelper.LogInformation("UrlTracker HttpModule | Incoming URL is: {0}", url);

            if (response.StatusCode == (int)HttpStatusCode.NotFound || ignoreHttpStatusCode)
            {
                if(response.StatusCode == (int)HttpStatusCode.NotFound)
                LoggingHelper.LogInformation("UrlTracker HttpModule | Response statusCode is 404 or , continue URL matching");

                string urlWithoutQueryString = url;
                if (InfoCaster.Umbraco.UrlTracker.Helpers.UmbracoHelper.IsReservedPathOrUrl(url))
                {
                    LoggingHelper.LogInformation("UrlTracker HttpModule | URL is an umbraco reserved path or url, ignore request");
                    return;
                }

                bool urlHasQueryString = request.QueryString.HasKeys() && url.Contains('?');
                if (urlHasQueryString)
                    urlWithoutQueryString = url.Substring(0, url.IndexOf('?'));

                string shortestUrl = UrlTrackerHelper.ResolveShortestUrl(urlWithoutQueryString);

                int rootNodeId = -1;
                List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();
                if (domains.Any())
                {
                    string fullRawUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Host, request.RawUrl);

                    UrlTrackerDomain urlTrackerDomain;
                    do
                    {
                        urlTrackerDomain = domains.FirstOrDefault(x => x.UrlWithDomain == fullRawUrl);
                        if (urlTrackerDomain != null)
                        {
                            rootNodeId = urlTrackerDomain.NodeId;
                            break;
                        }
                        fullRawUrl = fullRawUrl.Substring(0, fullRawUrl.Length - 1);
                    }
                    while (fullRawUrl.Length > 0);
                }
                if (rootNodeId == -1)
                {
                    rootNodeId = -1;
                    List<INode> children = new Node(rootNodeId).ChildrenAsList;
                    if (children != null && children.Any())
                        rootNodeId = children.First().Id;
                }
                LoggingHelper.LogInformation("UrlTracker HttpModule | Current request's rootNodeId: {0}", rootNodeId);

                string redirectUrl = null;
                int? redirectHttpCode = null;
                bool redirectPassThroughQueryString = true;

                // Normal matching
                string query = "SELECT * FROM icUrlTracker WHERE Is404 = 0 AND ForceRedirect = @forceRedirect AND (RedirectRootNodeId = @redirectRootNodeId OR RedirectRootNodeId IS NULL) AND (OldUrl = @url OR OldUrl = @shortestUrl) ORDER BY OldUrlQueryString DESC";
                using (IRecordsReader reader = _sqlHelper.ExecuteReader(query, _sqlHelper.CreateParameter("forceRedirect", ignoreHttpStatusCode ? "1" : "0"), _sqlHelper.CreateParameter("redirectRootNodeId", rootNodeId), _sqlHelper.CreateParameter("url", urlWithoutQueryString), _sqlHelper.CreateParameter("shortestUrl", shortestUrl)))
                {
                    while (reader.Read())
                    {
                        LoggingHelper.LogInformation("UrlTracker HttpModule | URL match found");
                        if (!reader.IsNull("RedirectNodeId"))
                        {
                            int redirectNodeId = reader.GetInt("RedirectNodeId");
                            LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node id: {0}", redirectNodeId);
                            Node n = new Node(redirectNodeId);
                            if (n != null && n.Name != null && n.Id > 0)
                            {
                                string tempUrl = UmbracoHelper.GetUrl(redirectNodeId);
                                redirectUrl = tempUrl.StartsWith("http") ? tempUrl : string.Format("{0}://{1}{2}{3}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port != 80 ? string.Concat(":", HttpContext.Current.Request.Url.Port) : string.Empty, tempUrl);
                                if (redirectUrl.StartsWith("http"))
                                {
                                    Uri redirectUri = new Uri(redirectUrl);
                                    redirectUrl = redirectUri.PathAndQuery.StartsWith("/") && redirectUri.PathAndQuery != "/" ? redirectUri.PathAndQuery.Substring(1) : redirectUri.PathAndQuery;
                                }
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
                            }
                            else
                            {
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node is invalid; node is null, name is null or id <= 0");
                                continue;
                            }
                        }
                        else if (!reader.IsNull("RedirectUrl"))
                        {
                            redirectUrl = reader.GetString("RedirectUrl");
                            LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
                        }

                        redirectPassThroughQueryString = reader.GetBoolean("RedirectPassThroughQueryString");
                        LoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is {0}", redirectPassThroughQueryString ? "enabled" : "disabled");

                        NameValueCollection oldUrlQueryString = null;
                        if (!reader.IsNull("OldUrlQueryString"))
                        {
                            oldUrlQueryString = HttpUtility.ParseQueryString(reader.GetString("OldUrlQueryString"));
                            LoggingHelper.LogInformation("UrlTracker HttpModule | Old URL query string set to: {0}", oldUrlQueryString.ToQueryString());
                        }

                        if ((urlHasQueryString || oldUrlQueryString != null) && (oldUrlQueryString != null && !request.QueryString.CollectionEquals(oldUrlQueryString)))
                        {
                            LoggingHelper.LogInformation("UrlTracker HttpModule | Aborting; query strings don't match");
                            continue;
                        }

                        redirectHttpCode = reader.GetInt("RedirectHttpCode");
                        LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);

                        break;
                    }
                }

                if (!redirectHttpCode.HasValue)
                {
                    // Regex matching
                    query = "SELECT * FROM icUrlTracker WHERE Is404 = 0 AND ForceRedirect = @forceRedirect AND RedirectRootNodeId = @redirectRootNodeId AND OldRegex IS NOT NULL ORDER BY Inserted DESC";
                    using (IRecordsReader reader = _sqlHelper.ExecuteReader(query, _sqlHelper.CreateParameter("forceRedirect", ignoreHttpStatusCode ? "1" : "0"), _sqlHelper.CreateParameter("redirectRootNodeId", rootNodeId)))
                    {
                        Regex regex;
                        while (reader.Read())
                        {
                            regex = new Regex(reader.GetString("OldRegex"));
                            if (regex.IsMatch(url))
                            {
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Regex match found");
                                if (!reader.IsNull("RedirectNodeId"))
                                {
                                    int redirectNodeId = reader.GetInt("RedirectNodeId");
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
                                else if (!reader.IsNull("RedirectUrl"))
                                {
                                    redirectUrl = reader.GetString("RedirectUrl");
                                    LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);

                                    if (_capturingGroupsRegex.IsMatch(redirectUrl))
                                    {
                                        LoggingHelper.LogInformation("UrlTracker HttpModule | Found regex capturing groups in the redirect url");
                                        redirectUrl = regex.Replace(url, redirectUrl);

                                        LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url changed to: {0} (because of regex capturing groups)", redirectUrl);
                                    }
                                }

                                redirectPassThroughQueryString = reader.GetBoolean("RedirectPassThroughQueryString");
                                LoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is enabled");

                                redirectHttpCode = reader.GetInt("RedirectHttpCode");
                                LoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);
                            }
                        }
                    }
                }

                if (redirectHttpCode.HasValue)
                {
                    response.Clear();
                    response.StatusCode = redirectHttpCode.Value;
                    LoggingHelper.LogInformation("UrlTracker HttpModule | Response statuscode set to: {0}", response.StatusCode);
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        if (redirectUrl == "/")
                            redirectUrl = string.Empty;
                        Uri redirectUri = new Uri(redirectUrl.StartsWith("http") ? redirectUrl : string.Format("{0}://{1}{2}/{3}", request.Url.Scheme, request.Url.Host, request.Url.Port != 80 ? string.Concat(":", request.Url.Port) : string.Empty, redirectUrl));
                        if (redirectPassThroughQueryString)
                        {
                            NameValueCollection redirectQueryString = HttpUtility.ParseQueryString(redirectUri.Query);
                            NameValueCollection newQueryString = HttpUtility.ParseQueryString(request.Url.Query);
                            if (redirectQueryString.HasKeys())
                                newQueryString = newQueryString.Merge(redirectQueryString);
                            redirectUri = new Uri(string.Format("{0}://{1}{2}/{3}{4}", redirectUri.Scheme, redirectUri.Host, redirectUri.Port != 80 ? string.Concat(":", redirectUri.Port) : string.Empty, redirectUri.PathAndQuery.Contains('?') ? redirectUri.PathAndQuery.Substring(0, redirectUri.PathAndQuery.IndexOf('?')) : redirectUri.PathAndQuery.StartsWith("/") ? redirectUri.PathAndQuery.Substring(1) : redirectUri.PathAndQuery, newQueryString.HasKeys() ? string.Concat("?", newQueryString.ToQueryString()) : string.Empty));
                        }
                        response.RedirectLocation = redirectUri.ToString();
                        LoggingHelper.LogInformation("UrlTracker HttpModule | Response redirectlocation set to: {0}", response.RedirectLocation);
                    }
                    response.End();
                }
                else if(!ignoreHttpStatusCode)
                {
                    // Log 404
                    if (!UrlTrackerSettings.NotFoundUrlsToIgnore.Contains(urlWithoutQueryString) && !UmbracoHelper.IsReservedPathOrUrl(urlWithoutQueryString))
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
                            _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateParameter("oldUrl", urlWithoutQueryString), _sqlHelper.CreateParameter("redirectRootNodeId", rootNodeId), _sqlHelper.CreateParameter("oldUrlQueryString", request.QueryString.ToString()), _sqlHelper.CreateParameter("referrer", request.UrlReferrer != null && !request.UrlReferrer.ToString().Contains(UrlTrackerSettings.ReferrerToIgnore) ? (object)request.UrlReferrer.ToString() : DBNull.Value));
                        }
                    }
                    if (UrlTrackerSettings.NotFoundUrlsToIgnore.Contains(urlWithoutQueryString))
                        LoggingHelper.LogInformation("UrlTracker HttpModule | No match found, url is configured to be ignored: {0}", urlWithoutQueryString);
                    else if (UmbracoHelper.IsReservedPathOrUrl(urlWithoutQueryString))
                        LoggingHelper.LogInformation("UrlTracker HttpModule | No match found, url is ignored because it's an umbraco reserved URL or path: {0}", urlWithoutQueryString);
                }
            }
            else
                LoggingHelper.LogInformation("UrlTracker HttpModule | Response statuscode is not 404, UrlTracker won't do anything");

            LoggingHelper.LogInformation("UrlTracker HttpModule | PostReleaseRequestState end");
        }
    }
}
