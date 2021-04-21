namespace NetSparkleUpdater.Enums
{
    /// <summary>
    /// Return value of the signature verification check functions.
    /// </summary>
    public enum ValidationResult
    {
        /// <summary>
        /// The public key and signature both exist and they are valid (the update file
        /// is safe to use).
        /// </summary>
        Valid = 1,

        /// <summary>
        /// Depending on the <see cref="SecurityMode"/> used, either the public key or the 
        /// signature doesn't exist -- or they exist but are not valid. 
        /// In this case the update file will be rejected.
        /// </summary>
        Invalid = 2,

        /// <summary>
        /// There wasn't any public key or signature available, and this is OK based on the 
        /// <see cref="SecurityMode"/> used.
        /// </summary>
        Unchecked = 3,
    }
}
