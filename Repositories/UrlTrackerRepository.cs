using System;
using System.Collections.Generic;
using System.Linq;
using InfoCaster.Umbraco.UrlTracker.Extensions;
using InfoCaster.Umbraco.UrlTracker.Helpers;
using InfoCaster.Umbraco.UrlTracker.Models;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.NodeFactory;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;

namespace InfoCaster.Umbraco.UrlTracker.Repositories
{
    public static class UrlTrackerRepository
    {
        static ISqlHelper _sqlHelper { get { return Application.SqlHelper; } }
        static readonly Uri _baseUri = new Uri("http://www.example.org");
        static List<UrlTrackerModel> _forcedRedirectsCache;
        static readonly object _cacheLock = new object();
        private static readonly DatabaseProviders DatabaseProvider = ApplicationContext.Current.DatabaseContext.DatabaseProvider;

        #region Add
        public static bool AddUrlMapping(IContent content, int rootNodeId, string url, AutoTrackingTypes type, bool isChild = false)
        {
            if (url != "#" && content.Template != null && content.Template.Id > 0)
            {
                string notes = isChild ? "An ancestor" : "This page";
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
                }

                url = UrlTrackerHelper.ResolveShortestUrl(url);

                if (!string.IsNullOrEmpty(url))
                {
                    string query = "SELECT 1 FROM icUrlTracker WHERE RedirectNodeId = @nodeId AND OldUrl = @url";
                    int exists = _sqlHelper.ExecuteScalar<int>(query, _sqlHelper.CreateParameter("nodeId", content.Id), _sqlHelper.CreateStringParameter("url", url));

                    if (exists != 1)
                    {
                        LoggingHelper.LogInformation("UrlTracker Repository | Adding mapping for node id: {0} and url: {1}", new string[] { content.Id.ToString(), url });

                        query = "INSERT INTO icUrlTracker (RedirectRootNodeId, RedirectNodeId, OldUrl, Notes) VALUES (@rootNodeId, @nodeId, @url, @notes)";
                        _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateParameter("rootNodeId", rootNodeId), _sqlHelper.CreateParameter("nodeId", content.Id), _sqlHelper.CreateStringParameter("url", url), _sqlHelper.CreateStringParameter("notes", notes));

                        if (content.Children().Any())
                        {
                            foreach (IContent child in content.Children())
                            {
                                Node node = new Node(child.Id);
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
            string query = "INSERT INTO icUrlTracker (OldUrl, OldUrlQueryString, OldRegex, RedirectRootNodeId, RedirectNodeId, RedirectUrl, RedirectHttpCode, RedirectPassThroughQueryString, ForceRedirect, Notes) VALUES (@oldUrl, @oldUrlQueryString, @oldRegex, @redirectRootNodeId, @redirectNodeId, @redirectUrl, @redirectHttpCode, @redirectPassThroughQueryString, @forceRedirect, @notes)";
            _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateStringParameter("oldUrl", urlTrackerModel.OldUrl), _sqlHelper.CreateStringParameter("oldUrlQueryString", urlTrackerModel.OldUrlQueryString), _sqlHelper.CreateStringParameter("oldRegex", urlTrackerModel.OldRegex), _sqlHelper.CreateParameter("redirectRootNodeId", urlTrackerModel.RedirectRootNodeId), _sqlHelper.CreateNullableParameter("redirectNodeId", urlTrackerModel.RedirectNodeId), _sqlHelper.CreateStringParameter("redirectUrl", urlTrackerModel.RedirectUrl), _sqlHelper.CreateParameter("redirectHttpCode", urlTrackerModel.RedirectHttpCode), _sqlHelper.CreateParameter("redirectPassThroughQueryString", urlTrackerModel.RedirectPassThroughQueryString), _sqlHelper.CreateParameter("forceRedirect", urlTrackerModel.ForceRedirect), _sqlHelper.CreateStringParameter("notes", urlTrackerModel.Notes));

            if (urlTrackerModel.ForceRedirect)
                ReloadForcedRedirectsCache();
        }

        public static void AddGoneEntryByNodeId(int nodeId)
        {
            string url = umbraco.library.NiceUrl(nodeId);
            if (url == "#")
                return;

            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                Uri uri = new Uri(url);
                url = uri.AbsolutePath;
            }
            url = UrlTrackerHelper.ResolveShortestUrl(url);

            string query = "SELECT 1 FROM icUrlTracker WHERE RedirectNodeId = @redirectNodeId AND OldUrl = @oldUrl AND RedirectHttpCode = 410";
            int exists = _sqlHelper.ExecuteScalar<int>(query, _sqlHelper.CreateParameter("redirectNodeId", nodeId), _sqlHelper.CreateStringParameter("oldUrl", url));
            if (exists != 1)
            {
                LoggingHelper.LogInformation("UrlTracker Repository | Inserting 410 Gone mapping for node with id: {0}", nodeId);

                query = "INSERT INTO icUrlTracker (RedirectNodeId, OldUrl, RedirectHttpCode, Notes) VALUES (@redirectNodeId, @oldUrl, 410, @notes)";
                _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateParameter("redirectNodeId", nodeId), _sqlHelper.CreateStringParameter("oldUrl", url), _sqlHelper.CreateStringParameter("notes", "Node removed"));
            }
            else
                LoggingHelper.LogInformation("UrlTracker Repository | Skipping 410 Gone mapping for node with id: {0} (already exists)", nodeId);
        }
        #endregion

        #region Delete
        public static void DeleteUrlTrackerEntriesByNodeId(int nodeId)
        {
            string query = "SELECT 1 FROM icUrlTracker WHERE RedirectNodeId = @nodeId AND RedirectHttpCode != 410";
            int exists = _sqlHelper.ExecuteScalar<int>(query, _sqlHelper.CreateParameter("nodeId", nodeId));
            if (exists == 1)
            {
                LoggingHelper.LogInformation("UrlTracker Repository | Deleting Url Tracker entry of node with id: {0}", nodeId);

                query = "DELETE FROM icUrlTracker WHERE RedirectNodeId = @nodeId AND RedirectHttpCode != 410";
                _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateParameter("nodeId", nodeId));
            }

            ReloadForcedRedirectsCache();
        }

        public static void DeleteNotFoundEntriesByOldUrl(string oldUrl)
        {
            string query = "SELECT 1 FROM icUrlTracker WHERE Is404 = 1 AND OldUrl = @oldUrl";
            int exists = _sqlHelper.ExecuteScalar<int>(query, _sqlHelper.CreateParameter("oldUrl", oldUrl));
            if (exists == 1)
            {
                LoggingHelper.LogInformation("UrlTracker Repository | Deleting Not Found entries with OldUrl: {0}", oldUrl);

                query = "DELETE FROM icUrlTracker WHERE Is404 = 1 AND OldUrl = @oldUrl";
                _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateParameter("oldUrl", oldUrl));
            }
        }

        public static void DeleteUrlTrackerEntry(int id)
        {
            LoggingHelper.LogInformation("UrlTracker Repository | Deleting Url Tracker entry with id: {0}", id);

            string query = "DELETE FROM icUrlTracker WHERE Id = @id";
            _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateParameter("id", id));

            ReloadForcedRedirectsCache();
        }

        public static void ClearNotFoundEntries()
        {
            LoggingHelper.LogInformation("UrlTracker Repository | Clearing all not found entries");

            string query = "DELETE FROM icUrlTracker WHERE Is404 = 1";
            _sqlHelper.ExecuteNonQuery(query);
        }

        public static void DeleteNotFoundEntriesByRootAndOldUrl(int redirectRootNodeId, string oldUrl)
        {
            // trigger delete, but without checking if it exists = unneccesary call to database
            LoggingHelper.LogInformation("UrlTracker Repository | Deleting Not Found entries with OldUrl: {0}", oldUrl);

            const string query = "DELETE FROM icUrlTracker WHERE Is404 = 1 AND OldUrl = @oldUrl AND RedirectRootNodeId = @rootId";
            _sqlHelper.ExecuteNonQuery(query,
                _sqlHelper.CreateParameter("oldUrl", oldUrl),
                _sqlHelper.CreateParameter("rootId", redirectRootNodeId));
        }
        #endregion

        #region Get
        public static UrlTrackerModel GetUrlTrackerEntryById(int id)
        {
            string query = "SELECT * FROM icUrlTracker WHERE Id = @id";
            using (IRecordsReader reader = _sqlHelper.ExecuteReader(query, _sqlHelper.CreateParameter("id", id)))
            {
                if (reader.Read())
                {
                    return new UrlTrackerModel(reader.GetInt("Id"), reader.GetString("OldUrl"), reader.GetString("OldUrlQueryString"), reader.GetString("OldRegex"), reader.GetInt("RedirectRootNodeId"), reader.Get<int?>("RedirectNodeId"), reader.GetString("RedirectUrl"), reader.GetInt("RedirectHttpCode"), reader.GetBoolean("RedirectPassThroughQueryString"), reader.GetBoolean("ForceRedirect"), reader.GetString("Notes"), reader.GetBoolean("Is404"), reader.GetString("Referrer"), reader.GetDateTime("Inserted"));
                }
            }
            return null;
        }

        [Obsolete("Remove not found entries also with root id, use other method")]
        public static UrlTrackerModel GetNotFoundEntryByUrl(string url)
        {
            return GetNotFoundEntries().Single(x => x.OldUrl == url);
        }

        public static UrlTrackerModel GetNotFoundEntryByRootAndUrl(int redirectRootNodeId, string url)
        {
            return GetNotFoundEntries().Single(x => x.OldUrl == url && x.RedirectRootNodeId == redirectRootNodeId);
        }

        public static List<UrlTrackerModel> GetUrlTrackerEntries(int? maximumRows, int? startRowIndex, string sortExpression = "", bool _404 = false, bool include410Gone = false, bool showAutoEntries = true, bool showCustomEntries = true, bool showRegexEntries = true, string keyword = "", bool onlyForcedRedirects = false)
        {
            List<UrlTrackerModel> urlTrackerEntries = new List<UrlTrackerModel>();
            int intKeyword = 0;

            string query = "SELECT * FROM icUrlTracker WHERE Is404 = @is404 AND RedirectHttpCode != @redirectHttpCode";
            if (onlyForcedRedirects)
                query = string.Concat(query, " AND ForceRedirect = 1");

            if (!string.IsNullOrEmpty(keyword))
            {
                query = string.Concat(query, " AND (OldUrl LIKE @keyword OR OldUrlQueryString LIKE @keyword OR OldRegex LIKE @keyword OR RedirectUrl LIKE @keyword OR Notes LIKE @keyword");
                if (int.TryParse(keyword, out intKeyword))
                    query = string.Concat(query, " OR RedirectNodeId = @intKeyword");
                query = string.Concat(query, ")");
            }
            List<IParameter> parameters = new List<IParameter>
            {
                _sqlHelper.CreateParameter("is404", _404 ? 1 : 0),
                _sqlHelper.CreateParameter("redirectHttpCode", include410Gone ? 0 : 410)
            };
            if (!string.IsNullOrEmpty(keyword))
                parameters.Add(_sqlHelper.CreateParameter("keyword", "%" + keyword + "%"));
            if (intKeyword != 0)
                parameters.Add(_sqlHelper.CreateParameter("intKeyword", intKeyword));
            using (IRecordsReader reader = _sqlHelper.ExecuteReader(query, parameters.ToArray()))
            {
                while (reader.Read())
                {
                    urlTrackerEntries.Add(new UrlTrackerModel(reader.GetInt("Id"), reader.GetString("OldUrl"), reader.GetString("OldUrlQueryString"), reader.GetString("OldRegex"), reader.GetInt("RedirectRootNodeId"), reader.Get<int?>("RedirectNodeId"), reader.GetString("RedirectUrl"), reader.GetInt("RedirectHttpCode"), reader.GetBoolean("RedirectPassThroughQueryString"), reader.GetBoolean("ForceRedirect"), reader.GetString("Notes"), reader.GetBoolean("Is404"), reader.GetString("Referrer"), reader.GetDateTime("Inserted")));
                }
            }

            urlTrackerEntries = urlTrackerEntries.Where(x => x.RedirectNodeIsPublished).ToList();

            if (!showAutoEntries || !showCustomEntries || !showRegexEntries || !string.IsNullOrEmpty(keyword))
            {
                IEnumerable<UrlTrackerModel> filteredUrlTrackerEntries = urlTrackerEntries;
                if (!showAutoEntries)
                    filteredUrlTrackerEntries = filteredUrlTrackerEntries.Where(x => x.ViewType != UrlTrackerViewTypes.Auto);
                if (!showCustomEntries)
                    filteredUrlTrackerEntries = filteredUrlTrackerEntries.Where(x => x.ViewType != UrlTrackerViewTypes.Custom || (showRegexEntries ? string.IsNullOrEmpty(x.OldUrl) : false));
                if (!showRegexEntries)
                    filteredUrlTrackerEntries = filteredUrlTrackerEntries.Where(x => !string.IsNullOrEmpty(x.OldUrl));
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
                string sortBy = sortExpression;
                bool isDescending = false;

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
                urlTrackerEntries = urlTrackerEntries.Skip(startRowIndex.Value).ToList();
            if (maximumRows.HasValue)
                urlTrackerEntries = urlTrackerEntries.Take(maximumRows.Value).ToList();

            return urlTrackerEntries;
        }

        public static List<UrlTrackerModel> GetNotFoundEntries(int? maximumRows, int? startRowIndex, string sortExpression = "", string keyword = "")
        {
            List<UrlTrackerModel> notFoundEntries = new List<UrlTrackerModel>();
            List<UrlTrackerModel> urlTrackerEntries = GetUrlTrackerEntries(maximumRows, startRowIndex, sortExpression, true);
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
                    notFoundEntry.UrlTrackerModel.Referrer = notFoundEntry.Referrer;
                notFoundEntry.UrlTrackerModel.Inserted = notFoundEntry.Inserted;
                notFoundEntries.Add(notFoundEntry.UrlTrackerModel);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                IEnumerable<UrlTrackerModel> filteredNotFoundEntries = notFoundEntries;
                if (!string.IsNullOrEmpty(keyword))
                {
                    filteredNotFoundEntries = filteredNotFoundEntries.Where(x =>
                        (x.CalculatedOldUrl != null && x.CalculatedOldUrl.ToLower().Contains(keyword)) ||
                        (x.Referrer != null && x.Referrer.ToLower().Contains(keyword))
                    );
                }
                notFoundEntries = filteredNotFoundEntries.ToList();
            }

            if (!string.IsNullOrEmpty(sortExpression))
            {
                string sortBy = sortExpression;
                bool isDescending = false;

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
                notFoundEntries = notFoundEntries.Skip(startRowIndex.Value).ToList();
            if (maximumRows.HasValue)
                notFoundEntries = notFoundEntries.Take(maximumRows.Value).ToList();

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
            string query = "SELECT 1 FROM icUrlTracker WHERE Is404 = 1";
            return _sqlHelper.ExecuteScalar<int>(query) == 1;
        }
        #endregion

        #region Update
        public static void UpdateUrlTrackerEntry(UrlTrackerModel urlTrackerModel)
        {
            string query = "UPDATE icUrlTracker SET OldUrl = @oldUrl, OldUrlQueryString = @oldUrlQueryString, OldRegex = @oldRegex, RedirectRootNodeId = @redirectRootNodeId, RedirectNodeId = @redirectNodeId, RedirectUrl = @redirectUrl, RedirectHttpCode = @redirectHttpCode, RedirectPassThroughQueryString = @redirectPassThroughQueryString, ForceRedirect = @forceRedirect, Notes = @notes, Is404 = @is404 WHERE Id = @id";
            _sqlHelper.ExecuteNonQuery(query, _sqlHelper.CreateStringParameter("oldUrl", urlTrackerModel.OldUrl), _sqlHelper.CreateStringParameter("oldUrlQueryString", urlTrackerModel.OldUrlQueryString), _sqlHelper.CreateStringParameter("oldRegex", urlTrackerModel.OldRegex), _sqlHelper.CreateParameter("redirectRootNodeId", urlTrackerModel.RedirectRootNodeId), _sqlHelper.CreateNullableParameter<int?>("redirectNodeId", urlTrackerModel.RedirectNodeId), _sqlHelper.CreateStringParameter("redirectUrl", urlTrackerModel.RedirectUrl), _sqlHelper.CreateParameter("redirectHttpCode", urlTrackerModel.RedirectHttpCode), _sqlHelper.CreateParameter("redirectPassThroughQueryString", urlTrackerModel.RedirectPassThroughQueryString), _sqlHelper.CreateParameter("forceRedirect", urlTrackerModel.ForceRedirect), _sqlHelper.CreateStringParameter("notes", urlTrackerModel.Notes), _sqlHelper.CreateParameter("is404", urlTrackerModel.Is404), _sqlHelper.CreateParameter("id", urlTrackerModel.Id));

            if (urlTrackerModel.ForceRedirect)
                ReloadForcedRedirectsCache();
        }
        #endregion
        
        #region Support
        public static bool GetUrlTrackerTableExists()
        {
            string query = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            return _sqlHelper.ExecuteScalar<int>(query, _sqlHelper.CreateParameter("tableName", UrlTrackerSettings.TableName)) == 1;
        }

        public static bool GetUrlTrackeOldTableExists()
        {
            string query = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
            return _sqlHelper.ExecuteScalar<int>(query, _sqlHelper.CreateParameter("tableName", UrlTrackerSettings.OldTableName)) == 1;
        }

        public static void CreateUrlTrackerTable()
        {
            if (UrlTrackerRepository.GetUrlTrackerTableExists())
                throw new Exception("Table already exists.");

            var folderName = GetFolderName();

            var createTableQuery = EmbeddedResourcesHelper.GetString(string.Concat(folderName, "create-table-1.sql"));
            _sqlHelper.ExecuteNonQuery(createTableQuery);
            createTableQuery = EmbeddedResourcesHelper.GetString(string.Concat(folderName, "create-table-2.sql"));
            _sqlHelper.ExecuteNonQuery(createTableQuery);
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
                            var reader = _sqlHelper.ExecuteReader(query);
                            while (reader.Read())
                            {
                                alreadyAdded = true;
                            }
                        }

                    }

                    if (!alreadyAdded)
                    {
                        var query =
                            EmbeddedResourcesHelper.GetString(string.Concat(folderName, "update-table-" + i + ".sql"));
                        if (!string.IsNullOrEmpty(query))
                            _sqlHelper.ExecuteNonQuery(query);
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
                throw new Exception("Url Tracker table not found.");
            if (!GetUrlTrackeOldTableExists())
                throw new Exception("Old Url Tracker table not found.");

            int newUrlTrackerEntriesCount = 0;
            List<OldUrlTrackerModel> oldUrlTrackerEntries = new List<OldUrlTrackerModel>();
            string query = string.Format("SELECT * FROM {0}", UrlTrackerSettings.OldTableName);
            IRecordsReader recordsReader = _sqlHelper.ExecuteReader(query);
            while(recordsReader.Read())
            {
                oldUrlTrackerEntries.Add(new OldUrlTrackerModel()
                {
                    NodeId = recordsReader.GetInt("NodeID"),
                    OldUrl = recordsReader.GetString("OldUrl"),
                    IsCustom = recordsReader.GetBoolean("IsCustom"),
                    Message = recordsReader.GetString("Message"),
                    Inserted = recordsReader.GetDateTime("Inserted"),
                    IsRegex = recordsReader.GetBoolean("IsRegex")
                });
            }

            foreach (OldUrlTrackerModel oldUrlTrackerEntry in oldUrlTrackerEntries)
            {
                Node node = new Node(oldUrlTrackerEntry.NodeId);
                if ((node.Id > 0 || true) && !string.IsNullOrEmpty(oldUrlTrackerEntry.OldUrl) && oldUrlTrackerEntry.OldUrl != "#")
                {
                    string oldUrl = oldUrlTrackerEntry.OldUrl;
                    Uri oldUri = null;
                    if (!oldUrlTrackerEntry.IsRegex)
                    {
                        if (!oldUrl.StartsWith(Uri.UriSchemeHttp))
                            oldUri = new Uri(_baseUri, oldUrl);
                        else
                            oldUri = new Uri(oldUrl);
                        oldUrl = UrlTrackerHelper.ResolveShortestUrl(oldUri.AbsolutePath);
                    }
                    else
                    {
                        if (oldUrl.StartsWith("^/"))
                            oldUrl = string.Concat("^", oldUrl.Substring(2));
                        if (oldUrl.StartsWith("/"))
                            oldUrl = oldUrl.Substring(1);
                        if (oldUrl.EndsWith("/$"))
                            oldUrl = string.Concat(oldUrl.Substring(0, oldUrl.Length - 2), "$");
                        if (oldUrl.EndsWith("/"))
                            oldUrl = oldUrl.Substring(0, oldUrl.Length - 1);
                    }

                    UrlTrackerModel newUrlTrackerEntry = new UrlTrackerModel(
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
            bool hasInvalidEntries = false;
            string query = "SELECT Id FROM icUrlTracker WHERE OldUrl IS NULL AND OldRegex IS NULL";
            using (IRecordsReader reader = _sqlHelper.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    hasInvalidEntries = true;
                    invalidRowIds.Add(reader.GetInt("Id"));
                }
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
                    _forcedRedirectsCache = GetUrlTrackerEntries(null, null, onlyForcedRedirects: true);
            }
        }

        public static List<UrlTrackerModel> GetForcedRedirects()
        {
            if (_forcedRedirectsCache == null)
                ReloadForcedRedirectsCache();
            return _forcedRedirectsCache;
        }
        #endregion
    }
}