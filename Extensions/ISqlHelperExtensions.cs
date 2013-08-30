using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.DataLayer;

namespace InfoCaster.Umbraco.UrlTracker.Extensions
{
	public static class ISqlHelperExtensions
	{
		public static IParameter CreateStringParameter(this ISqlHelper sqlHelper, string parameterName, string value)
		{
			return sqlHelper.CreateParameter(parameterName, string.IsNullOrEmpty(value) ? DBNull.Value : (object)value);
		}

		public static IParameter CreateNullableParameter<T>(this ISqlHelper sqlHelper, string parameterName, T value)
		{
			return sqlHelper.CreateParameter(parameterName, value == null ? DBNull.Value : (object)value);
		}
	}
}