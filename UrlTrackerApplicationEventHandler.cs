using InfoCaster.Umbraco.UrlTracker.Extensions;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web.UI.Pages;

namespace InfoCaster.Umbraco.UrlTracker
{
    public class UrlTrackerApplicationEventHandler : ApplicationEventHandler
    {
        protected ClientTools ClientTools
        {
            get
            {
                Page page = HttpContext.Current.CurrentHandler as Page;
                if (page != null)
                    return new ClientTools(page);
                return null;
            }
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            if (!UrlTrackerSettings.IsDisabled && !UrlTrackerSettings.IsTrackingDisabled)
            {
                UrlTrackerRepository.ReloadForcedRedirectsCache();

                ContentService.Moving += ContentService_Moving;
                ContentService.Publishing += ContentService_Publishing;
                ContentService.Deleting += ContentService_Deleting;
                content.BeforeClearDocumentCache += content_BeforeClearDocumentCache;
                DomainService.Deleted += DomainService_Deleted;
                DomainService.Saved += DomainService_Saved;
            }
        }

        void ContentService_Deleting(IContentService sender, DeleteEventArgs<IContent> e)
        {
            foreach (IContent content in e.DeletedEntities)
            {
#if !DEBUG
                try
#endif
                {
                    UrlTrackerRepository.DeleteUrlTrackerEntriesByNodeId(content.Id);
                }
#if !DEBUG
                catch (Exception ex)
                {
                    ex.LogException();
                }
#endif
            }
        }

        void ContentService_Publishing(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            // When content is renamed or 'umbracoUrlName' property value is added/updated
            foreach (IContent content in e.PublishedEntities)
            {
#if !DEBUG
                try
#endif
                {
                    Node node = new Node(content.Id);
                    if (node.Name != content.Name && !string.IsNullOrEmpty(node.Name)) // If name is null, it's a new document
                    {
                        // Rename occurred
                        UrlTrackerRepository.AddUrlMapping(content, node.GetDomainRootNode().Id, node.NiceUrl, AutoTrackingTypes.Renamed);

                        if (ClientTools != null)
                            ClientTools.ChangeContentFrameUrl(string.Concat("/umbraco/editContent.aspx?id=", content.Id));
                    }
                    if (content.HasProperty("umbracoUrlName"))
                    {
                        string contentUmbracoUrlNameValue = content.GetValue("umbracoUrlName") != null ? content.GetValue("umbracoUrlName").ToString() : string.Empty;
                        string nodeUmbracoUrlNameValue = node.GetProperty("umbracoUrlName") != null ? node.GetProperty("umbracoUrlName").Value : string.Empty;
                        if (contentUmbracoUrlNameValue != nodeUmbracoUrlNameValue)
                        {
                            // 'umbracoUrlName' property value added/changed
                            UrlTrackerRepository.AddUrlMapping(content, node.GetDomainRootNode().Id, node.NiceUrl, AutoTrackingTypes.UrlOverwritten);

                            if (ClientTools != null)
                                ClientTools.ChangeContentFrameUrl(string.Concat("/umbraco/editContent.aspx?id=", content.Id));
                        }
                    }
                    if (UrlTrackerSettings.SEOMetadataInstalled && content.HasProperty(UrlTrackerSettings.SEOMetadataPropertyName))
                    {
                        string contentSEOMetadataValue = content.GetValue(UrlTrackerSettings.SEOMetadataPropertyName) != null ? content.GetValue(UrlTrackerSettings.SEOMetadataPropertyName).ToString() : string.Empty;
                        string nodeSEOMetadataValue = node.GetProperty(UrlTrackerSettings.SEOMetadataPropertyName) != null ? node.GetProperty(UrlTrackerSettings.SEOMetadataPropertyName).Value : string.Empty;
                        if (contentSEOMetadataValue != nodeSEOMetadataValue)
                        {
                            dynamic contentJson = JObject.Parse(contentSEOMetadataValue);
                            string contentUrlName = contentJson.urlName;

                            dynamic nodeJson = JObject.Parse(nodeSEOMetadataValue);
                            string nodeUrlName = nodeJson.urlName;

                            if (contentUrlName != nodeUrlName)
                            {
                                // SEOMetadata UrlName property value added/changed
                                UrlTrackerRepository.AddUrlMapping(content, node.GetDomainRootNode().Id, node.NiceUrl, AutoTrackingTypes.UrlOverwrittenSEOMetadata);

                                if (ClientTools != null)
                                    ClientTools.ChangeContentFrameUrl(string.Concat("/umbraco/editContent.aspx?id=", content.Id));
                            }
                        }
                    }
                }
#if !DEBUG
                catch (Exception ex)
                {
                    ex.LogException();
                }
#endif
            }
        }

        void ContentService_Moving(IContentService sender, MoveEventArgs<IContent> e)
        {
            foreach (MoveEventInfo<IContent> moveEventInfo in e.MoveInfoCollection)
            {
                IContent content = moveEventInfo.Entity;
#if !DEBUG
            try
#endif
                {
                    if (content != null)
                    {
                        Node node = new Node(content.Id);

                        if (node != null && !string.IsNullOrEmpty(node.NiceUrl) && !content.Path.StartsWith("-1,-20")) // -1,-20 == Recycle bin | Not moved to recycle bin
                            UrlTrackerRepository.AddUrlMapping(content, node.GetDomainRootNode().Id, node.NiceUrl, AutoTrackingTypes.Moved);
                    }
                }
#if !DEBUG
            catch (Exception ex)
            {
                ex.LogException();
            }
#endif
            }
        }

#pragma warning disable 0618
        void content_BeforeClearDocumentCache(Document doc, DocumentCacheEventArgs e)
#pragma warning restore
        {
#if !DEBUG
            try
#endif
            {
                UrlTrackerRepository.AddGoneEntryByNodeId(doc.Id);
            }
#if !DEBUG
            catch (Exception ex)
            {
                ex.LogException();
            }
#endif
        }

        void DomainService_Saved(IDomainService sender, SaveEventArgs<IDomain> e)
        {
            UmbracoHelper.ClearDomains();
        }

        void DomainService_Deleted(IDomainService sender, DeleteEventArgs<IDomain> e)
        {
            UmbracoHelper.ClearDomains();
        }
    }
}