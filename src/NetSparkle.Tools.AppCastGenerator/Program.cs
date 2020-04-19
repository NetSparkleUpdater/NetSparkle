using NetSparkleUpdater;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Xml;
using NetSparkleUpdater.AppCastHandlers;

namespace NetSparkleUpdater.Tools.AppCastGenerator
{
    class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine(" Usage:");
            Console.WriteLine("   generate_appcast [<path_to_private_key>] <directory_with_update_files> [Options]");
            Console.WriteLine();
            Console.WriteLine(" Options:");
            Console.WriteLine("   /O <output file path>");
            Console.WriteLine("     -> Output to the given file. File will be overwritten if it exists. If not given, defaults to 'appcast.xml' in the directory");
            Console.WriteLine("             with update files.");
            Console.WriteLine("        eg. \"/O Blah\\my_app.xml\" will save the appcast file to that path and create \"Blah\" if it doesn't exist.");
            Console.WriteLine("   /S <search_pattern>");
            Console.WriteLine("     -> Only scan for .exe files that match the given search pattern.");
            Console.WriteLine("        eg. \"/S MyApp\" will only scan for \"*MyApp*.exe\" files.");
            Console.WriteLine("   /U <base_url>");
            Console.WriteLine("     -> Base URL for the Download Links.");
            Console.WriteLine("        eg. \"/U https://www.example.com/downloads\" will result in \"https://www.example.com/downloads/fileName.exe\".");
            Console.WriteLine("   /CL <path to changelog files>");
            Console.WriteLine("     -> File path to Markdown changelog files (expected extension: .md; version must match AssemblyVersion).");
            Console.WriteLine("        eg. \"/CL Installer\\MyChangeLogsDir\" scans MyChangeLogsDir for {Version}.md changelog files");
            Console.WriteLine("   /CLURL <changelog_base_url>");
            Console.WriteLine("     -> Base URL for the Changelog Download Links.");
            Console.WriteLine("        eg. \"/CLURL https://www.example.com/changelogs\" will result in \"https://www.example.com/changelogs/1.0.0.0.md\".");
            Console.WriteLine("        If not provided, will read changelogs (if available) and put them in the <description> part of the appcast.");
            Console.WriteLine();
            Console.WriteLine(" Examples:");
            Console.WriteLine("   generate_appcast .\\DirWithUpdateFiles");
            Console.WriteLine("   (this will generate appcast.xml for all \"*.exe\" files in the given directory)");
            Console.WriteLine();
            Console.WriteLine("   generate_appcast .\\DirWithUpdateFiles /S MyApp");
            Console.WriteLine("   (this will generate appcast.xml for all \"*MyApp*.exe\" files in the given directory)");
            Console.WriteLine();
            Console.WriteLine("   generate_appcast .\\NetSparkle_DSA.priv .\\DirWithUpdateFiles");
            Console.WriteLine("   (this will generate appcast.xml for all \"*.exe\" files in the given directory");
            Console.WriteLine("   and automatically add DSA signature)");
            Console.WriteLine();
            Console.WriteLine("   generate_appcast .\\NetSparkle_DSA.priv .\\DirWithUpdateFiles /S MyApp /U https://www.example.com/downloads");
            Console.WriteLine("   (this will generate appcast.xml for all \"*MyApp*.exe\" files in the given directory");
            Console.WriteLine("   and automatically add DSA signature.");
            Console.WriteLine("   Download Links will be \"https://www.example.com/downloads/<FileName>.exe\")");
        }

        private static string _privateKeyFilePath = "";
        private static string _searchPattern = "*.exe";
        private static string _baseUrl = "";

        private static string _changelogsDir = "";
        private static string _changelogsUrl = "";

        private static string _outputFilePath = "";

        static void Main(string[] args)
        {
            if (Environment.GetCommandLineArgs().Length == 1)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            try
            {
                var numArgs = Environment.GetCommandLineArgs().Length;
                var updateFilesDir = string.Empty;
                if (numArgs % 2 == 0)
                {
                    updateFilesDir = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                }
                else if (numArgs >= 3 && numArgs % 2 == 1)
                {
                    _privateKeyFilePath = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                    updateFilesDir = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[2]);
                }
                else
                {
                    PrintUsage();
                    Environment.Exit(1);
                }

                if (Directory.Exists(updateFilesDir))
                {
                    Console.WriteLine("Error: {0} does not exist on disk.", updateFilesDir);
                    Console.WriteLine("");
                    PrintUsage();
                    Environment.Exit(1);
                }

                ParseOptions();

                var exePaths = Directory.GetFiles(updateFilesDir, _searchPattern);

                if (exePaths.Length == 0)
                {
                    if (!string.IsNullOrEmpty(_searchPattern))
                    {
                        Console.WriteLine($"Didn't find any matching files (with pattern: \"{ _searchPattern }\"");
                    }
                    else
                    {
                        Console.WriteLine($"Didn't find any matching files");
                    }

                    Environment.Exit(1);
                }

                var productName = FileVersionInfo.GetVersionInfo(exePaths[0]).ProductName.Trim();
                var items = new List<AppCastItem>();
                var usesChangelogs = !string.IsNullOrWhiteSpace(_changelogsDir) && Directory.Exists(_changelogsDir);
                foreach (var exePath in exePaths)
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                    var fileInfo = new FileInfo(exePath);
                    var dsaSignature = NetSparkleUpdater.Utilities.GetDSASignature(exePath, _privateKeyFilePath);
                    var productVersion = versionInfo.ProductVersion.Trim();
                    var itemTitle = string.IsNullOrWhiteSpace(productName) ? productVersion : productName + " " + productVersion;
                    var remoteUpdateFile = _baseUrl + fileInfo.Name;
                    // changelog stuff
                    var changelogFileName = productVersion + ".md";
                    var changelogPath = Path.Combine(_changelogsDir, changelogFileName);
                    var hasChangelogForFile = usesChangelogs && File.Exists(changelogPath);
                    var changelogDSA = "";
                    if (hasChangelogForFile)
                    {
                        changelogDSA = NetSparkleUpdater.Utilities.GetDSASignature(changelogPath, _privateKeyFilePath);
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
                        DownloadDSASignature = dsaSignature,
                        OperatingSystemString = "windows",
                        MIMEType = "application/octet-stream"
                    };
                    if (hasChangelogForFile)
                    {
                        if (!string.IsNullOrWhiteSpace(_changelogsUrl))
                        {
                            item.ReleaseNotesDSASignature = changelogDSA;
                            item.ReleaseNotesLink = _changelogsUrl + changelogFileName;
                        }
                        else
                        {
                            item.Description = File.ReadAllText(changelogPath);
                        }
                    }

                    items.Add(item);
                }

                var appcastXmlDocument = XMLAppCast.GenerateAppCastXml(items, productName);
                var appcastFileName = string.IsNullOrWhiteSpace(_outputFilePath) ? "appcast.xml" : Path.GetFileName(_outputFilePath);
                if (string.IsNullOrWhiteSpace(appcastFileName))
                {
                    Console.WriteLine("Error: {0} has no valid file name", _outputFilePath);
                    Console.WriteLine("");
                    Environment.Exit(1);
                }
                var appcastXmlPath = string.IsNullOrWhiteSpace(_outputFilePath) ? Path.Combine(updateFilesDir, appcastFileName) : _outputFilePath;
                if (!Directory.Exists(Path.GetDirectoryName(appcastXmlPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(appcastXmlPath));
                }
                using (var w = XmlWriter.Create(appcastXmlPath, new XmlWriterSettings { NewLineChars = "\n" }))
                {
                    appcastXmlDocument.Save(w);
                }
                
                var signature = NetSparkleUpdater.Utilities.GetDSASignature(appcastXmlPath, _privateKeyFilePath);
                if (!string.IsNullOrEmpty(signature)) 
                { 
                    File.WriteAllText(appcastFileName + ".dsa", signature);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                PrintUsage();
                Environment.Exit(1);
            }
        }

        private static void ParseOptions()
        {
            for (int i = 2; i < Environment.GetCommandLineArgs().Length; i++)
            {
                if (Environment.GetCommandLineArgs()[i] == "/S")
                {
                    _searchPattern = $"*{ Environment.GetCommandLineArgs()[i+1] }*.exe";
                }
                else if (Environment.GetCommandLineArgs()[i] == "/U")
                {
                    var url = Environment.GetCommandLineArgs()[i + 1];
                    var trailingSlash = url.EndsWith("/") ? "" : "/";
                    _baseUrl = $"{url}{trailingSlash}";
                }
                else if (Environment.GetCommandLineArgs()[i] == "/CL")
                {
                    var dir = Environment.GetCommandLineArgs()[i + 1];
                    if (Directory.Exists(dir))
                    {
                        _changelogsDir = dir;
                    }
                }
                else if (Environment.GetCommandLineArgs()[i] == "/CLURL")
                {
                    var url = Environment.GetCommandLineArgs()[i + 1];
                    var trailingSlash = url.EndsWith("/") ? "" : "/";
                    _changelogsUrl = $"{url}{trailingSlash}";
                }
                else if (Environment.GetCommandLineArgs()[i] == "/O")
                {
                    var path = Environment.GetCommandLineArgs()[i + 1];
                    _outputFilePath = path;
                }
            }
        }
    }
}
