using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;

var checker = new Ed25519Checker(SecurityMode.Strict, string.Empty);
var sparkle = new SparkleUpdater("https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml", new DSAChecker(SecurityMode.Strict))
{
    //RelaunchAfterUpdate = true,
    //UseNotificationToast = true
};
await sparkle.CheckForUpdatesQuietly();