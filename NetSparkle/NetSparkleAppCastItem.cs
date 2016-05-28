using System;

namespace NetSparkle
{
    /// <summary>
    /// Item from a Sparkle AppCast file
    /// </summary>
    public class NetSparkleAppCastItem : IComparable<NetSparkleAppCastItem>
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
        /// The available version
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// The release notes linke
        /// </summary>
        public string ReleaseNotesLink { get; set; }
        /// <summary>
        /// The embedded description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The download link
        /// </summary>
        public string DownloadLink { get; set; }
        /// <summary>
        /// The DSA signature
        /// </summary>
        public string DSASignature { get; set; }
        /// <summary>
        /// Date item was published
        /// </summary>
        public DateTime PublicationDate { get; set; }

        #region IComparable<NetSparkleAppCastItem> Members
        /// <summary>
        /// Compares this instance to another
        /// </summary>
        /// <param name="other">the other instance</param>
        /// <returns>-1, 0, 1 if this instance is less than, equal to, or greater than the <paramref name="other"/></returns>
        public int CompareTo(NetSparkleAppCastItem other)
        {
            if (!Version.Contains(".") || !other.Version.Contains("."))
            {
                return 0;
            }
            Version v1 = new Version(Version);
            Version v2 = new Version(other.Version);
            return v1.CompareTo(v2);
        }

        #endregion
    }
}
