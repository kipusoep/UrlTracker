using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using umbraco.BusinessLogic;
using UmbracoLog = umbraco.BusinessLogic.Log;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
	public static class UrlTrackerLoggingHelper
	{
		static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public static void LogException(this Exception ex)
		{
			LogException(ex, -1);
		}

		public static void LogException(this Exception ex, int nodeId)
		{
			if (_logger != null)
				_logger.Error(null, ex);
			UmbracoLog.Add(LogTypes.Error, nodeId, string.Concat("Exception occurred in UrlTracker: ", ex.Message));
			UrlTrackerLogging.Log(ex);
		}

		public static void LogInformation(string message, params object[] args)
		{
			LogInformation(string.Format(message, args));
		}

		public static void LogInformation(string message)
		{
			if (UrlTrackerSettings.EnableLogging)
			{
				if (_logger != null)
					_logger.Debug(message);
				UmbracoLog.Add(LogTypes.Debug, -1, message);
				UrlTrackerLogging.Log(message);
			}
		}
	}
}