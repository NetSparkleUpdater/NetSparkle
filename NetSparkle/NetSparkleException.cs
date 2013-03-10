using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NetSparkle
{
    /// <summary>
    /// A NetSparkle exception
    /// </summary>
    [Serializable]
    public class NetSparkleException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">exception message</param>
        public NetSparkleException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">serialization info</param>
        /// <param name="context">the context</param>
        protected NetSparkleException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
