using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// Basic <seealso cref="IAppCastFilter"/> implementation for filtering
    /// your app cast items based on a channel name (e.g. "beta"). Makes it
    /// easy to allow your users to be on a beta software track or similar.
    /// </summary>
    public class ChannelAppCastFilter : IAppCastFilter
    {
        private ILogger? _logWriter;

        /// <summary>
        /// Constructor for <seealso cref="ChannelAppCastFilter"/>
        /// </summary>
        /// <param name="logWriter">Optional <seealso cref="ILogger"/> for logging data</param>
        public ChannelAppCastFilter(ILogger? logWriter = null)
        {
            RemoveOlderItems = true;
            KeepItemsWithNoSuffix = true;
            ChannelName = "";
            _logWriter = logWriter;
        }

        /// <summary>
        /// Set to true to remove older items (&lt;= the current installed version);
        /// false to keep them.
        /// Defaults to true.
        /// </summary>
        public bool RemoveOlderItems { get; set; }

        /// <summary>
        /// Channel name (e.g. "beta" or "alpha" to filter by).
        /// Defaults to "".
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// When filtering by <see cref="ChannelName"/>, true to keep items
        /// that have a version with no suffix (e.g. "1.2.3" only);
        /// false to get rid of those. Setting this to true will 
        /// allow users on a beta channel to get updates for the standard
        /// update channel.
        /// Has no effect when <see cref="ChannelName"/> is whitespace/empty.
        /// Defaults to true.
        /// </summary>
        public bool KeepItemsWithNoSuffix { get; set; }

        /// <inheritdoc/>
        public IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            var lowerChannelName = ChannelName.ToLower();
            return items.Where((item) => 
            {
                var semVer = SemVerLike.Parse(item.Version);
                if (RemoveOlderItems && semVer.CompareTo(installed) <= 0)
                {
                    _logWriter?.PrintMessage("Removing older item from filtered app cast results");
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(ChannelName))
                {
                    _logWriter?.PrintMessage("Filtering by channel: {0}; keeping items with no suffix = {1}", 
                        lowerChannelName, KeepItemsWithNoSuffix);
                    return semVer.AllSuffixes.ToLower().Contains(lowerChannelName) ||
                        (KeepItemsWithNoSuffix && string.IsNullOrWhiteSpace(semVer.AllSuffixes.Trim()));
                }
                return true;
            }).OrderByDescending(x => x.SemVerLikeVersion);
        }
    }
}