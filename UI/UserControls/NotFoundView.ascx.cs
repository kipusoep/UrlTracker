using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.NodeFactory;

namespace InfoCaster.Umbraco.UrlTracker.UI.UserControls
{
    public partial class NotFoundView : System.Web.UI.UserControl, IUrlTrackerView
    {
        public UrlTrackerModel UrlTrackerModel { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            lnkOldUrl.ToolTip = UrlTrackerResources.OldUrlTestInfo;
            tbRedirectUrl.Attributes["placeholder"] = UrlTrackerResources.RedirectUrlWatermark;
            rbPermanent.Text = UrlTrackerResources.RedirectType301;
            rbTemporary.Text = UrlTrackerResources.RedirectType302;
            cbRedirectPassthroughQueryString.Text = UrlTrackerResources.PassthroughQueryStringLabel;
        }

        public void LoadView()
        {
            UrlTrackerDomain domain = null;
            Node redirectRootNode = new Node(UrlTrackerModel.RedirectRootNodeId);

            List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();
            domain = domains.FirstOrDefault(x => x.NodeId == redirectRootNode.Id);
            if (domain == null)
                domain = new UrlTrackerDomain(-1, redirectRootNode.Id, HttpContext.Current.Request.Url.Host);
            if (!domains.Any())
                pnlRootNode.Visible = false;
            else
            {
                lnkRootNode.Text = UrlTrackerHelper.GetName(domain);
                lnkRootNode.ToolTip = UrlTrackerResources.SyncTree;
                lnkRootNode.NavigateUrl = string.Format("javascript:parent.UmbClientMgr.mainTree().syncTree('{1}', false);", redirectRootNode.Id, redirectRootNode.Path);
            }
            
            Uri oldUri = new Uri(UrlTrackerModel.CalculatedOldUrlWithDomain);
            lnkOldUrl.Text = string.Format("{0} <i class=\"icon-share\"></i>", oldUri.AbsolutePath.StartsWith("/") ? oldUri.AbsolutePath.Substring(1) : oldUri.AbsolutePath);
            lnkOldUrl.NavigateUrl = oldUri.ToString();
        }

        public void Save()
        {
            UrlTrackerModel.Is404 = false;
            UrlTrackerModel.Referrer = string.Empty;
            if (!string.IsNullOrEmpty(cpRedirectNode.Value))
                UrlTrackerModel.RedirectNodeId = int.Parse(cpRedirectNode.Value);
            else
                UrlTrackerModel.RedirectNodeId = null;
            UrlTrackerModel.RedirectUrl = tbRedirectUrl.Text;
            UrlTrackerModel.RedirectHttpCode = rbPermanent.Checked ? 301 : 302;
            UrlTrackerModel.RedirectPassThroughQueryString = cbRedirectPassthroughQueryString.Checked;
            UrlTrackerModel.Notes = tbNotes.Text;
            UrlTrackerRepository.UpdateUrlTrackerEntry(UrlTrackerModel);
            UrlTrackerRepository.DeleteNotFoundEntriesByRootAndOldUrl(UrlTrackerModel.RedirectRootNodeId, UrlTrackerModel.OldUrl);
        }
    }
}