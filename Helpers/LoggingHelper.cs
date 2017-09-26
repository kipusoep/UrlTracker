using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Web;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;
using UmbracoLog = umbraco.BusinessLogic.Log;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
    public static class LoggingHelper
    {
        public static void LogException(this Exception ex)
        {
            LogHelper.Error(typeof(LoggingHelper), ex.Message, ex);
        }

        public static void LogInformation(string message, params object[] args)
        {
            LogInformation(string.Format(message, args));
        }

        public static void LogInformation(string message)
        {
            if (UrlTrackerSettings.EnableLogging)
            {
                LogHelper.Debug(typeof(LoggingHelper), () => { return message; });
            }
        }
    }
}