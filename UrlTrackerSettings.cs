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

                        if (bool.TryParse(
                            ConfigurationManager.AppSettings["urlTracker:appendPortNumber"],
                            out parsedAppSetting))
                        {
                            appendPortNumber = parsedAppSetting;
                        }
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


        static bool? _isDisabled;
        static bool? _enableLogging;
        static string[] _notFoundUrlsToIgnore;
        static bool? _isTrackingDisabled;
        static bool? _isNotFoundTrackingDisabled;
        static bool? _appendPortNumber;
        static bool? _hasDomainOnChildNode;
    }
}