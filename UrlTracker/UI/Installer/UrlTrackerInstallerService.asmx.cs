using InfoCaster.Umbraco.UrlTracker.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml.Linq;

namespace InfoCaster.Umbraco.UrlTracker.UI.Installer
{
	/// <summary>
	/// Summary description for TestService
	/// </summary>
	[WebService(Namespace = "http://InfoCaster.Umbraco.UrlTracker/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	[System.Web.Script.Services.ScriptService]
	public class UrlTrackerInstallerService : System.Web.Services.WebService
	{
		static JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string InstallTable()
		{
			try
			{
				Wait();
				UrlTrackerRepository.CreateUrlTrackerTable();
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
			return string.Empty;
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string InstallDashboard()
		{
			try
			{
				Wait();
				string dashboardConfig;
				string dashboardConfigPath = HttpContext.Current.Server.MapPath("~/config/dashboard.config");
				using (StreamReader streamReader = File.OpenText(dashboardConfigPath))
					dashboardConfig = streamReader.ReadToEnd();
				XDocument dashboardDoc = XDocument.Parse(dashboardConfig, LoadOptions.PreserveWhitespace);
				XElement startupDashboardSectionElement = dashboardDoc.Element("dashBoard").Elements("section").SingleOrDefault(x => x.Attribute("alias").Value == "StartupDashboardSection");
				if (startupDashboardSectionElement == null)
					throw new Exception("Unable to add dashboard: StartupDashboardSection not found in ~/config/dashboard.config");

				List<XElement> tabs = startupDashboardSectionElement.Elements("tab").ToList();
				if (!tabs.Any())
					throw new Exception("Unable to add dashboard: No existing tabs found within the StartupDashboardSection");

				List<XElement> urlTrackerTabs = tabs.Where(x => x.Attribute("caption").Value == "Url Tracker").ToList();
				if (urlTrackerTabs.Any())
				{
					foreach (XElement tab in urlTrackerTabs)
					{
						List<XElement> urlTrackerTabControls = tab.Elements("control").ToList();
						if (urlTrackerTabControls.Any(x => x.Value == "/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManagerWrapper.ascx"))
							throw new Exception("Dashboard is already installed.");
					}
				}

				XElement lastTab = tabs.Last();
				XElement urlTrackerTab = new XElement("tab");
				urlTrackerTab.Add(new XAttribute("caption", "Url Tracker"));
				XElement urlTrackerControl = new XElement("control");
				urlTrackerControl.Add(new XAttribute("addPanel", true));
				urlTrackerControl.SetValue("/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManagerWrapper.ascx");
				urlTrackerTab.Add(urlTrackerControl);
				urlTrackerControl.AddBeforeSelf(string.Concat(Environment.NewLine, "      "));
				urlTrackerControl.AddAfterSelf(string.Concat(Environment.NewLine, "    "));
				lastTab.AddAfterSelf(urlTrackerTab);
				lastTab.AddAfterSelf(string.Concat(Environment.NewLine, "    "));
				dashboardDoc.Save(dashboardConfigPath, SaveOptions.None);
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
			return string.Empty;
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string CheckHttpModule()
		{
			try
			{
				Wait();
				Uri currentUri = HttpContext.Current.Request.Url;
				WebRequest request = WebRequest.Create(string.Format("{0}://{1}:{2}?{3}=1", currentUri.Scheme, currentUri.Host, currentUri.Port, UrlTrackerSettings.HttpModuleCheck));
				using (WebResponse response = request.GetResponse())
				using (Stream responseStream = response.GetResponseStream())
				{
					if (new StreamReader(responseStream).ReadToEnd() == UrlTrackerSettings.HttpModuleCheck)
						return string.Empty;
				}
				throw new Exception("The Http Module isn't responding.");
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string HasOldVersionInstalled()
		{
			try
			{
				Wait();
				return UrlTrackerRepository.GetUrlTrackeOldTableExists().ToString();
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
		}

		[WebMethod]
		[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public string MigrateData()
		{
			try
			{
				Wait();
				return UrlTrackerRepository.MigrateData().ToString();
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}

		}

		string HandleException(Exception ex)
		{
			if (ex.InnerException != null)
				return string.Format("error: {0} ({1})", ex.Message, ex.InnerException.Message);
			return string.Concat("error: ", ex.Message);
		}

		void Wait()
		{
			Thread.Sleep(1000);
		}
	}
}
