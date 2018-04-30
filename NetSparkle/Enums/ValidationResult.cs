namespace NetSparkle.Enums
{
    /// <summary>
    /// Return value of the DSA verification check functions.
    /// </summary>
    public enum ValidationResult
    {
        /// <summary>
        /// The DSA public key and signature exists and they are valid.
        /// </summary>
        Valid = 1,

        /// <summary>
        /// Depending on the SecirityMode at least one of DSA public key or the signature dosn't exist or
        /// they exists but they are not valid. In this case the file will be rejected.
        /// </summary>
        Invalid = 2,

        /// <summary>
        /// There wasn't any DSA public key or signature and SecurityMode said this is okay.
        /// </summary>
        Unchecked = 3,
    }
}
