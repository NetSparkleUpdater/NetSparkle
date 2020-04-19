using NetSparkleUpdater.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkleUpdater.Interfaces
{
    public interface ISignatureVerifier
    {
        bool HasValidKeyInformation();
        ValidationResult VerifySignature(string signature, byte[] dataToVerify);
        ValidationResult VerifySignatureOfFile(string signature, string binaryPath);
        ValidationResult VerifySignatureOfString(string signature, string data);
        SecurityMode SecurityMode { get; set; }
    }
}
