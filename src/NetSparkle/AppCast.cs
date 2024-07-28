using NetSparkleUpdater;
using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Interfaces;
using System.Collections.Generic;

namespace NetSparkleUpdater
{
    /// <summary>
    /// An XML-based app cast document downloader and handler
    /// </summary>
    public class AppCast
    {
        /// <summary>
        /// App cast title (usually the name of the application)
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// App cast language (e.g. "en")
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// App cast description (e.g. "updates for my application").
        /// Defaults to "Most recent changes with links to updates"
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Link to location to download this app cast
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        /// List of <seealso cref="AppCastItem"/> that were parsed in the app cast
        /// </summary>
        public List<AppCastItem> Items { get; set; }

        public AppCast()
        {
            Items = new List<AppCastItem>();
            Description = "Most recent changes with links to updates";
        }
    }
}