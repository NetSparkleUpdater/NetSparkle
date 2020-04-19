using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Events
{
    /// <summary>
    /// Event arguments for when a user responds to an available update
    /// </summary>
    public class UpdateResponseArgs : EventArgs
    {
        /// <summary>
        /// The user's response to the update
        /// </summary>
        public UpdateAvailableResult Result { get; set; }

        /// <summary>
        /// The AppCastItem that the user is responding to an update notice for
        /// </summary>
        public AppCastItem UpdateItem { get; set; }

        /// <summary>
        /// Constructor for UpdateResponseArgs that allows for easy setting
        /// of the result
        /// </summary>
        /// <param name="result">User's response of type UpdateAvailableResult</param>
        /// <param name="item">Item that the user is responding to an update message for</param>
        public UpdateResponseArgs(UpdateAvailableResult result, AppCastItem item) : base()
        {
            Result = result;
            UpdateItem = item;
        }
    }
}
