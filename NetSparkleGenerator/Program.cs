using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace NetSparkleGenerator
{
    class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine(" Usage:");
            Console.WriteLine("   generate_appcast [<path_to_private_key>] <directory_with_update_files> [Options]");
            Console.WriteLine();
            Console.WriteLine(" Options:");
            Console.WriteLine("   /S <search_pattern>");
            Console.WriteLine("     -> Only scan for .exe files that match the given search pattern.");
            Console.WriteLine("        eg. \"/S MyApp\" will only scan for \"*MyApp*.exe\" files.");
            Console.WriteLine("   /U <base_url>");
            Console.WriteLine("     -> Base URL for the Download Links.");
            Console.WriteLine("        eg. \"/U https://www.example.com/downloads\" will result in \"https://www.example.com/downloads/fileName.exe\".");
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
                if (numArgs == 2 || numArgs == 4 || numArgs == 6)
                {
                    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                }
                else if (numArgs == 3 || numArgs == 5 || numArgs == 7)
                {
                    _privateKeyFilePath = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[2]);
                }
                else
                {
                    PrintUsage();
                    Environment.Exit(1);
                }

                ParseOptions();

                var exePaths = Directory.GetFiles(Environment.CurrentDirectory, _searchPattern);

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

                var productName = FileVersionInfo.GetVersionInfo(exePaths[0]).ProductName;
                var appCastGenerator = new AppCastXMLGenerator(productName);

                foreach (var exePath in exePaths)
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                    var fileInfo = new FileInfo(exePath);
                    var dsaSignature = NetSparkleUtilities.Utilities.GetDSASignature(exePath, _privateKeyFilePath);

                    appCastGenerator.AddItem(
                        new AppCastXMLItem(versionInfo.ProductVersion,
                        _baseUrl + Path.GetFileName(versionInfo.FileName),
                        versionInfo.ProductVersion,
                        fileInfo.CreationTime,
                        length: fileInfo.Length.ToString(),
                        dsaSignature: dsaSignature)
                    );
                }

                var appcastXmlDocument = appCastGenerator.GenerateAppCast();
                appcastXmlDocument.Save("appcast.xml");

                var appcastXmlPath = Path.Combine(Environment.CurrentDirectory, "appcast.xml");
                var signature = NetSparkleUtilities.Utilities.GetDSASignature(appcastXmlPath, _privateKeyFilePath);
                if (!string.IsNullOrEmpty(signature)) 
                { 
                    File.WriteAllText("appcast.xml.dsa", signature);
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
            }
        }
    }
}
