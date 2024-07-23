#nullable enable

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// Interface for objects that can handle log information output
    /// (e.g. to a console or a file)
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Print a message to the log output.
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="arguments">Arguments to print (e.g. if using {0} format arguments)</param>
        void PrintMessage(string message, params object[]? arguments);
    }
}
