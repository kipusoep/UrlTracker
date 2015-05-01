using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace InfoCaster.Umbraco.UrlTracker.Providers
{
    public class EmbeddedResourcesVirtualPathProvider : VirtualPathProvider
    {
        public EmbeddedResourcesVirtualPathProvider()
        {
        }

        private bool IsEmbeddedResourcePath(string virtualPath)
        {
            var checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
            return checkPath.StartsWith("~/Umbraco/UrlTracker/", StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool FileExists(string virtualPath)
        {
            return IsEmbeddedResourcePath(virtualPath) || base.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsEmbeddedResourcePath(virtualPath))
                return new EmbeddedResourceVirtualFile(virtualPath);
            return base.GetFile(virtualPath);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (IsEmbeddedResourcePath(virtualPath))
                return null;
            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }

    public class EmbeddedResourceVirtualFile : VirtualFile
    {
        readonly string _path;

        public EmbeddedResourceVirtualFile(string virtualPath)
            : base(virtualPath)
        {
            _path = VirtualPathUtility.ToAppRelative(virtualPath);
        }

        public override Stream Open()
        {
            var resourceName = _path.Split('/').Last();
            var assembly = GetType().Assembly;
            resourceName = assembly.GetManifestResourceNames()
                .SingleOrDefault(x => x.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(resourceName))
                return null;
            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}