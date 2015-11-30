using InfoCaster.Umbraco.UrlTracker.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web.Routing;

namespace InfoCaster.Umbraco.UrlTracker.ContentFinders
{
    public class UrlTrackerContentFinder : IContentFinder
    {
        public bool TryFindContent(PublishedContentRequest contentRequest)
        {
            return UrlTrackerModule.UrlTrackerDo("UrlTrackerContentFinder");
        }
    }
}