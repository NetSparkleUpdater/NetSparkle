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
            manager = new SignatureManager();
            manager.SetStorageDirectory(Path.Combine(Path.GetTempPath(), "netsparkle-tests"));
            manager.Generate(true);
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
