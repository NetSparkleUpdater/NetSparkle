using NetSparkleUpdater.AppCastGenerator;
using System;
using System.IO;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class SignatureManagerFixture : IDisposable
    {
        public const string CollectionName = "signature-manager";

        private SignatureManager _manager;

        public SignatureManagerFixture()
        {
            _manager = CreateSignatureManager("netsparkle-tests");
        }

        public SignatureManager CreateSignatureManager(string temp_directory_name)
        {
            var signatureManager = new SignatureManager();
            signatureManager.SetStorageDirectory(Path.Combine(Path.GetTempPath(), temp_directory_name));
            signatureManager.Generate(true);
            return signatureManager;
        }

        public SignatureManager GetSignatureManager()
        {
            return _manager;
        }

        public void CleanupSignatureManager(SignatureManager manager)
        {
            var storageDir = manager.GetStorageDirectory();
            // the testing storage dir should never equal the default one,
            // but because I am paranoid, we will do the check. We never want
            // to erase someone's keys!
            if (Directory.Exists(storageDir) && storageDir != SignatureManager.GetDefaultStorageDirectory())
            {
                Directory.Delete(storageDir, true);
            }
        }

        public void Dispose()
        {
            CleanupSignatureManager(_manager);
        }
    }

    [CollectionDefinition(SignatureManagerFixture.CollectionName)]
    public class SignatureManagerCollection : ICollectionFixture<SignatureManagerFixture>
    {

    }
}
