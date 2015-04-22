using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using umbraco.DataLayer;
using Umbraco.Core;

namespace InfoCaster.Umbraco.UrlTracker
{
	[Serializable]
	public class RecordsReaderCacheEntry
	{
		internal string[] FieldNames { get; set; }
		internal IList<Dictionary<string, object>> Records { get; set; }

		public RecordsReaderCacheEntry()
		{
			FieldNames = new string[0];
			Records = new List<Dictionary<string, object>>();
		}

		public static RecordsReaderCacheEntry Create(IRecordsReader recordsReader)
		{
			var recordsReaderCacheEntry = new RecordsReaderCacheEntry();
			if (!recordsReader.HasRecords)
			{
				return recordsReaderCacheEntry;
			}

			var enumerator = recordsReader.GetEnumerator();

			while (enumerator.MoveNext())
			{
				var currentItem = enumerator.Current;
				if (!recordsReaderCacheEntry.FieldNames.Any())
				{
					var fieldNameLookup = GetPrivateFieldValue(currentItem, "_fieldNameLookup");
					recordsReaderCacheEntry.FieldNames = (string[])GetPrivateFieldValue(fieldNameLookup, "_fieldNames");
				}

				var currentRecordDictionary = new Dictionary<string, object>();

				var values = (object[])GetPrivateFieldValue(currentItem, "_values");

				foreach (var fieldName in recordsReaderCacheEntry.FieldNames)
				{
					var valueIndex = Array.IndexOf(recordsReaderCacheEntry.FieldNames, fieldName);
					var value = values[valueIndex];
					if (value is DBNull)
					{
						value = null;
					}

					currentRecordDictionary.Add(fieldName, value);
				}

				recordsReaderCacheEntry.Records.Add(currentRecordDictionary);
			}

			return recordsReaderCacheEntry;
		}

		private static object GetPrivateFieldValue(object target, string fieldName)
		{
			var targetType = target.GetType();
			var targetField = targetType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (targetField == null)
			{
				throw new NullReferenceException(string.Format("{0}.{1} is null", targetType.Name, fieldName));
			}
			var fieldValue = targetField.GetValue(target);
			return fieldValue;
		}
	}
}