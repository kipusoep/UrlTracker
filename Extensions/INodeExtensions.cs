using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.NodeFactory;

namespace InfoCaster.Umbraco.UrlTracker.Extensions
{
	public static class INodeExtensions
	{
		/// <summary>
		/// Retrieve the root node
		/// </summary>
		/// <param name="node">The current node</param>
		/// <returns>The root node</returns>
		public static Node GetRootNode(this INode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			INode parent = node;

			while (parent != null)
			{
				node = parent;
				parent = node.Parent;
			}

			return (Node)node;
		}

		/// <summary>
		/// Retreive the closest ancestor or self with a host name set, or the root node if no hostnames are set
		/// </summary>
		/// <param name="node">The current node</param>
		/// <returns>The closest ancestor or self with a host name set, or the root node if no hostnames are set</returns>
		public static Node GetDomainRootNode(this INode node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			INode parent = node;
            INode deepParent = node;
            INode deepNode = node;
            while (parent != null)
            {
                Domain[] domains = Domain.GetDomainsById(parent.Id);
                if (domains != null && domains.Any()){
                    node= (Node)parent;
                    break;
                }
                node = parent;
                parent = node.Parent;
            }
            if(UrlTrackerSettings.HasDomainOnChildNode){
                while (deepParent != null && deepParent.Parent != null)
                {
                    Domain[] domains = Domain.GetDomainsById(deepParent.Parent.Id);
                    if (domains != null && domains.Any()){
                        deepNode = (Node)deepParent;
                        break;
                    }
                    deepNode = deepParent;
                    deepParent = deepNode.Parent;
                }
            }

            if (((Node)deepNode).Parent.Id == node.Id)
            {
                // if the found node is a child of the normal root, then use this one...
                node = deepNode;
            }

			return (Node)node;
		}
	}
}