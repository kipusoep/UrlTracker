using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfoCaster.Umbraco.UrlTracker.Exceptions
{
    public class DashboardAlreadyInstalledException : Exception
    {
        public DashboardAlreadyInstalledException()
        {
        }

        public DashboardAlreadyInstalledException(string message)
            : base(message)
        {
        }

        public DashboardAlreadyInstalledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}