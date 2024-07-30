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
    /// An XML-based app cast document downloader and handler
    /// </summary>
    public class AppCastHandler : IAppCastHandler
    {
        private Configuration? _config;
        private string? _castUrl;
        private ISignatureVerifier? _signatureVerifier;
        private ILogger? _logWriter;
        private IAppCastDataDownloader? _dataDownloader;
        private IAppCastGenerator? _appCastGenerator;

        /// <summary>
        /// Extension (WITHOUT the "." at the start) for the signature
        /// file. Defaults to "signature".
        /// </summary>
        public string SignatureFileExtension { get; set; }

        /// <summary>
        /// An optional filtering component. Use this to manually filter
        /// items for custom channels (e.g. beta or alpha) or run your own
        /// logic on getting rid of older versions.
        /// </summary>
        public IAppCastFilter? AppCastFilter { get; set; }

        public AppCast? AppCast { get; set; }
        
        /// <summary>
        /// Create a new object with an empty list of <seealso cref="AppCastItem"/> items
        /// </summary>
        public AppCastHandler()
        {
            SignatureFileExtension = "signature";
        }

        /// <summary>
        /// Setup the app cast handler info for downloading and parsing app cast information
        /// </summary>
        /// <param name="dataDownloader">downloader that will manage the app cast download 
        /// (provided by <see cref="SparkleUpdater"/> via the 
        /// <see cref="SparkleUpdater.AppCastDataDownloader"/> property.</param>
        /// <param name="castUrl">full URL to the app cast file</param>
        /// <param name="config">configuration for handling update intervals/checks 
        /// (user skipped versions, etc.)</param>
        /// <param name="signatureVerifier">Object to check signatures of app cast information</param>
        /// <param name="logWriter">object that you can utilize to do any necessary logging</param>
        public void SetupAppCastHandler(IAppCastGenerator generator, IAppCastDataDownloader dataDownloader, string castUrl, Configuration config, ISignatureVerifier? signatureVerifier, ILogger? logWriter = null)
        {
            _dataDownloader = dataDownloader;
            _config = config;
            _castUrl = castUrl;
            _appCastGenerator = generator;

            _signatureVerifier = signatureVerifier;
            _logWriter = logWriter;
        }

        private void CheckSetupCalled()
        {
            if (_dataDownloader == null)
            {
                _logWriter?.PrintMessage("Warning: AppCastHandler has no IAppCastDataDownloader; did you forget to call SetupAppCastHandler()?");
            }
            if (_config == null)
            {
                _logWriter?.PrintMessage("Warning: AppCastHandler has no Configuration; did you forget to call SetupAppCastHandler()?");
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

        /// <inheritdoc/>
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
                var appcast = await _dataDownloader.DownloadAndGetAppCastDataAsync(_castUrl) ?? "";
                var signatureNeeded = Utilities.IsSignatureNeeded(
                    _signatureVerifier?.SecurityMode ?? SecurityMode.UseIfPossible, 
                    _signatureVerifier?.HasValidKeyInformation() ?? false, 
                    false);
                bool isValidAppcast = true;
                if (signatureNeeded)
                {
                    _logWriter?.PrintMessage("Downloading app cast signature data...");
                    var signature = "";
                    var extension = SignatureFileExtension?.Trim().TrimStart('.') ?? "signature";
                    try
                    {
                        signature = await _dataDownloader.DownloadAndGetAppCastDataAsync(_castUrl + "." + extension);
                    }
                    catch (Exception e)
                    {
                        _logWriter?.PrintMessage("Error reading app cast {0}.{2}: {1} ", _castUrl, e.Message, extension);
                    }
                    if (string.IsNullOrWhiteSpace(signature))
                    {
                        // legacy: check for .dsa file
                        try
                        {
                            _logWriter?.PrintMessage("Attempting to check for legacy .dsa signature data...");
                            signature = await _dataDownloader.DownloadAndGetAppCastDataAsync(_castUrl + ".dsa");
                        }
                        catch (Exception e)
                        {
                            _logWriter?.PrintMessage("Error reading app cast {0}.dsa: {1} ", _castUrl, e.Message);
                        }
                    }
                    isValidAppcast = VerifyAppCast(appcast, signature);
                }
                if (isValidAppcast)
                {
                    _logWriter?.PrintMessage("Appcast is valid!");
                    return appcast;
                }
                else
                {
                    _logWriter?.PrintMessage("Appcast is not valid!");
                }
            }
            catch (Exception e)
            {
                _logWriter?.PrintMessage("Error downloading app cast {0}: {1} ", _castUrl, e.Message);
            }
            _logWriter?.PrintMessage("Appcast is not valid");
            return null;
        }

        public async Task<AppCast> DeserializeAppCastAsync(string appCastStr)
        {
            if (_appCastGenerator != null)
            {
                return await _appCastGenerator.ReadAppCastAsync(appCastStr);
            }
            return new AppCast();
        }

        protected bool VerifyAppCast(string? appCast, string? signature)
        {
            if (string.IsNullOrWhiteSpace(appCast))
            {
                _logWriter?.PrintMessage("Cannot read response from URL {0}", _castUrl ?? "");
                return false;
            }

            // checking signature
            var signatureNeeded = Utilities.IsSignatureNeeded(
                _signatureVerifier?.SecurityMode ?? SecurityMode.UseIfPossible, 
                _signatureVerifier?.HasValidKeyInformation() ?? false,
                false);
            var appcastBytes = _dataDownloader?.GetAppCastEncoding().GetBytes(appCast);
            if (signatureNeeded && 
                (_signatureVerifier?.VerifySignature(signature ?? "", appcastBytes ?? Array.Empty<byte>()) ?? ValidationResult.Invalid) 
                    == ValidationResult.Invalid)
            {
                _logWriter?.PrintMessage("Signature check of appcast failed (Security mode is {0})", 
                    _signatureVerifier?.SecurityMode ?? SecurityMode.UseIfPossible);
                return false;
            }
            return true;
        }
        
        public List<AppCastItem> GetAvailableUpdates()
        {
            _logWriter?.PrintMessage("Getting available updates - there are {0} update(s) | latest = {1}", AppCast?.Items.Count ?? 0, AppCast?.Items.Count > 0 ? AppCast?.Items[0].Version : "" ?? "");
            return FilterUpdates(AppCast?.Items ?? new List<AppCastItem>());
        }
        
        public List<AppCastItem> GetAvailableUpdates(AppCast appCast)
        {
            _logWriter?.PrintMessage("Getting available updates - there are {0} update(s) | latest = {1}", appCast.Items.Count, appCast.Items.Count > 0 ? appCast.Items[0].Version ?? "" : "");
            return FilterUpdates(appCast.Items ?? new List<AppCastItem>());
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
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Windows update and we're on Windows", item.Version ?? "[Unknown version]", item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.NotThisPlatform;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !item.IsMacOSUpdate)
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a macOS update and we're on macOS", item.Version ?? "[Unknown version]", item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.NotThisPlatform;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !item.IsLinuxUpdate)
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it isn't a Linux update and we're on Linux", item.Version ?? "[Unknown version]", item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.NotThisPlatform;
            }
#endif
            return FilterItemResult.Valid;
        }

        /// <summary>
        /// Check if an AppCastItem update is valid, according to platform, signature requirements and current installed version number.
        /// In the case where your app implements a downgrade strategy, e.g. when switching from a beta to a 
        /// stable channel - there has to be a way to tell the update mechanism that you wish to ignore 
        /// the beta AppCastItem elements, and that the latest stable element should be installed.  
        /// </summary>
        /// <param name="installed">the currently installed Version</param>
        /// <param name="discardVersionsSmallerThanInstalled">if true, and the item's version is less than or equal to installed - the item will be discarded --> </param>
        /// <param name="signatureNeeded">whether or not a signature is required</param>
        /// <param name="item">the AppCastItem under consideration, every AppCastItem found in the appcast.xml file is presented to this function once</param>
        /// <returns>FilterItemResult.Valid if the AppCastItem should be considered as a valid target for installation.</returns>
        public FilterItemResult FilterAppCastItem(Version installed, bool discardVersionsSmallerThanInstalled, bool signatureNeeded, AppCastItem item)
        {
            return FilterAppCastItem(SemVerLike.Parse(installed.ToString()), discardVersionsSmallerThanInstalled, signatureNeeded, item);
        }

        /// <summary>
        /// Check if an AppCastItem update is valid, according to platform, signature requirements and current installed version number.
        /// In the case where your app implements a downgrade strategy, e.g. when switching from a beta to a 
        /// stable channel - there has to be a way to tell the update mechanism that you wish to ignore 
        /// the beta AppCastItem elements, and that the latest stable element should be installed.  
        /// </summary>
        /// <param name="installed">the currently installed Version</param>
        /// <param name="discardVersionsSmallerThanInstalled">if true, and the item's version is less than or equal to installed - the item will be discarded --> </param>
        /// <param name="signatureNeeded">whether or not a signature is required</param>
        /// <param name="item">the AppCastItem under consideration, every AppCastItem found in the appcast.xml file is presented to this function once</param>
        /// <returns>FilterItemResult.Valid if the AppCastItem should be considered as a valid target for installation.</returns>
        public FilterItemResult FilterAppCastItem(SemVerLike installed, bool discardVersionsSmallerThanInstalled, bool signatureNeeded, AppCastItem item)
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

            // filter versions without signature if we need signatures. But accept version without downloads.
            if (signatureNeeded && string.IsNullOrWhiteSpace(item.DownloadSignature) && !string.IsNullOrWhiteSpace(item.DownloadLink))
            {
                _logWriter?.PrintMessage("Rejecting update for {0} ({1}, {2}) because it we needed a DSA/other signature and " +
                    "the item has no signature yet has a download link of {3}", item.Version ?? "[Unknown version]",
                    item.ShortVersion ?? "[Unknown short version]", item.Title ?? "[Unknown title]", item.DownloadLink ?? "[Unknown download link]");
                return FilterItemResult.SignatureIsMissing;
            }

            return FilterItemResult.Valid;
        }

        /// <summary>
        /// Returns filtered list of updates between current installed version and latest version in <see cref="Items"/>. 
        /// Currently installed version is NOT included in the output.
        /// </summary>
        /// <returns>A list of <seealso cref="AppCastItem"/> updates that could be installed</returns>
        public virtual List<AppCastItem> FilterUpdates(List<AppCastItem> items)
        {
            CheckSetupCalled();
            var installed = SemVerLike.Parse(_config?.InstalledVersion ?? "");
            bool shouldFilterOutSmallerVersions = true;

            if (AppCastFilter != null)
            {
                _logWriter?.PrintMessage("Running custom AppCastFilter to filter out items...");
                items = AppCastFilter.GetFilteredAppCastItems(installed, items)?.ToList() ?? new List<AppCastItem>();

                // AppCastReducer user has responsibility to filter out both older and not needed versions,
                // so the XMLAppCast object no longer needs to handle filtering out old versions.
                // Also this allows to easily switch between pre-release and retail versions, on demand.
                // The XMLAppCast will still filter out items that don't match the current OS.
                shouldFilterOutSmallerVersions = false;
            }

            var signatureNeeded = Utilities.IsSignatureNeeded(
                    _signatureVerifier?.SecurityMode ?? SecurityMode.UseIfPossible, 
                    _signatureVerifier?.HasValidKeyInformation() ?? false, 
                    false);

            _logWriter?.PrintMessage("Looking for available updates; our installed version is {0}; do we need a signature? {1}; are we filtering out smaller versions than our current version? {2}", installed, signatureNeeded, shouldFilterOutSmallerVersions);
            return items.Where((item) =>
            {
                if (FilterAppCastItem(installed, shouldFilterOutSmallerVersions, signatureNeeded, item) == FilterItemResult.Valid)
                {
                    // accept all valid items
                    _logWriter?.PrintMessage("Item with version {0} ({1}) is a valid update! It can be downloaded at {2}", item.Version ?? "[Unknown version]", item.ShortVersion ?? "[Unknown short version]", item.DownloadLink ?? "[Unknown download link]");
                    // TODO: should we reject items with no Version or DownloadLink here?
                    return true;
                }
                return false;
            }).ToList();
        }
    }
}
