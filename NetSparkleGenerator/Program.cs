using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace NetSparkleGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Environment.GetCommandLineArgs().Length == 1)
            {
                Console.WriteLine(" Usage:");
                Console.WriteLine("       generate_appcast <directory_with_update_files> [<search_pattern>]");
                Console.WriteLine();
                Console.WriteLine("  e.g. generate_appcast .\\my-apps-setup-files");
                Console.WriteLine("  (this will generate appcast.xml for all \"*.exe\" files in the given directory)");
                Console.WriteLine();
                Console.WriteLine("  e.g. generate_appcast .\\my-apps-setup-files MyApp");
                Console.WriteLine("  (this will generate appcast.xml for all \"*MyApp*.exe\" files in the given directory)");
                Console.WriteLine();
                Console.WriteLine(" You may place a \"*.priv\" file with private DSA key in the <directory_with_update_files> for automatic signing");
                Environment.Exit(1);
            }

            if (Environment.GetCommandLineArgs().Length > 1 && !String.IsNullOrEmpty(Environment.GetCommandLineArgs()[1]))
            {
                try
                {
                    Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[1]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Environment.Exit(1);
                }
            }

            DSACryptoServiceProvider cryptoProvider = null;
            var privateKeyFilePaths = Directory.GetFiles(Environment.CurrentDirectory, "*.priv");
            if (privateKeyFilePaths.Length > 0)
            {
                // Reading private key
                String key = null;
                using (StreamReader reader = new StreamReader(privateKeyFilePaths[0]))
                {
                    key = reader.ReadToEnd();
                }

                cryptoProvider = new DSACryptoServiceProvider();
                cryptoProvider.FromXmlString(key);
            }

            var searchPattern = "*.exe";
            if (Environment.GetCommandLineArgs().Length > 2 && !String.IsNullOrEmpty(Environment.GetCommandLineArgs()[2]))
            {
                searchPattern = $"*{ Environment.GetCommandLineArgs()[2] }*.exe";
            }

            var exePaths = Directory.GetFiles(Environment.CurrentDirectory, searchPattern);

            if (exePaths.Length == 0)
            {
                Console.WriteLine($"Didn't find any matching files (pattern: \"{ searchPattern }\"");
                Environment.Exit(1);
            }

            foreach (var exePath in exePaths)
            {
                String dsaSignature = "";

                if (cryptoProvider != null)
                {
                    // open stream
                    Byte[] hash = null;
                    using (Stream inputStream = File.OpenRead(exePath))
                    {
                        hash = cryptoProvider.SignData(inputStream);
                    }

                    dsaSignature = Convert.ToBase64String(hash);
                }

                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                var fileInfo = new FileInfo(exePath);

                var appCastGenerator = new AppCastXMLGenerator(versionInfo.ProductName);
                appCastGenerator.AddItem(new AppCastXMLItem(versionInfo.ProductVersion, Path.GetFileName(versionInfo.FileName), versionInfo.ProductVersion, fileInfo.CreationTime, length: fileInfo.Length.ToString(), dsaSignature: dsaSignature));
                var appcastXmlDocument = appCastGenerator.GenerateAppCast();
                appcastXmlDocument.Save("appcast.xml");

                if (cryptoProvider != null)
                {
                    // open stream
                    Byte[] hash = null;
                    using (Stream inputStream = File.OpenRead("appcast.xml"))
                    {
                        hash = cryptoProvider.SignData(inputStream);
                    }

                    dsaSignature = Convert.ToBase64String(hash);
                    File.WriteAllText("appcast.xml.dsa", dsaSignature);
                }
            }
        }
    }
}
