using InfoCaster.Umbraco.UrlTracker.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.NodeFactory;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
	public enum UrlTrackerViewTypes
	{
		Custom,
		Auto,
		NotFound
	}

	[Serializable]
	public class UrlTrackerModel
	{
		#region Data fields
		public int Id { get; set; }
		public string OldUrl { get; set; }
		public string OldUrlQueryString { get; set; }
		public string OldRegex { get; set; }
		public int RedirectRootNodeId { get; set; }
		public int? RedirectNodeId { get; set; }
		public string RedirectUrl { get; set; }
		public int RedirectHttpCode { get; set; }
		public bool RedirectPassThroughQueryString { get; set; }
		public string Notes { get; set; }
		public bool Is404 { get; set; }
		public string Referrer { get; set; }
		public int? NotFoundCount { get; set; }
		public DateTime Inserted { get; set; }
		#endregion

		#region Calculated properties
		public string CalculatedOldUrlWithoutQuery
		{
			get
			{
				if (CalculatedOldUrl.StartsWith("Regex:"))
					return CalculatedOldUrl;
				return CalculatedOldUrl.Contains('?') ? CalculatedOldUrl.Substring(0, CalculatedOldUrl.IndexOf('?')) : CalculatedOldUrl;
			}
		}
		public string CalculatedOldUrl
		{
			get
			{
				if (CalculatedOldUrlWithDomain.StartsWith("Regex:"))
					return CalculatedOldUrlWithDomain;
				Uri calculatedOldUrlWithDomain = new Uri(CalculatedOldUrlWithDomain);
				string pathAndQuery = calculatedOldUrlWithDomain.PathAndQuery;
				return pathAndQuery.StartsWith("/") ? pathAndQuery.Substring(1) : pathAndQuery;
			}
		}
		public string CalculatedOldUrlWithDomain
		{
			get
			{
				if (string.IsNullOrEmpty(OldRegex) && string.IsNullOrEmpty(OldUrl))
					throw new InvalidOperationException("Both OldRegex and OldUrl are empty, which is invalid. Please correct this by removing any entries where the OldUrl and OldRegex columns are empty.");
				if (!string.IsNullOrEmpty(OldRegex) && string.IsNullOrEmpty(OldUrl))
					return string.Concat("Regex: ", OldRegex);

				UrlTrackerDomain domain = null;
				Node redirectRootNode = new Node(RedirectRootNodeId);
				List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();
				domain = domains.FirstOrDefault(x => x.NodeId == redirectRootNode.Id);
				if (domain == null)
					domain = new UrlTrackerDomain(-1, redirectRootNode.Id, HttpContext.Current.Request.Url.Host);

				return string.Format("{0}{1}{2}", new Uri(string.Concat(domain.UrlWithDomain, !domain.UrlWithDomain.EndsWith("/") && !OldUrl.StartsWith("/") ? "/" : string.Empty, UrlTrackerHelper.ResolveUmbracoUrl(OldUrl))), !string.IsNullOrEmpty(OldUrlQueryString) ? "?" : string.Empty, OldUrlQueryString);
			}
		}
		public string CalculatedRedirectUrl
		{
			get
			{
				string calculatedRedirectUrl = !string.IsNullOrEmpty(RedirectUrl) ?
					RedirectUrl :
					RedirectNodeId.HasValue ?
						!new Node(RedirectNodeId.Value).NiceUrl.EndsWith("#") ? new Uri(
							umbraco.library.NiceUrl(RedirectNodeId.Value).StartsWith("http") ? umbraco.library.NiceUrl(RedirectNodeId.Value) : string.Format("{0}://{1}{2}{3}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port != 80 ? string.Concat(":", HttpContext.Current.Request.Url.Port) : string.Empty, umbraco.library.NiceUrl(RedirectNodeId.Value))
						).AbsolutePath :
						string.Empty :
					string.Empty;
				return calculatedRedirectUrl.StartsWith("/") && calculatedRedirectUrl != "/" ? calculatedRedirectUrl.Substring(1) : calculatedRedirectUrl;
			}
		}
		public string RedirectRootNodeName { get { return new Node(RedirectRootNodeId).Name; } }
		public UrlTrackerViewTypes ViewType
		{
			get
			{
				if (RedirectNodeId.HasValue && ((Notes.StartsWith("A parent") || Notes.StartsWith("This page") || Notes.StartsWith("This document")) && (Notes.EndsWith(" was moved") || Notes.EndsWith(" was renamed") || Notes.EndsWith("'s property 'umbracoUrlName' changed"))))
					return UrlTrackerViewTypes.Auto;
				if (Is404)
					return UrlTrackerViewTypes.NotFound;
				return UrlTrackerViewTypes.Custom;
			}
		}
		#endregion

		public UrlTrackerModel() { }

		public UrlTrackerModel(string oldUrl, string oldUrlQueryString, string oldRegex, int redirectRootNodeId, int? redirectNodeId, string redirectUrl, int redirectHttpCode, bool redirectPassThroughQueryString, string notes)
		{
			OldUrl = oldUrl;
			OldUrlQueryString = oldUrlQueryString;
			OldRegex = oldRegex;
			RedirectRootNodeId = redirectRootNodeId;
			RedirectNodeId = redirectNodeId;
			RedirectUrl = redirectUrl;
			RedirectHttpCode = redirectHttpCode;
			RedirectPassThroughQueryString = redirectPassThroughQueryString;
			Notes = notes ?? string.Empty;
		}

		public UrlTrackerModel(int id, string oldUrl, string oldUrlQueryString, string oldRegex, int redirectRootNodeId, int? redirectNodeId, string redirectUrl, int redirectHttpCode, bool redirectPassThroughQueryString, string notes, bool is404, string referrer, DateTime inserted)
			: this(oldUrl, oldUrlQueryString, oldRegex, redirectRootNodeId, redirectNodeId, redirectUrl, redirectHttpCode, redirectPassThroughQueryString, notes)
		{
			Id = id;
			Is404 = is404;
			Referrer = referrer;
			Inserted = inserted;
		}
	}
}