namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Controls the Mode in which situations which files has to be signed with the DSA private key.
    /// If an DSA public key and an signature is preset they allways has to be valid.
    /// </summary>
    public enum SecurityMode
    {
        /// <summary>
        /// All files (with or without signature) will be accepted.
        /// This mode is strongly NOT recommended. It can cause critical security issues.
        /// </summary>
        Unsafe = 1,

        /// <summary>
        /// If there is a DSA public key, all files have to be signed. If there isn't any DSA public key
        ///  files without a signature will also be accepted. It's an mix between Unsafe and Strict and
        /// can have some security issues if the DSA public key gets lost in the application.
        /// </summary>
        UseIfPossible = 2,

        /// <summary>
        /// Every file has to be signed. This means the DSA public key must exist. This is the default mode.
        /// </summary>
        Strict = 3,
    }
}
