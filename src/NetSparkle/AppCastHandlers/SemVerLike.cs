using System;
using System.Collections.Generic;
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
        /// Compare version. As required by the SemVer spec, build metadata
        /// (everything after a `+`, e.g. the commit hash that .NET 8+ adds to the
        /// informational version) is ignored: `1.0.0+abc` and `1.0.0` have the same precedence.
        /// Pre-release suffixes are compared case-insensitively, identifier by identifier (`beta.2` is older
        /// than `beta.10`; `beta1`, `beta-1` and `beta.1` are the same), and common stages are ordered by
        /// maturity: dev/nightly/canary/snapshot/ci/pre-alpha, alpha, beta, preview/pre, rc, then any other
        /// label. Any pre-release is older than the release itself.
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
                diff = ComparePreRelease(PreReleaseSuffix, other.PreReleaseSuffix);
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
            // must match CompareTo: versions that compare as equal need the same hash code
            return (TextHelper.ExpandDigits(Version) + "|" + string.Join(".", GetPreReleaseIdentifiers(PreReleaseSuffix))).GetHashCode();
        }

        /// <summary>
        /// Create new instance of SemVerLike
        /// </summary>
        /// <param name="version">Version of software (if null, constructor sets to empty string)</param>
        /// <param name="allSuffixes">Suffixes of version, e.g. "-beta-1" (if null, constructor sets to empty string)</param>
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
        public static SemVerLike Parse(string? version)
        {
            int mark = version?.IndexOfAny(new char[] { '-', '+', }) ?? -1;
            if (string.IsNullOrWhiteSpace(version) || mark == -1)
            {
                return new SemVerLike(version, "");
            }
            else
            {
                return new SemVerLike(version?.Substring(0, mark), version?.Substring(mark));
            }
        }

        /// <summary>
        /// The pre-release part of the suffixes (`-beta.1`), without any build metadata (`+123`).
        /// Empty if this is a release version, even if it has build metadata.
        /// </summary>
        internal string PreReleaseSuffix => RemoveBuildMetadata(AllSuffixes);

        private static string RemoveBuildMetadata(string allSuffixes)
        {
            int buildMetadataStart = allSuffixes.IndexOf('+');
            return buildMetadataStart == -1 ? allSuffixes : allSuffixes.Substring(0, buildMetadataStart);
        }

        private static int ComparePreRelease(string preRelease, string otherPreRelease)
        {
            // `1.0.0` is newer than `1.0.0-alpha.1`.
            if (preRelease.Length == 0 || otherPreRelease.Length == 0)
            {
                return (preRelease.Length == 0).CompareTo(otherPreRelease.Length == 0);
            }
            var identifiers = GetPreReleaseIdentifiers(preRelease);
            var otherIdentifiers = GetPreReleaseIdentifiers(otherPreRelease);
            for (int i = 0; i < Math.Min(identifiers.Count, otherIdentifiers.Count); i++)
            {
                int diff = CompareIdentifiers(identifiers[i], otherIdentifiers[i]);
                if (diff != 0)
                {
                    return diff;
                }
            }
            // all shared identifiers are equal: `beta` is older than `beta.1`
            return identifiers.Count.CompareTo(otherIdentifiers.Count);
        }

        /// <summary>
        /// Split a pre-release suffix (`-Beta.2`, `-beta2`, `-pre-alpha`) into lower case identifiers:
        /// numbers (without leading zeros) and words. Separators are dropped and numbers are
        /// split from words, so `beta1`, `beta-1` and `beta.1` give the same identifiers.
        /// </summary>
        private static List<string> GetPreReleaseIdentifiers(string preRelease)
        {
            var lowerCased = preRelease.ToLowerInvariant().Replace("pre-alpha", "prealpha").Replace("pre.alpha", "prealpha");
            var identifiers = new List<string>();
            foreach (Match match in Regex.Matches(lowerCased, "[0-9]+|[^0-9.\\-]+"))
            {
                identifiers.Add(char.IsDigit(match.Value[0]) ? (match.Value.TrimStart('0').Length == 0 ? "0" : match.Value.TrimStart('0')) : match.Value);
            }
            return identifiers;
        }

        private static int CompareIdentifiers(string identifier, string otherIdentifier)
        {
            bool isNumber = char.IsDigit(identifier[0]);
            bool otherIsNumber = char.IsDigit(otherIdentifier[0]);
            if (isNumber && otherIsNumber)
            {
                // compare numbers of any size without parsing them
                int diff = identifier.Length.CompareTo(otherIdentifier.Length);
                return diff != 0 ? diff : string.CompareOrdinal(identifier, otherIdentifier);
            }
            if (isNumber || otherIsNumber)
            {
                return isNumber ? -1 : 1; // numbers are older than words
            }
            int rankDiff = GetStageRank(identifier).CompareTo(GetStageRank(otherIdentifier));
            return rankDiff != 0 ? rankDiff : string.CompareOrdinal(identifier, otherIdentifier);
        }

        /// <summary>
        /// Order common pre-release stages by maturity rather than alphabetically
        /// (otherwise `pre-alpha`, `preview` and `dev` would not be where you expect them to be).
        /// Other labels are newer than all of these and are ordered alphabetically.
        /// </summary>
        private static int GetStageRank(string word)
        {
            switch (word)
            {
                case "dev":
                case "nightly":
                case "canary":
                case "snapshot":
                case "ci":
                case "prealpha":
                    return 0;
                case "alpha":
                    return 1;
                case "beta":
                    return 2;
                case "preview":
                case "pre":
                    return 3;
                case "rc":
                    return 4;
                default:
                    return 5;
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
