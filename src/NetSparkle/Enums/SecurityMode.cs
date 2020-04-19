namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Controls the situations where files have to be signed with the DSA private key.
    /// If both a DSA public key and a signature are present, they always have to be valid.
    /// </summary>
    public enum SecurityMode
    {
        /// <summary>
        /// All files (with or without signature) will be accepted.
        /// This mode is strongly NOT recommended. It can cause critical security issues.
        /// </summary>
        Unsafe = 1,

        /// <summary>
        /// If there is a DSA public key, the app cast and download file have to be signed. 
        /// If there isn't a DSA public key, files without a signature will also be accepted. 
        /// This mode is a mix between Unsafe and Strict and can have some security issues if the 
        /// DSA public key gets lost in the application.
        /// </summary>
        UseIfPossible = 2,

        /// <summary>
        /// The app cast and download file have to be signed. This means the DSA public key must exist. This is the default mode.
        /// </summary>
        Strict = 3,
    }
}
