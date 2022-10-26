﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.NodeFactory;
using Umbraco.Web;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
    public class UrlTrackerDomain
    {
        public int Id { get; set; }
        public int NodeId { get; set; }
        public string Name { get; set; }

		private Node _node = null;
		public Node Node
		{
			get
			{
				if (_node == null)
				{
					_node = new Node(NodeId);
				}

				return _node;
			}
		}
        public string UrlWithDomain
        {
            get
            {
                var node = Node;
                if (UrlTrackerSettings.HasDomainOnChildNode && node.Parent != null)
                {
                    using (InfoCaster.Umbraco.UrlTracker.Helpers.ContextHelper.EnsureHttpContext())
                    {
                        // not sure if this will ever occur because the ensurehttpcontext is now added...
                        if (UmbracoContext.Current != null)
                        {
                            /*var url = new Node(node.Id).Url;
                            return url;*/
                            var url = !Node.Url.StartsWith("/")
                                ? Node.Url
                                : string.Format("{0}{1}{2}", Uri.UriSchemeHttp, Uri.SchemeDelimiter, HttpContext.Current.Request.Url.Host) + Node.Url;
                            return url.TrimEnd('/'); // do not re-instantiate
                        }
                        else
                        {
                            return string.Format("{0}{1}{2}", HttpContext.Current != null ? HttpContext.Current.Request.Url.Scheme : Uri.UriSchemeHttp, Uri.SchemeDelimiter, HttpContext.Current.Request.Url.Host + "/" + Node.Parent.UrlName + "/" + Node.UrlName);
                        }
                    }
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