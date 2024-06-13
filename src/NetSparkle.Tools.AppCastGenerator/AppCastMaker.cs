using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NetSparkleUpdater.AppCastHandlers;
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
            // File name is empty
            if (string.IsNullOrWhiteSpace(fullFileNameWithPath))
            {
                return null;
            }

            // Filename has no extension or ends in .
            if (fullFileNameWithPath.EndsWith("."))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(binaryDirectory))
            {
                fullFileNameWithPath = fullFileNameWithPath.Replace(binaryDirectory, "").Trim();
            }

            // Handle complex extensions and remove them if they exist
            string[] extensionPatterns = { @"\.tar\.gz$", @"\.tar$", @"\.gz$", @"\.zip$", @"\.txt$", @"\.exe$", @"\.bin$", @"\.msi$", @"\.excel", @"\.mcdx", @"\.pdf", @"\.dll", @"\.ted" };
            foreach (var pattern in extensionPatterns)
            {
                if (Regex.IsMatch(fullFileNameWithPath, pattern))
                {
                    fullFileNameWithPath = Regex.Replace(fullFileNameWithPath, pattern, "").Trim();
                    break;
                }
            }

            // Replace multiple spaces with a single space
            fullFileNameWithPath = Regex.Replace(fullFileNameWithPath, @"\s+", " ");

            // Replace _ with space
            fullFileNameWithPath = fullFileNameWithPath.Replace("_", " ");

            // Regex for simple version numbers (X.X.X or X.X.X.X)
            string simpleVersionPattern = @"^\d+(\.\d+){1,3}$";

            // Regex for semantic versioning
            string semverPattern = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)"
                                 + @"(-((0|[1-9]\d*)|\d*[a-zA-Z-][0-9a-zA-Z-]*)"
                                 + @"(\.(0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*)?"
                                 + @"(\+[0-9a-zA-Z-]+(\.[0-9a-zA-Z-]+)*)?$";

            // Function to check if a segment is a valid version
            bool IsValidVersion(string segment)
            {
                return Regex.IsMatch(segment, simpleVersionPattern) || Regex.IsMatch(segment, semverPattern);
            }

            // This regex finds the first text block that is not preceded by a + or - and is followed by a number (starting from left)
            string RemoveTextBlockFromLeft(string input)
            {
                if (!Regex.IsMatch(input, @"\d"))
                {
                    return "";
                }

                if (Regex.IsMatch(input, @"[+-]"))
                {
                    var match = Regex.Match(input, @"(?<![+-])[a-zA-Z]+(?=\d)");
                    if (match.Success)
                    {
                        return input.Substring(0, match.Index);
                    }
                    return input;
                }
                else
                {
                    var match = Regex.Match(input, @"[a-zA-Z]");
                    if (match.Success)
                    {
                        return input.Substring(0, match.Index);
                    }
                    return input;
                }
            }

            // This regex finds the first text block that is not preceded by a + or - and is followed by a number (starting from right)
            string RemoveTextBlockFromRight(string input)
            {
                var match = Regex.Match(input, @"(?<![a-zA-Z+-])[a-zA-Z]+(?=\d)");
                if (match.Success)
                {
                    return input.Substring(match.Index + match.Length);
                }
                return input;
            }

            var folderSplit = fullFileNameWithPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            // Loop through the last 4 folder names to find the version
            for (int j = folderSplit.Length - 1; j >= Math.Max(0, folderSplit.Length - 4); j--)
            {
                var fileName = folderSplit[j];

                // Split the filename by space to find the version segment
                var parts = fileName.Split(' ');

                // If there are multiple parts, we check the first and last parts only assuming version is in either
                // If the strings in the start and end both produce valid versions, we return the version from the end
                // If single string no spaces, we take the entire string and check it from left and right
                string leftPart = null;
                string rightPart = null;
                if (parts.Length > 1)
                {
                    leftPart = parts[0];
                    rightPart = parts[^1];
                }
                else
                {
                    leftPart = parts[0];
                    rightPart = parts[0];
                }

                // Checking left part with logic from left
                string lastValidVersionLeft = null;
                if (!string.IsNullOrEmpty(leftPart))
                {
                    // Remove any text block from left
                    // For example 0.1foo becomes 0.1, 0.1-foo stays 0.1-foo, 0.1+foo stays 0.1+foo
                    leftPart = RemoveTextBlockFromLeft(leftPart);

                    // Make sure leftpart has a number
                    if (Regex.IsMatch(leftPart, @"\d"))
                    {
                        // Check if its only numeric values and a simple version for quick check
                        if (Regex.IsMatch(leftPart, @"^[\d.]+$") && IsValidVersion(leftPart))
                        {
                            lastValidVersionLeft = leftPart;
                        }
                        else
                        {
                            // Its more complex so we check if its semantic version before splitting
                            if (IsValidVersion(leftPart))
                            {
                                lastValidVersionLeft = leftPart;
                            }
                            else
                            {
                                // Start splitting and going from left to right
                                // Keep record of last applicable version and check one more segment after it, if it fails then the last one we found is what we need
                                var segments = leftPart.Split('.');
                                string tempSegment = "";
                                bool lastVersionToCheck = false;
                                for (int i = segments.Length - 1; i >= 0; i--)
                                {
                                    var segment = segments[i];
                                    if (Regex.IsMatch(segment, @"[a-zA-Z]") && Regex.IsMatch(segment, @"\d"))
                                    {
                                        var match = Regex.Match(segment, @"[^+-]*[a-zA-Z]");
                                        if (match.Success)
                                        {
                                            segment = segment.Substring(match.Index + match.Length);
                                            lastVersionToCheck = true;
                                        }
                                    }

                                    tempSegment = string.IsNullOrEmpty(tempSegment) ? segment : segment + "." + tempSegment;
                                    tempSegment = tempSegment.Trim('.');

                                    if (IsValidVersion(tempSegment))
                                    {
                                        lastValidVersionLeft = tempSegment;
                                    }

                                    if (lastVersionToCheck)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // Checking right part with logic from right
                string lastValidVersionRight = null;
                if (!string.IsNullOrEmpty(rightPart))
                {
                    // Remove any text block from right
                    // For example foo0.1 becomes 0.1
                    rightPart = RemoveTextBlockFromRight(rightPart);

                    // Make sure rightpart has a number
                    if (Regex.IsMatch(rightPart, @"\d"))
                    {
                        // Check if its only numeric values and a simple version for quick check
                        if (Regex.IsMatch(rightPart, @"^[\d.]+$") && IsValidVersion(rightPart))
                        {
                            lastValidVersionRight = rightPart;
                        }
                        else
                        {
                            // Its more complex so we check if its semantic version before splitting
                            if (IsValidVersion(rightPart))
                            {
                                lastValidVersionRight = rightPart;
                            }
                            else
                            {
                                // Start splitting and going from left to right
                                // Keep record of last applicable version and check one more segment after it, if it fails then the last one we found is what we need
                                var segments = rightPart.Split('.');
                                string tempSegment = "";
                                bool lastVersionToCheck = false;
                                for (int i = segments.Length - 1; i >= 0; i--)
                                {
                                    var segment = segments[i];
                                    if (Regex.IsMatch(segment, @"[a-zA-Z]") && Regex.IsMatch(segment, @"\d"))
                                    {
                                        var match = Regex.Match(segment, @"[^+-]*[a-zA-Z]");
                                        if (match.Success)
                                        {
                                            segment = segment.Substring(match.Index + match.Length);
                                            lastVersionToCheck = true;
                                        }
                                    }

                                    tempSegment = string.IsNullOrEmpty(tempSegment) ? segment : segment + "." + tempSegment;
                                    tempSegment = tempSegment.Trim('.');

                                    if (IsValidVersion(tempSegment))
                                    {
                                        lastValidVersionRight = tempSegment;
                                    }

                                    if (lastVersionToCheck)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                // Right part is preferred over left part
                if (lastValidVersionRight != null)
                {
                    return lastValidVersionRight;
                }
                else if (lastValidVersionLeft != null)
                {
                    return lastValidVersionLeft;
                }
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

        public AppCastItem CreateAppCastItemFromFile(FileInfo binaryFileInfo, string productName, SemVerLike productVersion, bool useChangelogs, string changelogFileNamePrefix)
        {
            var fullProductVersionString = productVersion.ToString();
            var itemTitle = string.IsNullOrWhiteSpace(productName) 
                ? fullProductVersionString
                : productName + " " + fullProductVersionString;

            var urlEncodedFileName = Uri.EscapeDataString(binaryFileInfo.Name);
            var urlToUse = !string.IsNullOrWhiteSpace(_opts.BaseUrl)
                ? (_opts.BaseUrl.EndsWith("/") ? _opts.BaseUrl : _opts.BaseUrl + "/")
                : "";
            if (_opts.PrefixVersion)
            {
                urlToUse += $"{fullProductVersionString}/";
            }
            if (urlEncodedFileName.StartsWith("/") && urlEncodedFileName.Length > 1)
            {
                urlEncodedFileName = urlEncodedFileName.Substring(1);
            }
            var remoteUpdateFile = $"{urlToUse}{urlEncodedFileName}";

            // changelog stuff
            var changelogFileName = fullProductVersionString + ".md";
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
                Console.WriteLine($"Change log for version {productVersion} was successfully found: {changelogPath}", Color.LightBlue);
                changelogSignature = _signatureManager.GetSignatureForFile(changelogPath);
                changelogFileName = Path.GetFileName(changelogPath); // use whatever filename was found
            }
            if (useChangelogs && !hasChangelogForFile)
            {
                Console.WriteLine($"Unable to find change log for {productVersion}. The app cast will still be generated, but without a change log for this version.", Color.Yellow);
            }

            var productVersionLastDotIndex = productVersion.Version?.LastIndexOf('.') ?? -1;
            var item = new AppCastItem()
            {
                Title = itemTitle?.Trim(),
                DownloadLink = remoteUpdateFile?.Trim(),
                Version = fullProductVersionString?.Trim(),
                ShortVersion = productVersion.Version,
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
            var dirFileSearches = GetSearchExtensionsFromString(_opts.Extensions ?? "");
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
                    var semVerLikeVersion = SemVerLike.Parse(productVersion);

                    var itemFoundInAppcast = items.Where(x => x.SemVerLikeVersion != null && x.SemVerLikeVersion == semVerLikeVersion).FirstOrDefault();
                    if (itemFoundInAppcast != null && _opts.OverwriteOldItemsInAppcast)
                    {
                        Console.WriteLine($"Removing existing app cast item with version {semVerLikeVersion} so we can add the version on disk to the app cast...");
                        items.Remove(itemFoundInAppcast); // remove old item.
                        itemFoundInAppcast = null;
                    }
                    if (itemFoundInAppcast == null)
                    {
                        var item = CreateAppCastItemFromFile(fileInfo, productName, semVerLikeVersion, usesChangelogs, _opts.ChangeLogFileNamePrefix);
                        if (item != null)
                        {
                            items.Add(item);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"An app cast item with version {semVerLikeVersion} is already in the file, not adding it again...");
                    }
                }

                // order the list by version descending -- helpful when reparsing app cast to make sure things stay in order
                items.Sort((a, b) => b.SemVerLikeVersion.CompareTo(a.SemVerLikeVersion));

                // mark critical items as critical
                var criticalVersions = _opts.CriticalVersions?.Split(",").ToList()
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct();
                if (criticalVersions != null)
                {
                    foreach (var item in items)
                    {
                        if (criticalVersions.Contains(item.SemVerLikeVersion.ToString()))
                        {
                            item.IsCriticalUpdate = true;
                        }
                    }
                }

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
