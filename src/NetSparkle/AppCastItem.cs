using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Interfaces;
using System;
using System.Globalization;
using System.Xml.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetSparkleUpdater
{
    /// <summary>
    /// Item from a Sparkle app cast file with information
    /// about the download, release notes, etc.
    /// </summary>
    [Serializable]
    public class AppCastItem : IComparable<AppCastItem>
    {
        private SemVerLike? _semVerLikeCache;
        private string? _version;

        /// <summary>
        /// Default MIME type for an App Cast Item (application/octet-stream)
        /// </summary>
        public static string DefaultMIMEType = "application/octet-stream";
        /// <summary>
        /// Default operating system for an App Cast Item (windows)
        /// </summary>
        public static string DefaultOperatingSystem = "windows";

        /// <summary>
        /// Default constructor for an app cast item
        /// </summary>
        public AppCastItem()
        {
            MIMEType = DefaultMIMEType;
            OperatingSystem = DefaultOperatingSystem;
        }

        /// <summary>
        /// The item title
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        /// <summary>
        /// The available version -- this technically can be null if file parsing fails, 
        /// but since NetSparkleUpdater runs off of version information, doing this is
        /// not a great way to use this library.
        /// </summary>
        [JsonPropertyName("version")]
        public string? Version 
        { 
            get => _version; 
            set { _semVerLikeCache = null; _version = value; } 
        }
        /// <summary>
        /// The available version as a SemVerLike object (handles things like 1.2-alpha1)
        /// </summary>
        [JsonIgnore]
        public SemVerLike SemVerLikeVersion
        {
            get 
            {
                if (_semVerLikeCache == null)
                {
                    _semVerLikeCache = SemVerLike.Parse(Version);
                }
                return _semVerLikeCache;
            }
        }
        /// <summary>
        /// Shortened version
        /// </summary>
        [JsonPropertyName("short_version")]
        public string? ShortVersion { get; set; }
        /// <summary>
        /// The release notes link
        /// </summary>
        [JsonPropertyName("release_notes_link")]
        public string? ReleaseNotesLink { get; set; }
        /// <summary>
        /// The signature of the Release Notes file
        /// </summary>
        [JsonPropertyName("release_notes_signature")]
        public string? ReleaseNotesSignature { get; set; }
        /// <summary>
        /// The embedded description
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        /// <summary>
        /// The download link
        /// </summary>
        [JsonPropertyName("url")]
        public string? DownloadLink { get; set; }
        /// <summary>
        /// The signature of the download file
        /// </summary>
        [JsonPropertyName("signature")]
        public string? DownloadSignature { get; set; }
        /// <summary>
        /// Date item was published
        /// </summary>
        [JsonPropertyName("publication_date")]
        public DateTime PublicationDate { get; set; }
        /// <summary>
        /// Whether the update was marked critical or not via sparkle:critical
        /// </summary>
        [JsonPropertyName("is_critical")]
        public bool IsCriticalUpdate { get; set; }
        /// <summary>
        /// Length of update set via sparkle:length (usually the # of bytes of the update)
        /// </summary>
        [JsonPropertyName("size")]
        public long UpdateSize { get; set; }
        /// <summary>
        /// Operating system that this update applies to
        /// </summary>
        [JsonPropertyName("os")]
        public string? OperatingSystem { get; set; }
        /// <summary>
        /// Channel for this app cast item. A channel can also be set in
        /// the version property via semver, and when using 
        /// <seealso cref="ChannelAppCastFilter"/>, both the channel and version data
        /// will be used when filtering channels for this app cast item.
        /// Often a single word like "beta", "gamma", "rc", etc. Note that this can be
        /// an empty string, but "stable" or "release" will be treated as a special
        /// channel, NOT as a "normal" channel for all users.
        /// </summary>
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        /// <summary>
        /// True if this update is a windows update; false otherwise.
        /// Acceptable OS strings are: "win" or "windows" (this is 
        /// checked with a case-insensitive check). If not specified,
        /// assumed to be a Windows update.
        /// </summary>
        [JsonIgnore]
        public bool IsWindowsUpdate
        {
            get
            {
                if (OperatingSystem != null)
                {
                    var lowercasedOS = OperatingSystem.ToLower();
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
        [JsonIgnore]
        public bool IsMacOSUpdate
        {
            get
            {
                if (OperatingSystem != null)
                {
                    var lowercasedOS = OperatingSystem.ToLower();
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
        [JsonIgnore]
        public bool IsLinuxUpdate
        {
            get
            {
                if (OperatingSystem != null)
                {
                    var lowercasedOS = OperatingSystem.ToLower();
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
        [JsonPropertyName("type")]
        public string MIMEType { get; set; }

        #region IComparable<AppCastItem> Members

        /// <summary>
        /// Compares this <see cref="AppCastItem"/> version to the version of another <see cref="AppCastItem"/>.
        /// If versions are the same, uses channel, then title to sort.
        /// </summary>
        /// <param name="other">the other instance</param>
        /// <returns>-1, 0, 1 if this instance is less than, equal to, or greater than the <paramref name="other"/></returns>
        public int CompareTo(AppCastItem? other)
        {
            if (other == null)
            {
                return 1;
            }
            // if neither one has version information, compare them via their titles.
            if (string.IsNullOrWhiteSpace(Version) && string.IsNullOrWhiteSpace(other.Version))
            {
                return Title?.CompareTo(other.Title) ?? 0;
            }
            SemVerLike v1 = SemVerLike.Parse(Version);
            SemVerLike v2 = SemVerLike.Parse(other.Version);
            var versionCompare = v1.CompareTo(v2);
            if (versionCompare == 0)
            {
                var channelCompare = Channel?.CompareTo(other.Channel) ?? 0;
                return channelCompare == 0 ? Title?.CompareTo(other.Title) ?? 0 : channelCompare;
            }
            return versionCompare;
        }

        /// <summary>
        /// See if this this <see cref="AppCastItem"/> version equals the 
        /// version of another <see cref="AppCastItem"/>.
        /// Also checks to make sure the application names match.
        /// </summary>
        /// <param name="obj">the instance to compare to</param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is AppCastItem item))
            {
                return false;
            }
            if (ReferenceEquals(this, item))
            {
                return true;
            }
            return CompareTo(item) == 0;
        }

        /// <summary>
        /// Derive hashcode from immutable variables
        /// </summary>
        /// <returns>the integer haschode of this app cast item</returns>
        public override int GetHashCode()
        {
            return (Version?.GetHashCode() ?? 0) * 17 + (Title?.GetHashCode() ?? 0);
        }

        /// <summary>
        /// Check the equality of two <see cref="AppCastItem"/> instances
        /// </summary>
        /// <param name="left">first <see cref="AppCastItem"/> to compare</param>
        /// <param name="right">second <see cref="AppCastItem"/> to compare</param>
        /// <returns>True if items are the same; false otherwise</returns>
        public static bool operator ==(AppCastItem? left, AppCastItem? right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        /// <summary>
        /// Check if two <see cref="AppCastItem"/> instances are different
        /// </summary>
        /// <param name="left">first <see cref="AppCastItem"/> to compare</param>
        /// <param name="right">second <see cref="AppCastItem"/> to compare</param>
        /// <returns>True if items are different; false if they are the same</returns>
        public static bool operator !=(AppCastItem? left, AppCastItem? right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Less than comparison of version between two <see cref="AppCastItem"/> instances
        /// </summary>
        /// <param name="left">first <see cref="AppCastItem"/> to compare</param>
        /// <param name="right">second <see cref="AppCastItem"/> to compare</param>
        /// <returns>True if left version is less than right version; false otherwise</returns>
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
        /// Greater than comparison of version between two <see cref="AppCastItem"/> instances
        /// </summary>
        /// <param name="left">first <see cref="AppCastItem"/> to compare</param>
        /// <param name="right">second <see cref="AppCastItem"/> to compare</param>
        /// <returns>True if left version is greater than right version; false otherwise</returns>
        public static bool operator >(AppCastItem left, AppCastItem right)
        {
            return !(left is null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Greater than or equal to comparison of version between two <see cref="AppCastItem"/> instances
        /// </summary>
        /// <param name="left">first <see cref="AppCastItem"/> to compare</param>
        /// <param name="right">second <see cref="AppCastItem"/> to compare</param>
        /// <returns>True if left version is greater than or equal to right version; false otherwise</returns>
        public static bool operator >=(AppCastItem left, AppCastItem right)
        {
            return left is null ? right is null : left.CompareTo(right) >= 0;
        }

        #endregion
    }
}
