using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umbraco.DataLayer;

namespace InfoCaster.Umbraco.UrlTracker
{
	public class RecordsReader : IRecordsReader
	{
		private readonly RecordsReaderCacheEntry _recordsReaderCacheEntry;

		private int RecordsIndex { get; set; }
		private Dictionary<string, object> CurrentRecord
		{
			get { return RecordsReaderCacheEntry.Records[RecordsIndex]; }
		}

		public RecordsReaderCacheEntry RecordsReaderCacheEntry
		{
			get { return _recordsReaderCacheEntry; }
		}

		public RecordsReader(RecordsReaderCacheEntry recordsReaderCacheEntry)
		{
			RecordsIndex = -1;
			_recordsReaderCacheEntry = recordsReaderCacheEntry;
		}

		public IEnumerator GetEnumerator()
		{
			return RecordsReaderCacheEntry.Records.GetEnumerator();
		}

		public void Dispose()
		{
		}

		public bool Read()
		{
			if (!HasRecords || RecordsIndex + 1 >= RecordsReaderCacheEntry.Records.Count)
			{
				return false;
			}

			RecordsIndex++;

			return true;
		}

		public void Close()
		{
		}

		public bool ContainsField(string fieldName)
		{
			return RecordsReaderCacheEntry.FieldNames.Contains(fieldName);
		}

		public bool IsNull(string fieldName)
		{
			VerifyFieldExists(fieldName);

			return CurrentRecord[fieldName] == null;
		}

		public TFieldType Get<TFieldType>(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (TFieldType)CurrentRecord[fieldName];

			return returnValue;
		}

		public object GetObject(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = CurrentRecord[fieldName];

			return returnValue;
		}

		public bool GetBoolean(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (bool)CurrentRecord[fieldName];

			return returnValue;
		}

		public byte GetByte(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (byte)CurrentRecord[fieldName];

			return returnValue;
		}

		public DateTime GetDateTime(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (DateTime)CurrentRecord[fieldName];

			return returnValue;
		}

		public decimal GetDecimal(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (decimal)CurrentRecord[fieldName];

			return returnValue;
		}

		public double GetDouble(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (double)CurrentRecord[fieldName];

			return returnValue;
		}

		public float GetFloat(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (float)CurrentRecord[fieldName];

			return returnValue;
		}

		public Guid GetGuid(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (Guid)CurrentRecord[fieldName];

			return returnValue;
		}

		public short GetShort(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (short)CurrentRecord[fieldName];

			return returnValue;
		}

		public int GetInt(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (int)CurrentRecord[fieldName];

			return returnValue;
		}

		public long GetLong(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (long)CurrentRecord[fieldName];

			return returnValue;
		}

		public string GetString(string fieldName)
		{
			VerifyFieldExists(fieldName);

			var returnValue = (string)CurrentRecord[fieldName];

			return returnValue;
		}

		public bool HasRecords
		{
			get { return RecordsReaderCacheEntry.Records.Any(); }
		}

		private void VerifyFieldExists(string fieldName)
		{
			if (!ContainsField(fieldName))
			{
				throw new KeyNotFoundException(string.Format("The field {0} does not exist.", fieldName));
			}
		}
	}
}