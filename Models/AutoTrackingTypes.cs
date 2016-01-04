using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
    public enum AutoTrackingTypes
    {
        Renamed,
        Moved,
        UrlOverwritten,
        UrlOverwrittenSEOMetadata
    }
}