using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.Samples.Avalonia
{
    public class CustomSparkleUpdater : NetSparkleUpdater.SparkleUpdater
    {
        public CustomSparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier)
            : base(appcastUrl, signatureVerifier, null)
        { }
    }
}