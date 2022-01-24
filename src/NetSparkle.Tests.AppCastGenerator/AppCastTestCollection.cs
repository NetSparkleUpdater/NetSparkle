using NetSparkleUpdater.AppCastGenerator;
using System.IO;
using Xunit;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class SignatureManagerFixture
    {
        private SignatureManager manager;

        public SignatureManagerFixture()
        {
            manager = NewSignatureManager("netsparkle-tests");
        }

        public SignatureManager NewSignatureManager(string temp_directory_name)
        {
            var newManager = new SignatureManager();
            newManager.SetStorageDirectory(Path.Combine(Path.GetTempPath(), temp_directory_name));
            newManager.Generate(true);
            return newManager;
        }

        public SignatureManager GetSignatureManager()
        {
            return manager;
        }
    }

    [CollectionDefinition("Signature manager")]
    public class SignatureManagerCollection : ICollectionFixture<SignatureManagerFixture>
    {

    }
}
