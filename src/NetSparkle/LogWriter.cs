using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater
{
    /// <summary>
    /// A simple class to handle log information for NetSparkleUpdater.
    /// Make sure to do any setup for this class that you want
    /// to do before calling StartLoop on your <see cref="SparkleUpdater"/> object.
    /// </summary>
    public class LogWriter : ILogger
    {
        /// <summary>
        /// Tag to show before any log statements
        /// </summary>
        public static string tag = "netsparkle:";

        /// <summary>
        /// Empty constructor -> sets PrintDiagnosticToConsole to false
        /// </summary>
        public LogWriter()
        {
            PrintDiagnosticToConsole = false;
        }

        /// <summary>
        /// LogWriter constructor that takes a bool to determine
        /// the value for printDiagnosticToConsole
        /// </summary>
        /// <param name="printDiagnosticToConsole">False to print to <seealso cref="Trace.WriteLine(string)"/>;
        /// true to print to <seealso cref="Console.WriteLine(string)"/></param>
        public LogWriter(bool printDiagnosticToConsole)
        {
            PrintDiagnosticToConsole = printDiagnosticToConsole;
        }

        #region Properties

        /// <summary>
        /// True if this class should print to <seealso cref="Console.WriteLine(string)"/>;
        /// false if this object should print to <seealso cref="Trace.WriteLine(string)"/>.
        /// Defaults to false.
        /// </summary>
        public bool PrintDiagnosticToConsole { get; set; }

        #endregion
        
        /// <inheritdoc/>
        public virtual void PrintMessage(string message, params object[] arguments)
        {
            if (PrintDiagnosticToConsole)
            {
                Console.WriteLine(tag + " " + message, arguments);
            }
            else
            {
                Trace.WriteLine(string.Format(tag + " " + message, arguments));
            }
        }
    }
}
