using InfoCaster.Umbraco.UrlTracker.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;

namespace InfoCaster.Umbraco.UrlTracker.UI.Installer
{
	[ServiceContract(Namespace = "InfoCaster.Umbraco.UrlTracker")]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class UrlTrackerInstallerService
	{
		static JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();

		[OperationContract]
		[WebGet]
		public string InstallTable()
		{
			try
			{
				UrlTrackerRepository.CreateUrlTrackerTable();
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
			return string.Empty;
		}

		[OperationContract]
		[WebGet]
		public string CheckHttpModule()
		{
			try
			{
				Uri currentUri = HttpContext.Current.Request.Url;
				WebRequest request = WebRequest.Create(string.Format("{0}://{1}?{2}=1", currentUri.Scheme, currentUri.Host, UrlTrackerSettings.HttpModuleCheck));
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

		[OperationContract]
		[WebGet]
		public string HasOldVersionInstalled()
		{
			try
			{
				return UrlTrackerRepository.GetUrlTrackeOldTableExists().ToString();
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
		}

		[OperationContract]
		[WebGet]
		public string MigrateData()
		{
			try
			{
				return UrlTrackerRepository.MigrateData().ToString();
			}
			catch (Exception ex)
			{
				return HandleException(ex);
			}
			
		}

		string HandleException(Exception ex)
		{
			if(ex.InnerException != null)
				return string.Format("error: {0} ({1})", ex.Message, ex.InnerException.Message);
			return string.Concat("error: ", ex.Message);
		}
	}
}
