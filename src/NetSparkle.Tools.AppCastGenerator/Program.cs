using NetSparkleUpdater;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using NetSparkleUpdater.AppCastHandlers;
using System.Text;
using CommandLine;
using System.Text.RegularExpressions;
using System.Linq;
using Console = Colorful.Console;
using System.Drawing;
using NetSparkleUpdater.AppCastGenerator;
using System.Web;
using System.ComponentModel;

namespace NetSparkleUpdater.Tools.AppCastGenerator
{
    internal class Program
    {
        public class Options
        {
            [Option('a', "appcast-output-directory", Required = false, HelpText = "Directory to write appcast.xml")]
            public string OutputDirectory { get; set; }

            [Option('e', "ext", SetName = "local", Required = false, HelpText = "Search for file extensions.", Default = "exe")]
            public string Extensions { get; set; }

            [Option('b', "binaries", SetName = "local", Required = false, HelpText = "Directory containing binaries.", Default = ".")]
            public string SourceBinaryDirectory { get; set; }

            [Option('r', "search-binary-subdirectories", SetName = "local", Required = false, HelpText = "Search subdirectories of --binaries for binaries", Default = false)]
            public bool SearchBinarySubDirectories { get; set; }

            //[Option('g', "github-atom-feed", SetName = "github", Required = false, HelpText = "Generate from Github release atom feed (signatures not supported yet)")]
            //public string GithubAtomFeed { get; set; }

            [Option('f', "file-extract-version", SetName = "local", Required = false, HelpText = "Determine the version from the file name", Default = false)]
            public bool FileExtractVersion { get; set; }

            [Option('o', "os", Required = false, HelpText = "Operating System (windows, macos, linux)", Default = "windows")]
            public string OperatingSystem { get; set; }

            [Option('u', "base-url", SetName = "local", Required = false, HelpText = "Base URL for downloads", Default = null)]
            public Uri BaseUrl { get; set; }

            [Option('l', "change-log-url", SetName = "local", Required = false, HelpText = "Base URL to the location for your changelog files on " +
                "some server for downloading", Default = "")]
            public string ChangeLogUrl { get; set; }

            [Option('p', "change-log-path", SetName = "local", Required = false, HelpText = "File path to Markdown changelog files (expected extension: .md; " +
                "version must match AssemblyVersion, e.g. MyApp 1.0.0.md).", Default = "")]
            public string ChangeLogPath { get; set; }

            [Option('n', "product-name", Required = false, HelpText = "Product Name", Default = "Application")]
            public string ProductName { get; set; }

            [Option('x', "url-prefix-version", SetName = "local", Required = false, HelpText = "Add the version as a prefix to the download url", Default = false)]
            public bool PrefixVersion { get; set; }

            [Option("key-path", SetName = "local", Required = false, HelpText = "Path to NetSparkle_Ed25519.priv and NetSparkle_Ed25519.pub files")]
            public string PathToKeyFiles { get; set; }

            [Option("signature-file-extension", SetName = "local", Required = false,
                HelpText = "Suffix (without '.') to append to appcast.xml for signature file",
                Default = "signature")]
            public string SignatureFileExtension { get; set; }

            [Option("public-key-override", SetName = "local", Required = false, HelpText = "Public key override (ignores whatever is in the public key file) for signing binaries. This" +
                " overrides ALL other public keys set when verifying binaries, INCLUDING public key set via environment variables! " +
                "If not set, uses --key-path (if set) or the default SignatureManager location. Not used in --generate-keys or --export.", Default = "")]
            public string PublicKeyOverride { get; set; }

            [Option("private-key-override", SetName = "local", Required = false, HelpText = "Private key override (ignores whatever is in the private key file) for signing binaries. This" +
                " overrides ALL other private keys set when signing binaries, INCLUDING private key set via environment variables! " +
                "If not set, uses --key-path (if set) or the default SignatureManager location. Not used in --generate-keys or --export.", Default = "")]
            public string PrivateKeyOverride { get; set; }


            #region Key Generation

            [Option("generate-keys", SetName = "keys", Required = false, HelpText = "Generate keys", Default = false)]
            public bool GenerateKeys { get; set; }

            [Option("force", SetName = "keys", Required = false, HelpText = "Force regeneration of keys", Default = false)]
            public bool ForceRegeneration { get; set; }

            [Option("export", SetName = "keys", Required = false, HelpText = "Export keys", Default = false)]
            public bool Export { get; set; }

            #endregion

            #region Getting Signatures for Binaries

            [Option("generate-signature", SetName = "signing", Required = false, HelpText = "File path to binary to generate a signature for")]
            public string BinaryToSign { get; set; }

            #endregion

            #region Verifying Binary Signatures

            [Option("verify", SetName = "verify", Required = false, HelpText = "Path to file to verify")]
            public string BinaryToVerify { get; set; }

            [Option("signature", SetName = "verify", Required = false, HelpText = "Signature of file to verify")]
            public string Signature { get; set; }

            #endregion


            [Option("show-examples", SetName = "local", Required = false, HelpText = "Show extended examples", Default = false)]
            public bool ShowExtendedExamples { get; set; }
        }


        private static readonly string[] _operatingSystems = new string[] { "windows", "mac", "linux" };
        private static readonly SignatureManager _signatureManager = new SignatureManager();

        static void Main(string[] args)
        {
            // By default, if no args given, print help
            if (args.Count() == 0)
            {
                args = new string[] { "--help" };
            }
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run)
                .WithNotParsed(HandleParseError);
        }
        static void Run(Options opts)
        {
            /*
            if (opts.GithubAtomFeed != null)
            {
                GenerateFromAtom(opts);
                return;
            }*/

            if (opts.ShowExtendedExamples)
            {
                PrintExtendedExamples();
                return;
            }

            if (!string.IsNullOrWhiteSpace(opts.PathToKeyFiles))
            {
                _signatureManager.SetStorageDirectory(opts.PathToKeyFiles);
            }

            if (opts.Export)
            {
                Console.WriteLine("Private Key:");
                Console.WriteLine(Convert.ToBase64String(_signatureManager.GetPrivateKey()));
                Console.WriteLine("Public Key:");
                Console.WriteLine(Convert.ToBase64String(_signatureManager.GetPublicKey()));
                return;
            }

            if (opts.GenerateKeys)
            {
                var didSucceed = _signatureManager.Generate(opts.ForceRegeneration);
                if (didSucceed)
                {
                    Console.WriteLine("Keys successfully generated", Color.Green);
                }
                else
                {
                    Console.WriteLine("Keys failed to generate", Color.Red);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(opts.PublicKeyOverride))
            {
                _signatureManager.SetPublicKeyOverride(opts.PublicKeyOverride);
            }
            if (!string.IsNullOrWhiteSpace(opts.PrivateKeyOverride))
            {
                _signatureManager.SetPrivateKeyOverride(opts.PrivateKeyOverride);
            }

            if (opts.BinaryToSign != null)
            {
                var signature = _signatureManager.GetSignatureForFile(new FileInfo(opts.BinaryToSign));

                Console.WriteLine($"Signature: {signature}", Color.Green);

                return;
            }

            if (opts.BinaryToVerify != null)
            {
                var result = _signatureManager.VerifySignature(new FileInfo(opts.BinaryToVerify), opts.Signature);

                if (result)
                {
                    Console.WriteLine($"Signature valid", Color.Green);
                } 
                else
                {
                    Console.WriteLine($"Signature invalid", Color.Red);
                }

                return;
            }


            if (opts.SourceBinaryDirectory == ".")
            {
                opts.SourceBinaryDirectory = Environment.CurrentDirectory;
            }
            var searches = opts.Extensions.Split(",").ToList()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(extension => $"*.{extension.Trim()}");
            var binaries = searches
                .SelectMany(search => Directory.GetFiles(opts.SourceBinaryDirectory, search,
                            opts.SearchBinarySubDirectories 
                            ? SearchOption.AllDirectories 
                            : SearchOption.TopDirectoryOnly));
            if (!binaries.Any())
            {
                Console.WriteLine($"No files founds matching {string.Join(",", searches)} in {opts.SourceBinaryDirectory}", Color.Yellow);
                Environment.Exit(1);
            }

            if (!_operatingSystems.Any(opts.OperatingSystem.Contains))
            {
                Console.WriteLine($"Invalid operating system: {opts.OperatingSystem}", Color.Red);
                Console.WriteLine($"Valid options are: {0}", string.Join(", ", _operatingSystems));
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(opts.OutputDirectory))
            {
                opts.OutputDirectory = opts.SourceBinaryDirectory;
            }

            Console.WriteLine("");
            Console.WriteLine($"Operating System: {opts.OperatingSystem}", Color.LightBlue);
            Console.WriteLine($"Searching: {opts.SourceBinaryDirectory}", Color.LightBlue);
            Console.WriteLine($"Found {binaries.Count()} {string.Join(",", searches)} files(s)", Color.LightBlue);
            Console.WriteLine("");

            try
            {

                var productName = opts.ProductName;

                var items = new List<AppCastItem>();

                var usesChangelogs = !string.IsNullOrWhiteSpace(opts.ChangeLogPath) && Directory.Exists(opts.ChangeLogPath);

                foreach (var binary in binaries)
                {
                    string version = null;
                    var fileInfo = new FileInfo(binary);

                    if (opts.FileExtractVersion)
                    {
                        version = GetVersionFromName(fileInfo);
                    }
                    else
                    {
                        version = GetVersionFromAssembly(fileInfo);

                    }

                    if (version == null)
                    {
                        Console.WriteLine($"Unable to determine version of binary {fileInfo.Name}, try -f parameter");
                        Environment.Exit(1);
                    }

                    var productVersion = version;
                    var itemTitle = string.IsNullOrWhiteSpace(productName) ? productVersion : productName + " " + productVersion;

                    var urlEncodedFileName = Uri.EscapeDataString(fileInfo.Name);
                    var urlToUse = !string.IsNullOrWhiteSpace(opts.BaseUrl?.ToString()) 
                        ? (opts.BaseUrl.ToString().EndsWith("/") ? opts.BaseUrl.ToString() : opts.BaseUrl + "/") 
                        : "";
                    if (opts.PrefixVersion)
                    {
                        urlToUse += $"{version}/";
                    }
                    if (urlEncodedFileName.StartsWith("/") && urlEncodedFileName.Length > 1)
                    {
                        urlEncodedFileName = urlEncodedFileName.Substring(1);
                    }
                    var remoteUpdateFile = $"{urlToUse}{urlEncodedFileName}";

                    // changelog stuff
                    var changelogFileName = productVersion + ".md";
                    var changelogPath = Path.Combine(opts.ChangeLogPath, changelogFileName);
                    var hasChangelogForFile = usesChangelogs && File.Exists(changelogPath);
                    var changelogSignature = "";

                    if (hasChangelogForFile)
                    {
                        changelogSignature = _signatureManager.GetSignatureForFile(changelogPath);
                    }

                    //
                    var item = new AppCastItem()
                    {
                        Title = itemTitle?.Trim(),
                        DownloadLink = remoteUpdateFile?.Trim(),
                        Version = productVersion?.Trim(),
                        ShortVersion = productVersion?.Substring(0, productVersion.LastIndexOf('.'))?.Trim(),
                        PublicationDate = fileInfo.CreationTime,
                        UpdateSize = fileInfo.Length,
                        Description = "",
                        DownloadSignature = _signatureManager.KeysExist() ? _signatureManager.GetSignatureForFile(fileInfo) : null,
                        OperatingSystemString = opts.OperatingSystem?.Trim(),
                        MIMEType = MimeTypes.GetMimeType(fileInfo.Name)
                    };

                    if (hasChangelogForFile)
                    {
                        if (!string.IsNullOrWhiteSpace(opts.ChangeLogUrl))
                        {
                            item.ReleaseNotesSignature = changelogSignature;
                            var changeLogUrlBase = opts.ChangeLogUrl.EndsWith("/") || changelogFileName.StartsWith("/") 
                                ? opts.ChangeLogUrl
                                : opts.ChangeLogUrl + "/";
                            item.ReleaseNotesLink = (Path.Combine(opts.ChangeLogUrl, changelogFileName)).Trim();
                        }
                        else
                        {
                            item.Description = File.ReadAllText(changelogPath).Trim();
                        }
                    }

                    items.Add(item);
                }

                var appcastXmlDocument = XMLAppCast.GenerateAppCastXml(items, productName);

                var appcastFileName = Path.Combine(opts.OutputDirectory, "appcast.xml");

                var dirName = Path.GetDirectoryName(appcastFileName);

                if (!Directory.Exists(dirName))
                {
                    Console.WriteLine("Creating {0}", dirName);
                    Directory.CreateDirectory(dirName);
                }

                Console.WriteLine("Writing appcast to {0}", appcastFileName);

                using (var w = XmlWriter.Create(appcastFileName, new XmlWriterSettings { NewLineChars = "\n", Encoding = new UTF8Encoding(false) }))
                {
                    appcastXmlDocument.Save(w);
                }

                if (_signatureManager.KeysExist())
                {
                    var appcastFile = new FileInfo(appcastFileName);
                    var extension = opts.SignatureFileExtension?.TrimStart('.') ?? "signature";
                    var signatureFile = appcastFileName + "." + extension;
                    var signature = _signatureManager.GetSignatureForFile(appcastFile);

                    var result = _signatureManager.VerifySignature(appcastFile, signature);

                    if (result)
                    {
                        
                        File.WriteAllText(signatureFile, signature);
                        Console.WriteLine($"Wrote {signatureFile}", Color.Green);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to verify {signatureFile}", Color.Red);
                    }

                } 
                else
                {
                    Console.WriteLine("Skipped generating signature.  Generate keys with --generate-keys", Color.Red);
                    Environment.Exit(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Environment.Exit(1);
            }


        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            errs.Output();
        }

        static string GetVersionFromName(FileInfo fileInfo)
        {
            var regexPattern = @"\d+(\.\d+)+";
            var regex = new Regex(regexPattern);

            var match = regex.Match(fileInfo.FullName);

            return match.Captures[match.Captures.Count - 1].Value; // get the numbers at the end of the string incase the app is something like 1.0application1.0.0.dmg
        }

        static string GetVersionFromAssembly(FileInfo fileInfo)
        {
            return FileVersionInfo.GetVersionInfo(fileInfo.FullName).ProductVersion;
        }

        static void PrintExtendedExamples()
        {
            var examples = @"
#### Key Generation
# Generate Ed25519 keys for the first time
netsparkle-generate-appcast --generate-keys
# Store keys in a custom location
netsparkle-generate-appcast --key-path path/to/store/keys
# Pass in public key via command line
netsparkle-generate-appcast --public-key-override [YourPublicKeyHere]
# Pass in private key via command line
netsparkle-generate-appcast --private-key-override [YourPrivateKeyHere]

# By default, your Ed25519 signatures are stored on disk in your local 
# application data folder in a subdirectory called `netsparkle`. 
# If you want to export your keys to the console, you can do:
netsparkle-generate-appcast --export

# You can also store your keys in the following environment variables:
# set public key: SPARKLE_PUBLIC_KEY
# set private key: SPARKLE_PRIVATE_KEY

#### Generate a signature for a binary without creating an app cast:
netsparkle-generate-appcast --generate-signature path/to/binary.exe

#### Verifying Binaries
netsparkle-generate-appcast --verify path/to/binary.exe --signature base_64_signature

#### Using a custom key location:
# If your keys are sitting on disk somewhere
# (`NetSparkle_Ed25519.priv` and `NetSparkle_Ed25519.pub` -- both 
# in base 64 and both on disk in the same folder!), you can pass in 
# the path to these keys like this:
netsparkle-generate-appcast --key-path path/to/keys/

#### Generating an app cast

# Generate an app cast for Windows executables that are sitting in a 
# specific directory
netsparkle-generate-appcast -a directory/for/appcast/output/ -e exe -b directory/with/binaries/ -o windows

# Add change log info to your app cast
netsparkle-generate-appcast -b binary/folder -p change/log/folder

# Customize download URL for binaries and change logs
netsparkle-generate-appcast -b binary/folder -p change/log/folder -u https://example.com/downloads -p https://example.com/downloads/changelogs

# Set your application name for the app cast
netsparkle-generate-appcast -n ""My Awesome App"" -b binary/folder

# Use file versions in file names, e.g. for apps like ""My App 1.2.1.dmg""
netsparkle-generate-appcast - n ""macOS version"" - o macos - f true - b binary / folder - e dmg

";
            Console.Write(examples);
        }
    }
}
