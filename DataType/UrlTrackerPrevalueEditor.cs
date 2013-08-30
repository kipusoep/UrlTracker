using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace InfoCaster.Umbraco.UrlTracker.DataType
{
	public class UrlTrackerPrevalueEditor : PlaceHolder, IDataPrevalue
	{
		BaseDataType _datatype;

		public UrlTrackerPrevalueEditor(BaseDataType dataType)
		{
			_datatype = dataType;
		}

		public System.Web.UI.Control Editor
		{
			get { return this; }
		}

		public void Save()
		{
		}
	}
}