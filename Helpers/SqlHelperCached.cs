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
            return SqlHelper.ExecuteReader(commandText, parameters);
        }

        public TScalarType ExecuteScalar<TScalarType>(string commandText, params IParameter[] parameters)
        {
            return SqlHelper.ExecuteScalar<TScalarType>(commandText, parameters);
        }

        public XmlReader ExecuteXmlReader(string commandText, params IParameter[] parameters)
        {
            return ExecuteXmlReader(commandText, parameters);
        }

        public string ConnectionString
        {
            get { return SqlHelper.ConnectionString; }
        }

        public IUtilitySet Utility
        {
            get { return SqlHelper.Utility; }
        }
    }
}