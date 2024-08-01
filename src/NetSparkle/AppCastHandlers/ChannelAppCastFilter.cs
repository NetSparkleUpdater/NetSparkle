using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// Basic <seealso cref="IAppCastFilter"/> implementation for filtering
    /// your app cast items based on 
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
            ChannelName = "beta";
            _logWriter = logWriter;
        }

        public bool RemoveOlderItems { get; set; }
        public string ChannelName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items)
        {
            var lowerChannelName = ChannelName.ToLower();
            return items.Where((item) => 
            {
                var semVer = SemVerLike.Parse(item.Version);
                if (RemoveOlderItems && semVer.CompareTo(installed) <= 0)
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(ChannelName))
                {
                    return semVer.AllSuffixes.ToLower().Contains(lowerChannelName);
                }
                return true;
            }).OrderByDescending(x => x.SemVerLikeVersion);
        }
    }
}