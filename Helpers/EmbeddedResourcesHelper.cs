using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
	public static class EmbeddedResourcesHelper
	{
		public static StreamReader GetStream(System.Reflection.Assembly assembly, string name)
		{
			foreach (string resName in assembly.GetManifestResourceNames())
			{
				if (resName.EndsWith(name))
					return new StreamReader(assembly.GetManifestResourceStream(resName));
			}
			return null;
		}

		public static string GetString(System.Reflection.Assembly assembly, string name)
		{
            string data = null;
            using (StreamReader streamReader = EmbeddedResourcesHelper.GetStream(assembly, name))
            {
                if (streamReader != null)
                    data = streamReader.ReadToEnd();
            }
            return data;
		}

		public static string GetString(string name)
		{
			return EmbeddedResourcesHelper.GetString(typeof(EmbeddedResourcesHelper).Assembly, name);
		}
	}
}