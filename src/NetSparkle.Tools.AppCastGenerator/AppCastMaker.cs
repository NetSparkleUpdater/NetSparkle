using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Console = Colorful.Console;

namespace NetSparkleUpdater.AppCastGenerator
{
    public abstract class AppCastMaker
    {
        private static readonly string[] _operatingSystems = new string[] { "windows", "mac", "linux" };
        
        protected Options _opts;
        private SignatureManager _signatureManager;

        public AppCastMaker(SignatureManager signatureManager, Options options)
        {
            _opts = options;
            _signatureManager = signatureManager;
        }

        // to create your own app cast maker, you need to override the following three functions

        /// <summary>
        /// Get the file extension (without any "." preceding the extension) for the app cast file.
        /// e.g., "xml"
        /// </summary>
        /// <returns>Extension for the app cast file</returns>
        public abstract string GetAppCastExtension();
        /// <summary>
        /// Serialize a list of AppCastItem items to a file at the given path with the given title.
        /// Overwrites any file at the given path. Does not verify that the given directory structure
        /// for the output file exists.
        /// </summary>
        /// <param name="items">List of AppCastItem items to write</param>
        /// <param name="applicationTitle">Title of application (or app cast)</param>
        /// <param name="path">Output file path/name</param>
        public abstract void SerializeItemsToFile(List<AppCastItem> items, string applicationTitle, string path);
        /// <summary>
        /// Loads an existing app cast file and loads its AppCastItem items and any product name that is in the file.
        /// Should not return duplicate versions.
        /// The items list should always be non-null and sorted by AppCastItem.Version descending.
        /// </summary>
        /// <param name="appCastFileName">File name/path for app cast file to read</param>
        /// <param name="overwriteOldItemsInAppcast">If true and an item is loaded with a version that has already been found,
        /// overwrite the already found item with the newly found one</param>
        /// <returns>Tuple of items and the product name (product name can be null if file does not exist or file 
        /// does not contain a product name)</returns>
        public abstract (List<AppCastItem>, string) GetItemsAndProductNameFromExistingAppCast(string appCastFileName, bool overwriteOldItemsInAppcast);

        public static string GetVersionFromName(string fullFileNameWithPath, string binaryDirectory = "")
        {
            // get the numbers at the end of the string in case the app is something like 1.0application1.0.0.dmg
            // this solution is a mix of https://stackoverflow.com/a/22704755/3938401
            // and https://stackoverflow.com/a/31926058/3938401
            // basically, we pull out the last numbers out of the name that are separated by '.'
            if (string.IsNullOrWhiteSpace(fullFileNameWithPath))
            {
                return null;
            }
            if (fullFileNameWithPath.EndsWith("."))
            {
                // don't allow raw files that end in '.'
                return null;
            }
            // don't search above initial binary directory
            if (!string.IsNullOrWhiteSpace(binaryDirectory))
            {
                fullFileNameWithPath = fullFileNameWithPath.Replace(binaryDirectory, "");
            }
            var folderSplit = fullFileNameWithPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var numFolderSectionsChecked = 0;
            var nums = new List<int>();
            for (int j = folderSplit.Length - 1; j >= 0; j--)
            {
                var split = folderSplit[j].Split('.', StringSplitOptions.RemoveEmptyEntries);
                // start on last item and go until first item in file name
                for (int i = split.Length - 1; i >= 0; i--)
                {
                    var splitItem = split[i];
                    var foundAtEnd = false;
                    if (int.TryParse(splitItem, out int temp))
                    {
                        nums.Add(temp);
                    }
                    else
                    {
                        // look at the end of the string by default, then the start of the string
                        var regexPatternEndOfStr = @"\d+$";
                        var regexEndOfStr = new Regex(regexPatternEndOfStr);
                        var matchEndOfStr = regexEndOfStr.Match(splitItem);
                        if (matchEndOfStr.Success)
                        {
                            var matchNum = int.Parse(matchEndOfStr.Captures[^1].Value);
                            nums.Add(matchNum);
                            foundAtEnd = true;
                        }
                        else
                        {
                            // look at start of string instead
                            // in case we get something like 3 foo bar
                            var regexPatternStartOfStr = @"(^\d+)";
                            var regexStartOfStr = new Regex(regexPatternStartOfStr);
                            var startOfStrMatch = regexStartOfStr.Match(splitItem);
                            if (startOfStrMatch.Success)
                            {
                                var matchNum = int.Parse(startOfStrMatch.Captures[^1].Value);
                                nums.Add(matchNum);
                            }
                        }
                    }
                    if (splitItem.Contains(" ") && foundAtEnd)
                    {
                        // item had a space, so we're going to assume that the version was at the end
                        // of the string
                        break;
                    }
                    if (nums.Count >= 4)
                    {
                        break; // Major.Minor.Revision.Patch is all we allow
                    }
                }
                if (nums.Count > 0)
                {
                    // we found some part of a version number we can use, bail out!
                    break;
                }
                numFolderSectionsChecked++;
                // check up to 4 folders -- 4 is arbitrary but we don't want to
                // crawl up the entire folder directory tree/path
                if (numFolderSectionsChecked >= 4)
                {
                    break;
                }
            }
            if (nums.Count > 0)
            {
                nums.Reverse();
                return string.Join('.', nums);
            }
            return null;
        }

        public static string GetVersionFromAssembly(string fullFileNameWithPath)
        {
            return FileVersionInfo.GetVersionInfo(fullFileNameWithPath).ProductVersion?.Trim();
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

        public string GetVersionForBinary(FileInfo binaryFileInfo, bool useFileNameForVersion, string binaryDirectory)
        {
            if (binaryDirectory == ".")
            {
                binaryDirectory = Environment.CurrentDirectory;
            }
            string productVersion = useFileNameForVersion 
                ? GetVersionFromName(binaryFileInfo.FullName, Path.GetFullPath(binaryDirectory)) 
                : GetVersionFromAssembly(binaryFileInfo.FullName);
            if (productVersion == null)
            {
                Console.WriteLine($"Unable to determine version of binary {binaryFileInfo.Name}, try --file-extract-version parameter to determine version from file name", Color.Red);
            }
            return productVersion;
        }

        public AppCastItem CreateAppCastItemFromFile(FileInfo binaryFileInfo, string productName, string productVersion, bool useChangelogs, string changelogFileNamePrefix)
        {
            var itemTitle = string.IsNullOrWhiteSpace(productName) ? productVersion : productName + " " + productVersion;

            var urlEncodedFileName = Uri.EscapeDataString(binaryFileInfo.Name);
            var urlToUse = !string.IsNullOrWhiteSpace(_opts.BaseUrl)
                ? (_opts.BaseUrl.EndsWith("/") ? _opts.BaseUrl : _opts.BaseUrl + "/")
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
            var changelogPath = useChangelogs ? Path.Combine(_opts.ChangeLogPath, changelogFileName) : "";
            var hasChangelogForFile = useChangelogs && File.Exists(changelogPath);
            if (useChangelogs && !hasChangelogForFile && !string.IsNullOrWhiteSpace(changelogFileNamePrefix))
            {
                changelogPath = Path.Combine(_opts.ChangeLogPath, changelogFileNamePrefix.Trim() + " " + changelogFileName);
                hasChangelogForFile = File.Exists(changelogPath);
                if (!hasChangelogForFile)
                {
                    // make one more effort if user doesn't want the space in there
                    changelogPath = Path.Combine(_opts.ChangeLogPath, changelogFileNamePrefix.Trim() + changelogFileName);
                    hasChangelogForFile = File.Exists(changelogPath);
                }
            }
            // make an additional effort to find the changelog file if they didn't use the file name prefix and used their app's name instead.
            if (useChangelogs && !hasChangelogForFile)
            {
                changelogPath = Path.Combine(_opts.ChangeLogPath, productName.Trim() + " " + changelogFileName);
                hasChangelogForFile = File.Exists(changelogPath);
                if (!hasChangelogForFile)
                {
                    // make one more last, last ditch effort if user doesn't want the space in there
                    changelogPath = Path.Combine(_opts.ChangeLogPath, productName.Trim() + changelogFileName);
                    hasChangelogForFile = File.Exists(changelogPath);
                }
            }
            var changelogSignature = "";

            if (hasChangelogForFile)
            {
                changelogSignature = _signatureManager.GetSignatureForFile(changelogPath);
            }

            var productVersionLastDotIndex = productVersion?.LastIndexOf('.') ?? -1;
            var item = new AppCastItem()
            {
                Title = itemTitle?.Trim(),
                DownloadLink = remoteUpdateFile?.Trim(),
                Version = productVersion?.Trim(),
                ShortVersion = productVersionLastDotIndex >= 0 ? productVersion?.Substring(0, productVersionLastDotIndex)?.Trim() : productVersion,
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
        public string GetPathToAppCastOutput(string desiredOutputDirectory, string sourceBinaryDirectory)
        {
            if (string.IsNullOrWhiteSpace(desiredOutputDirectory))
            {
                desiredOutputDirectory = sourceBinaryDirectory;
            }
            return Path.Combine(desiredOutputDirectory, "appcast." + GetAppCastExtension());
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
                    var (existingItems, existingProductName) = GetItemsAndProductNameFromExistingAppCast(outputAppCastFileName, _opts.OverwriteOldItemsInAppcast);
                    items.AddRange(existingItems);
                    if (!string.IsNullOrWhiteSpace(existingProductName))
                    {
                        productName = existingProductName;
                    }
                }

                var usesChangelogs = !string.IsNullOrWhiteSpace(_opts.ChangeLogPath) && Directory.Exists(_opts.ChangeLogPath);
                var hasUsedOverrideVersion = false;
                foreach (var binary in binaries)
                {
                    var fileInfo = new FileInfo(binary);
                    string productVersion = GetVersionForBinary(fileInfo, _opts.FileExtractVersion, _opts.SourceBinaryDirectory);

                    if (!string.IsNullOrWhiteSpace(productVersion))
                    {
                        Console.WriteLine($"Found a binary with version {productVersion}");
                    }
                    if (productVersion == null && !string.IsNullOrWhiteSpace(_opts.FileVersion))
                    {
                        if (hasUsedOverrideVersion)
                        {
                            throw new NetSparkleException("More than 1 binary found that did not have a file version, and the file version parameter was set. This is not an allowed configuration since the app cast generator does not know which binary to apply the version to.");
                        }
                        productVersion = _opts.FileVersion;
                        hasUsedOverrideVersion = true;
                        Console.WriteLine($"Using product version from command line of {productVersion}");
                    }
                    if (productVersion == null)
                    {
                        Console.WriteLine($"Skipping {binary} because it has no product version...");
                        continue;
                    }

                    var itemFoundInAppcast = items.Where(x => x.Version != null && x.Version == productVersion?.Trim()).FirstOrDefault();
                    if (itemFoundInAppcast != null && _opts.OverwriteOldItemsInAppcast)
                    {
                        Console.WriteLine($"Removing existing app cast item with version {productVersion} so we can add the version on disk to the app cast...");
                        items.Remove(itemFoundInAppcast); // remove old item.
                        itemFoundInAppcast = null;
                    }
                    if (itemFoundInAppcast == null)
                    {
                        var item = CreateAppCastItemFromFile(fileInfo, productName, productVersion, usesChangelogs, _opts.ChangeLogFileNamePrefix);
                        if (item != null)
                        {
                            items.Add(item);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"An app cast item with version {productVersion} is already in the file, not adding it again...");
                    }
                }

                // order the list by version descending -- helpful when reparsing app cast to make sure things stay in order
                items.Sort((a, b) => b.Version.CompareTo(a.Version));
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
