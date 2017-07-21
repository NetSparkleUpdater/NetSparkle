using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkle
{
    /// <summary>
    /// A simple class to handle log information for NetSparkle
    /// </summary>
    public class SparkleLog
    {
        /// <summary>
        /// Empty constructor -> sets PrintDiagnosticToConsole to false
        /// </summary>
        public SparkleLog()
        {
            PrintDiagnosticToConsole = false;
        }

        /// <summary>
        /// SparkleLog constructor that takes a bool to determine
        /// the value for printDiagnosticToConsole
        /// </summary>
        /// <param name="printDiagnosticToConsole">Whether this object should print via Debug.WriteLine or Console.WriteLine</param>
        public SparkleLog(bool printDiagnosticToConsole)
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
        public virtual void PrintMessage(string message, params string[] arguments)
        {
            if (!PrintDiagnosticToConsole)
                Debug.WriteLine("netsparkle: " + message, arguments);
            else
                Console.WriteLine("netsparkle: " + message, arguments);
        }
    }
}
