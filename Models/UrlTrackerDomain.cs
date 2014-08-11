using System;
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
                if (UrlTrackerSettings.HasDomainOnChildNode && Node.Parent != null)
                {
                    return string.Format("{0}{1}{2}", HttpContext.Current != null ? HttpContext.Current.Request.Url.Scheme : Uri.UriSchemeHttp, Uri.SchemeDelimiter, HttpContext.Current.Request.Url.Host + "/" + Node.Parent.UrlName + "/" + Node.UrlName);
                }
                else
                {
                    if (Name.Contains(Uri.UriSchemeHttp))
                    {
                        return Name;
                    }
                    else
                    {
                        return string.Format("{0}{1}{2}", HttpContext.Current != null ? HttpContext.Current.Request.Url.Scheme : Uri.UriSchemeHttp, Uri.SchemeDelimiter, Name);
                    }
                }
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