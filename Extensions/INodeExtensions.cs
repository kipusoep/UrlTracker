using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace InfoCaster.Umbraco.UrlTracker.Extensions
{
    public static class INodeExtensions
    {
        static IDomainService _domainService { get { return ApplicationContext.Current.Services.DomainService; } }

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
                List<IDomain> domains = _domainService.GetAssignedDomains(parent.Id, true).ToList(); // Not sure about the includeWildcards param
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
                    List<IDomain> domains = _domainService.GetAssignedDomains(deepParent.Parent.Id, true).ToList(); // Not sure about the includeWildcards param
                    if (domains != null && domains.Any()){
                        deepNode = (Node)deepParent;
                        break;
                    }
                    deepNode = deepParent;
                    deepParent = deepNode.Parent;
                }
            }

            // I don't know what the purpose is, but it contains a bug (https://github.com/kipusoep/UrlTracker/issues/69) - Stefan Kip
            //if (((Node)deepNode).Parent.Id == node.Id)
            //{
            //    // if the found node is a child of the normal root, then use this one...
            //    node = deepNode;
            //}

            return (Node)node;
        }
    }
}