namespace NetSparkleUpdater.AppCastGenerator
{
    public interface IKeysProvider
    {
        byte[]? GetPrivateKey();

        byte[]? GetPublicKey();
    }
}
