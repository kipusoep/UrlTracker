using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Web;
using umbraco.BusinessLogic;
using UmbracoLog = umbraco.BusinessLogic.Log;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
	public static class UrlTrackerLoggingHelper
	{
		static Assembly _log4netAssembly;
		static bool _log4netAssemblyInitialized = false;

		public static void LogException(this Exception ex)
		{
			LogException(ex, -1);
		}

		public static void LogException(this Exception ex, int nodeId)
		{
			LogToLog4net(exception: ex);
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
				LogToLog4net(message: message);
				UmbracoLog.Add(LogTypes.Debug, -1, message);
				UrlTrackerLogging.Log(message);
			}
		}

		static void LogToLog4net(string message = "", Exception exception = null)
		{
			if (message == null && exception == null)
				throw new ArgumentNullException("message and exception");

			if (!_log4netAssemblyInitialized)
				_log4netAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(x => x.FullName.StartsWith("log4net"));
			if (_log4netAssembly != null)
			{
				Type logManagerType = _log4netAssembly.GetType("LogManager");
				MethodInfo getLoggerMethod = logManagerType.GetMethod("GetLogger");
				object iLog = getLoggerMethod.Invoke(null, new[] { MethodBase.GetCurrentMethod().DeclaringType });
				Type iLogType = _log4netAssembly.GetType("ILog");
				if (exception != null)
				{
					MethodInfo errorMethod = logManagerType.GetMethod("Error");
					errorMethod.Invoke(iLog, new[] { null, exception });
				}
				else
				{
					MethodInfo debugMethod = logManagerType.GetMethod("Error");
					debugMethod.Invoke(iLog, new[] { message });
				}
			}

			_log4netAssemblyInitialized = true;
		}
	}
}