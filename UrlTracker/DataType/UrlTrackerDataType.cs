using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;
using umbraco.interfaces;

namespace InfoCaster.Umbraco.UrlTracker.DataType
{
	public class UrlTrackerDataType : BaseDataType, IDataType
	{
		IDataEditor _editor;
		IData _baseData;
		UrlTrackerPrevalueEditor _prevalueEditor;

		public override IData Data
		{
			get
			{
				if (_baseData == null)
					_baseData = new DefaultData(this);
				return _baseData;
			}
		}

		public override IDataEditor DataEditor
		{
			get
			{
				if (_editor == null)
					_editor = new UrlTrackerDataEditor();
				return _editor;
			}
		}

		public override string DataTypeName
		{
			get { return "Url Tracker"; }
		}

		public override Guid Id
		{
			get { return new Guid("{E1B99FA8-1092-4E46-BB15-135FDC6B4840}"); }
		}

		public override IDataPrevalue PrevalueEditor
		{
			get
			{
				if (_prevalueEditor == null)
					_prevalueEditor = new UrlTrackerPrevalueEditor(this);
				return _prevalueEditor;
			}
		}
	}
}