using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using umbraco;

namespace InfoCaster.Umbraco.UrlTracker
{
    public static class UrlTrackerResources
    {
        public static string UrlTrackerManagerUrl
        {
            get
            {
                string urlTrackerManagerUrl = string.Format("~/umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManager.aspx?culture={0}&uiculture={1}", Thread.CurrentThread.CurrentCulture.ToString(), Thread.CurrentThread.CurrentUICulture.ToString());
                return VirtualPathUtility.ToAbsolute(urlTrackerManagerUrl);
            }
        }

        public static string UrlTrackerInfoUrl
        {
            get
            {
                string urlTrackerInfoUrl = string.Format("~/umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerInfo.aspx?culture={0}&uiculture={1}", Thread.CurrentThread.CurrentCulture.ToString(), Thread.CurrentThread.CurrentUICulture.ToString());
                return VirtualPathUtility.ToAbsolute(urlTrackerInfoUrl);
            }
        }

        public const string RootNode = "Root node";
        public const string RootNodeInfo = "The root node defines for which domain this redirect is meant";
        public const string OldUrl = "Old URL";
        public static readonly string OldUrlWatermark = string.Concat("/some/page", !GlobalSettings.UseDirectoryUrls ? ".aspx" : UmbracoSettings.AddTrailingSlash ? "/" : string.Empty);
        public const string OldUrlInfo = "The URL path and query string, which you'd like to redirect";
        public const string OldUrlTestInfo = "Click this link to test the redirect (opens in new window)";
        public const string Regex = "<b>or</b> Regex";
        public const string RegexStandalone = "Regex";
        public const string RegexWatermark = "^index\\.php\\?id=([0-9]+)$";
        public const string RegexInfo = "The input for Regex matching is the path AND query string, without leading slash.<br />(click to open RegExr)";
        public const string OldUrlQueryString = "Old URL query string";
        public const string OldUrlQueryStringWatermark = "id=1&param=value";
        public const string OldUrlQueryStringInfo = "The query string to match";
        public const string RedirectNode = "Redirect node";
        public const string RedirectNodeUnpublished = "The redirect node is unpublished";
        public const string RedirectNodeInfo = "The node to redirect to";
        public const string RedirectUrl = "<b>or</b> redirect URL";
        public const string RedirectUrlStandalone = "Redirect URL";
        public static readonly string RedirectUrlWatermark = string.Concat("/some/page", !GlobalSettings.UseDirectoryUrls ? ".aspx" : UmbracoSettings.AddTrailingSlash ? "/" : string.Empty);
        public const string RedirectUrlInfo = "The URL to redirect to";
        public const string RedirectType = "Redirect type";
        public const string RedirectTypeInfo = "Permanent redirects will be cached by browsers and search engines will update old entries with the new URL";
        public const string RedirectType301 = "Permanent (301)";
        public const string RedirectType302 = "Temporary (302)";
        public const string PassthroughQueryString = "Forward query string";
        public const string PassthroughQueryStringInfo = "When enabled, the query string of the original request is forwarded to the redirect location (pass through)";
        public const string PassthroughQueryStringLabel = "Yes";
        public const string ForceRedirect = "Force redirect";
        public const string ForceRedirectInfo = "When enabled, the UrlTracker will ALLWAYS redirect, even if a valid page exists at the specified Old URL";
        public const string ForceRedirectLabel = "Yes";
        public const string Notes = "Notes";
        public const string NotesWatermark = "Notes";
        public const string SyncTree = "Click to sync the tree on the left to this node";
        public const string ErrorMessageOldUrlAndOldRegexEmpty = "There are entries without one of the two mandatory properties (OldUrl & OldRegex) set. Please delete the row(s) with the following Id('s): {0}";
        public const string ErrorMessageOldUrlAndOldRegexEmptyButton = "Or click here to delete these rows automatically and reload the page";
    }
}