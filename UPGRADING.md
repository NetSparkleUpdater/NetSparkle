## Updating to 3.X

* Lines ending in [**] are candidates for backporting to 2.x if time/desired by users.
* Lines ending in [**!] have been backported.

**Breaking Changes**

* The built-in Avalonia UI now runs on version 11 of Avalonia (6a9a6426324e357daf0f9991d7a981b19a009b93, a0a7314317e9fc712270f75f31c6442a2d454da3, 46de3e9c9525cac4026a7959e44764752cdf36ee, `avalonia-preview` branch)
* Bumped minimum version of .NET Framework from 4.5.2 to 4.6.2 (57fa9ad2597ec6615894464b8f2ccef881dda52b) as 4.5.2 is EOL per https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-framework
* Removed `Newtonsoft.Json` dependency for .NET Framework build and use `System.Text.Json` (57fa9ad2597ec6615894464b8f2ccef881dda52b)
* `System.Text.Json` is only explicitly included for `netstandard2.0` and `net462` as otherwise the framework already includes it (#601)
* .NET Framework `ReleaseNotesGrabber` uses an `HttpClient` instead of a `WebClient` for downloading (31523e95b50f00fab3fb3e043dea7d32227d5862)
* `WebClientFileDownloader` renamed to `WebFileDownloader` to better reflect functionality (e85235f512f4344c9a675e56e9a4d50434c33959)
* Changed `net6` to `net6.0` to be consistent with newer versions
* Removed `netcoreapp3.1` and `net5.0` compatibility as they are both EOL per https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core (87381f2d3422e36fe5f614c85e43687eed497c42)
* Moved to official/non-preview builds of `System.Drawing.Common` for 8.x and bump up to `8.0.0` for older versions of .NET (87381f2d3422e36fe5f614c85e43687eed497c42) [**]
* Use `.axaml` in all Avalonia-related projects (46de3e9c9525cac4026a7959e44764752cdf36ee, e6d5ad20fec37e018a23ab46ef34d728b8104e96)
* `Exec(string cmd, bool waitForExit = true)` now returns a `bool`: `true` for the process starting, `false` otherwise
* The `IAppCastFilter` API has changed to: `IEnumerable<AppCastItem> GetFilteredAppCastItems(SemVerLike installed, IEnumerable<AppCastItem> items)`
* `IAppCastFilter` now expects you to filter out old versions and do sorting yourself as needed (previously filtering out old versions yourself could be avoided via a `bool` property on `FilterResult`; to do this easily yourself now, use the `AppCastReducers.RemoveOlderVersions` reducer like so: `AppCastReducers.RemoveOlderVersions(installed, itemsToFilter)`)
* The `FilterResult` class has been removed in its entirety (882ee260dd24245c53d351d4151b8b4bd4a53588)
* Used `SemVerLike` everywhere instead of `System.Version` for semver compatibility
* `WebFileDownloader` now deletes files on cancellation of a download like `LocalFileDownloader` did already (1cd2284c41bbe85d41566915965ad2acdb1a61f5)
* `WebFileDownloader` does not call `PrepareToDownloadFile()` in its constructor anymore. Now, it is called after the object is fully created. (420f961dfa9c9071332e2e0737b0f287d2cfa5dc)
* NOTE: If you update to .NET 8+, the location of `JSONConfiguration` save data, by default, will CHANGE due to a change in .NET 8 for `Environment.SpecialFolder.ApplicationData`. See: https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/8.0/getfolderpath-unix. This may cause user's "skipped version" or other data to be lost unless you migrate this data yourself. For most users who are using the defaults, this is likely only a minor (if any) inconvenience at all, but it is worth noting.
* `AssemblyReflectionAccessor` has been deprecated. Please use `AsmResolverAccessor` instead. (#587)
* `AssemblyDiagnosticAccessor.AssemblyVersion` now returns `FileVersionInfo.GetVersionInfo(assemblyName).ProductVersion` rather than `FileVersionInfo.GetVersionInfo(assemblyName).FileVersion` so we can get semver versioning information
* For ed25519 use, changed from BouncyCastle to Chaos.NaCl (large file size savings and we don't need all of BouncyCastle's features). You can verify its use and code online here: https://github.com/NetSparkleUpdater/Chaos.NaCl. Additionally, this package is available on NuGet for your own projects.
* `LogWriter` has undergone several changes:
  * `public static string tag = "netsparkle:";` has changed to `public string Tag { get; set; } = "netsparkle:";`
  * Instead of a simple `bool` to control printing to `Trace.WriteLine` or `Console.WriteLine`, there is now a new enum, `NetSparkleUpdater.Enums.LogWriterOutputMode`, which you can use to control whether `LogWriter` outputs to `Console`, `Trace`, `Debug`, or to `None` (don't output at all).
  * Removed `PrintDiagnosticToConsole` property and replaced it with `OutputMode` of type `LogWriterOutputMode`.
  * The default print mode for `LogWriter` is still `Trace` by default, as it was in prior versions.
  * By default, timestamps are now output along with the `Tag` and actual log item
* `RegistryConfiguration` has changed its default final path to `NetSparkleUpdater` instead of `AutoUpdate`. Please migrate saved configuration data yourself if you need to do so for your users (probably not necessary).
* `ShowUpdateNeededUI` no longer shows an update window if the number of update items is 0. (Arguably a bug fix, but technically a breaking change.)
* Major refactoring for app cast handling/parsing to clean up logic / make new formats easier
  * New `AppCast` model class that holds info on the actual app cast and its items
  * `AppCastItem` no longer contains information on serializing or parsing to/from XML
  * App cast parsing/serializing is now handled by an `IAppCastGenerator` implementation
    * `XMLAppCast` renamed to `AppCastHelper`
    * `SparkleUpdater` now has an `AppCastGenerator` member that handles deserializing the app cast rather than `AppCastHelper`
    * `AppCastItem` serialization now expects the full download link to already be known (serialization will not consider the overall app cast URL)
  * `IAppCastHandler` is no longer available/used.
    * App cast downloading and item filtering is now handled by `AppCastHelper`.
    * If you want to manage downloads, implement `IAppCastDataDownloader` and set `SparkleUpdater.AppCastDataDownloader`
    * If you want to manage deserializing/serializing the app cast, implement `IAppCastGenerator` and set `SparkleUpdater.AppCastGenerator`
    * If you want to manage filtering on your own, implement `IAppCastFilter` and set `SparkleUpdater.AppCastHelper.AppCastFilter`
      * `AppCastHelper` also has two new properties: `FilterOutItemsWithNoVersion` and `FilterOutItemsWithNoDownloadLink`, which both default to `true`.
    * If you need absolute control more than the above, you can subclass `AppCastHelper` and override methods: `SetupAppCastHelper`, `DownloadAppCast`, and `FilterUpdates`. This probably is not necessary, however, and you can do what you want through the interfaces, most likely.
  * `AppCastHelper.SetupAppCastHelper` signature is now `SetupAppCastHelper(IAppCastDataDownloader dataDownloader, string castUrl, string? installedVersion, ISignatureVerifier? signatureVerifier, ILogger? logWriter = null)` (note: no longer takes a `Configuration` object)
* Renamed `AppCastItem.OperatingSystemString` to `OperatingSystem`
* XML app casts write `version`, `shortVersion`, and `criticalUpdate` to the `<item>` tag and the `<enclosure>` (both for backwards/Sparkle compat; we'd rather not write to `<enclosure>` but we don't want to break anyone that updates their app cast gen without updating the main library).
  * If both the overall `<item>` and the `<enclosure>` have this data, the info from the `<item>` is prioritized.
  * JSON app casts are not affected.

**Changes/Fixes**

* Don't get download name if no need to (e63ebb054d62e26232b808189eb4619566952f3a) [**]
* Fixed some documentation in `LocalFileDownloader` (6ca714ca62577bc7412353338a148a03810ba81a) [**]
* Use net8.0 in Avalonia samples (46de3e9c9525cac4026a7959e44764752cdf36ee)
* Remove obsolete NetSparkleException override (3f54a53c7839fb99b291f8bc15b751aceb2d28de)
* Fixed `IsCriticalUpdate` not being written to the app cast (2142e8e0c277523ac33b2ba1d19d0e1fd9ca8483) [**!]
* Added more compatibility with Sparkle feeds (c9e2ddb4eea291c7f12e7747d9822aaa7fdb8c29) - see #275 [**]
* Added `InstallerProcess` public var for easier access of the installer process in case you are making an app that updates other apps (5e6c321846d08645e0fc891a33f1461780c4b405) [**]
* Allow user to avoid killing the parent process that started the update process -- `ShouldKillParentProcessWhenStartingInstaller`; defaults to `true` (19fae1ab1766eafea5a07881ac2598b4355f69f6) [**]
* Allow setting the process ID to kill when starting the installer -- `ProcessIDToKillBeforeInstallerRuns`; defaults to `null`(8cc81e0a561b82c220ecd3a86c5f424a236dc268) [**]
* Fixed Cancel button not working in Avalonia checking for updates window (f35e8968c1c7167664f90f91835ea3019acd2046) [**]
* Added `InstallerProcessAboutToStart` event. You can use this to modify/see the installer process before it actually begins and optionally make `NetSparkleUpdater` not run the process (you'll have to run the process yourself in that case). (a5cbbf1abce83de209f9d62a8fe2434f6b5cc85a)
* Added `SemVerLike` class that combines the normal .NET `System.Version` with semver properties (@kenjiuno)
* Added `VersionTrimmers` for trimming `SemVerLike` versions to `System.Version` (@kenjiuno)
* Added `AppCastReducers` helpers for common ways of filtering app cast items (@kenjiuno)
* Added `SparkleUpdater.InstallUpdateFailed` to see why `InstallUpdate` or its related installer methods fail. Also adds `Enums.InstallUpdateFailureReason`. (fe546de8667b1a5d6e0c4a72a7c128dc954f4aba)
* `WebFileDownloader.PrepareToDownloadFile` is now `virtual` (84df81122cfae9309de4f5b79489e46287bf3a62)
* The app cast maker now expects at least `Major.Minor` for version numbers and no longer accepts single digits as version numbers (this fixes things like "My Super App Version 2.exe" having 2 being detected as the version number when the version number is in a prior folder name, and also ensures that the right number is being read)
* If `JSONConfiguration` cannot save data, `SparkleUpdater` now uses the `DefaultConfiguration` class, which basically has no information set on it and will always check for updates for the user. (a33266ac99b3eed83ff481ee7ef06cc5a0b1ab40)
* Added `AsmResolverAccessor` class for assembly loading information without reflection (replaces `AssemblyReflectionAccessor`) (#587)
* Fixed Avalonia message window close button not working properly (394841d47c8b6739ca0ebcec87303529b81da904)
* Fixed `AssemblyReflectionEditor` not loading in a way that allowed for proper file closing of the DLL (fixes a unit testing issue)
* Avalonia UI now uses `CompiledBinding`
* Fixed `JSONConfiguration` not using correct default last config/check update time
* Added `nullability` compatibility to core and UI libraries (#595)
  * Base language version is now 8.0 (9.0 for Avalonia), but this is only used for nullability compatibility (compile-time), so this shouldn't affect older projects (`.NET 4.6.2`, `netstandard2.0`) and is thus a non-breaking change
* Fixed initialization issue in DownloadProgressWindow (WinForms) icon use
* Added `JsonAppCastGenerator` to read/write app casts from/to JSON (output json with the app cast generator option `--output-type json` and then set the `AppCastGenerator` on your `SparkleUpdater` object to an instance of `JsonAppCastGenerator`)
* Added `ChannelAppCastFilter` (implements `IAppCastFilter`) for easy way to filter your app cast items by a channel, e.g. `beta` or `alpha`. Use by setting `AppCastHelper.AppCastFilter`. Uses simple `string.Contains` invariant lowercase string check to search for channels in the `AppCastItem`'s version information.
  * If you want to allow versions like `2.0.0-beta.1`, set `ChannelSearchNames` to `new List<string>() {"beta"}`
  * Set `RemoveOlderItems` to `false` if you want to keep old versions when filtering, e.g. for rolling back to an old version
  * Set `KeepItemsWithNoChannelInfo` to `false` if you want to remove all items that don't match the given channel (doing this will not let people on a beta version update to a non-beta version!)
* `AppCast? SparkleUpdater.AppCastCache` holds the most recently deserialized app cast information.
* `AppCastItem` has a new `Channel` property. Use it along with `ChannelAppCastFilter` if you want to use channels that way instead of via your `<Version>` property. In the app cast generator, use the `--channel` option to set this (note that using this will set ALL items added to the app cast to that specific channel; to add only new items to your app cast instead of rewriting the whole thing, use the `--reparse-existing` option).
* App cast generator has a new `--use-ed25519-signature-attribute` to use `edSignature` in its XML output instead of `signature` (json output unaffected) to match the original Sparkle library

## Updating from 0.X or 1.X to 2.X

This section holds info on major changes when moving from versions 0.X or 1.Y. If we've forgotten something important, please [file an issue](https://github.com/NetSparkleUpdater/NetSparkle/issues).

* Minimum .NET Framework requirement, if running on .NET Framework, is now .NET Framework 4.5.2 instead of 4.5.1. Otherwise, the core library is .NET Standard 2.0 compatible, which means you can use it in your .NET Core and .NET 5+ projects.
* Change of base namespace from `NetSparkle` to `NetSparkleUpdater`
* `Sparkle` renamed to `SparkleUpdater` for clarity
* More logs are now written via `LogWriter` to help you debug and figure out any issues that are going on
* The default `NetSparkleUpdater` package (`NetSparkleUpdater.SparkleUpdater`) **has no built-in UI**. Please use one of the NetSparkleUpdater packages with a UI if you want a built-in UI that is provided for you.
  * Note that if you do _not_ use a `UIFactory`, you **must** use the `CloseApplication` or `CloseApplicationAsync` events to close your application; otherwise, your downloaded update file will never be executed/read! The only exception to this is if you want to handle all aspects of installing the update package yourself.
* XML docs are now properly shipped with the code for all public and protected methods rather than being here in this README file
  * Enabled build time warnings for functions that need documentation that don't have it
* `SparkleUpdater` constructors now require an `ISignatureVerifier` in order to "force" you to choose your signature verification method
* `SecurityMode` is a new enum that defines which signatures are required and which signatures are not required
  * Added `SecurityMode.OnlyVerifySoftwareDownloads` if you want to verify only software download signatures and don't care about verifying your app cast or release notes
* UIs are now in different namespaces. If you want to use a UI, you must pass in a `UIFactory` that implements `IUIFactory` and handles showing/handling all user interface elements
  * `NetSparkleUpdater` offers basic, built-in UIs for WinForms, WPF, and Avalonia. Copy & paste these files to your project and modify them to make them work better in your project!
  * `SparkleUpdater` no longer holds its own `Icon`. This is now handled by the `UIFactory` object.
  * `HideReleaseNotes`, `HideRemindMeLaterButton`, and `HideSkipButton` are all handled by the `UIFactory` objects
* Added built-in UIs for WPF and [Avalonia](https://github.com/AvaloniaUI/Avalonia) 0.10.X.
* Localization capabilities are now non-functional and are expected to come back in a later version. See [this issue](https://github.com/NetSparkleUpdater/NetSparkle/issues/92). (Contributions are welcome!)
* Most `SparkleUpdater` elements are now configurable. For example, you can implement `IAppCastHandler` to implement your own app cast parsing and checking.
  * `IAppCastDataDownloader` to implement downloading of your app cast file
  * `IUpdateDownloader` to implement downloading of your actual binary/installer as well as getting file names from the server (if `CheckServerFileName` is `true`)
  * `IAppCastHandler` to implement your own app cast parsing
  * `ISignatureVerifier` to implement your own download/app cast signature checking. NetSparkleUpdater has built-in DSA and Ed25519 signature verifiers.
  * `IUIFactory` to implement your own UI
    * `IUIFactory` implementors must now have `ReleaseNotesHTMLTemplate` and `AdditionalReleaseNotesHeaderHTML` -- it's ok if these are `string.Empty`/`""`/`null`.
    * `IUIFactory` methods all now take a reference to the `SparkleUpdater` instance that called the method
  * `ILogger` to implement your own logger class (rather than being forced to subclass `LogWriter` like in previous versions)
  * `Configuration` subclasses now take an `IAssemblyAccessor` in their constructor(s) in order to define where assembly information is loaded from
  * Many `SparkleUpdater` functions are now virtual and thus more easily overridden for your specific use case
  * Several `ReleaseNotesGrabber` functions are now virtual as well.
* Many delegates, events, and functions have been renamed, removed, and/or tweaked for clarity and better use
  * `DownloadEvent` now has the `AppCastItem` that is being downloaded rather than being just the download path
  * `AboutToExitForInstallerRun`/`AboutToExitForInstallerRunAsync` has been renamed to `PreparingToExit`/`PreparingToExitAsync`, respectively
  * The `UserSkippedVersion` event has been removed. Use `UserRespondedToUpdate` instead.
  * The `RemindMeLaterSelected` event has been removed. Use `UserRespondedToUpdate` instead.
  * The `FinishedDownloading`/`DownloadedFileReady` events have been removed. Use `DownloadFinished` instead.
  * The `StartedDownloading` event has been removed and replaced with `DownloadStarted`
  * The `DownloadError` event has been removed and replaced with `DownloadHadError`
  * `Sparkle.RunUpdate` no longer exists. Use `SparkleUpdater.InstallUpdate` instead.
  * `Sparkle.DownloadPathForAppCastItem` -> `SparkleUpdater.GetDownloadPathForAppCastItem`
  * `AppCastItem.DownloadDSASignature` -> `AppCastItem.DownloadSignature`
  * `SilentModeTypes` enum renamed to `UserInteractionMode`
  * `Sparkle.SilentMode` renamed to `Sparkle.UserInteractionMode`
  * `UseSyncronizedForms` renamed to `ShowsUIOnMainThread`
* Samples have been updated and improved
  * Sample apps for [Avalonia](https://github.com/AvaloniaUI/Avalonia), WinForms, and WPF UIs
  * Sample app to demonstrate how to handle events yourself with your own, custom UI
* By default, the app cast signature file now has a `.signature` extension. The app cast downloader will look for a file with the old `.dsa` signature if data is not available or found in a `appcast.xml.signature` on your server. You can change the extension using the `signature-file-extension` option in the app cast generator and via the `XMLAppCast.SignatureFileExtension` property.
* `sparkle:dsaSignature` is now `sparkle:signature` instead. If no `sparkle:signature` is found, `sparkle:dsaSignature` will be used (if available). If `sparkle:dsaSignature` is not found, `sparkle:edSignature` will be used (if available). This is to give us as much compatibility with old versions of `NetSparkle` as well as the macOS `Sparkle` library.
* An entirely new app cast generator tool is now available for use.
* By default, the app cast generator tool now uses Ed25519 signatures. If you don't want to use files on disk to store your keys, set the `SPARKLE_PRIVATE_KEY` and `SPARKLE_PUBLIC_KEY` environment variables before running the app cast generator tool. You can also store these signatures in a custom location with the `--key-path` flag.
  * You can still use DSA signatures via the DSAHelper tool and the `DSAChecker` class. This is not recommended.
  * `Ed25519Checker` is the class responsible for handling Ed25519 signatures. `DSAChecker` sticks around for verifying DSA signatures if they're still used.
* Removed `AssemblyAccessor` class in lieu of `IAssemblyAccessor` implementors
* The server file name for each app cast download is now checked before doing any downloads or showing available updates to the client. To disable this behavior and use the name in the app cast, set `SparkleUpdater.CheckServerFileName` to `false`.
* `bool ignoreSkippedVersions = false` has been added to `CheckForUpdatesAtUserRequest`, `CheckForUpdatesQuietly`, and `GetUpdateStatus` to make ignoring skipped versions easier.
* The file name/path used by `RelaunchAfterUpdate` are controlled by `RestartExecutableName` and `RestartExecutablePath`, respectively. `SparkleUpdater` makes a best effort to figure these out for you; however, you can override them if you need to. NOTE: The way these parameters are fetched has CHANGED in recent previews (as of 2021-04-18) -- YOU HAVE BEEN WARNED!!
* **Breaking change**: `CheckForUpdatesQuietly` now shows no UI ever. It could show a UI before, which didn't make a lot of sense based on the function name. Make sure that if you use this function that you handle showing a UI yourself if necessary. (See the HandleEventsYourself sample if you want help.) You can always trigger the built-in `SparkleUpdater` by calling `_sparkle.ShowUpdateNeededUI(updateInfo.Updates)`. 
* **Breaking change**: DLL assembly names for .NET Framework WinForms UI dlls changed from `NetSparkle.UI.WinForms` to `NetSparkleUpdater.UI.WinForms`.
* **Breaking change**: DLL assembly names for Avalonia UI dlls changed from `NetSparkle.UI.Avalonia` to `NetSparkleUpdater.UI.Avalonia`.
* **We now rely on Portable.BouncyCastle** (BouncyCastle.Crypto.dll) for the ed25519 implementation. This means there is another DLL to reference when you use NetSparkle!
* **We now rely on System.Text.Json (netstandard2.0) OR Newtonsoft.Json (.NET Framework 4.5.2)** for the JSON items. This means there is another DLL to reference when you use NetSparkle, and it will change depending on if the `System.Text.Json` or `Newtonsoft.Json` item is used!
