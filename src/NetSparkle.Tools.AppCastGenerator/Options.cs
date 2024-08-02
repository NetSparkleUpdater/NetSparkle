using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class Options
    {
        [Option('a', "appcast-output-directory", Required = false, HelpText = "Directory to write appcast.xml")]
        public string? OutputDirectory { get; set; }

        [Option('e', "ext", SetName = "local", Required = false, HelpText = "Search for file extensions. Can be something like " +
            "\"exe\" or include multiple extensions with something like \"exe,msi\" (the extensions list is comma-separated)", Default = "exe")]
        public string? Extensions { get; set; }

        [Option('b', "binaries", SetName = "local", Required = false, HelpText = "Directory containing binaries.", Default = ".")]
        public string? SourceBinaryDirectory { get; set; }

        [Option('r', "search-binary-subdirectories", SetName = "local", Required = false, HelpText = "Search subdirectories of --binaries for binaries", Default = false)]
        public bool SearchBinarySubDirectories { get; set; }

        [Option('f', "file-extract-version", SetName = "local", Required = false, HelpText = "Determine the version from the file path. Tries each string in between directory separators that contains a version number starting from the end of the path. Only searches the last four directory items (incl. the file name) and does not search 'above' the binary directory (-b option). See unit tests for what is parseable.", Default = false)]
        public bool FileExtractVersion { get; set; }

        [Option("file-version", SetName="local", Required = false, HelpText = "Use to set the version for a binary going into an app cast. Note that this version can only be set once, so when generating an app cast, make sure you either: A) have only one binary in your app cast | B) Utilize the --reparse-existing parameter so that old items get picked up. If the generator finds 2 binaries without any known version and --file-version is set, then an error will be emitted.", Default = null)]
        public string? FileVersion { get; set; }

        [Option('o', "os", Required = false, HelpText = "Operating System (windows, macos, linux)", Default = "windows")]
        public string? OperatingSystem { get; set; }

        [Option("description-tag", Required = false, HelpText = "Text to put in the app cast <description> tag", Default = "Most recent changes with links to updates")]
        public string? AppCastDescription { get; set; }

        [Option("link-tag", Required = false, HelpText = "Text to put in the app cast <link> tag. Should be your app cast download URL if you use this.", Default = "")]
        public string? AppCastLink { get; set; }

        [Option('u', "base-url", SetName = "local", Required = false, HelpText = "Base URL for downloads", Default = "")]
        public string? BaseUrl { get; set; }

        [Option('l', "change-log-url", SetName = "local", Required = false, HelpText = "Base URL to the location for your changelog files on " +
            "some server for downloading", Default = "")]
        public string? ChangeLogUrl { get; set; }

        [Option('p', "change-log-path", SetName = "local", Required = false, HelpText = "File path to Markdown changelog files (expected extension: .md; " +
            "version must match AssemblyVersion, e.g. MyApp 1.0.0.md).", Default = "")]
        public string? ChangeLogPath { get; set; }

        [Option("change-log-name-prefix", SetName = "local", Required = false, HelpText = "Prefix for change log file names. By default, the generator searches for file names with the format \"[Version].md\". If you set this to (for example) \"My App\", it will search for file names with the format \"My App [Version].md\" as well as \"[Version].md\".", Default = "")]
        public string? ChangeLogFileNamePrefix { get; set; }

        [Option('n', "product-name", Required = false, HelpText = "Product name. This will be used in the app cast <title>. " +
            "If you use --reparse-existing, then this field will be ignored and the existing product name will be used (if available).", Default = "Application")]
        public string? ProductName { get; set; }

        [Option('x', "url-prefix-version", SetName = "local", Required = false, HelpText = "Add the version as a prefix to the download url", Default = false)]
        public bool PrefixVersion { get; set; }

        [Option("key-path", Required = false, HelpText = "Path to NetSparkle_Ed25519.priv and NetSparkle_Ed25519.pub files")]
        public string? PathToKeyFiles { get; set; }

        [Option("signature-file-extension", SetName = "local", Required = false,
            HelpText = "Suffix (without '.') to append to appcast.xml for signature file. If you change this, make sure to also set AppCastHelper.SignatureFileExtension in the core NetSparkleUpdater lib",
            Default = "signature")]
        public string? SignatureFileExtension { get; set; }

        [Option("public-key-override", SetName = "local", Required = false, HelpText = "Public key override (ignores whatever is in the public key file) for signing binaries. This" +
            " overrides ALL other public keys set when verifying binaries, INCLUDING public key set via environment variables! " +
            "If not set, uses --key-path (if set) or the default SignatureManager location. Not used in --generate-keys or --export.", Default = "")]
        public string? PublicKeyOverride { get; set; }

        [Option("private-key-override", SetName = "local", Required = false, HelpText = "Private key override (ignores whatever is in the private key file) for signing binaries. This" +
            " overrides ALL other private keys set when signing binaries, INCLUDING private key set via environment variables! " +
            "If not set, uses --key-path (if set) or the default SignatureManager location. Not used in --generate-keys or --export.", Default = "")]
        public string? PrivateKeyOverride { get; set; }

        [Option("reparse-existing", SetName = "local", Required = false, HelpText = "Re-parse an existing app cast rather than overriding it and creating it anew. " +
            "Skips versions already in the app cast, so if you deploy a new binary with the same version, you will need to manually " +
            "edit your app cast to remove the old listing" +
            "for the version you are re-deploying.", Default = false)]
        public bool ReparseExistingAppCast { get; set; }

        [Option("overwrite-old-items", SetName = "local", Required = false, HelpText =
            "If--overwrite-old-items is used, this option will cause app cast " +
            "items to be rewritten in the app cast if the a binary on disk with the same version number is found. " +
            "In other words, if 1.0.1 is in the app cast already (either from reparsing or from another binary), " +
            "and another 1.0.1 is found on disk, then the 1.0.1 data in the app cast will be rewritten based on the binary found. " +
            "Note that this means that if you have multiple 1.0.1 versions on disk (which you shouldn't do...), the last one found " +
            "will be the one in your app cast!", Default = false)]
        public bool OverwriteOldItemsInAppcast { get; set; }

        [Option("human-readable", SetName = "local", Required = false, Default = false,
            HelpText = "If true, makes the output app cast file human readable (newslines, indents)")]
        public bool HumanReadableOutput { get; set; }

        [Option("critical-versions", SetName = "local", Required = false, HelpText = "Comma-separated list of versions to mark as critical in the app cast. Must match version text exactly. E.g., \"1.0.2,1.2.3.1\"", Default = "")]
        public string? CriticalVersions { get; set; }

        [Option("output-type", SetName = "local", Required = false, HelpText = "Output type for app cast file (xml or json); defaults to 'xml'", Default = "xml")]
        public string? OutputType { get; set; }

        #region Key Generation

        [Option("generate-keys", SetName = "keys", Required = false, HelpText = "Generate keys", Default = false)]
        public bool GenerateKeys { get; set; }

        [Option("force", SetName = "keys", Required = false, HelpText = "Force regeneration of keys", Default = false)]
        public bool ForceRegeneration { get; set; }

        [Option("export", SetName = "keys", Required = false, HelpText = "Export keys", Default = false)]
        public bool ExportKeys { get; set; }

        #endregion

        #region Getting Signatures for Binaries

        [Option("generate-signature", SetName = "signing", Required = false, HelpText = "File path to binary to generate a signature for")]
        public string? BinaryToSign { get; set; }

        #endregion

        #region Verifying Binary Signatures

        [Option("verify", SetName = "verify", Required = false, HelpText = "Path to file to verify")]
        public string? BinaryToVerify { get; set; }

        [Option("signature", SetName = "verify", Required = false, HelpText = "Signature of file to verify")]
        public string? Signature { get; set; }

        #endregion


        [Option("show-examples", SetName = "local", Required = false, HelpText = "Show extended examples", Default = false)]
        public bool ShowExtendedExamples { get; set; }
    }
}
