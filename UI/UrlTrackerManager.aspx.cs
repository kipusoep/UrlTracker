using InfoCaster.Umbraco.UrlTracker.Models;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using InfoCaster.Umbraco.UrlTracker.UI.UserControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.controls;
using Umbraco.Web.UI.Pages;

namespace InfoCaster.Umbraco.UrlTracker.UI
{
    public partial class UrlTrackerManager : UmbracoEnsuredPage
    {
        protected AutoView icAutoView;
        protected CustomView icCustomView;
        protected NotFoundView icNotFoundView;
        protected AdvancedView icAdvancedView;
        protected CreateView icCreateView;
        protected UrlTrackerModel UrlTrackerModel
        {
            get { return ViewState["DE916296-47D7-4878-BBFC-83FE0F146FDB"] as UrlTrackerModel ?? null; }
            set { ViewState["DE916296-47D7-4878-BBFC-83FE0F146FDB"] = value; }
        }
        protected int PageSize
        {
            get
            {
                HttpCookie pageSizeCookie = Request.Cookies.Get("8D697695-9132-49B8-A489-BFBC3D7C3313");
                if (pageSizeCookie != null)
                {
                    int pageSize;
                    if (int.TryParse(pageSizeCookie.Value, out pageSize))
                        return pageSize;
                }
                return _defaultPageSize;
            }
            set
            {
                HttpCookie pageSizeCookie = Request.Cookies.Get("8D697695-9132-49B8-A489-BFBC3D7C3313");
                if (pageSizeCookie != null)
                    pageSizeCookie.Value = value.ToString();

                Response.Cookies.Add(new HttpCookie("8D697695-9132-49B8-A489-BFBC3D7C3313", value.ToString())
                {
                    Expires = DateTime.Now.AddYears(1)
                });
            }
        }

        ContentPicker _contentPicker = new ContentPicker();
        bool _gridviewFiltered = false;
        bool _isNotFoundView { get { return mvSwitchButtons.GetActiveView() == vwSwitchButtonsUrlTracker; } }
        bool _earlyErrorDetected { get { return mvUrlTrackerError.GetActiveView() == vwUrlTrackerErrorMessage; } }

        const int _defaultPageSize = 20;

        protected void scriptManager_AsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
        {
            if (e.Exception != null && e.Exception.InnerException != null)
            {
                scriptManager.AsyncPostBackErrorMessage = e.Exception.InnerException.Message;
                if (e.Exception.InnerException.InnerException != null)
                    scriptManager.AsyncPostBackErrorMessage = string.Concat(scriptManager.AsyncPostBackErrorMessage, " | (inner exception: ", e.Exception.InnerException.InnerException.Message, ")");
            }
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
                List<int> invalidRowIds;
                if (UrlTrackerRepository.HasInvalidEntries(out invalidRowIds))
                {
                    ltlError.Text = string.Format(UrlTrackerResources.ErrorMessageOldUrlAndOldRegexEmpty, string.Join(", ", invalidRowIds));
                    lbDeleteErrorRows.Text = UrlTrackerResources.ErrorMessageOldUrlAndOldRegexEmptyButton;
                    mvUrlTrackerError.SetActiveView(vwUrlTrackerErrorMessage);
                    odsUrlTrackerEntries.Selecting += odsUrlTrackerEntries_Selecting;
                }

                gvUrlTracker.Sort("Inserted", SortDirection.Descending);
                gvNotFound.Sort("NotFoundCount", SortDirection.Descending);
            }

            if (!_earlyErrorDetected)
            {
                pnlBreadcrumb.Visible = false;

                if (icAutoView == null)
                {
                    icAutoView = (AutoView)LoadControl("~/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.AutoView.ascx");
                    icCustomView = (CustomView)LoadControl("~/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.CustomView.ascx");
                    icNotFoundView = (NotFoundView)LoadControl("~/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.NotFoundView.ascx");
                    icAdvancedView = (AdvancedView)LoadControl("~/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.AdvancedView.ascx");
                    icCreateView = (CreateView)LoadControl("~/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UserControls.CreateView.ascx");

                    pnlEditValidationGroup.Controls.AddAt(0, icAutoView);
                    pnlEditValidationGroup.Controls.AddAt(1, icCustomView);
                    pnlEditValidationGroup.Controls.AddAt(2, icNotFoundView);
                    pnlEditValidationGroup.Controls.AddAt(3, icAdvancedView);
                    pnlCreateValidationGroup.Controls.AddAt(0, icCreateView);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!_earlyErrorDetected)
            {
                IUrlTrackerView activeView = GetActiveView();
                if (activeView != null)
                    activeView.UrlTrackerModel = icAdvancedView.UrlTrackerModel = UrlTrackerModel;

                if (gvUrlTracker.PageSize != PageSize)
                    gvUrlTracker.PageSize = PageSize;
                if (gvNotFound.PageSize != PageSize)
                    gvNotFound.PageSize = PageSize;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!_earlyErrorDetected)
            {
                liDetails.Visible = mvUrlTracker.GetActiveView() == vwUrlTrackerDetail;
                liNew.Visible = mvUrlTracker.GetActiveView() == vwUrlTrackerNew;

                bool hasNotFoundEntries = UrlTrackerRepository.HasNotFoundEntries();
                ltlNotFoundText.Visible = mvSwitchButtons.Visible = hasNotFoundEntries;
                lbDeleteSelected.Visible = (_isNotFoundView ? gvNotFound : gvUrlTracker).Rows.Count > 0;

                mvUrlTrackerEntries.SetActiveView(vwUrlTrackerEntriesTable);
                pnlFilter.Visible = true;
                if (gvUrlTracker.Rows.Count == 0 && !_gridviewFiltered)
                {
                    mvUrlTrackerEntries.SetActiveView(vwUrlTrackerEntriesNone);
                    pnlFilter.Visible = false;
                }

                if (gvUrlTracker.PageSize != PageSize)
                    gvUrlTracker.PageSize = PageSize;
                if (gvNotFound.PageSize != PageSize)
                    gvNotFound.PageSize = PageSize;
                ddlPageSize.SelectedValue = PageSize.ToString();
            }
        }

        void odsUrlTrackerEntries_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.Cancel = true;
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
                    DataControlFieldCell dataControlFieldCell = cells[i] as DataControlFieldCell;
                    if (dataControlFieldCell != null)
                    {
                        BoundField boundField = dataControlFieldCell.ContainingField as BoundField;
                        if (boundField != null && boundField.DataField == "Referrer")
                            cells[i].CssClass += " max";
                    }
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
            if (gridView.SelectedDataKey.Values.Count > 1)
            {
                LoadDetail(gridView.SelectedDataKey.Values[0], gridView.SelectedDataKey.Values[1] as string);
            }
            else
            {
                LoadDetail(gridView.SelectedDataKey.Value);
            }
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

        protected void cbFilters_CheckedChanged(object sender, EventArgs e)
        {
            Filter();
        }

        protected void tbSearch_TextChanged(object sender, EventArgs e)
        {
            Filter();
        }

        void Filter()
        {
            Parameter showAutoEntriesParameter = new Parameter("showAutoEntries", DbType.Boolean, cbShowAutoEntries.Checked.ToString());
            Parameter showCustomEntriesParameter = new Parameter("showCustomEntries", DbType.Boolean, cbShowCustomEntries.Checked.ToString());
            Parameter showRegexEntriesParameter = new Parameter("showRegexEntries", DbType.Boolean, cbShowRegexEntries.Checked.ToString());
            Parameter searchParameter = new Parameter("keyword", DbType.String, tbSearch.Text);

            if (!_isNotFoundView)
            {
                odsUrlTrackerEntries.SelectParameters.Clear();
                odsUrlTrackerEntries.SelectParameters.Add(showAutoEntriesParameter);
                odsUrlTrackerEntries.SelectParameters.Add(showCustomEntriesParameter);
                odsUrlTrackerEntries.SelectParameters.Add(showRegexEntriesParameter);
                odsUrlTrackerEntries.SelectParameters.Add(searchParameter);

                gvUrlTracker.DataBind();
                mvUrlTrackerFilter.SetActiveView(vwUrlTrackerFilterGrid);
                if (gvUrlTracker.Rows.Count == 0)
                    mvUrlTrackerFilter.SetActiveView(vwUrlTrackerFilterNoResults);
            }
            else
            {
                odsNotFoundEntries.SelectParameters.Clear();
                odsNotFoundEntries.SelectParameters.Add(searchParameter);

                gvNotFound.DataBind();
                mvNotFoundFilter.SetActiveView(vwNotFoundFilterGrid);
                if (gvNotFound.Rows.Count == 0)
                    mvNotFoundFilter.SetActiveView(vwNotFoundFilterNoResults);
            }

            _gridviewFiltered = true;
        }

        protected void ShowOverview(object sender, EventArgs e)
        {
            mvUrlTracker.SetActiveView(vwUrlTrackerOverview);
            gvUrlTracker.DataBind();
        }

        protected void lbNotFoundView_Click(object sender, EventArgs e)
        {
            mvMainButtons.SetActiveView(vwMainButtonsClearNotFound);
            mvSwitchButtons.SetActiveView(vwSwitchButtonsUrlTracker);
            mvGridViews.SetActiveView(vwGridViewsNotFound);
            gvNotFound.DataBind();
            lbCreate.Visible = false;
            pnlFilters.Visible = false;
        }

        protected void lbUrlTrackerView_Click(object sender, EventArgs e)
        {
            mvMainButtons.SetActiveView(vwMainButtonsCreate);
            mvSwitchButtons.SetActiveView(vwSwitchButtonsNotFound);
            mvGridViews.SetActiveView(vwGridViewsUrlTracker);
            gvUrlTracker.DataBind();
            lbCreate.Visible = true;
            pnlFilters.Visible = true;
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

        protected void lbDeleteErrorRows_Click(object sender, EventArgs e)
        {
            List<int> invalidRowIds;
            if (UrlTrackerRepository.HasInvalidEntries(out invalidRowIds))
                invalidRowIds.ForEach(x => { UrlTrackerRepository.DeleteUrlTrackerEntry(x); });
            Response.Redirect(Request.RawUrl);
        }

        void LoadDetail(object dataKey, string secondKey = null)
        {
            ResetViews();
            pnlBreadcrumb.Visible = true;
            mvUrlTracker.SetActiveView(vwUrlTrackerDetail);

            if (secondKey == null)
            {
                UrlTrackerModel = UrlTrackerRepository.GetUrlTrackerEntryById((int)dataKey);
            }
            else if (dataKey is int && !string.IsNullOrEmpty(secondKey))
            {
                UrlTrackerModel = UrlTrackerRepository.GetNotFoundEntryByRootAndUrl((int)dataKey, secondKey);
            }
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

        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int pageSize;
            if (int.TryParse(ddlPageSize.SelectedValue, out pageSize))
                PageSize = pageSize;
            if (!_isNotFoundView)
                gvUrlTracker.DataBind();
            else
                gvNotFound.DataBind();
        }

        protected void lbClearNotFound_Click(object sender, EventArgs e)
        {
            UrlTrackerRepository.ClearNotFoundEntries();

            lbUrlTrackerView_Click(sender, e);
        }
    }
}