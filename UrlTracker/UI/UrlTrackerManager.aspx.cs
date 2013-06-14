using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using InfoCaster.Umbraco.UrlTracker.UI.UserControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using umbraco.controls;

namespace InfoCaster.Umbraco.UrlTracker.UI
{
	public partial class UrlTrackerManager : umbraco.BasePages.UmbracoEnsuredPage
	{
		protected AutoView icAutoView;
		protected CustomView icCustomView;
		protected NotFoundView icNotFoundView;
		protected AdvancedView icAdvancedView;
		protected CreateView icCreateView;

		ContentPicker _contentPicker = new ContentPicker();

		protected UrlTrackerModel UrlTrackerModel
		{
			get { return ViewState["DE916296-47D7-4878-BBFC-83FE0F146FDB"] as UrlTrackerModel ?? null; }
			set { ViewState["DE916296-47D7-4878-BBFC-83FE0F146FDB"] = value; }
		}

		protected override void OnInit(EventArgs e)
		{
			string culture = Request.QueryString["culture"];
			if (!string.IsNullOrEmpty(culture))
				Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
			string uiCulture = Request.QueryString["uiculture"];
			if (!string.IsNullOrEmpty(uiCulture))
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(uiCulture);

			base.OnInit(e);

			if (!IsPostBack)
			{
				gvUrlTracker.Sort("Inserted", SortDirection.Descending);
				gvNotFound.Sort("NotFoundCount", SortDirection.Descending);
			}

			pnlBreadcrumb.Visible = false;

			if (icAutoView == null)
			{
				icAutoView = (AutoView)LoadControl("/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.AutoView.ascx");
				icCustomView = (CustomView)LoadControl("/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.CustomView.ascx");
				icNotFoundView = (NotFoundView)LoadControl("/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.NotFoundView.ascx");
				icAdvancedView = (AdvancedView)LoadControl("/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.AdvancedView.ascx");
				icCreateView = (CreateView)LoadControl("/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.CreateView.ascx");

				pnlEditValidationGroup.Controls.AddAt(0, icAutoView);
				pnlEditValidationGroup.Controls.AddAt(1, icCustomView);
				pnlEditValidationGroup.Controls.AddAt(2, icNotFoundView);
				pnlEditValidationGroup.Controls.AddAt(3, icAdvancedView);
				pnlCreateValidationGroup.Controls.AddAt(0, icCreateView);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			IUrlTrackerView activeView = GetActiveView();
			if (activeView != null)
				activeView.UrlTrackerModel = icAdvancedView.UrlTrackerModel = UrlTrackerModel;
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			liDetails.Visible = mvUrlTracker.GetActiveView() == vwUrlTrackerDetail;
			liNew.Visible = mvUrlTracker.GetActiveView() == vwUrlTrackerNew;

			bool hasNotFoundEntries = UrlTrackerRepository.HasNotFoundEntries();
			ltlNotFoundText.Visible = hasNotFoundEntries;
			if (mvSwitchButtons.GetActiveView() == vwSwitchButtonsNotFound)
			{
				mvSwitchButtons.Visible = hasNotFoundEntries;
				lbDeleteSelected.Visible = gvUrlTracker.Rows.Count > 0;
			}
			else
				lbDeleteSelected.Visible = gvNotFound.Rows.Count > 0;

			mvUrlTrackerEntries.SetActiveView(vwUrlTrackerEntriesTable);
			if (gvUrlTracker.Rows.Count == 0)
				mvUrlTrackerEntries.SetActiveView(vwUrlTrackerEntriesNone);
		}

		protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				GridView gridView = (GridView)sender;
				UrlTrackerModel urlTrackerModel = (UrlTrackerModel)e.Row.DataItem;

				List<TableCell> cells = e.Row.Cells.Cast<TableCell>().ToList();
				for (int i = 1; i < cells.Count - 2; i++)
				{
					cells[i].Attributes["onclick"] = Page.ClientScript.GetPostBackEventReference(gridView, "Select$" + e.Row.RowIndex.ToString());
					cells[i].CssClass += " clickable";
				}
				ImageButton lbSelect = (ImageButton)cells.Last().FindControl("lbSelect");
				lbSelect.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.edit.png");

				ImageButton lbDelete = (ImageButton)cells.Last().FindControl("lbDelete");
				lbDelete.ImageUrl = Page.ClientScript.GetWebResourceUrl(typeof(UrlTrackerResources), "InfoCaster.Umbraco.UrlTracker.UI.res.img.trash.png");

				HiddenField hfId = e.Row.FindControl("hfId") as HiddenField;
				if (hfId != null)
					hfId.Value = urlTrackerModel.Id.ToString();

				HiddenField hfOldUrl = e.Row.FindControl("hfOldUrl") as HiddenField;
				if (hfOldUrl != null)
					hfOldUrl.Value = urlTrackerModel.OldUrl;
			}
		}

		protected void GridView_SelectedIndexChanged(object sender, EventArgs e)
		{
			GridView gridView = (GridView)sender;
			LoadDetail(gridView.SelectedDataKey.Value);
		}

		protected void gvNotFound_RowDeleted(object sender, GridViewDeletedEventArgs e)
		{
			if (!UrlTrackerRepository.HasNotFoundEntries())
			{
				lbUrlTrackerView_Click(this, EventArgs.Empty);
				mvSwitchButtons.Visible = false;
			}

			gvUrlTracker.DataBind();
			if (gvUrlTracker.Rows.Count == 0)
			{
				lbDeleteSelected.Visible = false;
			}
		}

		protected void ShowOverview(object sender, EventArgs e)
		{
			mvUrlTracker.SetActiveView(vwUrlTrackerOverview);
			gvUrlTracker.DataBind();
		}

		protected void lbNotFoundView_Click(object sender, EventArgs e)
		{
			mvSwitchButtons.SetActiveView(vwSwitchButtonsUrlTracker);
			mvGridViews.SetActiveView(vwGridViewsNotFound);
			gvNotFound.DataBind();
			lbCreate.Visible = false;
		}

		protected void lbUrlTrackerView_Click(object sender, EventArgs e)
		{
			mvSwitchButtons.SetActiveView(vwSwitchButtonsNotFound);
			mvGridViews.SetActiveView(vwGridViewsUrlTracker);
			gvUrlTracker.DataBind();
			lbCreate.Visible = true;
		}

		protected void lbSave_Click(object sender, EventArgs e)
		{
			if (icAdvancedView.Visible)
				icAdvancedView.Save();
			else
				GetActiveView().Save();
			pnlBreadcrumb.Visible = false;
			ResetViews();
			mvUrlTracker.SetActiveView(vwUrlTrackerOverview);
			gvUrlTracker.DataBind();
			gvNotFound.DataBind();
			if (mvGridViews.GetActiveView() == vwGridViewsNotFound && !UrlTrackerRepository.HasNotFoundEntries())
			{
				lbUrlTrackerView_Click(this, EventArgs.Empty);
				mvSwitchButtons.Visible = false;
			}
		}

		protected void lbSwitchToAdvancedView_Click(object sender, EventArgs e)
		{
			IUrlTrackerView activeView = GetActiveView();
			((UserControl)activeView).Visible = false;
			icAdvancedView.Visible = true;
			icAdvancedView.UrlTrackerModel = UrlTrackerModel;
			icAdvancedView.LoadView();
			mvViewSwitcher.SetActiveView(vwViewSwitcherBack);
			pnlBreadcrumb.Visible = true;
		}

		protected void lbSwitchToNormalView_Click(object sender, EventArgs e)
		{
			IUrlTrackerView activeView = GetActiveView();
			((UserControl)activeView).Visible = true;
			icAdvancedView.Visible = false;
			activeView.UrlTrackerModel = UrlTrackerModel;
			activeView.LoadView();
			mvViewSwitcher.SetActiveView(vwViewSwitcherAdvanced);
			pnlBreadcrumb.Visible = true;
			mvViewSwitcher.Visible = true;
			if (activeView == icNotFoundView)
				mvViewSwitcher.Visible = false;
		}

		protected void lbDeleteSelected_Click(object sender, EventArgs e)
		{
			GridView activeGridView = mvGridViews.GetActiveView() == vwGridViewsNotFound ? gvNotFound : gvUrlTracker;
			foreach (GridViewRow row in activeGridView.Rows.OfType<GridViewRow>().Where(x => x.RowType == DataControlRowType.DataRow))
			{
				CheckBox cbSelect = (CheckBox)row.FindControl("cbSelect");
				if (cbSelect.Checked)
				{
					HiddenField hfId = row.FindControl("hfId") as HiddenField;
					if (hfId != null)
						UrlTrackerRepository.DeleteUrlTrackerEntry(int.Parse(hfId.Value));
					else
					{
						HiddenField hfOldUrl = row.FindControl("hfOldUrl") as HiddenField;
						if (hfOldUrl != null)
							UrlTrackerRepository.DeleteNotFoundEntriesByOldUrl(hfOldUrl.Value);
					}
				}
			}
			activeGridView.DataBind();
			if (activeGridView == gvNotFound && !UrlTrackerRepository.HasNotFoundEntries())
			{
				lbUrlTrackerView_Click(this, EventArgs.Empty);
				mvSwitchButtons.Visible = false;
			}
		}

		protected void lbCreate_Click(object sender, EventArgs e)
		{
			ResetViews();
			pnlBreadcrumb.Visible = true;
			mvUrlTracker.SetActiveView(vwUrlTrackerNew);
		}

		protected void lbCreateNew_Click(object sender, EventArgs e)
		{
			icCreateView.CreateNew();
			ResetViews();
			mvUrlTracker.SetActiveView(vwUrlTrackerOverview);
			gvUrlTracker.DataBind();
		}

		void LoadDetail(object dataKey)
		{
			ResetViews();
			pnlBreadcrumb.Visible = true;
			mvUrlTracker.SetActiveView(vwUrlTrackerDetail);

			if (dataKey is int)
				UrlTrackerModel = UrlTrackerRepository.GetUrlTrackerEntryById((int)dataKey);
			else if (dataKey is string)
				UrlTrackerModel = UrlTrackerRepository.GetNotFoundEntryByUrl((string)dataKey);
			lbSwitchToNormalView_Click(this, EventArgs.Empty);
		}

		void ResetViews()
		{
			pnlEditValidationGroup.Controls.OfType<IUrlTrackerView>().ToList().ForEach(x => { ((UserControl)x).Visible = false; });
		}

		IUrlTrackerView GetActiveView()
		{
			UserControl view;
			if (UrlTrackerModel == null)
				return null;
			switch (UrlTrackerModel.ViewType)
			{
				case UrlTrackerViewTypes.Auto:
					view = icAutoView;
					break;
				case UrlTrackerViewTypes.NotFound:
					view = icNotFoundView;
					break;
				case UrlTrackerViewTypes.Custom:
					view = icCustomView;
					break;
				default:
					view = icAdvancedView;
					break;
			}
			return (IUrlTrackerView)view;
		}
	}
}