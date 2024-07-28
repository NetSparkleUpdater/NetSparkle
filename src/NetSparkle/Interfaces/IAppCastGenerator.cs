using NetSparkleUpdater;
using NetSparkleUpdater.Configurations;
using System.Collections.Generic;

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Interface used by objects that initiate a download process
    /// for an app cast, perform any needed signature verification on
    /// the app cast, and parse the app cast's items into a list of
    /// <see cref="AppCastItem"/>.
    /// Implement this interface if you would like to use a custom parsing
    /// method for your app cast that isn't yet built into NetSparkle.
    /// </summary>
    public interface IAppCastGenerator
    {
        /// <summary>
        /// Parse the app cast string into a list of <see cref="AppCastItem"/> objects.
        /// When complete, the Items list should contain the parsed information
        /// as <see cref="AppCastItem"/> objects.
        /// </summary>
        /// <param name="appCast">the non-null string XML app cast</param>
        AppCast ReadAppCast(string appCastString);
        AppCast ReadAppCastFromFile(string filePath);
        string SerializeAppCast(AppCast appCast);
        void SerializeAppCastToFile(AppCast appCast, string outputPath);
    }
}