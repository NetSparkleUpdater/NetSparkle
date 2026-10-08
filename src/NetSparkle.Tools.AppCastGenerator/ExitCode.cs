namespace NetSparkleUpdater.Tools.AppCastGenerator
{
    /// <summary>
    /// Exit codes returned by the app cast generator process.
    /// Each failure has its own value so CI scripts can tell
    /// different errors apart instead of guessing from console output.
    /// </summary>
    internal enum ExitCode
    {
        Success = 0,
        ErrorInvalidArguments = 1,
        ErrorKeysDoNotExist = 2,
        ErrorCouldNotLoadPrivateKey = 3,
        ErrorCouldNotLoadPublicKey = 4,
        ErrorKeyGenerationFailed = 5,
        ErrorCouldNotGenerateSignature = 6,
        ErrorSignatureInvalid = 7,
        ErrorNoOutputDirectory = 8,
        ErrorCouldNotLoadAppCastItems = 9,
    }
}
