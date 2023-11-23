using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NetSparkleUpdater
{
    /// <summary>
    /// An exception that occurred during NetSparkleUpdater's operations
    /// </summary>
    [Serializable]
    public class NetSparkleException : Exception
    {
        /// <summary>
        /// Create an exception with the given message
        /// </summary>
        /// <param name="message">the message to use for this exception</param>
        public NetSparkleException(string message) : base(message)
        {
        }
    }
}
