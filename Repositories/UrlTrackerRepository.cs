using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InfoCaster.Umbraco.UrlTracker.Extensions;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Web;

namespace InfoCaster.Umbraco.UrlTracker.Repositories
{
    public static class UrlTrackerRepository
    {
        static readonly Uri _baseUri = new Uri("http://www.example.org");
        static List<UrlTrackerModel> _forcedRedirectsCache;
        static DateTime LastForcedRedirectCacheRefreshTime = DateTime.UtcNow;
        static readonly object _cacheLock = new object();
        static readonly object _timeoutCacheLock = new object();
        static readonly DatabaseProviders DatabaseProvider = ApplicationContext.Current.DatabaseContext.DatabaseProvider;

        #region Add
        public static bool AddUrlMapping(IContent content, int rootNodeId, string url, AutoTrackingTypes type, bool isChild = false)
        {
            if (url != "#" && content.Template != null && content.Template.Id > 0)
            {
                var notes = isChild ? "An ancestor" : "This page";
                switch (type)
                {
                    case AutoTrackingTypes.Moved:
                        notes += " was moved";
                        break;
                    case AutoTrackingTypes.Renamed:
                        notes += " was renamed";
                        break;
                    case AutoTrackingTypes.UrlOverwritten:
                        notes += "'s property 'umbracoUrlName' changed";
                        break;
                    case AutoTrackingTypes.UrlOverwrittenSEOMetadata:
                        notes += string.Format("'s property '{0}' changed", UrlTrackerSettings.SEOMetadataPropertyName);
                        break;
                }

                url = UrlTrackerHelper.ResolveShortestUrl(url);

                if (UrlTrackerSettings.HasDomainOnChildNode)
                {
                    var rootNode = new Node(rootNodeId);
                    var rootUri = new Uri(rootNode.NiceUrl);
                    var shortRootUrl = UrlTrackerHelper.ResolveShortestUrl(rootUri.AbsolutePath);
                    if (url.StartsWith(shortRootUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        url = UrlTrackerHelper.ResolveShortestUrl(url.Substring(shortRootUrl.Length));
                    }
                }

                var database = ApplicationContext.Current.DatabaseContext.Database;

                if (!string.IsNullOrEmpty(url))
                {
                    var query = "SELECT 1 FROM icUrlTracker WHERE RedirectNodeId = @nodeId AND OldUrl = @url";

                    var exists = database.ExecuteScalar<int?>(query, new object[] { new { nodeId = content.Id }, new { url = url } });

                    if (exists != 1)
                    {
                        LoggingHelper.LogInformation("UrlTracker Repository | Adding mapping for node id: {0} and url: {1}", new string[] { content.Id.ToString(), url });

                        database.Insert(
                            "icUrlTracker",
                            "Id",
                            true,
                            new { RedirectRootNodeId = rootNodeId, RedirectNodeId = content.Id, OldUrl = url, Notes = notes });
                        if (content.Children().Any())
                        {
                            foreach (var child in content.Children())
                            {
                                var node = new Node(child.Id);
                                AddUrlMapping(child, rootNodeId, node.NiceUrl, type, true);
                            }
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        public static void AddUrlTrackerEntry(UrlTrackerModel urlTrackerModel)
        {
            var database = ApplicationContext.Current.DatabaseContext.Database;
            database.Insert(urlTrackerModel);
            
            if (urlTrackerModel.ForceRedirect)
            {
                ReloadForcedRedirectsCache();
            }
        }

        public static void AddGoneEntryByNodeId(int nodeId)
        {
            if (UmbracoContext.Current == null) // NiceUrl will throw an exception if UmbracoContext is null, and we'll be unable to retrieve the URL of the node
            {
                return;
            }

            var url = umbraco.library.NiceUrl(nodeId);
            if (url == "#")
            {
                return;
            }

            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                var uri = new Uri(url);
                url = uri.AbsolutePath;
            }
            url = UrlTrackerHelper.ResolveShortestUrl(url);

            var database = ApplicationContext.Current.DatabaseContext.Database;

            var query = "SELECT 1 FROM icUrlTracker WHERE RedirectNodeId = @redirectNodeId AND OldUrl = @oldUrl AND RedirectHttpCode = 410";

            var exists = database.ExecuteScalar<int?>(query, new object[] { new { redirectNodeId = nodeId }, new { oldUrl = url } });
            if (exists != 1)
            {
                LoggingHelper.LogInformation("UrlTracker Repository | Inserting 410 Gone mapping for node with id: {0}", nodeId);
                database.Insert(
                    "icUrlTracker",
                    "Id",
                    true,
                    new { RedirectNodeId = nodeId, OldUrl = url, RedirectHttpCode = 410, Notes = "Node removed" });
            }
            else
            {
                LoggingHelper.LogInformation("UrlTracker Repository | Skipping 410 Gone mapping for node with id: {0} (already exists)", nodeId);
            }
        }
        #endregion

        #region Delete
        public static void DeleteUrlTrackerEntriesByNodeId(int nodeId)
        {
            var query = "SELECT 1 FROM icUrlTracker WHERE RedirectNodeId = @nodeId AND RedirectHttpCode != 410";
            var exists = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(query, new object[] { new { nodeId = nodeId } });
            if (exists == 1)
            {
                LoggingHelper.LogInformation("UrlTracker Repository | Deleting Url Tracker entry of node with id: {0}", nodeId);

                query = "DELETE FROM icUrlTracker WHERE RedirectNodeId = @nodeId AND RedirectHttpCode != 410";
                ApplicationContext.Current.DatabaseContext.Database.Execute(query, new object[] { new { nodeId = nodeId } });
            }

            ReloadForcedRedirectsCache();
        }

        public static void DeleteNotFoundEntriesByOldUrl(string oldUrl)
        {
            var query = "SELECT 1 FROM icUrlTracker WHERE Is404 = 1 AND OldUrl = @oldUrl";
            var exists = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(query, new object[] { new { oldUrl = oldUrl } });
            if (exists == 1)
            {
                LoggingHelper.LogInformation("UrlTracker Repository | Deleting Not Found entries with OldUrl: {0}", oldUrl);

                query = "DELETE FROM icUrlTracker WHERE Is404 = 1 AND OldUrl = @oldUrl";
                ApplicationContext.Current.DatabaseContext.Database.Execute(query, new object[] { new { oldUrl = oldUrl } });
            }
        }

        public static void DeleteUrlTrackerEntry(int id)
        {
            LoggingHelper.LogInformation("UrlTracker Repository | Deleting Url Tracker entry with id: {0}", id);

            var query = "DELETE FROM icUrlTracker WHERE Id = @id";
            ApplicationContext.Current.DatabaseContext.Database.Execute(query, new object[] { new { Id = id } });

            ReloadForcedRedirectsCache();
        }

        public static void ClearNotFoundEntries()
        {
            LoggingHelper.LogInformation("UrlTracker Repository | Clearing all not found entries");

            var query = "DELETE FROM icUrlTracker WHERE Is404 = 1";
            ApplicationContext.Current.DatabaseContext.Database.Execute(query);
        }

        public static void DeleteNotFoundEntriesByRootAndOldUrl(int redirectRootNodeId, string oldUrl)
        {
            // trigger delete, but without checking if it exists = unneccesary call to database
            LoggingHelper.LogInformation("UrlTracker Repository | Deleting Not Found entries with OldUrl: {0}", oldUrl);

            const string query = "DELETE FROM icUrlTracker WHERE Is404 = 1 AND OldUrl = @oldUrl AND RedirectRootNodeId = @rootId";
            ApplicationContext.Current.DatabaseContext.Database.Execute(
                query,
                new object[] { new { oldUrl = oldUrl }, new { rootId = redirectRootNodeId } });
        }
        #endregion

        #region Get
        public static UrlTrackerModel GetUrlTrackerEntryById(int id)
        {
            var query = "SELECT * FROM icUrlTracker WHERE Id = @id";

            return ApplicationContext.Current.DatabaseContext.Database.FirstOrDefault<UrlTrackerModel>(query, new object[] { new { Id = id } });
        }

        [Obsolete("Remove not found entries also with root id, use other method")]
        public static UrlTrackerModel GetNotFoundEntryByUrl(string url)
        {
            return GetNotFoundEntries().Single(x => x.OldUrl.ToLower() == url.ToLower());
        }

        public static UrlTrackerModel GetNotFoundEntryByRootAndUrl(int redirectRootNodeId, string url)
        {
            return GetNotFoundEntries().Single(x => x.OldUrl.ToLower() == url.ToLower() && x.RedirectRootNodeId == redirectRootNodeId);
        }

        public static List<UrlTrackerModel> GetUrlTrackerEntries(int? maximumRows, int? startRowIndex, string sortExpression = "", bool _404 = false, bool include410Gone = false, bool showAutoEntries = true, bool showCustomEntries = true, bool showRegexEntries = true, string keyword = "", bool onlyForcedRedirects = false)
        {
            var urlTrackerEntries = new List<UrlTrackerModel>();
            var intKeyword = 0;

            var query = "SELECT * FROM icUrlTracker WHERE Is404 = @is404 AND RedirectHttpCode != @redirectHttpCode";
            if (onlyForcedRedirects)
            {
                query = string.Concat(query, " AND ForceRedirect = 1");
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = string.Concat(query, " AND (OldUrl LIKE @keyword OR OldUrlQueryString LIKE @keyword OR OldRegex LIKE @keyword OR RedirectUrl LIKE @keyword OR Notes LIKE @keyword");
                if (int.TryParse(keyword, out intKeyword))
                {
                    query = string.Concat(query, " OR RedirectNodeId = @intKeyword");
                }
                query = string.Concat(query, ")");
            }
            var parameters = new List<object>
                                 {
                                     new { is404 = _404 ? 1 : 0 },
                                     new { redirectHttpCode = include410Gone ? 0 : 410 }
                                 };

            if (!string.IsNullOrEmpty(keyword))
            {
                parameters.Add(new { keyword = "%" + keyword + "%" });
            }
            if (intKeyword != 0)
            {
                parameters.Add(new { intKeyword = intKeyword });
            }

            var data = ApplicationContext.Current.DatabaseContext.Database.Query<UrlTrackerModel>(query, parameters.ToArray());

            foreach (var model in data)
            {
                urlTrackerEntries.Add(model);
            }

            urlTrackerEntries = urlTrackerEntries.Where(x => x.RedirectNodeIsPublished).ToList();

            if (!showAutoEntries || !showCustomEntries || !showRegexEntries || !string.IsNullOrEmpty(keyword))
            {
                IEnumerable<UrlTrackerModel> filteredUrlTrackerEntries = urlTrackerEntries;
                if (!showAutoEntries)
                {
                    filteredUrlTrackerEntries = filteredUrlTrackerEntries.Where(x => x.ViewType != UrlTrackerViewTypes.Auto);
                }
                if (!showCustomEntries)
                {
                    filteredUrlTrackerEntries = filteredUrlTrackerEntries.Where(x => x.ViewType != UrlTrackerViewTypes.Custom || (showRegexEntries ? string.IsNullOrEmpty(x.OldUrl) : false));
                }
                if (!showRegexEntries)
                {
                    filteredUrlTrackerEntries = filteredUrlTrackerEntries.Where(x => !string.IsNullOrEmpty(x.OldUrl));
                }
                //if (!string.IsNullOrEmpty(keyword))
                //{
                //	filteredUrlTrackerEntries = filteredUrlTrackerEntries.Where(x =>
                //		(x.CalculatedOldUrl != null && x.CalculatedOldUrl.ToLower().Contains(keyword)) ||
                //		(x.CalculatedRedirectUrl != null && x.CalculatedRedirectUrl.ToLower().Contains(keyword)) ||
                //		(x.OldRegex != null && x.OldRegex.ToLower().Contains(keyword)) ||
                //		(x.Notes != null && x.Notes.ToLower().Contains(keyword))
                //	);
                //}
                urlTrackerEntries = filteredUrlTrackerEntries.ToList();
            }

            if (!string.IsNullOrEmpty(sortExpression))
            {
                var sortBy = sortExpression;
                var isDescending = false;

                if (sortExpression.ToLowerInvariant().EndsWith(" desc"))
                {
                    sortBy = sortExpression.Substring(0, sortExpression.Length - " desc".Length);
                    isDescending = true;
                }

                switch (sortBy)
                {

                    case "RedirectRootNodeName":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.RedirectRootNodeName) : urlTrackerEntries.OrderBy(x => x.RedirectRootNodeName)).ToList();
                        break;
                    case "CalculatedOldUrl":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.CalculatedOldUrl) : urlTrackerEntries.OrderBy(x => x.CalculatedOldUrl)).ToList();
                        break;
                    case "CalculatedRedirectUrl":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.CalculatedRedirectUrl) : urlTrackerEntries.OrderBy(x => x.CalculatedRedirectUrl)).ToList();
                        break;
                    case "RedirectHttpCode":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.RedirectHttpCode) : urlTrackerEntries.OrderBy(x => x.RedirectHttpCode)).ToList();
                        break;
                    case "Referrer":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.Referrer) : urlTrackerEntries.OrderBy(x => x.Referrer)).ToList();
                        break;
                    case "NotFoundCount":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.NotFoundCount) : urlTrackerEntries.OrderBy(x => x.NotFoundCount)).ToList();
                        break;
                    case "Notes":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.Notes) : urlTrackerEntries.OrderBy(x => x.Notes)).ToList();
                        break;
                    case "Inserted":
                        urlTrackerEntries = (isDescending ? urlTrackerEntries.OrderByDescending(x => x.Inserted) : urlTrackerEntries.OrderBy(x => x.Inserted)).ToList();
                        break;
                }
            }
            if (startRowIndex.HasValue)
            {
                urlTrackerEntries = urlTrackerEntries.Skip(startRowIndex.Value).ToList();
            }
            if (maximumRows.HasValue)
            {
                urlTrackerEntries = urlTrackerEntries.Take(maximumRows.Value).ToList();
            }

            return urlTrackerEntries;
        }

        public static List<UrlTrackerModel> GetNotFoundEntries(int? maximumRows, int? startRowIndex, string sortExpression = "", string keyword = "")
        {
            var notFoundEntries = new List<UrlTrackerModel>();
            var urlTrackerEntries = GetUrlTrackerEntries(maximumRows, startRowIndex, sortExpression, true, keyword: keyword);
            foreach (var notFoundEntry in urlTrackerEntries.GroupBy(x => x.OldUrl).Select(x => new
            {
                Count = x.Count(),
                UrlTrackerModel = x.First(),
                Referrer = x.Select(y => y.Referrer).Any(y => !string.IsNullOrEmpty(y)) ? x.Select(y => y.Referrer).Where(y => !string.IsNullOrEmpty(y)).GroupBy(y => y).OrderByDescending(y => y.Count()).First().Select(z => z).First() : string.Empty,
                Inserted = x.Select(y => y.Inserted).OrderByDescending(y => y).First()
            }
            ))
            {
                notFoundEntry.UrlTrackerModel.NotFoundCount = notFoundEntry.Count;
                if (!notFoundEntry.Referrer.Contains(UrlTrackerSettings.ReferrerToIgnore))
                {
                    notFoundEntry.UrlTrackerModel.Referrer = notFoundEntry.Referrer;
                }
                notFoundEntry.UrlTrackerModel.Inserted = notFoundEntry.Inserted;
                notFoundEntries.Add(notFoundEntry.UrlTrackerModel);
            }

            if (!string.IsNullOrEmpty(sortExpression))
            {
                var sortBy = sortExpression;
                var isDescending = false;

                if (sortExpression.ToLowerInvariant().EndsWith(" desc"))
                {
                    sortBy = sortExpression.Substring(0, sortExpression.Length - " desc".Length);
                    isDescending = true;
                }

                switch (sortBy)
                {
                    case "CalculatedOldUrl":
                        notFoundEntries = (isDescending ? notFoundEntries.OrderByDescending(x => x.CalculatedOldUrl) : notFoundEntries.OrderBy(x => x.CalculatedOldUrl)).ToList();
                        break;
                    case "Referrer":
                        notFoundEntries = (isDescending ? notFoundEntries.OrderByDescending(x => x.Referrer) : notFoundEntries.OrderBy(x => x.Referrer)).ToList();
                        break;
                    case "NotFoundCount":
                        notFoundEntries = (isDescending ? notFoundEntries.OrderByDescending(x => x.NotFoundCount) : notFoundEntries.OrderBy(x => x.NotFoundCount)).ToList();
                        break;
                    case "Inserted":
                        notFoundEntries = (isDescending ? notFoundEntries.OrderByDescending(x => x.Inserted) : notFoundEntries.OrderBy(x => x.Inserted)).ToList();
                        break;
                }
            }
            if (startRowIndex.HasValue)
            {
                notFoundEntries = notFoundEntries.Skip(startRowIndex.Value).ToList();
            }
            if (maximumRows.HasValue)
            {
                notFoundEntries = notFoundEntries.Take(maximumRows.Value).ToList();
            }

            return notFoundEntries;
        }

        public static List<UrlTrackerModel> GetUrlTrackerEntries(string sortExpression = "", bool showAutoEntries = true, bool showCustomEntries = true, bool showRegexEntries = true, string keyword = "")
        {
            return GetUrlTrackerEntries(null, null, sortExpression, showAutoEntries: showAutoEntries, showCustomEntries: showCustomEntries, showRegexEntries: showRegexEntries, keyword: keyword);
        }

        public static List<UrlTrackerModel> GetUrlTrackerEntries(string sortExpression)
        {
            return GetUrlTrackerEntries(null, null, sortExpression);
        }

        public static List<UrlTrackerModel> GetNotFoundEntries(string sortExpression, string keyword = "")
        {
            return GetNotFoundEntries(null, null, sortExpression, keyword);
        }

        public static List<UrlTrackerModel> GetNotFoundEntries(string sortExpression)
        {
            return GetNotFoundEntries(null, null, sortExpression);
        }

        public static List<UrlTrackerModel> GetUrlTrackerEntries()
        {
            return GetUrlTrackerEntries(null, null);
        }

        public static List<UrlTrackerModel> GetNotFoundEntries()
        {
            return GetNotFoundEntries(null, null);
        }

        public static bool HasNotFoundEntries()
        {
            var query = "SELECT 1 FROM icUrlTracker WHERE Is404 = 1";

            var database = ApplicationContext.Current.DatabaseContext.Database;

            return database.ExecuteScalar<int>(query) == 1;
        }
        #endregion

        #region Update
        public static void UpdateUrlTrackerEntry(UrlTrackerModel urlTrackerModel)
        {
            var database = ApplicationContext.Current.DatabaseContext.Database;

            database.Update(urlTrackerModel);

            if (urlTrackerModel.ForceRedirect)
            {
                ReloadForcedRedirectsCache();
            }
        }
        #endregion

        #region Support
        public static bool GetUrlTrackerTableExists()
        {
            var query = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            var database = ApplicationContext.Current.DatabaseContext.Database;

            return database.ExecuteScalar<int>(query, new object[] { new { tableName = UrlTrackerSettings.TableName } }) == 1;
        }

        public static bool GetUrlTrackeOldTableExists()
        {
            var query = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            return ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                                          query,
                                          new object[] { new { tableName = UrlTrackerSettings.OldTableName } }) == 1;
        }

        public static void CreateUrlTrackerTable()
        {
            if (UrlTrackerRepository.GetUrlTrackerTableExists())
            {
                throw new Exception("Table already exists.");
            }

            var folderName = GetFolderName();

            var createTableQuery = EmbeddedResourcesHelper.GetString(string.Concat(folderName, "create-table-1.sql"));
            ApplicationContext.Current.DatabaseContext.Database.Execute(createTableQuery);
        }

        public static void UpdateUrlTrackerTable()
        {
            if (UrlTrackerRepository.GetUrlTrackerTableExists())
            {
                var folderName = GetFolderName();

                for (var i = 1; i <= 3; i++)
                {
                    var alreadyAdded = false;
                    if (DatabaseProvider == DatabaseProviders.SqlServerCE)
                    {
                        //Check if columns exists
                        var query =
                            EmbeddedResourcesHelper.GetString(string.Concat(folderName, "check-table-" + i + ".sql"));
                        if (!string.IsNullOrEmpty(query))
                        {
                            var ex = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int?>(query);
                            alreadyAdded = ex == 1;
                        }

                    }

                    if (!alreadyAdded)
                    {
                        var query =
                            EmbeddedResourcesHelper.GetString(string.Concat(folderName, "update-table-" + i + ".sql"));
                        if (!string.IsNullOrEmpty(query))
                        {
                            ApplicationContext.Current.DatabaseContext.Database.Execute(query);
                        }
                    }
                }
            }
        }

        private static string GetFolderName()
        {
            const string basicFolderName = "InfoCaster.Umbraco.UrlTracker.SQL.";
            var folderName = basicFolderName;
            if (DatabaseProvider == DatabaseProviders.SqlServerCE)
            {
                folderName += "SqlServerCompact.";
            }
            else
            {
                folderName += "MicrosoftSqlServer.";
            }
            return folderName;
        }

        public static int MigrateData()
        {
            if (!GetUrlTrackerTableExists())
            {
                throw new Exception("Url Tracker table not found.");
            }
            if (!GetUrlTrackeOldTableExists())
            {
                throw new Exception("Old Url Tracker table not found.");
            }

            var newUrlTrackerEntriesCount = 0;

            var query = string.Format("SELECT * FROM {0}", UrlTrackerSettings.OldTableName);

            var oldUrlTrackerEntries = ApplicationContext.Current.DatabaseContext.Database.Query<OldUrlTrackerModel>(query).ToList();

            foreach (var oldUrlTrackerEntry in oldUrlTrackerEntries)
            {
                var node = new Node(oldUrlTrackerEntry.NodeId);
                if ((node.Id > 0 || true) && !string.IsNullOrEmpty(oldUrlTrackerEntry.OldUrl) && oldUrlTrackerEntry.OldUrl != "#")
                {
                    var oldUrl = oldUrlTrackerEntry.OldUrl;
                    Uri oldUri = null;
                    if (!oldUrlTrackerEntry.IsRegex)
                    {
                        if (!oldUrl.StartsWith(Uri.UriSchemeHttp))
                        {
                            oldUri = new Uri(_baseUri, oldUrl);
                        }
                        else
                        {
                            oldUri = new Uri(oldUrl);
                        }
                        oldUrl = UrlTrackerHelper.ResolveShortestUrl(oldUri.AbsolutePath);
                    }
                    else
                    {
                        if (oldUrl.StartsWith("^/"))
                        {
                            oldUrl = string.Concat("^", oldUrl.Substring(2));
                        }
                        if (oldUrl.StartsWith("/"))
                        {
                            oldUrl = oldUrl.Substring(1);
                        }
                        if (oldUrl.EndsWith("/$"))
                        {
                            oldUrl = string.Concat(oldUrl.Substring(0, oldUrl.Length - 2), "$");
                        }
                        if (oldUrl.EndsWith("/"))
                        {
                            oldUrl = oldUrl.Substring(0, oldUrl.Length - 1);
                        }
                    }

                    var newUrlTrackerEntry = new UrlTrackerModel(
                        !oldUrlTrackerEntry.IsRegex ? oldUrl : string.Empty,
                        oldUri != null ? !string.IsNullOrEmpty(oldUri.Query) && oldUri.Query.StartsWith("?") ? oldUri.Query.Substring(1) : oldUri.Query : string.Empty,
                        oldUrlTrackerEntry.IsRegex ? oldUrl : string.Empty,
                        node.GetDomainRootNode().Id,
                        oldUrlTrackerEntry.NodeId,
                        string.Empty,
                        301,
                        true,
                        false,
                        oldUrlTrackerEntry.Message);
                    newUrlTrackerEntry.Inserted = oldUrlTrackerEntry.Inserted;

                    AddUrlTrackerEntry(newUrlTrackerEntry);

                    newUrlTrackerEntriesCount++;
                }
            }

            return newUrlTrackerEntriesCount;
        }

        public static bool HasInvalidEntries(out List<int> invalidRowIds)
        {
            invalidRowIds = new List<int>();
            var hasInvalidEntries = false;
            var query = "SELECT Id FROM icUrlTracker WHERE OldUrl IS NULL AND OldRegex IS NULL";

            var data = ApplicationContext.Current.DatabaseContext.Database.Query<UrlTrackerModel>(query);

            foreach (var model in data)
            {
                hasInvalidEntries = true;
                invalidRowIds.Add(model.Id);
            }

            return hasInvalidEntries;
        }
        #endregion

        #region Forced redirects cache
        public static void ReloadForcedRedirectsCache()
        {
            lock (_cacheLock)
            {
                if (GetUrlTrackerTableExists())
                {
                    _forcedRedirectsCache = GetUrlTrackerEntries(null, null, onlyForcedRedirects: true);
                    LastForcedRedirectCacheRefreshTime = DateTime.UtcNow;
                }
            }
        }

        public static List<UrlTrackerModel> GetForcedRedirects()
        {
            if (_forcedRedirectsCache == null)
            {
                ReloadForcedRedirectsCache();
            }
            else if (UrlTrackerSettings.ForcedRedirectCacheTimeoutEnabled
                     && LastForcedRedirectCacheRefreshTime.AddSeconds(UrlTrackerSettings.ForcedRedirectCacheTimeoutSeconds) < DateTime.UtcNow)
            {
                // Allow continued access to the existing cache when one thread is already reloading it
                if (Monitor.TryEnter(_timeoutCacheLock))
                {
                    try
                    {
                        if (LastForcedRedirectCacheRefreshTime.AddSeconds(UrlTrackerSettings.ForcedRedirectCacheTimeoutSeconds) < DateTime.UtcNow)
                        {
                            ReloadForcedRedirectsCache();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_timeoutCacheLock);
                    }
                }
            }
            return _forcedRedirectsCache;
        }
        #endregion
    }
}