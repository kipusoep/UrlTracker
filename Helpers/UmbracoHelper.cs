using InfoCaster.Umbraco.UrlTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.DataLayer;
using umbraco.IO;
using umbraco.NodeFactory;
using Umbraco.Core;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
	public static class UmbracoHelper
	{
		static readonly object _locker = new object();
		static volatile string _reservedUrlsCache;
		static string _reservedPathsCache;
		static StartsWithContainer _reservedList = new StartsWithContainer();

		static Assembly _umbracoAssembly;
		static bool _umbracoAssemblyInitialized = false;

		/// <summary>
		/// Determines whether the specified URL is reserved or is inside a reserved path.
		/// </summary>
		/// <param name="url">The URL to check.</param>
		/// <returns>
		/// 	<c>true</c> if the specified URL is reserved; otherwise, <c>false</c>.
		/// </returns>
		internal static bool IsReservedPathOrUrl(string url)
		{
			if (_reservedUrlsCache == null)
			{
				lock (_locker)
				{
					if (_reservedUrlsCache == null)
					{
						// store references to strings to determine changes
						_reservedPathsCache = GlobalSettings.ReservedPaths;
						_reservedUrlsCache = GlobalSettings.ReservedUrls;

						// add URLs and paths to a new list
						StartsWithContainer _newReservedList = new StartsWithContainer();
						foreach (string reservedUrl in _reservedUrlsCache.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
						{
							//resolves the url to support tilde chars
							string reservedUrlTrimmed = IOHelper.ResolveUrl(reservedUrl).Trim().ToLower();
							if (reservedUrlTrimmed.Length > 0)
								_newReservedList.Add(reservedUrlTrimmed);
						}

						foreach (string reservedPath in _reservedPathsCache.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
						{
							bool trimEnd = !reservedPath.EndsWith("/");
							//resolves the url to support tilde chars
							string reservedPathTrimmed = IOHelper.ResolveUrl(reservedPath).Trim().ToLower();

							if (reservedPathTrimmed.Length > 0)
								_newReservedList.Add(reservedPathTrimmed + (reservedPathTrimmed.EndsWith("/") ? "" : "/"));
						}

						// use the new list from now on
						_reservedList = _newReservedList;
					}
				}
			}

			//The url should be cleaned up before checking:
			// * If it doesn't contain an '.' in the path then we assume it is a path based URL, if that is the case we should add an trailing '/' because all of our reservedPaths use a trailing '/'
			// * We shouldn't be comparing the query at all
			var pathPart = url.Split('?')[0];
			if (!pathPart.Contains(".") && !pathPart.EndsWith("/"))
				pathPart += "/";

			// return true if url starts with an element of the reserved list
			return _reservedList.StartsWith(pathPart.ToLowerInvariant());
		}

		static List<UrlTrackerDomain> _urlTrackerDomains;
		internal static List<UrlTrackerDomain> GetDomains()
		{
			if (_urlTrackerDomains == null)
			{
				_urlTrackerDomains = new List<UrlTrackerDomain>();
				ISqlHelper sqlHelper = Application.SqlHelper;
				using (var dr = sqlHelper.ExecuteReader("SELECT * FROM umbracoDomains"))
				{
					while (dr.Read())
					{
						_urlTrackerDomains.Add(new UrlTrackerDomain(dr.GetInt("id"), dr.GetInt("domainRootStructureID"), dr.GetString("domainName")));
					}
				}
			}
			return _urlTrackerDomains;
		}

		internal static string GetUmbracoUrlSuffix()
		{
			return !GlobalSettings.UseDirectoryUrls ? ".aspx" : UmbracoSettings.AddTrailingSlash ? "/" : string.Empty;
		}

		internal static string GetUrl(int nodeId)
		{
			try
			{
				if (!_umbracoAssemblyInitialized)
				{
					IEnumerable<Assembly> umbracoCoreAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("umbraco, "));
					if (umbracoCoreAssemblies != null && umbracoCoreAssemblies.Any())
						_umbracoAssembly = umbracoCoreAssemblies.OrderByDescending(x => x.ImageRuntimeVersion).First();
				}
				if (_umbracoAssembly != null)
				{
					Type umbracoContextType = _umbracoAssembly.GetType("Umbraco.Web.UmbracoContext");
					if (umbracoContextType != null)
					{
						object umbracoContextCurrent = umbracoContextType.GetProperty("Current").GetValue(null, null);
						if (umbracoContextCurrent == null)
						{
							MethodInfo ensureContextMethod = umbracoContextType.GetMethod("EnsureContext", new Type[] { typeof(HttpContextBase), typeof(ApplicationContext) });
							if (ensureContextMethod != null)
								ensureContextMethod.Invoke(null, new object[] { new HttpContextWrapper(HttpContext.Current), ApplicationContext.Current });
						}
					}
				}

				_umbracoAssemblyInitialized = true;

				return umbraco.library.NiceUrl(nodeId);
			}
			catch (Exception ex)
			{
				ex.LogException(nodeId);
				Exception newEx = new Exception(string.Format("Please upgrade your version of umbraco to the latest 6.1.x, because at the moment I'm not able to fetch the NiceUrl of the node (#{0}) to redirect to :-(", nodeId));
				newEx.LogException(nodeId);
				throw newEx;
			}
		}

		internal static string GetUrl(this Node node)
		{
			return GetUrl(node.Id);
		}
	}
}