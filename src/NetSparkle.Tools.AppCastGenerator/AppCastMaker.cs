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

        public static string GetVersionFromName(FileInfo fileInfo)
        {
            var regexPattern = @"\d+(\.\d+)+";
            var regex = new Regex(regexPattern);

            var match = regex.Match(fileInfo.FullName);

            return match.Captures[match.Captures.Count - 1].Value; // get the numbers at the end of the string incase the app is something like 1.0application1.0.0.dmg
        }

        public static string GetVersionFromAssembly(FileInfo fileInfo)
        {
            return FileVersionInfo.GetVersionInfo(fileInfo.FullName).ProductVersion;
        }

        public IEnumerable<string> GetSearchExtensionsFromString(string extensions)
        {
            return extensions.Split(",").ToList()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Select(extension => $"*.{extension.Trim()}");
        }

        public IEnumerable<string> FindBinaries(string binaryDirectory, IEnumerable<string> extensionsStringForSearch, bool searchSubdirectories)
        {
            if (binaryDirectory == ".")
            {
                binaryDirectory = Environment.CurrentDirectory;
            }
            var searchOption = searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return extensionsStringForSearch.SelectMany(search => Directory.GetFiles(binaryDirectory, search, searchOption));
        }

        public (List<AppCastItem>, string) GetItemsAndProductNameFromExistingAppCast(string appCastFileName)
        {
            Console.WriteLine("Parsing existing app cast at {0}...", appCastFileName);
            var items = new List<AppCastItem>();
            string productName = null;
            if (!File.Exists(appCastFileName))
            {
                Console.WriteLine("App cast does not exist at {0}, so creating it anew...", appCastFileName, Color.Red);
            }
            else
            {
                XDocument doc = XDocument.Parse(File.ReadAllText(appCastFileName));
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
            return (items, productName);
        }

        public string GetVersionForBinary(FileInfo binaryFileInfo, bool useFileNameForVersion)
        {
            string productVersion = useFileNameForVersion ? GetVersionFromName(binaryFileInfo) : GetVersionFromAssembly(binaryFileInfo);
            if (productVersion == null)
            {
                Console.WriteLine($"Unable to determine version of binary {binaryFileInfo.Name}, try -f parameter to determine version from file name", Color.Red);
            }
            return productVersion;
        }

        public AppCastItem CreateAppCastItemFromFile(FileInfo binaryFileInfo, string productName, string productVersion, bool useChangelogs)
        {
            var itemTitle = string.IsNullOrWhiteSpace(productName) ? productVersion : productName + " " + productVersion;

            var urlEncodedFileName = Uri.EscapeDataString(binaryFileInfo.Name);
            var urlToUse = !string.IsNullOrWhiteSpace(_opts.BaseUrl?.ToString())
                ? (_opts.BaseUrl.ToString().EndsWith("/") ? _opts.BaseUrl.ToString() : _opts.BaseUrl + "/")
                : "";
            if (_opts.PrefixVersion)
            {
                urlToUse += $"{productVersion}/";
            }
            if (urlEncodedFileName.StartsWith("/") && urlEncodedFileName.Length > 1)
            {
                urlEncodedFileName = urlEncodedFileName.Substring(1);
            }
            var remoteUpdateFile = $"{urlToUse}{urlEncodedFileName}";

            // changelog stuff
            var changelogFileName = productVersion + ".md";
            var changelogPath = Path.Combine(_opts.ChangeLogPath, changelogFileName);
            var hasChangelogForFile = useChangelogs && File.Exists(changelogPath);
            var changelogSignature = "";

            if (hasChangelogForFile)
            {
                changelogSignature = _signatureManager.GetSignatureForFile(changelogPath);
            }

            var item = new AppCastItem()
            {
                Title = itemTitle?.Trim(),
                DownloadLink = remoteUpdateFile?.Trim(),
                Version = productVersion?.Trim(),
                ShortVersion = productVersion?.Substring(0, productVersion.LastIndexOf('.'))?.Trim(),
                PublicationDate = binaryFileInfo.CreationTime,
                UpdateSize = binaryFileInfo.Length,
                Description = "",
                DownloadSignature = _signatureManager.KeysExist() ? _signatureManager.GetSignatureForFile(binaryFileInfo) : null,
                OperatingSystemString = _opts.OperatingSystem?.Trim(),
                MIMEType = MimeTypes.GetMimeType(binaryFileInfo.Name)
            };

            if (hasChangelogForFile)
            {
                if (!string.IsNullOrWhiteSpace(_opts.ChangeLogUrl))
                {
                    item.ReleaseNotesSignature = changelogSignature;
                    item.ReleaseNotesLink = (Path.Combine(_opts.ChangeLogUrl, changelogFileName)).Trim();
                }
                else
                {
                    item.Description = File.ReadAllText(changelogPath).Trim();
                }
            }
            return item;
        }

        /// <summary>
        /// Gets the path to the app cast output file. If desiredOutputDirectory is
        /// null/whitespace, uses sourceBinaryDirectory as the output location instead.
        /// Does not check to see if the directory exists!
        /// </summary>
        /// <param name="desiredOutputDirectory"></param>
        /// <param name="sourceBinaryDirectory"></param>
        /// <returns></returns>
        public virtual string GetPathToAppCastOutput(string desiredOutputDirectory, string sourceBinaryDirectory)
        {
            if (string.IsNullOrWhiteSpace(desiredOutputDirectory))
            {
                desiredOutputDirectory = sourceBinaryDirectory;
            }
            return Path.Combine(desiredOutputDirectory, "appcast.xml");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceBinaryDirectory"></param>
        /// <returns>items, product name</returns>
        public (List<AppCastItem>, string) LoadAppCastItemsAndProductName(string sourceBinaryDirectory, bool useExistingAppCastItems, string outputAppCastFileName)
        {
            var items = new List<AppCastItem>();
            var dirFileSearches = GetSearchExtensionsFromString(_opts.Extensions);
            var binaries = FindBinaries(sourceBinaryDirectory, dirFileSearches, _opts.SearchBinarySubDirectories);
            if (!binaries.Any())
            {
                Console.WriteLine($"No files founds matching extensions -- {string.Join(",", dirFileSearches)} -- in {sourceBinaryDirectory}", Color.Yellow);
                return (null, null);
            }

            if (!_operatingSystems.Any(_opts.OperatingSystem.Contains))
            {
                Console.WriteLine($"Invalid operating system: {_opts.OperatingSystem}", Color.Red);
                Console.WriteLine($"Valid options are: {0}", string.Join(", ", _operatingSystems));
                return (null, null);
            }

            Console.WriteLine();
            Console.WriteLine($"Operating System: {_opts.OperatingSystem}", Color.LightBlue);
            Console.WriteLine($"Searching: {sourceBinaryDirectory}", Color.LightBlue);
            Console.WriteLine($"Found {binaries.Count()} {string.Join(",", dirFileSearches)} files(s)", Color.LightBlue);
            Console.WriteLine();

            try
            {
                var productName = _opts.ProductName;
                if (useExistingAppCastItems)
                {
                    var (existingItems, existingProductName) = GetItemsAndProductNameFromExistingAppCast(outputAppCastFileName);
                    items.AddRange(existingItems);
                    if (!string.IsNullOrWhiteSpace(existingProductName))
                    {
                        productName = existingProductName;
                    }
                }

                var usesChangelogs = !string.IsNullOrWhiteSpace(_opts.ChangeLogPath) && Directory.Exists(_opts.ChangeLogPath);
                foreach (var binary in binaries)
                {
                    var fileInfo = new FileInfo(binary);
                    string productVersion = GetVersionForBinary(fileInfo, _opts.FileExtractVersion);

                    if (productVersion == null)
                    {
                        continue;
                    }

                    var itemFoundInAppcast = items.Where(x => x.Version != null && x.Version == productVersion?.Trim()).FirstOrDefault();
                    if (itemFoundInAppcast != null && _opts.OverwriteOldItemsInAppcast)
                    {
                        Console.WriteLine("Removing existing app cast item with version {0} so we can add the version on disk to the app cast...", productVersion);
                        items.Remove(itemFoundInAppcast); // remove old item.
                        itemFoundInAppcast = null;
                    }
                    if (itemFoundInAppcast == null)
                    {
                        var item = CreateAppCastItemFromFile(fileInfo, productName, productVersion, usesChangelogs);
                        if (item != null)
                        {
                            items.Add(item);
                        }
                    }
                    else
                    {
                        Console.WriteLine("An app cast item with version {0} is already in the file, not adding it again...", productVersion);
                    }
                }

                // order the list by version -- helpful when reparsing app cast to make sure things stay in order
                items.Sort((a, b) => a.Version.CompareTo(b.Version));
                return (items, productName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error generating app cast file", Color.Red);
                Console.WriteLine(e.Message, Color.Red);
                Console.WriteLine();
            }
            return (null, null);
        }

        // marked virtual so that in theory all you would have to do to create a JSON representation
        // of your app cast would be to overwrite this function and GetPathToAppCastOutput
        public virtual void SerializeItemsToFile(List<AppCastItem> items, string applicationTitle, string path)
        {
            var appcastXmlDocument = XMLAppCast.GenerateAppCastXml(items, applicationTitle);
            Console.WriteLine("Writing app cast to {0}", path);
            using (var xmlWriter = XmlWriter.Create(path, new XmlWriterSettings { NewLineChars = "\n", Encoding = new UTF8Encoding(false) }))
            {
                appcastXmlDocument.Save(xmlWriter);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appCastFileName"></param>
        /// <param name="signatureFileExtension"></param>
        /// <returns>true if signature file written; false otherwise</returns>
        public bool CreateSignatureFile(string appCastFileName, string signatureFileExtension)
        {
            if (_signatureManager.KeysExist())
            {
                var appcastFile = new FileInfo(appCastFileName);
                var extension = signatureFileExtension?.TrimStart('.') ?? "signature";
                var signatureFileName = appCastFileName + "." + extension;
                var signature = _signatureManager.GetSignatureForFile(appcastFile);

                var result = _signatureManager.VerifySignature(appcastFile, signature);
                if (result)
                {
                    File.WriteAllText(signatureFileName, signature);
                    Console.WriteLine($"Wrote {signatureFileName}", Color.Green);
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to verify {signatureFileName}", Color.Red);
                }
            }
            else
            {
                Console.WriteLine("Skipped generating signature. Generate keys with --generate-keys", Color.Red);
            }
            return false;
        }
    }
}
