using System;
using System.Web;
using log4net;
using System.Web.Caching;
using System.Threading;

namespace InfoCaster.Umbraco.UrlTracker
{
	public class SqlCacheProvider
	{
		public static SqlCacheProvider Instance = new SqlCacheProvider();

		private static readonly ILog Log = LogManager.GetLogger(typeof(SqlCacheProvider));
		private static Cache Cache { get { return HttpRuntime.Cache; } }
		private static readonly ReaderWriterLock ReaderWriterLock = new ReaderWriterLock();

		private static int CachTimeoutInSeconds
		{
			get { return UrlTrackerSettings.CacheDuration; }
		}

		public TCachedEntity GetItem<TCachedEntity>(string itemKey, Func<TCachedEntity> dataRetrevalFunc)
			where TCachedEntity : class
		{
			TCachedEntity data;

			if (TryGetItemFromCache(itemKey, out data) == false)
			{
				Log.Info(string.Format("{0} cache is stale, obtaining newer data", itemKey));

				if (TryGetItemFromSource(itemKey, dataRetrevalFunc, out data) == false)
				{
					Log.Warn("Data retrieval delegate failed");
				}
			}

			return data;
		}

		private bool TryGetItemFromSource<TCachedEntity>(string itemKey, Func<TCachedEntity> dataRetrevalFunc, out TCachedEntity data)
			where TCachedEntity : class
		{
			try
			{
				data = dataRetrevalFunc();

				// Add data to cache if it is correctly retrieved.
				if (data != null)
				{
					InsertCacheItem(itemKey, data);
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Data retrieval delegate failed.", ex);
				data = null;
			}

			return false;
		}

		private bool TryGetItemFromCache<TCachedEntity>(string itemKey, out TCachedEntity data)
			where TCachedEntity : class
		{
			try
			{
				data = GetItemFromCache<TCachedEntity>(itemKey);
			}
			catch (Exception ex)
			{
				Log.Error(string.Format("Retrieving data from cache failed (Required Type: {0}, Item Key: {1}).", typeof(TCachedEntity).Name, itemKey), ex);
				data = null;
			}

			return data != null;
		}

		private static void InsertCacheItem<TCachedEntity>(string itemKey, TCachedEntity itemValue)
		{
			ReaderWriterLock.AcquireWriterLock(0);
			try
			{
				Cache.Insert(itemKey, itemValue, null, DateTime.Now.AddSeconds(CachTimeoutInSeconds), Cache.NoSlidingExpiration);
			}
			finally
			{
				ReaderWriterLock.ReleaseWriterLock();
			}
		}



		private static TCachedEntity GetItemFromCache<TCachedEntity>(string itemKey)
			where TCachedEntity : class
		{
			ReaderWriterLock.AcquireReaderLock(0);
			try
			{
				var item = Cache.Get(itemKey);
				return item as TCachedEntity;
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Unable to retrieve cached item (Required Type: {0}, Item Key: {1}).", typeof(TCachedEntity).Name, itemKey), ex);
			}
			finally
			{
				ReaderWriterLock.ReleaseReaderLock();
			}
		}
	}
}