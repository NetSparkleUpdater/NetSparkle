using NetSparkleUpdater;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using NetSparkleUpdater.AppCastHandlers;
using System.Text;
using CommandLine;
using System.Text.RegularExpressions;
using System.Linq;
using Console = Colorful.Console;
using System.Drawing;
using NetSparkleUpdater.AppCastGenerator;
using System.Web;
using System.ComponentModel;
using System.Xml.Linq;

namespace NetSparkleUpdater.Tools.AppCastGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // By default, if no args given, print help
            if (args.Count() == 0)
            {
                args = new string[] { "--help" };
            }
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run)
                .WithNotParsed(HandleParseError);
        }
        static void Run(Options opts)
        {

            if (opts.ShowExtendedExamples)
            {
                PrintExtendedExamples();
                return;
            }

            var signatureManager = new SignatureManager();

            if (!string.IsNullOrWhiteSpace(opts.PathToKeyFiles))
            {
                signatureManager.SetStorageDirectory(opts.PathToKeyFiles);
            }

            if (opts.ExportKeys)
            {
                Console.WriteLine("Private Key:");
                Console.WriteLine(Convert.ToBase64String(signatureManager.GetPrivateKey()));
                Console.WriteLine("Public Key:");
                Console.WriteLine(Convert.ToBase64String(signatureManager.GetPublicKey()));
                return;
            }

            if (opts.GenerateKeys)
            {
                var didSucceed = signatureManager.Generate(opts.ForceRegeneration);
                if (didSucceed)
                {
                    Console.WriteLine("Keys successfully generated", Color.Green);
                }
                else
                {
                    Console.WriteLine("Keys failed to generate", Color.Red);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(opts.PublicKeyOverride))
            {
                signatureManager.SetPublicKeyOverride(opts.PublicKeyOverride);
            }
            if (!string.IsNullOrWhiteSpace(opts.PrivateKeyOverride))
            {
                signatureManager.SetPrivateKeyOverride(opts.PrivateKeyOverride);
            }

            if (opts.BinaryToSign != null)
            {
                var signature = signatureManager.GetSignatureForFile(new FileInfo(opts.BinaryToSign));

                Console.WriteLine($"Signature: {signature}", Color.Green);

                return;
            }

            if (opts.BinaryToVerify != null)
            {
                var result = signatureManager.VerifySignature(new FileInfo(opts.BinaryToVerify), opts.Signature);

                if (result)
                {
                    Console.WriteLine($"Signature valid", Color.Green);
                } 
                else
                {
                    Console.WriteLine($"Signature invalid", Color.Red);
                }

                return;
            }

            // actually create the app cast
            var generator = new XMLAppCastMaker(signatureManager, opts);
            var appCastFileName = generator.GetPathToAppCastOutput(opts.OutputDirectory, opts.SourceBinaryDirectory);
            var outputDirName = Path.GetDirectoryName(appCastFileName);
            if (!Directory.Exists(outputDirName))
            {
                Console.WriteLine("Creating {0}", outputDirName);
                Directory.CreateDirectory(outputDirName);
            }
            var (items, productName) = generator.LoadAppCastItemsAndProductName(opts.SourceBinaryDirectory, opts.OverwriteOldItemsInAppcast, appCastFileName);
            if (items != null)
            {
                generator.SerializeItemsToFile(items, productName, appCastFileName);
                generator.CreateSignatureFile(appCastFileName, opts.SignatureFileExtension);
            }
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            errs.Output();
        }

        static void PrintExtendedExamples()
        {
            var examples = @"
#### Key Generation
# Generate Ed25519 keys for the first time
netsparkle-generate-appcast --generate-keys
# Store keys in a custom location
netsparkle-generate-appcast --key-path path/to/store/keys
# Pass in public key via command line
netsparkle-generate-appcast --public-key-override [YourPublicKeyHere]
# Pass in private key via command line
netsparkle-generate-appcast --private-key-override [YourPrivateKeyHere]

# By default, your Ed25519 signatures are stored on disk in your local 
# application data folder in a subdirectory called `netsparkle`. 
# If you want to export your keys to the console, you can do:
netsparkle-generate-appcast --export

# You can also store your keys in the following environment variables:
# set public key: SPARKLE_PUBLIC_KEY
# set private key: SPARKLE_PRIVATE_KEY

#### Generate a signature for a binary without creating an app cast:
netsparkle-generate-appcast --generate-signature path/to/binary.exe

#### Verifying Binaries
netsparkle-generate-appcast --verify path/to/binary.exe --signature base_64_signature

#### Using a custom key location:
# If your keys are sitting on disk somewhere
# (`NetSparkle_Ed25519.priv` and `NetSparkle_Ed25519.pub` -- both 
# in base 64 and both on disk in the same folder!), you can pass in 
# the path to these keys like this:
netsparkle-generate-appcast --key-path path/to/keys/

#### Generating an app cast

# Generate an app cast for Windows executables that are sitting in a 
# specific directory
netsparkle-generate-appcast -a directory/for/appcast/output/ -e exe -b directory/with/binaries/ -o windows

# Add change log info to your app cast
netsparkle-generate-appcast -b binary/folder -p change/log/folder

# Customize download URL for binaries and change logs
netsparkle-generate-appcast -b binary/folder -p change/log/folder -u https://example.com/downloads -p https://example.com/downloads/changelogs

# Set your application name for the app cast
netsparkle-generate-appcast -n ""My Awesome App"" -b binary/folder

# Use file versions in file names, e.g. for apps like ""My App 1.2.1.dmg""
netsparkle-generate-appcast -n ""macOS version"" -o macos -f true -b binary_folder -e dmg

# Don't overwrite the entire app cast file
netsparkle-generate-appcast --reparse-existing

# Don't overwrite the entire app cast file, but do overwrite items that are still on disk
netsparkle-generate-appcast --reparse-existing --overwrite-old-items

";
            Console.Write(examples);
        }
    }
}
