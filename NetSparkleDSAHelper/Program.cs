using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace NetSparkle.DSAHelper
{
    class Program
    {
        private static String _dsaPrivKey = "NetSparkle_DSA.priv";
        private static String _dsaPubKey = "NetSparkle_DSA.pub";

        static void Main(string[] args)
        {
            try
            {
                // check if we have some parameters
                if (args.Count() < 1)
                {
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

                            // Reading private key
                            String key = null;
                            using (StreamReader reader = new StreamReader(privKey))
                            {
                                key = reader.ReadToEnd();
                            }

                            DSACryptoServiceProvider prv = new DSACryptoServiceProvider();
                            prv.FromXmlString(key);

                            // open stream
                            Byte[] hash = null;
                            using (Stream inputStream = File.OpenRead(binary))
                            {
                                hash = prv.SignData(inputStream);
                            }

                            String base64Hash = Convert.ToBase64String(hash);
                            Console.WriteLine(base64Hash);
                        }
                        break;
                    case "/verify_update":
                        {
                            if (args.Count() != 4)
                            {
                                Usage();
                                Environment.ExitCode = -1;
                                return;
                            }

                            // get parameter
                            String binary = args[1];
                            String pubKeyFile = args[2];
                            String sign = args[3];

                            sign = sign.TrimStart('"');
                            sign = sign.TrimEnd('"');

                            NetSparkle.DSAVerificator dsaVerif = new NetSparkle.DSAVerificator(SecurityMode.UseIfPossible, null, pubKeyFile);
                            switch (dsaVerif.VerifyDSASignatureFile(sign, binary))
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

        static private void Usage()
        {
            ShowHeadLine();

            Console.WriteLine("NetSparkle.DSAHelper.exe /genkey_pair");
            Console.WriteLine("");
            Console.WriteLine("Generates a public and a private DSA key pair which is stored in the current");
            Console.WriteLine("working directory. The private is stored in the file NetSparkle_DSA.priv");
            Console.WriteLine("The public key will be stored in a file named NetSparkle_DSA.pub. Add the");
            Console.WriteLine("public key file as resource to your application.");
            Console.WriteLine("");
            Console.WriteLine("NetSparkle.DSAHelper.exe /sign_update {YourPackage.msi} {NetSparkle_DSA.priv}");
            Console.WriteLine("");
            Console.WriteLine("Allows to sign an existing update package unattended. YourPackage.msi has to be");
            Console.WriteLine("a valid path to the package binary as self (mostly Windows Installer packages).");
            Console.WriteLine("The NetSparkle_DSA.priv has to be a path to the generated DAS private key,");
            Console.WriteLine("which has to be used for signing.");
            Console.WriteLine("");
            Console.WriteLine("NetSparkle.DSAHelper.exe /verify_update {YourPackage.msi} {NetSparkle_DSA.pub} \"{Base64SignatureString}\"");
            Console.WriteLine("");
            
        }

        private static void ShowHeadLine()
        {
            Console.WriteLine("NetSparkle DSA Helper");
            Console.WriteLine("(c) 2011 Dirk Eisenberg under the terms of MIT license");
            Console.WriteLine("");
        }
    }
}
