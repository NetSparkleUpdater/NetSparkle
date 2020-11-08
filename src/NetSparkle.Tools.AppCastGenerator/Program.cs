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
            public string Extension { get; set; }

            [Option('b', "binaries", SetName = "local", Required = false, HelpText = "Directory containing binaries.", Default = ".")]
            public string SourceBinaryDirectory { get; set; }

            //[Option('g', "github-atom-feed", SetName = "github", Required = false, HelpText = "Generate from Github release atom feed (signatures not supported yet)")]
            //public string GithubAtomFeed { get; set; }

            [Option('f', "file-extract-version", SetName = "local", Required = false, HelpText = "Determine the version from the file name", Default = false)]
            public bool FileExtractVersion { get; set; }

            [Option('o', "os", Required = false, HelpText = "Operating System (windows, macos, linux)", Default = "windows")]
            public string OperatingSystem { get; set; }

            [Option('u', "base-url", SetName = "local", Required = false, HelpText = "Base URL for downloads", Default = null)]
            public Uri BaseUrl { get; set; }

            [Option('l', "change-log-url", SetName = "local", Required = false, HelpText = "Base URL to the location for your changelog files on some server for downloading", Default = "")]
            public string ChangeLogUrl { get; set; }

            [Option('p', "change-log-path", SetName = "local", Required = false, HelpText = "File path to Markdown changelog files (expected extension: .md; version must match AssemblyVersion, e.g. MyApp 1.0.0.md).", Default = "")]
            public string ChangeLogPath { get; set; }

            [Option('n', "product-name", Required = false, HelpText = "Product Name", Default = "Application")]
            public string ProductName { get; set; }

            [Option('x', "url-prefix-version", SetName = "local", Required = false, HelpText = "Add the version as a prefix to the download url", Default = false)]
            public bool PrefixVersion { get; set; }

            [Option("key-path", SetName = "local", Required = false, HelpText = "Path to NetSparkle_Ed25519.priv and NetSparkle_Ed25519.pub files")]
            public string PathToKeyFiles { get; set; }


            #region Key Generation

            [Option("generate-keys", SetName = "keys", Required = false, HelpText = "Generate keys", Default = false)]
            public bool GenerateKeys { get; set; }

            [Option("force", SetName = "keys", Required = false, HelpText = "Force regeneration of keys", Default = false)]
            public bool ForceRegeneration { get; set; }

            [Option("export", SetName = "keys", Required = false, HelpText = "Export keys", Default = false)]
            public bool Export { get; set; }

            #endregion

            #region Getting Signatures for Binaries

            [Option("generate-signature", SetName = "signing", Required = false, HelpText = "Generate signature from binary")]
            public string BinaryToSign { get; set; }

            #endregion

            #region Verifying Binary Signatures

            [Option("verify", SetName = "verify", Required = false, HelpText = "Binary to verify")]
            public string BinaryToVerify { get; set; }

            [Option("signature", SetName = "verify", Required = false, HelpText = "Signature")]
            public string Signature { get; set; }

            #endregion

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


            var search = $"*.{opts.Extension}";

            if (opts.SourceBinaryDirectory == ".")
            {
                opts.SourceBinaryDirectory = Environment.CurrentDirectory;
            }

            var binaries = Directory.GetFiles(opts.SourceBinaryDirectory, search);

            if (binaries.Length == 0)
            {
                Console.WriteLine($"No files founds matching {search} in {opts.SourceBinaryDirectory}", Color.Yellow);
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
            Console.WriteLine($"Found {binaries.Count()} {opts.Extension} files(s)", Color.LightBlue);
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

                    var urlEncodedFileName = HttpUtility.UrlEncode(fileInfo.Name);
                    var urlToUse = opts.BaseUrl.ToString().EndsWith("/")
                        ? opts.BaseUrl.ToString()
                        : opts.BaseUrl + "/";
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
                        Title = itemTitle,
                        DownloadLink = remoteUpdateFile,
                        Version = productVersion,
                        ShortVersion = productVersion.Substring(0, productVersion.LastIndexOf('.')),
                        PublicationDate = fileInfo.CreationTime,
                        UpdateSize = fileInfo.Length,
                        Description = "",
                        DownloadSignature = _signatureManager.KeysExist() ? _signatureManager.GetSignatureForFile(fileInfo) : null,
                        OperatingSystemString = opts.OperatingSystem,
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
                            item.ReleaseNotesLink = opts.ChangeLogUrl + changelogFileName;
                        }
                        else
                        {
                            item.Description = File.ReadAllText(changelogPath);
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
                    var signatureFile = appcastFileName + ".signature";
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


    }
}
