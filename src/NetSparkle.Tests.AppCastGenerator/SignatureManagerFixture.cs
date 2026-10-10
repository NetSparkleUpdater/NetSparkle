using System;
using System.IO;
using NetSparkleUpdater.AppCastGenerator;

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
            var keysProvider = new KeysProvider(
            [
                new EnvironmentKeysProvider(),
                new ValueKeysProvider(),
                new FileKeysProvider()
            ]);
            var signatureManager = new SignatureManager(keysProvider);
            var fileKeysProvider = signatureManager.KeysProvider.GetRequiredProvider<FileKeysProvider>();
            fileKeysProvider.SetStorageDirectory(Path.Combine(Path.GetTempPath(), temp_directory_name));
            signatureManager.Generate(true);
            return signatureManager;
        }

        public SignatureManager GetSignatureManager()
        {
            return _manager;
        }

        public void CleanupSignatureManager(SignatureManager manager)
        {
            var fileKeysProvider = manager.KeysProvider.GetRequiredProvider<FileKeysProvider>();
            var storageDir = fileKeysProvider.GetStorageDirectory();
            // the testing storage dir should never equal the default one,
            // but because I am paranoid, we will do the check. We never want
            // to erase someone's keys!
            if (Directory.Exists(storageDir) && storageDir != FileKeysProvider.GetDefaultStorageDirectory())
            {
                Directory.Delete(storageDir, true);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupSignatureManager(_manager);
            }
        }
    }
}