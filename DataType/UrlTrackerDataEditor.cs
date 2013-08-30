using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using umbraco.interfaces;

namespace InfoCaster.Umbraco.UrlTracker.DataType
{
	public class UrlTrackerDataEditor : UpdatePanel, IDataEditor
	{
		public Control Editor
		{
			get { return this; }
		}

		public void Save()
		{
		}

		public bool ShowLabel
		{
			get { return true; }
		}

		public bool TreatAsRichTextEditor
		{
			get { return false; }
		}
	}
}