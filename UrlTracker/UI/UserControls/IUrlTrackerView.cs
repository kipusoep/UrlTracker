using InfoCaster.Umbraco.UrlTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfoCaster.Umbraco.UrlTracker.UI.UserControls
{
	interface IUrlTrackerView
	{
		UrlTrackerModel UrlTrackerModel { get; set; }
		void LoadView();
		void Save();
	}
}
