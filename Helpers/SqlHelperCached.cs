using System.Security.Cryptography;
using System.Text;
using System.Xml;
using umbraco.DataLayer;
using umbraco.DataLayer.Utility;

namespace InfoCaster.Umbraco.UrlTracker.Helpers
{
    public class SqlHelperCached : ISqlHelper
    {
        private ISqlHelper SqlHelper { get; set; }
        public SqlHelperCached(ISqlHelper sqlHelper)
        {
            SqlHelper = sqlHelper;
        }

        public void Dispose()
        {
            SqlHelper.Dispose();
        }

        public IParameter CreateParameter(string parameterName, object value)
        {
            return SqlHelper.CreateParameter(parameterName, value);
        }

        public string EscapeString(string value)
        {
            return SqlHelper.EscapeString(value);
        }

        public string Concat(params string[] values)
        {
            return SqlHelper.Concat(values);
        }

        public int ExecuteNonQuery(string commandText, params IParameter[] parameters)
        {
            return SqlHelper.ExecuteNonQuery(commandText, parameters);
        }

        public IRecordsReader ExecuteReader(string commandText, params IParameter[] parameters)
        {
            if (UrlTrackerSettings.IsCacheDisabled)
            {
                return SqlHelper.ExecuteReader(commandText, parameters);
            }

            var stringBuilder = new StringBuilder(commandText);

            foreach (var parameter in parameters)
            {
                stringBuilder.Append(parameter.ParameterName);
                stringBuilder.Append(parameter.Value);
            }

            var key = stringBuilder.ToString();

            string hash;

            using (var md5Hash = MD5.Create())
            {
                hash = HashHelper.GetMd5Hash(md5Hash, key);
            }

            var recordReaderCacheEntry = SqlCacheProvider.Instance.GetItem(hash, () => GetRecordReaderCacheEntry(commandText, parameters));

            return new RecordsReader(recordReaderCacheEntry);
        }

        public TScalarType ExecuteScalar<TScalarType>(string commandText, params IParameter[] parameters)
        {
            return SqlHelper.ExecuteScalar<TScalarType>(commandText, parameters);
        }

        public XmlReader ExecuteXmlReader(string commandText, params IParameter[] parameters)
        {
            return SqlHelper.ExecuteXmlReader(commandText, parameters);
        }

        public string ConnectionString
        {
            get { return SqlHelper.ConnectionString; }
        }

        public IUtilitySet Utility
        {
            get { return SqlHelper.Utility; }
        }

        private RecordsReaderCacheEntry GetRecordReaderCacheEntry(string commandText, IParameter[] parameters)
        {
            var recordsReader = SqlHelper.ExecuteReader(commandText, parameters);
            var recordsReaderCacheEntry = RecordsReaderCacheEntry.Create(recordsReader);
            return recordsReaderCacheEntry;
        }
    }
}