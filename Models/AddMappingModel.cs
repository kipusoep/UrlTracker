using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Persistence;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
    [TableName("icUrlTracker")]
    public class AddMappingModel
    {
        // Todo: What happens if empty?
        public int RedirectRootNodeId { get; set; }
        public int RedirectNodeId { get; set; }
        // Todo: What happens if empty?
        public int RedirectHttpCode { get; set; }
        public string OldUrl { get; set; }
        public string Notes { get; set; }
    }
}