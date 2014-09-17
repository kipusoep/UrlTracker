﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using umbraco.NodeFactory;

namespace InfoCaster.Umbraco.UrlTracker.UI.UserControls
{
    public partial class CreateView : UserControl
    {
        public string OldUrlClientUniqueId { get { return tbOldUrl.UniqueID; } }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();
            if (ddlRootNode.Items.Count == 1 && domains.Count > 1)
            {
                ddlRootNode.DataSource = domains.Select(x => new ListItem(UrlTrackerHelper.GetName(x), x.NodeId.ToString()));
                ddlRootNode.DataBind();
            }
            else if (domains.Count <= 1)
                pnlRootNode.Visible = false;
        }

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

        public void CreateNew()
        {
            List<UrlTrackerDomain> domains = UmbracoHelper.GetDomains();

            UrlTrackerRepository.AddUrlTrackerEntry(new UrlTrackerModel(UrlTrackerHelper.ResolveShortestUrl(tbOldUrl.Text), tbOldUrlQueryString.Text, tbOldRegex.Text, domains.Count > 1 ? int.Parse(ddlRootNode.SelectedValue) : domains.Any() ? domains.Single().NodeId : new Node(-1).ChildrenAsList.First().Id, !string.IsNullOrEmpty(cpRedirectNode.Value) ? (int?)int.Parse(cpRedirectNode.Value) : null, tbRedirectUrl.Text, rbPermanent.Checked ? 301 : 302, cbRedirectPassthroughQueryString.Checked, cbForceRedirect.Checked, tbNotes.Text));

            if (ddlRootNode.SelectedIndex != -1)
                ddlRootNode.SelectedIndex = 0;
            tbOldUrl.Text = tbOldUrlQueryString.Text = tbOldRegex.Text = cpRedirectNode.Value = tbRedirectUrl.Text = tbNotes.Text = string.Empty;
            rbPermanent.Checked = cbRedirectPassthroughQueryString.Checked = true;
            rbTemporary.Checked = false;
        }
    }
}