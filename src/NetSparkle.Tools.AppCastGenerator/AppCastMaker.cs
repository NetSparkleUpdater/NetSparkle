using NetSparkleUpdater.AppCastHandlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Console = Colorful.Console;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class AppCastMaker
    {
        private static readonly string[] _operatingSystems = new string[] { "windows", "mac", "linux" };
        
        private Options _opts;
        private SignatureManager _signatureManager;

        public AppCastMaker(SignatureManager signatureManager, Options options)
        {
            _opts = options;
            _signatureManager = signatureManager;
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

        public void Run()
        {
            if (_opts.SourceBinaryDirectory == ".")
            {
                _opts.SourceBinaryDirectory = Environment.CurrentDirectory;
            }
            var searches = _opts.Extensions.Split(",").ToList()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(extension => $"*.{extension.Trim()}");
            var binaries = searches
                .SelectMany(search => Directory.GetFiles(_opts.SourceBinaryDirectory, search,
                            _opts.SearchBinarySubDirectories
                            ? SearchOption.AllDirectories
                            : SearchOption.TopDirectoryOnly));
            if (!binaries.Any())
            {
                Console.WriteLine($"No files founds matching {string.Join(",", searches)} in {_opts.SourceBinaryDirectory}", Color.Yellow);
                Environment.Exit(1);
            }

            if (!_operatingSystems.Any(_opts.OperatingSystem.Contains))
            {
                Console.WriteLine($"Invalid operating system: {_opts.OperatingSystem}", Color.Red);
                Console.WriteLine($"Valid options are: {0}", string.Join(", ", _operatingSystems));
                Environment.Exit(1);
            }

            if (string.IsNullOrEmpty(_opts.OutputDirectory))
            {
                _opts.OutputDirectory = _opts.SourceBinaryDirectory;
            }

            Console.WriteLine("");
            Console.WriteLine($"Operating System: {_opts.OperatingSystem}", Color.LightBlue);
            Console.WriteLine($"Searching: {_opts.SourceBinaryDirectory}", Color.LightBlue);
            Console.WriteLine($"Found {binaries.Count()} {string.Join(",", searches)} files(s)", Color.LightBlue);
            Console.WriteLine("");

            try
            {

                var productName = _opts.ProductName;

                var items = new List<AppCastItem>();

                var appcastFileName = Path.Combine(_opts.OutputDirectory, "appcast.xml");

                var dirName = Path.GetDirectoryName(appcastFileName);

                if (!Directory.Exists(dirName))
                {
                    Console.WriteLine("Creating {0}", dirName);
                    Directory.CreateDirectory(dirName);
                }

                if (_opts.ReparseExistingAppCast)
                {
                    Console.WriteLine("Parsing existing app cast at {0}...", appcastFileName);
                    if (!File.Exists(appcastFileName))
                    {
                        Console.WriteLine("App cast does not exist at {0}, so creating it anew...", appcastFileName, Color.Red);
                    }
                    else
                    {
                        XDocument doc = XDocument.Parse(File.ReadAllText(appcastFileName));

                        // for any .xml file, there is a product name - we can pull this out automatically when there is just one channel.
                        List<XElement> allTitles = doc.Root?.Element("channel")?.Elements("title")?.ToList() ?? new List<XElement>();
                        if (allTitles.Count == 1 && !string.IsNullOrWhiteSpace(allTitles[0].Value))
                        {
                            productName = allTitles[0].Value;
                            Console.WriteLine("Using title in app cast: {0}...", productName, Color.LightBlue);
                        }

                        var docDescendants = doc.Descendants("item");
                        var logWriter = new LogWriter(true);
                        foreach (var item in docDescendants)
                        {
                            var currentItem = AppCastItem.Parse("", "", "/", item, logWriter);
                            Console.WriteLine("Found an item in the app cast: version {0} ({1}) -- os = {2}",
                                currentItem?.Version, currentItem?.ShortVersion, currentItem.OperatingSystemString);
                            items.Add(currentItem);
                        }
                    }
                }

                var usesChangelogs = !string.IsNullOrWhiteSpace(_opts.ChangeLogPath) && Directory.Exists(_opts.ChangeLogPath);

                foreach (var binary in binaries)
                {
                    string version = null;
                    var fileInfo = new FileInfo(binary);

                    if (_opts.FileExtractVersion)
                    {
                        version = GetVersionFromName(fileInfo);
                    }
                    else
                    {
                        version = GetVersionFromAssembly(fileInfo);

                    }

                    if (version == null)
                    {
                        Console.WriteLine($"Unable to determine version of binary {fileInfo.Name}, try -f parameter to determine version from file name", Color.Red);
                        Environment.Exit(1);
                    }

                    var productVersion = version;
                    var itemFoundInAppcast = items.Where(x => x.Version != null && x.Version == productVersion?.Trim()).FirstOrDefault();
                    if (itemFoundInAppcast != null && _opts.OverwriteOldItemsInAppcast)
                    {
                        Console.WriteLine("Removing existing app cast item with version {0} so we can add the version on disk to the app cast...", productVersion);
                        items.Remove(itemFoundInAppcast); // remove old item.
                        itemFoundInAppcast = null;
                    }
                    if (itemFoundInAppcast == null)
                    {
                        var itemTitle = string.IsNullOrWhiteSpace(productName) ? productVersion : productName + " " + productVersion;

                        var urlEncodedFileName = Uri.EscapeDataString(fileInfo.Name);
                        var urlToUse = !string.IsNullOrWhiteSpace(_opts.BaseUrl?.ToString())
                            ? (_opts.BaseUrl.ToString().EndsWith("/") ? _opts.BaseUrl.ToString() : _opts.BaseUrl + "/")
                            : "";
                        if (_opts.PrefixVersion)
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
                        var changelogPath = Path.Combine(_opts.ChangeLogPath, changelogFileName);
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
                            OperatingSystemString = _opts.OperatingSystem?.Trim(),
                            MIMEType = MimeTypes.GetMimeType(fileInfo.Name)
                        };

                        if (hasChangelogForFile)
                        {
                            if (!string.IsNullOrWhiteSpace(_opts.ChangeLogUrl))
                            {
                                item.ReleaseNotesSignature = changelogSignature;
                                var changeLogUrlBase = _opts.ChangeLogUrl.EndsWith("/") || changelogFileName.StartsWith("/")
                                    ? _opts.ChangeLogUrl
                                    : _opts.ChangeLogUrl + "/";
                                item.ReleaseNotesLink = (Path.Combine(_opts.ChangeLogUrl, changelogFileName)).Trim();
                            }
                            else
                            {
                                item.Description = File.ReadAllText(changelogPath).Trim();
                            }
                        }
                        items.Add(item);
                    }
                    else
                    {
                        Console.WriteLine("An app cast item with version {0} is already in the file, not adding it again...", productVersion);
                    }
                }

                // order the list by version -- helpful when reparsing app cast to make sure things stay in order
                items.Sort((a, b) => a.Version.CompareTo(b.Version));

                var appcastXmlDocument = XMLAppCast.GenerateAppCastXml(items, productName);

                Console.WriteLine("Writing appcast to {0}", appcastFileName);

                using (var w = XmlWriter.Create(appcastFileName, new XmlWriterSettings { NewLineChars = "\n", Encoding = new UTF8Encoding(false) }))
                {
                    appcastXmlDocument.Save(w);
                }

                if (_signatureManager.KeysExist())
                {
                    var appcastFile = new FileInfo(appcastFileName);
                    var extension = _opts.SignatureFileExtension?.TrimStart('.') ?? "signature";
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
                    Console.WriteLine("Skipped generating signature. Generate keys with --generate-keys", Color.Red);
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
    }
}
