using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetSparkleUpdater.AppCastHandlers
{
    /// <summary>
    /// The AppCastHelper class is responsible for downloading
    /// app cast data, downloading and checking app cast signature data,
    /// and filtering app cast items when looking for available updates.
    /// </summary>
    public class AppCastHelper
    {
        private string? _installedVersion;
        private string? _castUrl;
        private ISignatureVerifier? _signatureVerifier;
        private ILogger? _logWriter;
        private IAppCastDataDownloader? _dataDownloader;

        /// <summary>
        /// Extension (WITHOUT the "." at the start) for the signature
        /// file. Defaults to "signature".
        /// </summary>
        public string SignatureFileExtension { get; set; }

        /// <summary>
        /// An optional filtering component. Use this to manually filter
        /// items for custom channels (e.g. beta or alpha) or run your own
        /// logic on getting rid of older versions.
        /// NOTE: When you use this interface
        /// with <seealso cref="AppCastHelper"/>, you must filter out old versions of <seealso cref="AppCastItem"/>
        /// yourself if you want that to happen! In other words, <seealso cref="AppCastHelper"/> skips this step
        /// when there is an <seealso cref="IAppCastFilter"/> implementation available.
        /// 
        /// Note that items will still be filtered based on OS and signature information as necessary. You don't
        /// need to handle that.
        /// </summary>
        public IAppCastFilter? AppCastFilter { get; set; }

        /// <summary>
        /// When filtering for available updates, remove items with
        /// no version information set.
        /// </summary>
        public bool FilterOutItemsWithNoVersion { get; set; }

        /// <summary>
        /// When filtering for available updates, remove items with
        /// no download link set.
        /// </summary>
        public bool FilterOutItemsWithNoDownloadLink { get; set; }
        
        /// <summary>
        /// Create a new object with an empty list of <seealso cref="AppCastItem"/> items
        /// </summary>
        public AppCastHelper()
        {
            SignatureFileExtension = "signature";
            FilterOutItemsWithNoVersion = true;
            FilterOutItemsWithNoDownloadLink = true;
        }

        /// <summary>
        /// Setup the app cast handler info for downloading and parsing app cast information
        /// </summary>
        /// <param name="dataDownloader">downloader that will manage the app cast download 
        /// (provided by <see cref="SparkleUpdater"/> via the 
        /// <see cref="SparkleUpdater.AppCastDataDownloader"/> property.</param>
        /// <param name="castUrl">full URL to the app cast file</param>
        /// <param name="installedVersion">installed version of the software</param>
        /// <param name="signatureVerifier">Object to check signatures of app cast information</param>
        /// <param name="logWriter">object that you can utilize to do any necessary logging</param>
        public virtual void SetupAppCastHelper(IAppCastDataDownloader dataDownloader, string castUrl, 
            string? installedVersion, ISignatureVerifier? signatureVerifier, ILogger? logWriter = null)
        {
            _dataDownloader = dataDownloader;
            _installedVersion = installedVersion;
            _castUrl = castUrl;

            _signatureVerifier = signatureVerifier;
            _logWriter = logWriter;
        }

        private void CheckSetupCalled()
        {
            if (_dataDownloader == null)
            {
                _logWriter?.PrintMessage("Warning: AppCastHandler has no IAppCastDataDownloader; did you forget to call SetupAppCastHandler()?");
            }
            if (string.IsNullOrWhiteSpace(_castUrl))
            {
                _logWriter?.PrintMessage("Warning: AppCastHandler has no app cast URL; did you forget to call SetupAppCastHandler()?");
            }
            if (_signatureVerifier == null)
            {
                _logWriter?.PrintMessage("Warning: AppCastHandler has no ISignatureVerifier; did you forget to call SetupAppCastHandler()?");
            }
        }

        private bool IsSignatureNeeded()
        {
            return Utilities.IsSignatureNeeded(
                    _signatureVerifier?.SecurityMode ?? SecurityMode.UseIfPossible, 
                    _signatureVerifier?.HasValidKeyInformation() ?? false, 
                    false);
        }

        private async Task<string?> DownloadSignatureData(string appCastUrl, string signatureExtension)
        {
            try
            {
                if (_dataDownloader != null)
                {
                    var signatureData = await _dataDownloader.DownloadAndGetAppCastDataAsync(appCastUrl + "." + signatureExtension.Trim().TrimStart('.'));
                    return signatureData;
                }
            }
            catch (Exception e)
            {
                _logWriter?.PrintMessage("Error grabbing signature {0}.{2}: {1} ", appCastUrl, e.Message, signatureExtension);
            }
            return null;
        }

        /// <summary>
        /// Downloads an app cast from the app cast URL sent to <see cref="SetupAppCastHelper"/>
        /// and verifies its signature (if signatures are required for app casts).
        /// Configure functionality by implementing your own <seealso cref="IAppCastDataDownloader"/> 
        /// or by overriding this method in a subclass.
        /// </summary>
        /// <returns>Downloaded app cast raw string if download and verification succeeded. Null if
        /// the operation failed somehow.</returns>
        public virtual async Task<string?> DownloadAppCast()
        {
            CheckSetupCalled();
            if (_castUrl == null || string.IsNullOrWhiteSpace(_castUrl))
            {
                _logWriter?.PrintMessage("Warning: DownloadAndParse called with no app cast URL set; did you forget to call SetupAppCastHandler()?");
                return null;
            }
            if (_dataDownloader == null)
            {
                _logWriter?.PrintMessage("Warning: DownloadAndParse called with no data downloader set; did you forget to call SetupAppCastHandler()?");
                return null;
            }
            try
            {
                _logWriter?.PrintMessage("Downloading app cast data...");
                var appCast = await _dataDownloader.DownloadAndGetAppCastDataAsync(_castUrl) ?? "";
                if (string.IsNullOrWhiteSpace(appCast))
                {
                    _logWriter?.PrintMessage("Failed to download app cast from URL {0}", _castUrl ?? "");
                    return null;
                }
                bool isValidAppCast = true;
                if (IsSignatureNeeded())
                {
                    _logWriter?.PrintMessage("Downloading app cast signature data...");
                    var signature = "";
                    var extension = SignatureFileExtension?.Trim() ?? "signature";
                    signature = await DownloadSignatureData(_castUrl, extension);
                    if (string.IsNullOrWhiteSpace(signature))
                    {
                        _logWriter?.PrintMessage("Attempting to check for legacy .dsa signature data...");
                        signature = await DownloadSignatureData(_castUrl, ".dsa");
                    }
                    isValidAppCast = VerifyAppCast(appCast, signature);
                }
                if (isValidAppCast)
                {
                    _logWriter?.PrintMessage("Appcast is valid!");
                    return appCast;
                }
            }
            catch (Exception e)
            {
                _logWriter?.PrintMessage("Error downloading app cast {0}: {1} ", _castUrl, e.Message);
            }
            _logWriter?.PrintMessage("Appcast is not valid");
            return null;
        }

        /// <summary>
        /// Verify a given app cast string based on a given cryptographic signature.
        /// Verified via the <seealso cref="ISignatureVerifier"/> sent to 
        /// <see cref="SetupAppCastHelper"/>.
        /// </summary>
        /// <param name="appCast">App cast string to verify</param>
        /// <param name="signature">Signature of app cast</param>
        /// <returns>True if signature is not required OR required and valid; false otherwise</returns>
        protected bool VerifyAppCast(string? appCast, string? signature)
        {
            if (string.IsNullOrWhiteSpace(appCast))
            {
                _logWriter?.PrintMessage("Cannot read response from URL {0}", _castUrl ?? "");
                return false;
            }

            // checking signature
            var appcastBytes = _dataDownloader?.GetAppCastEncoding().GetBytes(appCast);
            if (IsSignatureNeeded() && 
                (_signatureVerifier?.VerifySignature(
                    signature ?? "", appcastBytes ?? Array.Empty<byte>()) ?? ValidationResult.Invalid) 
                    == ValidationResult.Invalid)
            {
                _logWriter?.PrintMessage("Signature check of appcast failed (Security mode is {0})", 
                    _signatureVerifier?.SecurityMode ?? SecurityMode.UseIfPossible);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns filtered list of updates between current installed version and latest version.
        /// By default, checks operating system (always does this no matter what unless this function is overriden), 
        /// whether or not signatures are available (if signatures are required), and
        /// version of the software (filters out anything that is the same version or older).
        /// Adjust behavior via the <seealso cref="FilterOutItemsWithNoVersion"/> and 
        /// <seealso cref="FilterOutItemsWithNoDownloadLink"/> bool properties,
        /// <seealso cref="AppCastHelper.AppCastFilter"/> (NOTE: if you use this, you will need to handle filtering 
        /// out items that are older than the user's current version), or override this method in a subclass.
        /// </summary>
        /// <param name="items">List of <seealso cref="AppCastItem"/> items to filter</param>
        /// <returns>A list of filtered <seealso cref="AppCastItem"/> updates that could be installed</returns>
        public virtual List<AppCastItem> FilterUpdates(List<AppCastItem> items)
        {
            CheckSetupCalled();
            var installedVersion = SemVerLike.Parse(_installedVersion ?? "");
            bool shouldFilterOutSmallerVersions = true;

            if (AppCastFilter != null)
            {
                _logWriter?.PrintMessage("Using custom AppCastFilter to filter out items...");
                items = AppCastFilter.GetFilteredAppCastItems(installedVersion, items)?.ToList() ?? new List<AppCastItem>();

                // AppCastFilter user has responsibility to filter out both older and not needed versions,
                // so the AppCastHandler object no longer needs to handle filtering out old versions.
                // Also this allows to easily switch between pre-release and retail versions, on demand.
                // The AppCastHandler will still filter out items that don't match the current OS.
                shouldFilterOutSmallerVersions = false;
            }

            var signatureNeeded = IsSignatureNeeded();
            _logWriter?.PrintMessage("Looking for available updates; our installed version is {0}; " +
                "do we need a signature? {1}; are we filtering out smaller versions than our " +
                "current version? {2}", installedVersion, signatureNeeded, shouldFilterOutSmallerVersions);
            return items.Where((item) =>
            {
                var filterResult = FilterAppCastItem(installedVersion, shouldFilterOutSmallerVersions, signatureNeeded, item);
                if (filterResult == FilterItemResult.Valid)
                {
                    if (FilterOutItemsWithNoVersion && 
                        (item.Version == null || string.IsNullOrWhiteSpace(item.Version)))
                    {
                        return false;
                    }
                    if (FilterOutItemsWithNoDownloadLink &&
                        (item.DownloadLink == null || string.IsNullOrWhiteSpace(item.DownloadLink)))
                    {
                        return false;
                    }
                    // accept all valid items
                    _logWriter?.PrintMessage("Item with version {0} ({1}) is a valid update! It can be downloaded at {2}", 
                        item.Version ?? "[Unknown version]", item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                    return true;
                }
                return false;
            }).ToList();
        }

        /// <summary>
        /// Check if an AppCastItem update is valid, according to platform, signature requirements 
        /// and current installed version number.
        /// In the case where your app implements a downgrade strategy, e.g. when switching from a beta to a 
        /// stable channel - there has to be a way to tell the update mechanism that you wish to ignore 
        /// the beta AppCastItem elements, and that the latest stable element should be installed.  
        /// </summary>
        /// <param name="installed">the currently installed Version</param>
        /// <param name="discardVersionsSmallerThanInstalled">if true, and the item's version is less than or equal to installed - the item will be discarded</param>
        /// <param name="signatureNeeded">whether or not a signature is required</param>
        /// <param name="item">the AppCastItem under consideration, every AppCastItem found in the appcast.xml file is presented to this function once</param>
        /// <returns>FilterItemResult.Valid if the AppCastItem should be considered as a valid target for installation.</returns>
        protected FilterItemResult FilterAppCastItem(SemVerLike installed, bool discardVersionsSmallerThanInstalled, bool signatureNeeded, AppCastItem item)
        {
            var osFilterResult = FilterAppCastItemByOS(item);
            if (osFilterResult != FilterItemResult.Valid)
            {
                return osFilterResult;
            }

            if (discardVersionsSmallerThanInstalled)
            {
                // filter smaller versions
                if (SemVerLike.Parse(item.Version).CompareTo(installed) <= 0)
                {
                    _logWriter?.PrintMessage(
                        "Rejecting update for {0} ({1}, {2}) because it is older than our current version of {3}",
                        item.Version ?? "[Unknown version]",
                        item.ShortVersion ?? "[Unknown short version]", item.Title ?? "[Unknown title]", installed);
                    return FilterItemResult.VersionIsOlderThanCurrent;
                }
            }

            // filter versions without signature if we need signatures
            if (signatureNeeded && 
                string.IsNullOrWhiteSpace(item.DownloadSignature) && !string.IsNullOrWhiteSpace(item.DownloadLink))
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it we needed a Ed25519/other signature and " +
                    "the item has no signature yet has a download link of {3}", item.Version ?? "[Unknown version]",
                    item.ShortVersion ?? "[Unknown short version]", item.Title ?? "[Unknown title]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.SignatureIsMissing;
            }

            return FilterItemResult.Valid;
        }

        /// <summary>
        /// Check if an AppCastItem update is valid based on operating system. The user's current operating system
        /// needs to match the operating system of the AppCastItem for the AppCastItem to be valid.
        /// </summary>
        /// <param name="item">the AppCastItem under consideration</param>
        /// <returns>FilterItemResult.Valid if the AppCastItem should be considered as a valid target for installation;
        /// FilterItemResult.NotThisPlatform otherwise.</returns>
        protected FilterItemResult FilterAppCastItemByOS(AppCastItem item)
        {
#if NETFRAMEWORK
            // don't allow non-windows updates
            if (!item.IsWindowsUpdate)
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Windows update and we're on Windows", item.Version ?? "[Unknown version]", 
                    item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.NotThisPlatform;
            }
#else
            // check operating system and filter out ones that don't match the current
            // operating system
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !item.IsWindowsUpdate)
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Windows " +
                    "update and we're on Windows", item.Version ?? "[Unknown version]", 
                    item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.NotThisPlatform;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !item.IsMacOSUpdate)
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a macOS " +
                    "update and we're on macOS", item.Version ?? "[Unknown version]", 
                    item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.NotThisPlatform;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !item.IsLinuxUpdate)
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Linux " +
                    "update and we're on Linux", item.Version ?? "[Unknown version]", 
                    item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.NotThisPlatform;
            }
#endif
            return FilterItemResult.Valid;
        }
    }
}
