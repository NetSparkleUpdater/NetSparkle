using System;
using System.Diagnostics;

namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Output mode for the <seealso cref="LogWriter"/> class
    /// </summary>
    public enum LogWriterOutputMode
    {
        /// <summary>
        /// Don't output anything
        /// </summary>
        None = 0,
        /// <summary>
        /// Output to <seealso cref="Console.WriteLine(string, object[])"/>
        /// </summary>
        Console = 1,
        /// <summary>
        /// Output to <seealso cref="Trace.WriteLine(string)"/>
        /// </summary>
        Trace = 2,
        /// <summary>
        /// Output to <seealso cref="Debug.WriteLine(string, object[])"/>
        /// </summary>
        Debug = 3
    }
}
