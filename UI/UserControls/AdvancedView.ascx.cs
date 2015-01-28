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
    public partial class AdvancedView : System.Web.UI.UserControl, IUrlTrackerView
    {
        public UrlTrackerModel UrlTrackerModel { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            tbOldUrl.Attributes["placeholder"] = UrlTrackerResources.OldUrlWatermark;
            tbOldRegex.Attributes["placeholder"] = UrlTrackerResources.RegexWatermark;
            tbOldUrlQueryString.Attributes["placeholder"] = UrlTrackerResources.OldUrlQueryStringWatermark;
            tbRedirectUrl.Attributes["placeholder"] = UrlTrackerResources.RedirectUrlWatermark;
            rbPermanent.Text = UrlTrackerResources.RedirectType301;
            rbTemporary.Text = UrlTrackerResources.RedirectType302;
            cbRedirectPassthroughQueryString.Text = UrlTrackerResources.PassthroughQueryStringLabel;
            cbForceRedirect.Text = UrlTrackerResources.ForceRedirectLabel;
            tbNotes.Attributes["placeholder"] = UrlTrackerResources.NotesWatermark;
        }

        public void LoadView()
        {
            List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();
            if (ddlRootNode.Items.Count == 1 && domains.Count > 1)
            {
                ddlRootNode.DataSource = domains.Select(x => new ListItem(UrlTrackerHelper.GetName(x), x.NodeId.ToString()));
                ddlRootNode.DataBind();
            }
            else if (domains.Count <= 1)
            {
                pnlRootNode.Visible = false;
            }

            ddlRootNode.SelectedValue = UrlTrackerModel.RedirectRootNodeId.ToString();
            if (!string.IsNullOrEmpty(UrlTrackerModel.OldRegex) && string.IsNullOrEmpty(UrlTrackerModel.OldUrl))
            {
                tbOldRegex.Text = UrlTrackerModel.OldRegex;
            }
            else
            {
                tbOldUrl.Text = UrlTrackerModel.OldUrl;
                tbOldUrlQueryString.Text = UrlTrackerModel.OldUrlQueryString;
            }
            if (UrlTrackerModel.RedirectNodeId.HasValue)
                cpRedirectNode.Value = UrlTrackerModel.RedirectNodeId.Value.ToString();
            tbRedirectUrl.Text = UrlTrackerModel.RedirectUrl;
            if (UrlTrackerModel.RedirectHttpCode == 301)
                rbPermanent.Checked = true;
            else if (UrlTrackerModel.RedirectHttpCode == 302)
                rbTemporary.Checked = true;
            cbRedirectPassthroughQueryString.Checked = UrlTrackerModel.RedirectPassThroughQueryString;
            cbForceRedirect.Checked = UrlTrackerModel.ForceRedirect;
            tbNotes.Text = UrlTrackerModel.Notes;
            pnlReferrer.Visible = !string.IsNullOrEmpty(UrlTrackerModel.Referrer);
            lblReferrer.Text = UrlTrackerModel.Referrer;
            lblInserted.Text = UrlTrackerModel.Inserted.ToString();
        }

        public void Save()
        {
            List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();

            UrlTrackerModel.OldUrl = UrlTrackerHelper.ResolveShortestUrl(tbOldUrl.Text);
            UrlTrackerModel.OldUrlQueryString = tbOldUrlQueryString.Text;
            UrlTrackerModel.OldRegex = tbOldRegex.Text;
            UrlTrackerModel.RedirectRootNodeId = domains.Count > 1 ? int.Parse(ddlRootNode.SelectedValue) : domains.Any() ? domains.Single().NodeId : new Node(-1).ChildrenAsList.First().Id;
            if (!string.IsNullOrEmpty(cpRedirectNode.Value))
                UrlTrackerModel.RedirectNodeId = int.Parse(cpRedirectNode.Value);
            else
                UrlTrackerModel.RedirectNodeId = null;
            UrlTrackerModel.RedirectUrl = tbRedirectUrl.Text;
            UrlTrackerModel.RedirectHttpCode = rbPermanent.Checked ? 301 : 302;
            UrlTrackerModel.RedirectPassThroughQueryString = cbRedirectPassthroughQueryString.Checked;
            UrlTrackerModel.ForceRedirect = cbForceRedirect.Checked;
            UrlTrackerModel.Notes = tbNotes.Text;
            UrlTrackerRepository.UpdateUrlTrackerEntry(UrlTrackerModel);
        }
    }
}