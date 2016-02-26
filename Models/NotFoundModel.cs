using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
    [TableName("icUrlTracker")]
    public class NotFoundModel
    {
        public string OldUrl { get; set; }
        public int RedirectRootNodeId { get; set; }
        public string OldUrlQueryString { get; set; }
        public bool Is404 { get { return true; } }
        public string Referrer { get; set; }
    }
}