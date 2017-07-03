namespace InfoCaster.Umbraco.UrlTracker.Models
{
    using System;
    using System.Web;

    using global::Umbraco.Web;

    using umbraco.NodeFactory;

    public class UrlTrackerDomain
    {
        private Node node;

        public UrlTrackerDomain()
        {
        }

        public UrlTrackerDomain(int id, int nodeId, string name)
        {
            Id = id;
            NodeId = nodeId;
            Name = name;
            Node = new Node(NodeId);
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public Node Node
        {
            get => node ?? (node = new Node(NodeId));
            private set => node = value;
        }

        public int NodeId { get; set; }

        public string UrlWithDomain
        {
            get
            {
                var httpContext = HttpContext.Current;
                
                if (UrlTrackerSettings.HasDomainOnChildNode && Node.Parent != null)
                {
                    using (Helpers.ContextHelper.EnsureHttpContext())
                    {
                        // not sure if this will ever occur because the ensurehttpcontext is now added...
                        if (UmbracoContext.Current != null)
                        {
                            var url = !Node.Url.StartsWith("/")
                                          ? Node.Url
                                          : string.Format(
                                                "{0}{1}{2}",
                                                Uri.UriSchemeHttp,
                                                Uri.SchemeDelimiter,
                                                httpContext.Request.Url.Host) + Node.Url;
                            return url.TrimEnd('/'); // do not re-instantiate
                        }

                        return string.Format(
                            "{0}{1}{2}",
                            httpContext != null ? httpContext.Request.Url.Scheme : Uri.UriSchemeHttp,
                            Uri.SchemeDelimiter,
                            httpContext.Request.Url.Host + "/" + Node.Parent.UrlName + "/" + Node.UrlName);
                    }
                }

                if (Name.Contains(Uri.UriSchemeHttp))
                {
                    return Name;
                }

                return string.Format(
                    "{0}{1}{2}",
                    httpContext != null ? httpContext.Request.Url.Scheme : Uri.UriSchemeHttp,
                    Uri.SchemeDelimiter,
                    Name);
            }
        }

        public override string ToString()
        {
            return UrlWithDomain;
        }
    }
}
