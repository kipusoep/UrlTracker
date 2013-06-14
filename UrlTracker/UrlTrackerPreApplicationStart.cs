using InfoCaster.Umbraco.UrlTracker;
using InfoCaster.Umbraco.UrlTracker.Modules;
using InfoCaster.Umbraco.UrlTracker.Providers;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Hosting;

[assembly: System.Web.PreApplicationStartMethod(typeof(UrlTrackerPreApplicationStart), "RegisterModule")]
namespace InfoCaster.Umbraco.UrlTracker
{
	public class UrlTrackerPreApplicationStart
	{
		public static void RegisterModule()
		{
			DynamicModuleUtility.RegisterModule(typeof(UrlTrackerModule));
			HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedResourcesVirtualPathProvider());
		}
	}
}