using InfoCaster.Umbraco.UrlTracker.Extensions;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.NodeFactory;

namespace InfoCaster.Umbraco.UrlTracker
{
    public class UrlTrackerApplicationBase : ApplicationBase
    {
        public UrlTrackerApplicationBase()
        {
            if (!UrlTrackerSettings.IsDisabled && !UrlTrackerSettings.IsTrackingDisabled)
            {
                Document.BeforeMove += Document_BeforeMove;
                Document.BeforePublish += Document_BeforePublish;
                content.BeforeClearDocumentCache += content_BeforeClearDocumentCache;
                Document.BeforeDelete += Document_BeforeDelete;
                Domain.AfterDelete += Domain_AfterDelete;
                Domain.AfterSave += Domain_AfterSave;
                Domain.New += Domain_New;
            }
        }

        void Domain_New(Domain sender, NewEventArgs e)
        {
            UmbracoHelper.ClearDomains();
        }

        void Domain_AfterSave(Domain sender, SaveEventArgs e)
        {
            UmbracoHelper.ClearDomains();
        }

        void Domain_AfterDelete(Domain sender, DeleteEventArgs e)
        {
            UmbracoHelper.ClearDomains();
        }

        void Document_BeforePublish(Document doc, PublishEventArgs e)
        {
            // When document is renamed or 'umbracoUrlName' property value is added/updated
#if !DEBUG
            try
#endif
            {
                Node node = new Node(doc.Id);
                if (node.Name != doc.Text && !string.IsNullOrEmpty(node.Name)) // If name is null, it's a new document
                {
                    // Rename occurred
                    UrlTrackerRepository.AddUrlMapping(doc, node.GetDomainRootNode().Id, node.NiceUrl, AutoTrackingTypes.Renamed);

                    if (BasePage.Current != null)
                        BasePage.Current.ClientTools.ChangeContentFrameUrl(string.Concat("/umbraco/editContent.aspx?id=", doc.Id));
                }
                if (doc.getProperty("umbracoUrlName") != null && node.GetProperty("umbracoUrlName") != null && node.GetProperty("umbracoUrlName").Value != doc.getProperty("umbracoUrlName").Value.ToString())
                {
                    // 'umbracoUrlName' property value added/changed
                    UrlTrackerRepository.AddUrlMapping(doc, node.GetDomainRootNode().Id, node.NiceUrl, AutoTrackingTypes.UrlOverwritten);

                    if (BasePage.Current != null)
                        BasePage.Current.ClientTools.ChangeContentFrameUrl(string.Concat("/umbraco/editContent.aspx?id=", doc.Id));
                }
            }
#if !DEBUG
            catch (Exception ex)
            {
                ex.LogException(doc.Id);
            }
#endif
        }

        void Document_BeforeMove(object sender, MoveEventArgs e)
        {
#if !DEBUG
            try
#endif
            {
                Document doc = sender as Document;
#if !DEBUG
                try
#endif
                {
                    if (doc != null)
                    {
                        Node node = new Node(doc.Id);

                        if (node != null && !string.IsNullOrEmpty(node.NiceUrl) && !doc.Path.StartsWith("-1,-20")) // -1,-20 == Recycle bin | Not moved to recycle bin
                            UrlTrackerRepository.AddUrlMapping(doc, node.GetDomainRootNode().Id, node.NiceUrl, AutoTrackingTypes.Moved);
                    }
                }
#if !DEBUG
                catch (Exception ex)
                {
                    ex.LogException(doc.Id);
                }
#endif
            }
#if !DEBUG
            catch (Exception ex)
            {
                ex.LogException();
            }
#endif
        }

        void content_BeforeClearDocumentCache(Document doc, DocumentCacheEventArgs e)
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
                ex.LogException(doc.Id);
            }
#endif
        }

        void Document_BeforeDelete(Document doc, DeleteEventArgs e)
        {
#if !DEBUG
            try
#endif
            {
                UrlTrackerRepository.DeleteUrlTrackerEntriesByNodeId(doc.Id);
            }
#if !DEBUG
            catch (Exception ex)
            {
                ex.LogException(doc.Id);
            }
#endif
        }
    }
}