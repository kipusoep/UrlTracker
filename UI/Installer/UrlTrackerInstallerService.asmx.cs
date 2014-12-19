using InfoCaster.Umbraco.UrlTracker.Exceptions;
using InfoCaster.Umbraco.UrlTracker.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool DontWait { get; set; }

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
                if (string.IsNullOrEmpty(dashboardConfig))
                    throw new Exception("Unable to add dashboard: Couldn't read current ~/config/dashboard.config, permissions issue?");
                XDocument dashboardDoc = XDocument.Parse(dashboardConfig, LoadOptions.PreserveWhitespace);
                if (dashboardDoc == null)
                    throw new Exception("Unable to add dashboard: Unable to parse current ~/config/dashboard.config file, invalid XML?");
                XElement dashBoardElement = dashboardDoc.Element("dashBoard");
                if (dashBoardElement == null)
                    throw new Exception("Unable to add dashboard: dashBoard element not found in ~/config/dashboard.config file");
                List<XElement> sectionElements = dashBoardElement.Elements("section").ToList();
                if (sectionElements == null || !sectionElements.Any())
                    throw new Exception("Unable to add dashboard: No section elements found in ~/config/dashboard.config file");
                XElement startupDashboardSectionElement = sectionElements.SingleOrDefault(x => x.Attribute("alias") != null && x.Attribute("alias").Value == "StartupDashboardSection");
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
                        if (urlTrackerTabControls.Any(x => x.Value == "~/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManagerWrapper.ascx"))
                            throw new DashboardAlreadyInstalledException("Dashboard is already installed.");
                    }
                }

                XElement lastTab = tabs.Last();
                XElement urlTrackerTab = new XElement("tab");
                urlTrackerTab.Add(new XAttribute("caption", "Url Tracker"));
                XElement urlTrackerControl = new XElement("control");
                urlTrackerControl.Add(new XAttribute("addPanel", true));
                urlTrackerControl.SetValue("~/Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI.UrlTrackerManagerWrapper.ascx");
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
                string responseText;
                Uri currentUri = HttpContext.Current.Request.Url;
                WebRequest request = WebRequest.Create(string.Format("{0}://{1}:{2}?{3}=1", currentUri.Scheme, currentUri.Host, currentUri.Port, UrlTrackerSettings.HttpModuleCheck));
                using (WebResponse response = request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                {
                    responseText = new StreamReader(responseStream).ReadToEnd();
                    if (responseText.Trim().ToUpper() == UrlTrackerSettings.HttpModuleCheck.Trim().ToUpper())
                        return string.Empty;
                }
                throw new Exception(string.Format("The Http Module isn't responding, response was: '{0}'", responseText));
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
            string exceptionString = "error: ";
            if (ex.InnerException != null)
                exceptionString += string.Format("{0} ({1})", ex.Message, ex.InnerException.Message);
            else
                exceptionString += ex.Message;

            StackTrace stackTrace = new StackTrace(true);
            exceptionString += " | Stacktrace: " + stackTrace.ToString().Replace("\r", string.Empty).Replace("\n", "<br />");
            return exceptionString;
        }

        void Wait()
        {
            if (!DontWait)
                Thread.Sleep(1000);
        }
    }
}
