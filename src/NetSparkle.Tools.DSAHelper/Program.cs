using System;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using NetSparkleUpdater.Enums;

namespace NetSparkleUpdater.DSAHelper
{
    class Program
    {
        private static string _dsaPrivKey = "NetSparkle_DSA.priv";
        private static string _dsaPubKey = "NetSparkle_DSA.pub";

        static void Main(string[] args)
        {
            try
            {
                // check if we have some parameters
                if (args.Count() < 1)
                {
                    Console.WriteLine("[ERROR] args.Count() < 1");
                    Usage();
                    return;
                }

                // check what parameter we have
                switch (args[0].ToLower())
                {
                    case "/genkey_pair":
                        {
                            // show headline
                            ShowHeadLine();

                            // verify if output file exists
                            if (File.Exists(_dsaPrivKey) || File.Exists(_dsaPubKey))
                            {
                                Console.WriteLine("Error: Output files are currently exists");
                                Environment.ExitCode = -1;
                                return;
                            }

                            // start key generation
                            Console.WriteLine("Generating key pair with 1024 Bits...");
                            DSACryptoServiceProvider prv = new DSACryptoServiceProvider();

                            Console.WriteLine("Storing private key to " + _dsaPrivKey);
                            using (StreamWriter sw = new StreamWriter(_dsaPrivKey))
                            {
                                sw.Write(prv.ToXmlString(true));
                            }

                            Console.WriteLine("Storing public key to " + _dsaPubKey);
                            using (StreamWriter sw = new StreamWriter(_dsaPubKey))
                            {
                                sw.Write(prv.ToXmlString(false));
                            }

                            Console.WriteLine("");
                        }
                        break;
                    case "/sign_update":
                        {
                            if (args.Count() != 3)
                            {
                                Console.WriteLine("[ERROR] args.Count() != 3");
                                PrintArgs(args);
                                Usage();
                                Environment.ExitCode = -1;
                                return;
                            }

                            // get parameter
                            String binary = args[1];
                            String privKey = args[2];

                            if (!File.Exists(binary))
                            {
                                Console.Error.WriteLine("Target binary " + binary + " does not exists");
                                Environment.ExitCode = -1;
                                return;
                            }

                            if (!File.Exists(privKey))
                            {
                                Console.Error.WriteLine("Private key file does not exists");
                                Environment.ExitCode = -1;
                                return;
                            }

                            Console.WriteLine(Utilities.GetDSASignature(binary, privKey));
                        }
                        break;
                    case "/verify_update":
                        {
                            if (args.Count() != 4)
                            {
                                Console.WriteLine("[ERROR] args.Count() != 4");
                                PrintArgs(args);
                                Usage();
                                Environment.ExitCode = -1;
                                return;
                            }

                            // get parameter
                            string binary = args[1];
                            string pubKeyFile = args[2];
                            string sign = args[3];

                            sign = sign.TrimStart('"');
                            sign = sign.TrimEnd('"');

                            NetSparkleUpdater.SignatureVerifiers.DSAChecker dsaVerif = 
                                new NetSparkleUpdater.SignatureVerifiers.DSAChecker(SecurityMode.UseIfPossible, null, pubKeyFile);
                            switch (dsaVerif.VerifySignatureOfFile(sign, binary))
                            {
                                case ValidationResult.Valid:
                                    Console.WriteLine("Binary " + binary + " is valid");
                                    break;
                                case ValidationResult.Invalid:
                                    Console.WriteLine("Binary " + binary + " is NOT valid");
                                    break;
                                case ValidationResult.Unchecked:
                                    Console.WriteLine("Binary " + binary + " could not be checked");
                                    break;
                            }
                        }
                        break;
                    default: 
                        Usage();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong :-(");
                Console.WriteLine(e.StackTrace);
            }
        }

        static private void PrintArgs(string[] args)
        {
            var i = 0;
            foreach (var arg in args)
            {
                Console.WriteLine("{0}: {1}", i++, arg);
            }
        }

        static private void Usage()
        {
            ShowHeadLine();

            Console.WriteLine("netsparkle-dsa /genkey_pair");
            Console.WriteLine("");
            Console.WriteLine("Generates a public and a private DSA key pair which is stored in the current");
            Console.WriteLine("working directory. The private is stored in the file NetSparkle_DSA.priv");
            Console.WriteLine("The public key will be stored in a file named NetSparkle_DSA.pub. Add the");
            Console.WriteLine("public key file as resource to your application.");
            Console.WriteLine("");
            Console.WriteLine("netsparkle-dsa /sign_update {YourPackage.msi} {NetSparkle_DSA.priv}");
            Console.WriteLine("");
            Console.WriteLine("Allows to sign an existing update package unattended. YourPackage.msi has to be");
            Console.WriteLine("a valid path to the package binary as self (mostly Windows Installer packages).");
            Console.WriteLine("The NetSparkle_DSA.priv has to be a path to the generated DAS private key,");
            Console.WriteLine("which has to be used for signing.");
            Console.WriteLine("");
            Console.WriteLine("netsparkle-dsa /verify_update {YourPackage.msi} {NetSparkle_DSA.pub} \"{Base64SignatureString}\"");
            Console.WriteLine("");
            
        }

        private static void ShowHeadLine()
        {
            Console.WriteLine("NetSparkle DSA Helper");
            Console.WriteLine("(c) 2011 Dirk Eisenberg, 2020-2023 Deadpikle under the terms of MIT license");
            Console.WriteLine("[NOTE] DSA signatures are considered insecure. Please consider using the");
            Console.WriteLine("[NOTE] NetSparkleUpdater.Tools.AppCastGenerator package instead to make use of");
            Console.WriteLine("[NOTE] ed25519 signatures instead. Thanks!");
            Console.WriteLine("");
        }
    }
}
