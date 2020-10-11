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

        /// <summary>
        /// Create an exception with the given serialization information and streaming context
        /// </summary>
        /// <param name="info">The serialized exception information</param>
        /// <param name="context">the context of the serialization operation for this exception</param>
        protected NetSparkleException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
