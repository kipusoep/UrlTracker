using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace InfoCaster.Umbraco.UrlTracker.Extensions
{
	public static class NameValueCollectionExtension
	{
		public static bool CollectionEquals(this NameValueCollection nameValueCollection1, NameValueCollection nameValueCollection2)
		{
			return nameValueCollection1.ToKeyValue().SequenceEqual(nameValueCollection2.ToKeyValue());
		}

		public static NameValueCollection Clone(NameValueCollection collection)
		{
			return new NameValueCollection(collection);
		}

		/// <summary>
		/// Adds the second dictionary to the first. If a key occurs in both dictionaries, the value of the second dictionary is taken.
		/// </summary>
		public static void Append(this NameValueCollection nameValueCollection1, NameValueCollection nameValueCollection2)
		{
			if (nameValueCollection1 == null)
				throw new ArgumentNullException("first");

			if (nameValueCollection2 != null)
			{
				for (int i = 0; i < nameValueCollection2.Count; i++)
					nameValueCollection1.Set(nameValueCollection2.GetKey(i), nameValueCollection2.Get(i));
			}
		}

		/// <summary>
		/// Merges two collections. If a key occurs in both collections, the value of the second collection is taken.
		/// </summary>
		public static NameValueCollection Merge(this NameValueCollection nameValueCollection1, NameValueCollection nameValueCollection2)
		{
			if (nameValueCollection1 == null && nameValueCollection2 == null)
				return null;
			else if (nameValueCollection1 != null && nameValueCollection2 == null)
				return Clone(nameValueCollection1);
			else if (nameValueCollection1 == null && nameValueCollection2 != null)
				return Clone(nameValueCollection2);

			NameValueCollection result = Clone(nameValueCollection1);
			Append(result, nameValueCollection2);
			return result;
		}

		/// <summary>
		/// Constructs a QueryString (string).
		/// Consider this method to be the opposite of <see cref="System.Web.HttpUtility.ParseQueryString"/>
		/// </summary>
		/// <param name="nvc">NameValueCollection</param>
		/// <returns>String</returns>
		public static string ToQueryString(this NameValueCollection nameValueCollection)
		{
			List<string> items = new List<string>();

			foreach (string name in nameValueCollection)
				items.Add(string.Concat(name, "=", System.Web.HttpUtility.UrlEncode(nameValueCollection[name])));

			return string.Join("&", items.ToArray());
		}

		static IEnumerable<object> ToKeyValue(this NameValueCollection nameValueCollection)
		{
			return nameValueCollection.AllKeys.OrderBy(x => x).Select(x => new { Key = x, Value = nameValueCollection[x] });
		}
	}
}