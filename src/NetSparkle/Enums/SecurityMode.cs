namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Controls the situations where files have to be signed with the private key.
    /// If both a public key and a signature are present, they always have to be valid.
    /// 
    /// We recommend using SecurityMode.Strict if at all possible.
    /// </summary>
    public enum SecurityMode
    {
        /// <summary>
        /// All files (with or without signature) will be accepted.
        /// This mode is strongly NOT recommended. It can cause critical security issues.
        /// </summary>
        Unsafe = 1,

        /// <summary>
        /// If there is a public key, the app cast and download file have to be signed. 
        /// If there isn't a public key, files without a signature will also be accepted. 
        /// This mode is a mix between Unsafe and Strict and can have some security issues if the 
        /// public key gets lost in the application.
        /// </summary>
        UseIfPossible = 2,

        /// <summary>
        /// The app cast and download file have to be signed. This means the public key must exist. This is the default mode.
        /// </summary>
        Strict = 3,

        /// <summary>
        /// Only verify the signature of software downloads (via an ISignatureVerifier).
        /// Do not verify the signature of anything else: app casts, release notes, etc.
        /// </summary>
        OnlyVerifySoftwareDownloads = 4,
    }
}
