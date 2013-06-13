using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
	public static class UrlTrackerHelper
	{
		public static string ResolveShortestUrl(string url)
		{
			if (url.StartsWith("http://") || url.StartsWith("https://"))
			{
				Uri uri = new Uri(url);
				url = uri.PathAndQuery;
			}
			// The URL should be stored as short as possible (e.g.: /page.aspx -> page | /page/ -> page)
			if (url.StartsWith("/"))
				url = url.Substring(1);
			if (url.EndsWith("/"))
				url = url.Substring(0, url.Length - "/".Length);
			if (url.EndsWith(".aspx"))
				url = url.Substring(0, url.Length - ".aspx".Length);
			return url;
		}
	}
}