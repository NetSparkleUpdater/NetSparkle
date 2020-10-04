using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkleUpdater.UI.Avalonia.Interfaces
{
    /// <summary>
    /// Interface that allows one object to tell another that the user has
    /// responded to an update check with a given response. Used by the
    /// <see cref="UpdateAvailableWindow"/>'s view model.
    /// </summary>
    public interface IUserRespondedToUpdateCheck
    {
        /// <summary>
        /// The user responded to a given update check with some sort of response
        /// (e.g. skip update, install update)
        /// </summary>
        /// <param name="response">The user's response to the update notification</param>
        void UserRespondedToUpdateCheck(UpdateAvailableResult response);
    }
}
