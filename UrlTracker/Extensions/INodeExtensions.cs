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

			while (parent != null)
			{
				Domain[] domains = Domain.GetDomainsById(parent.Id);
				if (domains != null && domains.Any())
					return (Node)parent;
				node = parent;
				parent = node.Parent;
			}

			return (Node)node;
		}
	}
}