using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
	public class OldUrlTrackerModel
	{
		public int NodeId { get;set;}
		public string OldUrl { get;set;}
		public bool IsCustom { get;set;}
		public string Message { get;set;}
		public DateTime Inserted { get;set;}
		public bool IsRegex { get; set; }
	}
}