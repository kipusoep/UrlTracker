using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using UmbracoLog = umbraco.BusinessLogic.Log;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
	public static class UrlTrackerLoggingHelper
	{
		public static void LogException(this Exception ex)
		{
			LogException(ex, -1);
		}

		public static void LogException(this Exception ex, int nodeId)
		{
			UmbracoLog.Add(LogTypes.Error, nodeId, string.Concat("Exception occurred in UrlTracker: ", ex.Message));
			UrlTrackerErrorLogging.Log(ex);
		}

		public static void LogInformation(string message)
		{
			LogInformation(message, -1);
		}

		public static void LogInformation(string message, params object[] args)
		{
			LogInformation(message, -1, args);
		}

		public static void LogInformation(string message, int nodeId, params object[] args)
		{
			LogInformation(string.Format(message, args), nodeId);
		}

		public static void LogInformation(string message, int nodeId)
		{
			if (UrlTrackerSettings.EnableLogging)
				UmbracoLog.Add(LogTypes.Debug, nodeId, message);
		}
	}
}