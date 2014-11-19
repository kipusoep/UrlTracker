using InfoCaster.Umbraco.UrlTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using umbraco;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
	public static class UrlTrackerHelper
	{
		static readonly Regex _urlWithDotRegex = new Regex("\\S+\\.\\S+");

		public static string ResolveShortestUrl(string url)
		{
			if (url.StartsWith("http://") || url.StartsWith("https://"))
			{
				Uri uri = new Uri(url);
				url = Uri.UnescapeDataString(uri.PathAndQuery);
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

		public static string ResolveUmbracoUrl(string url)
		{
			if (url.StartsWith("http://") || url.StartsWith("https://"))
			{
				Uri uri = new Uri(url);
				url = Uri.UnescapeDataString(uri.PathAndQuery);
			}

			if (url != "/" && !_urlWithDotRegex.IsMatch(url))
			{
				if (!GlobalSettings.UseDirectoryUrls && !url.EndsWith(".aspx"))
					url += ".aspx";
				else if (UmbracoSettings.AddTrailingSlash && !url.EndsWith("/"))
					url += "/";
			}

			return url;
		}

        public static string GetName(UrlTrackerDomain domain)
        {
            if (UrlTrackerSettings.HasDomainOnChildNode)
            {
                var result = string.Format("{0}", domain.Node.Parent == null ? domain.Node.Name : domain.Node.Parent.Name + "/" + domain.Node.Name);
                if (string.IsNullOrEmpty(result))
                {
                    result = "(root)";
                }
                return result;
            }
            else
            {
                return string.Format("{0} ({1})", domain.Node.Name, domain.Name);
            }
        }
	}
}