using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using System;
using System.Diagnostics;

namespace NetSparkleUpdater
{
    /// <summary>
    /// A simple class to handle logging information.
    /// Make sure to do any setup for this class that you want
    /// to do before calling StartLoop on your <see cref="SparkleUpdater"/> object.
    /// </summary>
    public class LogWriter : ILogger
    {
        /// <summary>
        /// Tag to show before any log statements. Can be set to null.
        /// </summary>
        public string Tag { get; set; } = "netsparkle:";

        /// <summary>
        /// Create a LogWriter that outputs to <seealso cref="Trace.WriteLine(string)"/> by default
        /// </summary>
        public LogWriter()
        {
            OutputMode = LogWriterOutputMode.Trace;
        }

        /// <summary>
        /// Create a LogWriter that outputs via the given LogWriterOutputMode mode
        /// </summary>
        /// <param name="outputMode"><seealso cref="LogWriterOutputMode"/> for this LogWriter instance</param>;
        public LogWriter(LogWriterOutputMode outputMode)
        {
            OutputMode = outputMode;
        }

        #region Properties

        /// <summary>
        /// <seealso cref="LogWriterOutputMode"/> for this LogWriter instance. Set to
        /// <seealso cref="LogWriterOutputMode.None"/> to make this LogWriter output nothing.
        /// </summary>
        public LogWriterOutputMode OutputMode { get; set; }

        #endregion
        
        /// <inheritdoc/>
        public virtual void PrintMessage(string message, params object[] arguments)
        {
            switch (OutputMode)
            {
                case LogWriterOutputMode.Console:
                    Console.WriteLine(Tag + (string.IsNullOrWhiteSpace(Tag) ? "" : " ") + message, arguments);
                    break;
                case LogWriterOutputMode.Trace:
                    Trace.WriteLine(string.Format(Tag + (string.IsNullOrWhiteSpace(Tag) ? "" : " ") + message, arguments));
                    break;
                case LogWriterOutputMode.Debug:
                    Debug.WriteLine(Tag + (string.IsNullOrWhiteSpace(Tag) ? "" : " ") + message, arguments);
                    break;
                case LogWriterOutputMode.None:
                    break;
                default:
                    break;
            }
        }
    }
}
