using NetSparkleUpdater;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
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
//using CodeHollow.FeedReader;
using System.Threading.Tasks;

namespace NetSparkleUpdater.Tools.AppCastGenerator
{
    class Program
    {
        public class Options
        {
            [Option('e', "ext", SetName = "local", Required = false, HelpText = "Search for file extensions.", Default = "exe")]
            public string Extension { get; set; }

            [Option('b', "binaries", SetName = "local", Required = false, HelpText = "Directory containing binaries.")]
            public string SourceBinaryDirectory { get; set; }

            [Option('a', "appcast-output-directory", Required = false, HelpText = "Directory to write appcast.xml")]
            public string OutputDirectory { get; set; }

            //[Option('g', "github-atom-feed", SetName = "github", Required = false, HelpText = "Generate from Github release atom feed (signatures not supported yet)")]
            //public string GithubAtomFeed { get; set; }

            [Option('f', "file-extract-version", SetName = "local", Required = false, HelpText = "Determine the version from the file name", Default = false)]
            public bool FileExtractVersion { get; set; }

            [Option('o', "os", Required = false, HelpText = "Operating System (windows,macos,linux)", Default = "windows")]
            public string OperatingSystem { get; set; }

            [Option('u', "base-url", SetName = "local", Required = false, HelpText = "Base URL for downloads", Default = "")]
            public string BaseUrl { get; set; }

            [Option('l', "change-log-url", SetName = "local", Required = false, HelpText = "File path to Markdown changelog files (expected extension: .md; version must match AssemblyVersion).", Default = "")]
            public string ChagenLogUrl { get; set; }

            [Option('p', "change-log-path", SetName = "local", Required = false, HelpText = "", Default = "")]
            public string ChangeLogPath { get; set; }

            [Option('k', "private-key", Required = false, HelpText = "Private key path")]
            public string PrivateKeyPath { get; set; }

            [Option('n', "product-name", Required = true, HelpText = "Product Name")]
            public string ProductName { get; set; }

            [Option('x', "url-prefix-version", SetName = "local", Required = false, HelpText = "Add the version as a prefix to the download url")]
            public bool PrefixVersion { get; set; }

        }


        private static string[] _operatingSystems = new string[] { "windows", "mac", "linux" };

        static void Main(string[] args)
        {
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

            var search = $"*.{opts.Extension}";
            var binaries = Directory.GetFiles(opts.SourceBinaryDirectory, search);

            if (binaries.Length == 0)
            {
                Console.WriteLine($"No files founds matching {search}");
                Environment.Exit(1);
            }

            if (!_operatingSystems.Any(opts.OperatingSystem.Contains))
            {
                Console.WriteLine($"Invalid operating system: {opts.OperatingSystem}", Color.Red);
                Console.WriteLine($"Valid options are : windows, macos or linux");
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(opts.OutputDirectory))
            {
                opts.OutputDirectory = opts.SourceBinaryDirectory;
            }

            Console.WriteLine("");
            Console.WriteLine($"Operating System: {opts.OperatingSystem}", Color.Blue);
            Console.WriteLine($"Searching: {opts.SourceBinaryDirectory}", Color.Blue);
            Console.WriteLine($"Found {binaries.Count()} {opts.Extension} files(s)", Color.Blue);
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

                    if(version == null)
                    {
                        Console.WriteLine($"Unable to determine version of binary {fileInfo.Name}, try -f parameter");
                        Console.WriteLine();
                        Environment.Exit(1);
                    }

                    
                    var dsaSignature = Utilities.GetDSASignature(binary, opts.PrivateKeyPath);
                    var productVersion = version;
                    var itemTitle = string.IsNullOrWhiteSpace(productName) ? productVersion : productName + " " + productVersion;
                    var remoteUpdateFile = $"{opts.BaseUrl}/{(opts.PrefixVersion ? $"{version}/" :"")}{HttpUtility.UrlEncode(fileInfo.Name)})";

                    // changelog stuff
                    var changelogFileName = productVersion + ".md";
                    var changelogPath = Path.Combine(opts.ChangeLogPath, changelogFileName);
                    var hasChangelogForFile = usesChangelogs && File.Exists(changelogPath);
                    var changelogDSA = "";

                    if (hasChangelogForFile)
                    {
                        changelogDSA = Utilities.GetDSASignature(changelogPath, opts.PrivateKeyPath);
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
                        DownloadSignature = dsaSignature,
                        OperatingSystemString = opts.OperatingSystem,
                        MIMEType = MimeTypes.GetMimeType(fileInfo.Name)
                    };

                    if (hasChangelogForFile)
                    {
                        if (!string.IsNullOrWhiteSpace(opts.ChagenLogUrl))
                        {
                            item.ReleaseNotesSignature = changelogDSA;
                            item.ReleaseNotesLink = opts.ChagenLogUrl + changelogFileName;
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

                if (!string.IsNullOrEmpty(opts.PrivateKeyPath))
                {
                    var signature = Utilities.GetDSASignature(appcastFileName, opts.PrivateKeyPath);
                    if (!string.IsNullOrEmpty(signature))
                    {
                        Console.WriteLine("Writing signature to {0}.signature", appcastFileName, Color.Green);
                        File.WriteAllText(appcastFileName + ".signature", signature);
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid signature generated. Do {0} and {1} exist?", appcastFileName, opts.PrivateKeyPath, Color.Red);
                        Environment.Exit(1);
                    }
                } else
                {
                    Console.WriteLine("Skipped generating signature", Color.Red);
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

        private static async Task GenerateFromAtom(Options opts)
        {
            //var feed = await FeedReader.ReadAsync(opts.GithubAtomFeed);
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
