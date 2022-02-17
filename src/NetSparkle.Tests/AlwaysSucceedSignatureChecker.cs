using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUnitTests
{
    public class AlwaysSucceedSignatureChecker : ISignatureVerifier
    {
        public SecurityMode SecurityMode { get; set; }

        public bool HasValidKeyInformation()
        {
            return true;
        }

        public ValidationResult VerifySignature(string signature, byte[] dataToVerify)
        {
            return ValidationResult.Valid;
        }

        public ValidationResult VerifySignatureOfFile(string signature, string binaryPath)
        {
            return ValidationResult.Valid;
        }

        public ValidationResult VerifySignatureOfString(string signature, string data)
        {
            return ValidationResult.Valid;
        }
    }
}