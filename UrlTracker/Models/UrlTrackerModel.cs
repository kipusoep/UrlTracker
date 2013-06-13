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
		public string CalculatedOldUrl
		{
			get
			{
				if (!string.IsNullOrEmpty(OldRegex) && string.IsNullOrEmpty(OldUrl))
					return string.Concat("Regex: ", OldRegex);
				return string.Format("{0}{1}{2}", OldUrl, !string.IsNullOrEmpty(OldUrlQueryString) ? "?" : string.Empty, OldUrlQueryString);
			}
		}
		public string CalculatedRedirectUrl
		{
			get
			{
				string calculatedRedirectUrl = !string.IsNullOrEmpty(RedirectUrl) ?
					RedirectUrl :
					RedirectNodeId.HasValue ?
						new Node(RedirectNodeId.Value).NiceUrl != "#" ? new Uri(new Node(RedirectNodeId.Value).NiceUrl).AbsolutePath :
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
				if (RedirectNodeId.HasValue && ((Notes.StartsWith("A parent") || Notes.StartsWith("This document")) && (Notes.EndsWith(" was moved") || Notes.EndsWith(" was renamed") || Notes.EndsWith("'s property 'umbracoUrlName' changed"))))
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