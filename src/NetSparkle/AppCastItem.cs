using NetSparkleUpdater.AppCastHandlers;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace NetSparkleUpdater
{
    /// <summary>
    /// Item from a Sparkle AppCast file
    /// </summary>
    [Serializable]
    public class AppCastItem : IComparable<AppCastItem>
    {
        /// <summary>
        /// The application name
        /// </summary>
        public string AppName { get; set; }
        /// <summary>
        /// The installed version
        /// </summary>
        public string AppVersionInstalled { get; set; }
        /// <summary>
        /// The item title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// The available version
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Shortened version
        /// </summary>
        public string ShortVersion { get; set; }
        /// <summary>
        /// The release notes link
        /// </summary>
        public string ReleaseNotesLink { get; set; }
        /// <summary>
        /// The DSA signature of the Release Notes file
        /// </summary>
        public string ReleaseNotesDSASignature { get; set; }
        /// <summary>
        /// The embedded description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The download link
        /// </summary>
        public string DownloadLink { get; set; }
        /// <summary>
        /// The DSA signature of the download file
        /// </summary>
        public string DownloadDSASignature { get; set; }
        /// <summary>
        /// Date item was published
        /// </summary>
        public DateTime PublicationDate { get; set; }
        /// <summary>
        /// Whether the update was marked critical or not via sparkle:critical
        /// </summary>
        public bool IsCriticalUpdate { get; set; }
        /// <summary>
        /// Length of update set via sparkle:length
        /// </summary>
        public long UpdateSize { get; set; }

        /// <summary>
        /// Operating system that this update applies to
        /// </summary>
        public string OperatingSystemString { get; set; }

        /// <summary>
        /// True if this update is a windows update; false otherwise.
        /// Acceptable OS strings are: "win" or "windows" (this is 
        /// checked with a case-insensitive check). If not specified,
        /// assumed to be a Windows update.
        /// </summary>
        public bool IsWindowsUpdate
        {
            get
            {
                if (OperatingSystemString != null)
                {
                    var lowercasedOS = OperatingSystemString.ToLower();
                    if (lowercasedOS == "win" || lowercasedOS == "windows")
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// True if this update is a macOS update; false otherwise.
        /// Acceptable OS strings are: "mac", "osx", or "macos" (this is 
        /// checked with a case-insensitive check). If not specified,
        /// assumed to be a Windows update.
        /// </summary>
        public bool IsMacOSUpdate
        {
            get
            {
                if (OperatingSystemString != null)
                {
                    var lowercasedOS = OperatingSystemString.ToLower();
                    if (lowercasedOS == "mac" || lowercasedOS == "macos" || lowercasedOS == "osx")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// True if this update is a macOS update; false otherwise.
        /// Acceptable OS strings are: "linux" (this is 
        /// checked with a case-insensitive check). If not specified,
        /// assumed to be a Linux update.
        /// </summary>
        public bool IsLinuxUpdate
        {
            get
            {
                if (OperatingSystemString != null)
                {
                    var lowercasedOS = OperatingSystemString.ToLower();
                    if (lowercasedOS == "linux")
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// MIME type for file as specified in the closure tag. Defaults to "application/octet-stream".
        /// </summary>
        public string MIMEType { get; set; }

        #region XML

        private const string _itemNode = "item";
        private const string _titleNode = "title";
        private const string _enclosureNode = "enclosure";
        private const string _releaseNotesLinkNode = "releaseNotesLink";
        private const string _descriptionNode = "description";
        private const string _versionAttribute = "version";
        private const string _shortVersionAttribute = "shortVersionString";
        private const string _dsaSignature = "dsaSignature";
        private const string _criticalAttribute = "criticalUpdate";
        private const string _operatingSystemAttribute = "os";
        private const string _lengthAttribute = "length";
        private const string _typeAttribute = "type";
        private const string _urlAttribute = "url";
        private const string _pubDateNode = "pubDate";
        private const string _defaultOperatingSystem = "windows";
        private const string _defaultType = "application/octet-stream";

        /// <summary>
        /// Parse item Xml Node to AppCastItem
        /// </summary>
        /// <param name="installedVersion">Currently installed version</param>
        /// <param name="applicationName">Application name</param>
        /// <param name="castUrl">The url of the appcast</param>
        /// <param name="item">The item XML node</param>
        /// <param name="logWriter">logwriter instance</param>
        /// <returns>AppCastItem from Xml Node</returns>
        public static AppCastItem Parse(string installedVersion, string applicationName, string castUrl, XElement item, LogWriter logWriter)
        {

            var newAppCastItem = new AppCastItem()
            {
                AppVersionInstalled = installedVersion,
                AppName = applicationName,
                UpdateSize = 0,
                IsCriticalUpdate = false,
                OperatingSystemString = _defaultOperatingSystem,
                MIMEType = _defaultType
            };

            //title
            newAppCastItem.Title = item.Element(_titleNode)?.Value ?? string.Empty;

            //release notes
            var releaseNotesElement = item.Element(XMLAppCast.SparkleNamespace + _releaseNotesLinkNode);
            newAppCastItem.ReleaseNotesDSASignature = releaseNotesElement?.Attribute(XMLAppCast.SparkleNamespace + _dsaSignature)?.Value ?? string.Empty;
            newAppCastItem.ReleaseNotesLink = releaseNotesElement?.Value.Trim() ?? string.Empty;

            //description
            newAppCastItem.Description = item.Element(_descriptionNode)?.Value.Trim() ?? string.Empty;

            //enclosure
            var enclosureElement = item.Element(_enclosureNode) ?? item.Element(XMLAppCast.SparkleNamespace + _enclosureNode);

            newAppCastItem.Version = enclosureElement?.Attribute(XMLAppCast.SparkleNamespace + _versionAttribute)?.Value ?? string.Empty;
            newAppCastItem.ShortVersion = enclosureElement?.Attribute(XMLAppCast.SparkleNamespace + _shortVersionAttribute)?.Value ?? string.Empty;
            newAppCastItem.DownloadLink = enclosureElement?.Attribute(_urlAttribute)?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(newAppCastItem.DownloadLink) && !newAppCastItem.DownloadLink.Contains("/"))
            {
                // Download link contains only the filename -> complete with _castUrl
                newAppCastItem.DownloadLink = castUrl.Substring(0, castUrl.LastIndexOf('/') + 1) + newAppCastItem.DownloadLink;
            }

            newAppCastItem.DownloadDSASignature = enclosureElement?.Attribute(XMLAppCast.SparkleNamespace + _dsaSignature)?.Value ?? string.Empty;
            string length = enclosureElement?.Attribute(_lengthAttribute)?.Value ?? string.Empty;
            if (length != null)
            {
                if (long.TryParse(length, out var size))
                {
                    newAppCastItem.UpdateSize = size;
                }
                else
                {
                    newAppCastItem.UpdateSize = 0;
                }
            }
            bool isCritical = false;
            string critical = enclosureElement?.Attribute(XMLAppCast.SparkleNamespace + _criticalAttribute)?.Value ?? string.Empty;
            if (critical != null && critical == "true" || critical == "1")
            {
                isCritical = true;
            }
            newAppCastItem.IsCriticalUpdate = isCritical;

            newAppCastItem.OperatingSystemString = enclosureElement?.Attribute(XMLAppCast.SparkleNamespace + _operatingSystemAttribute)?.Value ?? _defaultOperatingSystem;

            newAppCastItem.MIMEType = enclosureElement?.Attribute(_typeAttribute)?.Value ?? _defaultType;

            //pub date
            var pubDateElement = item.Element(_pubDateNode);
            if (pubDateElement != null)
            {
                // "ddd, dd MMM yyyy HH:mm:ss zzz" => Standard date format
                //      e.g. "Sat, 26 Oct 2019 22:05:11 -05:00"
                // "ddd, dd MMM yyyy HH:mm:ss Z" => Check for MS AppCenter Sparkle date format which ends with GMT
                //      e.g. "Sat, 26 Oct 2019 22:05:11 GMT"
                // "ddd, dd MMM yyyy HH:mm:ss" => Standard date format with no timezone (fallback)
                //      e.g. "Sat, 26 Oct 2019 22:05:11"
                string[] formats = { "ddd, dd MMM yyyy HH:mm:ss zzz", "ddd, dd MMM yyyy HH:mm:ss Z", "ddd, dd MMM yyyy HH:mm:ss" };
                string dt = pubDateElement.Value.Trim();
                if (DateTime.TryParseExact(dt, formats, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateValue))
                {
                    logWriter?.PrintMessage("While parsing app cast item, converted '{0}' to {1}.", dt, dateValue);
                    newAppCastItem.PublicationDate = dateValue;
                }
                else
                {
                    logWriter?.PrintMessage("Cannot parse item's DateTime: {0}", dt);
                }
            }

            return newAppCastItem;
        }

        /// <summary>
        /// Create Xml node from this instance of AppCastItem
        /// </summary>
        /// <returns>An XML node</returns>
        public XElement GetXElement()
        {
            var item = new XElement(_itemNode);

            item.Add(new XElement(_titleNode) { Value = Title });

            if (!string.IsNullOrEmpty(ReleaseNotesLink))
            {
                var releaseNotes = new XElement(XMLAppCast.SparkleNamespace + _releaseNotesLinkNode) { Value = ReleaseNotesLink };
                if (!string.IsNullOrEmpty(ReleaseNotesDSASignature))
                {
                    releaseNotes.Add(new XAttribute(XMLAppCast.SparkleNamespace + _dsaSignature, ReleaseNotesDSASignature));
                }
                item.Add(releaseNotes);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                item.Add(new XElement(_descriptionNode) { Value = Description });
            }

            if (PublicationDate != DateTime.MinValue && PublicationDate != DateTime.MaxValue)
            {
                item.Add(new XElement(_pubDateNode) { Value = PublicationDate.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture) });
            }

            if (!string.IsNullOrEmpty(DownloadLink))
            {
                var enclosure = new XElement(_enclosureNode);
                enclosure.Add(new XAttribute(_urlAttribute, DownloadLink));
                enclosure.Add(new XAttribute(XMLAppCast.SparkleNamespace + _versionAttribute, Version));

                if (!string.IsNullOrEmpty(ShortVersion))
                {
                    enclosure.Add(new XAttribute(XMLAppCast.SparkleNamespace + _shortVersionAttribute, ShortVersion));
                }

                enclosure.Add(new XAttribute(_lengthAttribute, UpdateSize));
                enclosure.Add(new XAttribute(XMLAppCast.SparkleNamespace + _operatingSystemAttribute, OperatingSystemString ?? _defaultOperatingSystem));
                enclosure.Add(new XAttribute(_typeAttribute, MIMEType ?? _defaultType));

                if (!string.IsNullOrEmpty(DownloadDSASignature))
                {
                    enclosure.Add(new XAttribute(XMLAppCast.SparkleNamespace + _dsaSignature, DownloadDSASignature));
                }
                item.Add(enclosure);
            }
            return item;
        }

        #endregion

        #region IComparable<AppCastItem> Members

        /// <summary>
        /// Compares this instance to another
        /// </summary>
        /// <param name="other">the other instance</param>
        /// <returns>-1, 0, 1 if this instance is less than, equal to, or greater than the <paramref name="other"/></returns>
        public int CompareTo(AppCastItem other)
        {
            if (!Version.Contains(".") || !other.Version.Contains("."))
            {
                return 0;
            }
            Version v1 = new Version(Version);
            Version v2 = new Version(other.Version);
            return v1.CompareTo(v2);
        }

        /// <summary>
        /// Equality check to another instance
        /// </summary>
        /// <param name="obj">the instance to compare to</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is AppCastItem item))
            {
                return false;
            }
            if (ReferenceEquals(this, item))
            {
                return true;
            }
            return AppName.Equals(item.AppName) && CompareTo(item) == 0;
        }

        /// <summary>
        /// Derive hashcode from immutable variables
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Version.GetHashCode() * 17 + AppName.GetHashCode();
        }

        /// <summary>
        /// Check equality of two AppCastItem instances
        /// </summary>
        /// <param name="left">AppCastItem to compare</param>
        /// <param name="right">AppCastItem to compare</param>
        /// <returns>True if items are the same</returns>
        public static bool operator ==(AppCastItem left, AppCastItem right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Check if two AppCastItem instances are different
        /// </summary>
        /// <param name="left">AppCastItem to compare</param>
        /// <param name="right">AppCastItem to compare</param>
        /// <returns>True if items are different</returns>
        public static bool operator !=(AppCastItem left, AppCastItem right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Less than comparison of version between two AppCastItem instances
        /// </summary>
        /// <param name="left">AppCastItem to compare</param>
        /// <param name="right">AppCastItem to compare</param>
        /// <returns>True if left version is less than right version</returns>
        public static bool operator <(AppCastItem left, AppCastItem right)
        {
            return left is null ? !(right is null) : left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Less than or equal to comparison of version between two AppCastItem instances
        /// </summary>
        /// <param name="left">AppCastItem to compare</param>
        /// <param name="right">AppCastItem to compare</param>
        /// <returns>True if left version is less than or equal to right version</returns>
        public static bool operator <=(AppCastItem left, AppCastItem right)
        {
            return left is null || left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Greater than comparison of version between two AppCastItem instances
        /// </summary>
        /// <param name="left">AppCastItem to compare</param>
        /// <param name="right">AppCastItem to compare</param>
        /// <returns>True if left version is greater than right version</returns>
        public static bool operator >(AppCastItem left, AppCastItem right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Greater than or equal to comparison of version between two AppCastItem instances
        /// </summary>
        /// <param name="left">AppCastItem to compare</param>
        /// <param name="right">AppCastItem to compare</param>
        /// <returns>True if left version is greater than or equal to right version</returns>
        public static bool operator >=(AppCastItem left, AppCastItem right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }

        #endregion
    }
}
