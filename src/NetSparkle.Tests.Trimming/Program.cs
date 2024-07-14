using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var checker = new Ed25519Checker(SecurityMode.Strict, "");
var sparkle = new SparkleUpdater("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml", new DSAChecker(NetSparkleUpdater.Enums.SecurityMode.Strict))
            {
                ShowsUIOnMainThread = false,
                //RelaunchAfterUpdate = true,
                //UseNotificationToast = true
            };
            await sparkle.CheckForUpdatesQuietly();