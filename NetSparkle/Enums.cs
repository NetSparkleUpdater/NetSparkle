using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetSparkle
{
    /// <summary>
    /// Everytime when netsparkle detects an update the 
    /// consumer can decide what should happen as next with the help 
    /// of the UpdateDatected event
    /// </summary>
    public enum NextUpdateAction
    {
        /// <summary>
        /// Show the user interface
        /// </summary>
        ShowStandardUserInterface = 1,
        /// <summary>
        /// Perform an unattended install
        /// </summary>
        PerformUpdateUnattended = 2,
        /// <summary>
        /// Prohibit the update
        /// </summary>
        ProhibitUpdate = 3
    }
}
