using InfoCaster.Umbraco.UrlTracker.Extensions;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.interfaces;
using umbraco.NodeFactory;
using UmbracoHelper = InfoCaster.Umbraco.UrlTracker.Helpers.UmbracoHelper;

namespace InfoCaster.Umbraco.UrlTracker.Modules
{
	public class UrlTrackerModule : IHttpModule
	{
		static ISqlHelper _sqlHelper { get { return Application.SqlHelper; } }
		static Regex _capturingGroupsRegex = new Regex("\\$\\d+");

		#region IHttpModule Members
		public void Dispose() { }

		public void Init(HttpApplication context)
		{
			context.EndRequest += context_EndRequest;

			UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Subscribed to EndRequest event");
		}
		#endregion

		void context_EndRequest(object sender, EventArgs e)
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

			UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | EndRequest start");

			if (UrlTrackerSettings.IsDisabled)
			{
				UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | UrlTracker is disabled by config");
				return;
			}

			if (response.StatusCode == 404)
			{
				UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Response statusCode is 404, continue URL matching");

				string url = request.RawUrl;
				if (url.StartsWith("/"))
					url = url.Substring(1);

				UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Incoming URL is: {0}", url);

				string urlWithoutQueryString = url;
				if (InfoCaster.Umbraco.UrlTracker.Helpers.UmbracoHelper.IsReservedPathOrUrl(url))
				{
					UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | URL is an umbraco reserved path or url, ignore request");
					return;
				}

				bool urlHasQueryString = request.QueryString.HasKeys() && url.Contains('?');
				if (urlHasQueryString)
					urlWithoutQueryString = url.Substring(0, url.IndexOf('?'));

				string shortestUrl = UrlTrackerHelper.ResolveShortestUrl(urlWithoutQueryString);

				string host = request.Url.Host;

				int rootNodeId = Domain.GetRootFromDomain(host);
				if (rootNodeId == -1)
				{
					rootNodeId = -1;
					List<INode> children = new Node(rootNodeId).ChildrenAsList;
					if (children != null && children.Any())
						rootNodeId = children.First().Id;

				}
				UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Current request's rootNodeId: {0}", rootNodeId);

				string redirectUrl = null;
				int? redirectHttpCode = null;
				bool redirectPassThroughQueryString = true;

				// Normal matching
				string query = "SELECT * FROM icUrlTracker WHERE Is404 = 0 AND (RedirectRootNodeId = @redirectRootNodeId OR RedirectRootNodeId IS NULL) AND (OldUrl = @url OR OldUrl = @shortestUrl) ORDER BY OldUrlQueryString DESC";
				using (IRecordsReader reader = _sqlHelper.ExecuteReader(query, _sqlHelper.CreateParameter("redirectRootNodeId", rootNodeId), _sqlHelper.CreateParameter("url", urlWithoutQueryString), _sqlHelper.CreateParameter("shortestUrl", shortestUrl)))
				{
					while (reader.Read())
					{
						UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | URL match found");
						if (!reader.IsNull("RedirectNodeId"))
						{
							int redirectNodeId = reader.GetInt("RedirectNodeId");
							UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node id: {0}", redirectNodeId);
							Node n = new Node(redirectNodeId);
							if (n != null && n.Name != null && n.Id > 0)
							{
								redirectUrl = umbraco.library.NiceUrl(redirectNodeId).StartsWith("http") ? umbraco.library.NiceUrl(redirectNodeId) : string.Format("{0}://{1}{2}{3}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port != 80 ? string.Concat(":", HttpContext.Current.Request.Url.Port) : string.Empty, umbraco.library.NiceUrl(redirectNodeId));
								if (redirectUrl.StartsWith("http"))
								{
									Uri redirectUri = new Uri(redirectUrl);
									redirectUrl = redirectUri.PathAndQuery.StartsWith("/") && redirectUri.PathAndQuery != "/" ? redirectUri.PathAndQuery.Substring(1) : redirectUri.PathAndQuery;
								}
								UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
							}
							else
								UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node is invalid; node is null, name is null or id <= 0");
						}
						else if (!reader.IsNull("RedirectUrl"))
						{
							redirectUrl = reader.GetString("RedirectUrl");
							UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
						}

						redirectPassThroughQueryString = reader.GetBoolean("RedirectPassThroughQueryString");
						UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is {0}", redirectPassThroughQueryString ? "enabled" : "disabled");

						NameValueCollection oldUrlQueryString = null;
						if (!reader.IsNull("OldUrlQueryString"))
						{
							oldUrlQueryString = HttpUtility.ParseQueryString(reader.GetString("OldUrlQueryString"));
							UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Old URL query string set to: {0}", oldUrlQueryString.ToQueryString());
						}

						if ((urlHasQueryString || oldUrlQueryString != null) && (oldUrlQueryString != null && !request.QueryString.CollectionEquals(oldUrlQueryString)))
						{
							UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Aborting; query strings don't match");
							continue;
						}

						redirectHttpCode = reader.GetInt("RedirectHttpCode");
						UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);
					}
				}

				if (!redirectHttpCode.HasValue)
				{
					// Regex matching
					query = "SELECT * FROM icUrlTracker WHERE Is404 = 0 AND RedirectRootNodeId = @redirectRootNodeId AND OldRegex IS NOT NULL ORDER BY Inserted DESC";
					using (IRecordsReader reader = _sqlHelper.ExecuteReader(query, _sqlHelper.CreateParameter("redirectRootNodeId", rootNodeId)))
					{
						Regex regex;
						while (reader.Read())
						{
							regex = new Regex(reader.GetString("OldRegex"));
							if (regex.IsMatch(url))
							{
								UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Regex match found");
								if (!reader.IsNull("RedirectNodeId"))
								{
									int redirectNodeId = reader.GetInt("RedirectNodeId");
									UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node id: {0}", redirectNodeId);
									Node n = new Node(redirectNodeId);
									if (n != null && n.Name != null && n.Id > 0)
									{
										redirectUrl = umbraco.library.NiceUrl(redirectNodeId);
										UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);
									}
									else
										UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect node is invalid; node is null, name is null or id <= 0");
								}
								else if (!reader.IsNull("RedirectUrl"))
								{
									redirectUrl = reader.GetString("RedirectUrl");
									UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url set to: {0}", redirectUrl);

									if (_capturingGroupsRegex.IsMatch(redirectUrl))
									{
										UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Found regex capturing groups in the redirect url");
										redirectUrl = regex.Replace(url, redirectUrl);

										UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect url changed to: {0} (because of regex capturing groups)", redirectUrl);
									}
								}

								redirectPassThroughQueryString = reader.GetBoolean("RedirectPassThroughQueryString");
								UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | PassThroughQueryString is enabled");

								redirectHttpCode = reader.GetInt("RedirectHttpCode");
								UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Redirect http code set to: {0}", redirectHttpCode);
							}
						}
					}
				}

				if (redirectHttpCode.HasValue)
				{
					response.Clear();
					response.StatusCode = redirectHttpCode.Value;
					UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Response statuscode set to: {0}", response.StatusCode);
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
						UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Response redirectlocation set to: {0}", response.RedirectLocation);
					}
					response.End();
				}
				else
				{
					// Log 404
					if (!UrlTrackerSettings.NotFoundUrlsToIgnore.Contains(urlWithoutQueryString) && !UmbracoHelper.IsReservedPathOrUrl(urlWithoutQueryString))
					{
						UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | No match found, logging as 404 not found");
						query = "INSERT INTO icUrlTracker (OldUrl, RedirectRootNodeId, ";
						if (urlHasQueryString)
							query += "OldUrlQueryString, ";
						query += "Is404, Referrer) VALUES (@oldUrl, @redirectRootNodeId, ";
						if (urlHasQueryString)
							query += "@oldUrlQueryString, ";
						query += "1, @referrer)";
						_sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateParameter("oldUrl", urlWithoutQueryString), _sqlHelper.CreateParameter("redirectRootNodeId", rootNodeId), _sqlHelper.CreateParameter("oldUrlQueryString", request.QueryString.ToString()), _sqlHelper.CreateParameter("referrer", request.UrlReferrer != null ? (object)request.UrlReferrer.ToString() : DBNull.Value));
					}
					if (UrlTrackerSettings.NotFoundUrlsToIgnore.Contains(urlWithoutQueryString))
						UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | No match found, url is configured to be ignored: {0}", urlWithoutQueryString);
					else if (UmbracoHelper.IsReservedPathOrUrl(urlWithoutQueryString))
						UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | No match found, url is ignored because it's an umbraco reserved URL or path: {0}", urlWithoutQueryString);
				}
			}
			else
				UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | Response statuscode is not 404, UrlTracker won't do anything");

			UrlTrackerLoggingHelper.LogInformation("UrlTracker HttpModule | EndRequest end");
		}
	}
}
