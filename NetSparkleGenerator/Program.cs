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
            Console.WriteLine("   generate_appcast [<path_to_private_key>] <directory_with_update_files> [<search_pattern>]");
            Console.WriteLine();
            Console.WriteLine(" Examples:");
            Console.WriteLine("   generate_appcast .\\DirWithUpdateFiles");
            Console.WriteLine("   (this will generate appcast.xml for all \"*.exe\" files in the given directory)");
            Console.WriteLine();
            Console.WriteLine("   generate_appcast .\\DirWithUpdateFiles MyApp");
            Console.WriteLine("   (this will generate appcast.xml for all \"*MyApp*.exe\" files in the given directory)");
            Console.WriteLine();
            Console.WriteLine("   generate_appcast .\\NetSparkle_DSA.priv .\\DirWithUpdateFiles");
            Console.WriteLine("   (this will generate appcast.xml for all \"*.exe\" files in the given directory");
            Console.WriteLine("   and automatically add DSA signature)");
            Console.WriteLine();
            Console.WriteLine("   generate_appcast .\\NetSparkle_DSA.priv .\\DirWithUpdateFiles MyApp");
            Console.WriteLine("   (this will generate appcast.xml for all \"*MyApp*.exe\" files in the given directory");
            Console.WriteLine("   and automatically add DSA signature)");
        }

        static void Main(string[] args)
        {
            var privateKeyFilePath = "";
            var searchPattern = "*.exe";

            if (Environment.GetCommandLineArgs().Length == 1)
            {
                PrintUsage();
                Environment.Exit(1);
            }

            try
            {
                if (Environment.GetCommandLineArgs().Length == 2 && !string.IsNullOrEmpty(Environment.GetCommandLineArgs()[1]))
                {
                    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                }
                else if (Environment.GetCommandLineArgs().Length == 3
                    && !string.IsNullOrEmpty(Environment.GetCommandLineArgs()[1])
                    && !string.IsNullOrEmpty(Environment.GetCommandLineArgs()[2]))
                {
                    if (Environment.GetCommandLineArgs()[1].EndsWith(".priv", StringComparison.OrdinalIgnoreCase))
                    {
                        // private key file path and directory were given
                        privateKeyFilePath = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                        Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[2]);
                    }
                    else
                    {
                        // directory and search pattern were given
                        Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                        searchPattern = $"*{ Environment.GetCommandLineArgs()[2] }*.exe"; ;
                    }
                }
                else if (Environment.GetCommandLineArgs().Length == 4
                    && !string.IsNullOrEmpty(Environment.GetCommandLineArgs()[1])
                    && !string.IsNullOrEmpty(Environment.GetCommandLineArgs()[2])
                    && !string.IsNullOrEmpty(Environment.GetCommandLineArgs()[3]))
                {
                    // all arguments given
                    privateKeyFilePath = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[2]);
                    searchPattern = $"*{ Environment.GetCommandLineArgs()[3] }*.exe"; ;
                }

                var exePaths = Directory.GetFiles(Environment.CurrentDirectory, searchPattern);

                if (exePaths.Length == 0)
                {
                    if (!string.IsNullOrEmpty(searchPattern))
                    {
                        Console.WriteLine($"Didn't find any matching files (with pattern: \"{ searchPattern }\"");
                    }
                    else
                    {
                        Console.WriteLine($"Didn't find any matching files (with pattern: \"{ searchPattern }\"");
                    }

                    Environment.Exit(1);
                }

                var productName = FileVersionInfo.GetVersionInfo(exePaths[0]).ProductName;
                var appCastGenerator = new AppCastXMLGenerator(productName);

                foreach (var exePath in exePaths)
                {
                    var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                    var fileInfo = new FileInfo(exePath);
                    var dsaSignature = NetSparkleUtilities.Utilities.GetDSASignature(exePath, privateKeyFilePath);

                    appCastGenerator.AddItem(
                        new AppCastXMLItem(versionInfo.ProductVersion,
                        Path.GetFileName(versionInfo.FileName),
                        versionInfo.ProductVersion,
                        fileInfo.CreationTime,
                        length: fileInfo.Length.ToString(),
                        dsaSignature: dsaSignature)
                    );
                }

                var appcastXmlDocument = appCastGenerator.GenerateAppCast();
                appcastXmlDocument.Save("appcast.xml");

                var appcastXmlPath = Path.Combine(Environment.CurrentDirectory, "appcast.xml");
                var signature = NetSparkleUtilities.Utilities.GetDSASignature(appcastXmlPath, privateKeyFilePath);
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
    }
}
