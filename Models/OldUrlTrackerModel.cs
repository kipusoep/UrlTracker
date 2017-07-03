using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfoCaster.Umbraco.UrlTracker.Models
{
    using global::Umbraco.Core.Persistence;

    [TableName("infocaster301")]
    public class OldUrlTrackerModel
    {
        [Column("NodeId")]
        public int NodeId { get; set; }

        [Column("OldUrl")]
        public string OldUrl { get; set; }

        [Column("IsCustom")]
        public bool IsCustom { get; set; }

        [Column("Message")]
        public string Message { get; set; }

        [Column("Inserted")]
        public DateTime Inserted { get; set; }

        [Column("IsRegex")]
        public bool IsRegex { get; set; }
    }
}