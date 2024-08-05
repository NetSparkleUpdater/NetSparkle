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
    /// Note that a "stable" channel search string will not be interpreted as versions 
    /// like "1.0.0"; it will look for versions like "1.0.0-stable1" (aka search for
    /// the string "stable").
    /// Names are compared in a case-insensitive manner.
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
            KeepItemsWithNoVersionSuffix = true;
            ChannelSearchNames = new List<string>();
            _logWriter = logWriter;
        }

        /// <summary>
        /// Set to true to remove older items (&lt;= the current installed version);
        /// false to keep them.
        /// Defaults to true.
        /// </summary>
        public bool RemoveOlderItems { get; set; }

        /// <summary>
        /// Channel names (e.g. "beta" or "alpha") to filter by.
        /// Defaults to "". 
        /// Names are compared in a case-insensitive manner.
        /// </summary>
        public List<string> ChannelSearchNames { get; set; }

        /// <summary>
        /// When filtering by <see cref="ChannelSearchNames"/>, true to keep items
        /// that have a version with no suffix (e.g. "1.2.3" only);
        /// false to get rid of those. Setting this to true will 
        /// allow users on a beta channel to get updates for the standard
        /// update channel.
        /// Has no effect when <see cref="ChannelSearchNames"/> is whitespace/empty.
        /// Defaults to true.
        /// </summary>
        public bool KeepItemsWithNoVersionSuffix { get; set; }

        /// <inheritdoc/>
        public IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            var lowerChannelNames = ChannelSearchNames.Select(s => s.ToLowerInvariant()).ToArray();
            return items.Where((item) => 
            {
                var semVer = SemVerLike.Parse(item.Version);
                if (RemoveOlderItems && semVer.CompareTo(installed) <= 0)
                {
                    _logWriter?.PrintMessage("Removing older item from filtered app cast results");
                    return false;
                }
                if (lowerChannelNames.Length > 0)
                {
                    foreach (var channelName in lowerChannelNames)
                    {
                        _logWriter?.PrintMessage("Filtering by channel: {0}; keeping items with no suffix = {1}", 
                            channelName, KeepItemsWithNoVersionSuffix);
                        var shouldKeep = semVer.AllSuffixes.ToLower().Contains(channelName) ||
                            (KeepItemsWithNoVersionSuffix && string.IsNullOrWhiteSpace(semVer.AllSuffixes.Trim()));
                        if (shouldKeep)
                        {
                            return true;
                        }
                    }
                    _logWriter?.PrintMessage("Item with version {0} was discarded", semVer.ToString());
                    return false;
                }
                else
                {
                    // if we are not wanting any channels but we have a suffix on an item, discard it
                    if (!string.IsNullOrWhiteSpace(semVer.AllSuffixes))
                    {
                        return false;
                    }
                }
                return true;
            }).OrderByDescending(x => x.SemVerLikeVersion);
        }
    }
}