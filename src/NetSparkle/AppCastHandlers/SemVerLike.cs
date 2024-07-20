#nullable enable

using System;
using System.Text.RegularExpressions;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// A complete version representation which keeps both SemVer and .NET AssemblyVersion Version styles
    /// </summary>
    public class SemVerLike : IComparable<SemVerLike>
    {
        /// <summary>
        /// e.g.
        /// `1.0.0` for semver,
        /// `1.0.0.0` for .NET style AssemblyVersion.
        /// Keep untouched in order to keep these variants correctly.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// e.g.
        /// `-alpha.1`, `+123`, `-beta.1+123` and so on for semver.
        /// `String.Empty` for .NET style AssemblyVersion.
        /// </summary>
        public string AllSuffixes { get; }

        /// <summary>
        /// Get string representation of this sem ver like.
        /// </summary>
        /// <returns>Complete version</returns>
        public override string ToString() => $"{Version}{AllSuffixes}";

        /// <summary>
        /// Compare version
        /// </summary>
        /// <param name="other">Another version</param>
        /// <returns>-1, 0 or 1</returns>
        public int CompareTo(SemVerLike? other)
        {
            if (other == null)
            {
                return 1;
            }
            int diff;
            if ((diff = TextHelper.ExpandDigits(Version).CompareTo(TextHelper.ExpandDigits(other.Version))) == 0)
            {
                // `1.0.0` is newer than `1.0.0-alpha.1`.
                if ((diff = (AllSuffixes.Length == 0).CompareTo(other.AllSuffixes.Length == 0)) == 0)
                {
                    diff = TextHelper.ExpandDigits(AllSuffixes).CompareTo(TextHelper.ExpandDigits(other.AllSuffixes));
                }
            }
            return diff;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is SemVerLike smv && CompareTo(smv) == 0;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Create new instance of SemVerLike
        /// </summary>
        /// <param name="version"></param>
        /// <param name="allSuffixes"></param>
        public SemVerLike(string? version, string? allSuffixes)
        {
            Version = version ?? "";
            AllSuffixes = allSuffixes ?? "";
        }

        /// <summary>
        /// Parse a version.
        /// </summary>
        /// <param name="version">Version representation</param>
        /// <returns>A valid SemVerLike instance anyway</returns>
        public static SemVerLike Parse(string version)
        {
            int mark = version.IndexOfAny(new char[] { '-', '+', });
            if (mark == -1)
            {
                return new SemVerLike(version, "");
            }
            else
            {
                return new SemVerLike(version.Substring(0, mark), version.Substring(mark));
            }
        }

        private static class TextHelper
        {
            private static Regex _detectDigits = new Regex("\\d+");

            /// <summary>
            /// Expand all digits so that they can be compared in dictionary order:
            /// `1.2.3`   → `0000000001.0000000002.0000000003`
            /// `100.0.0` → `0000000100.0000000000.0000000000`
            /// </summary>
            /// <param name="str">String to expand</param>
            /// <param name="totalWidth">Total amount of digits (chars) for resulting string</param>
            /// <returns>Expanded digit string</returns>
            internal static string ExpandDigits(string str, int totalWidth = 10)
            {
                return _detectDigits.Replace(str, match => match.Value.PadLeft(totalWidth, '0'));
            }
        }
    }
}
