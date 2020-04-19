using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater
{
    /// <summary>
    /// A simple class to handle log information for NetSparkleUPdater.
    /// Make sure to do any setup for this class that you want
    /// to do before calling StartLoop on your SparkleUpdater object.
    /// </summary>
    public class LogWriter
    {
        /// <summary>
        /// Tag to show before any log statements
        /// </summary>
        protected string tag = "netsparkle:";
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
        /// <param name="printDiagnosticToConsole">Whether this object should print via Debug.WriteLine or Console.WriteLine</param>
        public LogWriter(bool printDiagnosticToConsole)
        {
            PrintDiagnosticToConsole = printDiagnosticToConsole;
        }

        #region Properties

        /// <summary>
        /// true if this class should print to Console.WriteLine.
        /// false if this object should print to Debug.WriteLine.
        /// Defaults to false.
        /// </summary>
        public bool PrintDiagnosticToConsole { get; set; }

        #endregion

        /// <summary>
        /// Print a message to the log output.
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="arguments">Arguments to print (e.g. if using {0} format arguments)</param>
        public virtual void PrintMessage(string message, params object[] arguments)
        {
            if (PrintDiagnosticToConsole)
            {
                Console.WriteLine(tag + " " + message, arguments);
            }
            else
            {
                Debug.WriteLine(tag + " " + message, arguments);
            }
        }
    }
}
