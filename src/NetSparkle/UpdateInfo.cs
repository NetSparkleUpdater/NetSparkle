#nullable enable

using NetSparkleUpdater.Enums;
using System.Collections.Generic;

namespace NetSparkleUpdater
{
    /// <summary>
    /// A simple class to hold information on potential updates to a software product.
    /// </summary>
    public class UpdateInfo
    {
        /// <summary>
        /// Create information about an update with the given status and no available update items
        /// </summary>
        /// <param name="status">Information on whether an update is available</param>
        public UpdateInfo(UpdateStatus status) : this(status, null)
        {
        }

        /// <summary>
        /// Create information about an update with the given status and update items
        /// </summary>
        /// <param name="status">Information on whether an update is available</param>
        /// <param name="updates">The list of updates that are available to update to</param>
        public UpdateInfo(UpdateStatus status, List<AppCastItem>? updates)
        {
            Status = status;
            Updates = updates ?? new List<AppCastItem>();
        }

        /// <summary>
        /// Whether or not an update is available
        /// </summary>
        public UpdateStatus Status { get; set; }
        
        /// <summary>
        /// Any available updates for the product that the user could
        /// potentially install
        /// </summary>
        public List<AppCastItem> Updates { get; set; }
    }
}
