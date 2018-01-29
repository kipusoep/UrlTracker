using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("InfoCaster UrlTracker")]
[assembly: AssemblyDescription("UrlTracker for Umbraco")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("InfoCaster")]
[assembly: AssemblyProduct("UrlTracker")]
[assembly: AssemblyCopyright("© InfoCaster")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("F0A031A5-3DC0-4B41-8029-7D45C4F47F1E")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("3.15.*")]
[assembly: AssemblyInformationalVersion("3.15.0")]

// SQL
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.MicrosoftSqlServer.create-table-1.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.MicrosoftSqlServer.update-table-1.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.MicrosoftSqlServer.update-table-2.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.MicrosoftSqlServer.update-table-3.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.SqlServerCompact.create-table-1.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.SqlServerCompact.check-table-1.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.SqlServerCompact.check-table-2.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.SqlServerCompact.update-table-1.sql", "text/plain")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.SQL.SqlServerCompact.update-table-2.sql", "text/plain")]

// CSS
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.css.bootstrap.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.css.bootstrap.min.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.css.installer.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.css.info.css", "text/css", PerformSubstitution = true)]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.css.urltracker.css", "text/css", PerformSubstitution = true)]

// IMG
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.asc-hover.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.asc.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.cross.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.desc-hover.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.desc.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.edit.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.glyphicons-halflings-white.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.glyphicons-halflings.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.info.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.trash.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.urltracker-icon.png", "image/png")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.img.urltracker.png", "image/png")]

// JS
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.bootstrap.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.bootstrap.min.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.installer.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-1.11.2.min.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery-migrate-1.2.1.min.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.additional-methods.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.additional-methods.min.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.jquery.validate.min.js", "text/javascript")]
[assembly: WebResource("InfoCaster.Umbraco.UrlTracker.UI.res.js.main.js", "text/javascript")]