using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.NodeFactory;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
    public class UrlTrackerDomain
    {
        public int Id { get; set; }
        public int NodeId { get; set; }
        public string Name { get; set; }

        public Node Node { get { return new Node(NodeId); } }
        public string UrlWithDomain
        {
            get
            {
                return string.Format("{0}://{1}", HttpContext.Current != null ? HttpContext.Current.Request.Url.Scheme : "http", Name);
            }
        }

        public UrlTrackerDomain() { }

        public UrlTrackerDomain(int id, int nodeId, string name)
        {
            Id = id;
            NodeId = nodeId;
            Name = name;
        }

        public override string ToString()
        {
            return UrlWithDomain;
        }
    }
}