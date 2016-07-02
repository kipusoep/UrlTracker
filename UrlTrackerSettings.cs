using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using umbraco;

namespace InfoCaster.Umbraco.UrlTracker
{
    public static class UrlTrackerSettings
    {
        public const string TableName = "icUrlTracker";
        public const string OldTableName = "infocaster301";
        public const string HttpModuleCheck = "890B748D-E226-49FA-A0D7-9AFD3A359F88";
        public static bool SEOMetadataInstalled
        {
            get
            {
                if (!_seoMetadataInstalled.HasValue)
                {
                    _seoMetadataInstalled = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.Contains("Epiphany.SeoMetadata"));
                }
                return _seoMetadataInstalled.Value;
            }
        }
        public static string SEOMetadataPropertyName
        {
            get
            {
                return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SeoMetadata.PropertyName"]) ? ConfigurationManager.AppSettings["SeoMetadata.PropertyName"] : "metadata";
            }
        }

        static bool? _seoMetadataInstalled;

        /// <summary>
        /// Returns wether or not the UrlTracker is disabled
        /// </summary>
        /// <remarks>
        /// appSetting: 'urlTracker:disabled'
        /// </remarks>
        public static bool IsDisabled
        {
            get
            {
                if (!_isDisabled.HasValue)
                {
                    bool isDisabled = false;
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:disabled"]))
                    {
                        bool parsedAppSetting;
                        if (bool.TryParse(ConfigurationManager.AppSettings["urlTracker:disabled"], out parsedAppSetting))
                            isDisabled = parsedAppSetting;
                    }
                    _isDisabled = isDisabled;
                }
                return _isDisabled.Value;
            }
        }
        /// <summary>
        /// Returns wether or not logging for the UrlTracker is enabled
        /// </summary>
        /// <remarks>
        /// appSettings: 'urlTracker:enableLogging' & 'umbracoDebugMode'
        /// </remarks>
        public static bool EnableLogging
        {
            get
            {
                if (!_enableLogging.HasValue)
                {
                    bool enableLogging = false;
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:enableLogging"]))
                    {
                        bool parsedAppSetting;
                        if (bool.TryParse(ConfigurationManager.AppSettings["urlTracker:enableLogging"], out parsedAppSetting))
                            enableLogging = parsedAppSetting;
                    }
                    _enableLogging = enableLogging;
                }
                return _enableLogging.Value && GlobalSettings.DebugMode;
            }
        }
        /// <summary>
        /// Returns the URLs to ignore for 404 Not Found logging
        /// </summary>
        /// <remarks>
        /// appSetting: 'urlTracker:404UrlsToIgnore'
        /// </remarks>
        public static string[] NotFoundUrlsToIgnore
        {
            get
            {
                if (_notFoundUrlsToIgnore == null)
                {
                    _notFoundUrlsToIgnore = new string[] { "favicon.ico" };
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:404UrlsToIgnore"]))
                        _notFoundUrlsToIgnore = _notFoundUrlsToIgnore.Union(ConfigurationManager.AppSettings["urlTracker:404UrlsToIgnore"].Split(',')).ToArray();
                }
                return _notFoundUrlsToIgnore;
            }
        }
        /// <summary>
        /// Returns the regex patterns for NotFound urls to ignore
        /// </summary>
        public static Regex[] RegexNotFoundUrlsToIgnore
        {
            get
            {
                return new Regex[] { new Regex("__browserLink/requestData/.*", RegexOptions.Compiled), new Regex("[^/]/arterySignalR/ping", RegexOptions.Compiled) };
            }
        }
        /// <summary>
        /// Returns the UrlTracker UI URL to ignore as referrer
        /// </summary>
        public static string ReferrerToIgnore
        {
            get { return "Umbraco/UrlTracker/InfoCaster.Umbraco.UrlTracker.UI"; }
        }
        /// <summary>
        /// Returns wether or not tracking URL changes is disabled
        /// </summary>
        /// <remarks>
        /// appSetting: 'urlTracker:trackingDisabled'
        /// </remarks>
        public static bool IsTrackingDisabled
        {
            get
            {
                if (!_isTrackingDisabled.HasValue)
                {
                    bool isTrackingDisabled = false;
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:trackingDisabled"]))
                    {
                        bool parsedAppSetting;
                        if (bool.TryParse(ConfigurationManager.AppSettings["urlTracker:trackingDisabled"], out parsedAppSetting))
                            isTrackingDisabled = parsedAppSetting;
                    }
                    _isTrackingDisabled = isTrackingDisabled;
                }
                return _isTrackingDisabled.Value;
            }
        }
        /// <summary>
        /// Returns wether or not not found (404) tracking is disabled
        /// </summary>
        /// <remarks>
        /// appSetting: 'urlTracker:notFoundTrackingDisabled'
        /// </remarks>
        public static bool IsNotFoundTrackingDisabled
        {
            get
            {
                if (!_isNotFoundTrackingDisabled.HasValue)
                {
                    bool isNotFoundTrackingDisabled = false;
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:notFoundTrackingDisabled"]))
                    {
                        bool parsedAppSetting;
                        if (bool.TryParse(ConfigurationManager.AppSettings["urlTracker:notFoundTrackingDisabled"], out parsedAppSetting))
                            isNotFoundTrackingDisabled = parsedAppSetting;
                    }
                    _isNotFoundTrackingDisabled = isNotFoundTrackingDisabled;
                }
                return _isNotFoundTrackingDisabled.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not to append port numbers to URLs. Default is true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we are to append the port number; otherwise, <c>false</c>.
        /// </value>
        public static bool AppendPortNumber
        {
            get
            {
                if (!_appendPortNumber.HasValue)
                {
                    bool appendPortNumber = true;
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:appendPortNumber"]))
                    {
                        bool parsedAppSetting;
                        if (bool.TryParse(ConfigurationManager.AppSettings["urlTracker:appendPortNumber"], out parsedAppSetting))
                            appendPortNumber = parsedAppSetting;
                    }
                    _appendPortNumber = appendPortNumber;
                }
                return _appendPortNumber.Value;
            }
        }

        /// <summary>
        /// Returns wether or not a child node has a domain configured
        /// </summary>
        /// <remarks>
        /// appSetting: 'urlTracker:hasDomainOnChildNode'
        /// </remarks>
        public static bool HasDomainOnChildNode
        {
            get
            {
                if (!_hasDomainOnChildNode.HasValue)
                {
                    var hasDomainOnChildNode = false;
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:hasDomainOnChildNode"]))
                    {
                        bool parsedAppSetting;
                        if (bool.TryParse(ConfigurationManager.AppSettings["urlTracker:hasDomainOnChildNode"], out parsedAppSetting))
                            hasDomainOnChildNode = parsedAppSetting;
                    }
                    _hasDomainOnChildNode = hasDomainOnChildNode;
                }
                return _hasDomainOnChildNode.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not forced redirects should be cached for a period of time. Default is false.
        /// Setting this to true will enabled forced redirect updates and additions to propagate to all servers in a multi-server environment
        /// </summary>
        /// <value>
        ///   <c>true</c> if we are to cache forced redirects for a period of time; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// appSetting: 'urlTracker:forcedRedirectCacheTimeoutEnabled'
        /// </remarks>
        public static bool ForcedRedirectCacheTimeoutEnabled
        {
            get
            {
                if (!_forcedRedirectCacheTimeoutEnabled.HasValue)
                {
                    bool forcedRedirectCacheTimeoutEnabled = false;
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:forcedRedirectCacheTimeoutEnabled"]))
                    {
                        bool parsedAppSetting;
                        if (bool.TryParse(ConfigurationManager.AppSettings["urlTracker:forcedRedirectCacheTimeoutEnabled"], out parsedAppSetting))
                            forcedRedirectCacheTimeoutEnabled = parsedAppSetting;
                    }
                    _forcedRedirectCacheTimeoutEnabled = forcedRedirectCacheTimeoutEnabled;
                }
                return _forcedRedirectCacheTimeoutEnabled.Value;
            }
        }

        /// <summary>
        /// Amount of time, in seconds, that the forced redirects will be cached for. Default is 14400 (4 hours).
        /// The default value will be used when the app setting is less than 1.
        /// This setting does nothing unless urlTracker:forcedRedirectCacheTimeoutEnabled is true
        /// </summary>
        /// <remarks>
        /// appSetting: 'urlTracker:forcedRedirectCacheTimeoutSeconds'
        /// </remarks>
        public static int ForcedRedirectCacheTimeoutSeconds
        {
            get
            {
                if (!_forcedRedirectCacheTimeoutSeconds.HasValue)
                {
                    int forcedRedirectCacheTimeoutSeconds = 14400; // 4 hours
                    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["urlTracker:forcedRedirectCacheTimeoutSeconds"]))
                    {
                        int parsedAppSetting;
                        if (int.TryParse(ConfigurationManager.AppSettings["urlTracker:forcedRedirectCacheTimeoutSeconds"], out parsedAppSetting)
                            && parsedAppSetting > 0)
                        {
                            forcedRedirectCacheTimeoutSeconds = parsedAppSetting;
                        }
                    }
                    _forcedRedirectCacheTimeoutSeconds = forcedRedirectCacheTimeoutSeconds;
                }
                return _forcedRedirectCacheTimeoutSeconds.Value;
            }
        }
        

        static bool? _isDisabled;
        static bool? _enableLogging;
        static string[] _notFoundUrlsToIgnore;
        static bool? _isTrackingDisabled;
        static bool? _isNotFoundTrackingDisabled;
        static bool? _appendPortNumber;
        static bool? _hasDomainOnChildNode;
        static bool? _forcedRedirectCacheTimeoutEnabled;
        static int? _forcedRedirectCacheTimeoutSeconds;
    }
}