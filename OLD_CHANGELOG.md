# Changelog
All notable changes to this project will be documented in this file.

NOTICE: Please view GitHub releases for new release information post-0.19.0. Thanks!

## [0.19.0]

### Added

- `CheckServerFileName` property lets you disable the checking of download file names with the server. Defaults to `true`.
- New function `IsDownloadingItem` lets you see if an item is currently being downloaded

### Changed

- Fixed bug with threading and `HttpClient` by using `ConfigureAwait(false)`. See also https://stackoverflow.com/a/10351400/3938401.

## [0.18.2] (and 0.18.3 because Deadpikle failed)

### Changed

- Fixed bug where process IDs were sometimes not matched properly (@kenjiuno ) -- #80 (PR)

## [0.18.1]

### Changed

- Set AutoEllipsis to True for App Captions in dialog windows (@Mostlypyjamas) -- #78 (Issue), #79 (PR)

## [0.18.0]

### Changed

- `RegistryConfiguration.BuildRegistryInfo()` is now `public virtual` rather than `private`

## [0.17.0]

### Added

- NetSparkle now handles the `ddd, dd MMM yyyy HH:mm:ss Z` and `ddd, dd MMM yyyy HH:mm:ss` date formats when parsing an app cast file (@Mostlypyjamas)

### Changed

- [Breaking Change] NetSparkle defaults to using the server's file name as the download file name rather than the app cast file name (@Mostlypyjamas)
- Many buttons use `AutoSize = true` to alleviate some concerns outlined in #44

### Removed

## [0.16.2]

### Fixed

- Fixed a bug where release notes were downloaded incorrectly
- Fixed a bug where the update form window wasn't closed properly

## [0.16.1]

### Added

- Appcast download links can now be relative (this change may be removed in a future X.0 verison of `NetSparkle` as it does not follow the RSS spec). This change was made to be consistent with how release notes are downloaded.

### Changed

- `NetSparkleUtilities` namespace renamed to just `NetSparkle`.

## [0.16.0]

### Added

- We now offer a `generate_appcast.exe` tool in the `NetSparkle.New.Tools` NuGet! This works very similarly to macOS Sparkle's `generate_appcast` tool and is due to the work by @ndreisg.
- Started to work on adding formal unit tests for the project. Lots to do here, still.

## [0.15.0]

### Changed

- `AppCastItem` is now marked serializable
- If using `SecurityMode.Unsafe`, files are **always** redownloaded because the library has no good way of knowing whether or not the file that is on disk is the file that is actually on the server (@rolikoff)
- Fixed a bug where if you cancel the download process or if an error occurs during download, the update file stays in the same directory (@rolikoff)
- Fixed a bug where the `DownloadCanceled` event was fired twice (@rolikoff)

## [0.14.0] (Same as 0.14.0.1 because Deadpikle goofed)

### Added

- NetSparkle now supports the `sparkle:os` attribute (#17). If this is not present, an update is assumed to be a Windows update. Valid types for Windows are "win" or "windows". The operating system string check is a case-insensitive check.
    - Added `OperatingSystemString` (default "windows") and `bool IsWindowsUpdate` to `AppCastItem`
    - `AppCast.GetUpdates()` no longer returns non-Windows updates
- To increase compatibility with the main macOS Sparkle project, the `enclosure` tag can now be either `enclosure` or `sparkle:enclosure`
- Added `AppCastItem.MIMEType` to read the `<enclosure type="">` attribute if they want to (#15). Defaults to `application/octet-stream`.
- Added `UpdateDetectedEventArgs.AppCastItems` if you want to look at all available app cast items
- Added `DownloadPathForAppCastItem(AppCastItem item)` to easily grab the download path for a given downloadable appcast item
- Added `RunUpdate(AppCastItem item)` to allow you to run an update without waiting for the latest version to download. The DSA signature of the file is still checked!

### Changed

- `NetSparkle.UpdateSystemProfileInformation` is now private
- `AppCast` no longer takes a Sparkle object and instead takes only those parameters that it needs to operate
- `NextUpdateAction` is now in its own file in the `NetSparkle.Enums` namespace

### Removed

## [0.13.0] - 2017-12-06

### Added

- **BREAKING CHANGE** Added `HideRemindMeLaterButton()` to `IUpdateAvailable`
- **BREAKING CHANGE** Added `HideSkipButton()` to `IUpdateAvailable`
- Added `HideRemindMeLaterButton` to the `NetSparkle` class. Defaults to false. Set to true to make `NetSparkle` call `HideRemindMeLaterButton()` when showing the update window.
- Added `HideSkipButton` to the `NetSparkle` class. Defaults to false. Set to true to make `NetSparkle` call `HideSkipButton()` when showing the update window.
- Added `RemindMeLaterSelected` to the `NetSparkle` class. Defaults to null. Use this event to be notified when the user has clicked the `Remind Me Later` button in the update window. (@enscope)

### Changed

- Release notes are now downloaded asynchronously, which should speed up the time it takes to show the download window
- Release note date is now Date.ToString("D") instead of "dd MMM yyyy" so that release notes show localized date strings
- **POTENTIALLY BREAKING CHANGE** Fixed bug where `ValidationResult.Unchecked` was not returned properly from `OnDownloadFinished` if download file signature is null (@keithclanton)
- **BREAKING CHANGE** `IUpdateAvailable` now has a `Result` of type `UpdateAvailableResult` rather than `DialogResult` in order to remove a dependency on WinForms. Use `DefaultUIFactory.ConvertDialogResultToUpdateAvailableResult` to convert from `DialogResult` to `UpdateAvailableResult` if needed. (@enscope) 

### Removed

## [0.12.0] - 2017-08-02
### Added

- Added new `LogWriter` class for printing diagnostic messages to the console. You can now create your own child class that inherits from `LogWriter` to customize how information is logged to the console (or file, or wherever else you want diagnostic messages sent!)!
- Added .gitattributes file for line ending consistency for all developers (@stephenwade)

### Changed
- Moved `UpdateStatus` enum to `NetSparkle.Enums`
- Moved `UpdateInfo` class to its own file
- Fixed bug in `Configuration.cs` where a few values were not set properly in the constructor due to `InitWithDefaultValues` being called at the wrong time (@devstudiosoft)
- **BREAKING CHANGE** Fixed bug in `AssemblyDiagnosticsAccessor` where `AssemblyProduct` returned the assembly version and not the assembly name (@devstudiosoft)

### Removed

- **BREAKING CHANGE** Removed `public void NetSparkle.ReportDiagnosticMessage` in lieu of new `LogWriter` class.

## [0.11.0] - 2017-07-16
### Added
- Refactored logic to quit application to a separate `QuitApplication()` function

### Changed
- `RunDownloadedInstaller()` is now virtual and protected
- Renamed some files and variables
- Moved `SecurityMode` and `ValidationResult` enums to the `NetSparkle.Enums` namespace

## [0.10.0] - 2017-07-11
### Added
- This changelog
- `Sparkle` class documentation to the readme
- Section about how the appcast works to the readme

### Changed
Much thanks to @stephenwade for his contributions to 0.10.0

- Cleaned up and added documentation comments throughout the code (@stephenwade)
- Renamed lots of identifiers throughout the project to remove "NetSparkle" (i.e., `NetSparkleAppCast` to `AppCast`, `NetSparkleConfiguration` to `Configuration`, etc.) (@stephenwade)
- Renamed property `UseSyncronizedForms` to `ShowsUIOnMainThread` to better represent what it does (@stephenwade)
- Renamed events `CloseWPFSoftware` and `CloseWPFSoftwareAsync` to `CloseApplication` and `CloseApplicationAsync` (@stephenwade)
    - These events are now always run, if present (instead of only on `RunningFromWPF`)
    - If one of these events is set, it will be run instead of quitting your app (to allow you a custom quit procedure), so these events should take care of quitting your app.
- Renamed `DSAVerificator` to `DSAChecker` (@stephenwade)
- Folder output changed to be more organized
- Updated LICENSE file
- Update NuGet package items

### Removed
- deprecated property `EnableSilentMode`
- property `RunningFromWPF`

## [0.9.1.1] - 2017-06-06
### Added
- `ClearOldInstallers` Action that you can implement on your own to remove old installers. Use this if you download installers to a custom folder and need to erase them later.

### Changed
- Fixed compilation issue with `EnableSilentMode` (not sure how I never came across this!)

## [0.9.1] - 2017-03-30
### Added
- `UpdateSize` to `NetSparkleAppCastItem`, analogous to the `length` field within the `<enclosure>` tag
- `IsCriticalUpdate` to `NetSparkleAppCastItem`
    - To use, add `sparkle:criticalUpdate="true"` as an attribute to the `<enclosure>` tag
    - When any update that the user needs is marked as critical, the skip and remind me later buttons are disabled
    - When an update is marked as critical, the release notes for that version state that the update is critical
    - To do something about a critical update in your own software, check `Sparkle.LatestAppCastItems` or `Sparkle.UpdateMarkedCritical` to see if an update in the list of updates that the user needs is critical

## [0.9] - 2017-03-28
### Added
- Several more diagnostic messages for debugging on the console
- New `SilentMode` option to allow for the "normal" update process (`NotSilent`), completely silent updates (`DownloadAndInstall`), or silent downloads that you as the developer initiate the start of the update manually (`DownloadNoInstall`)
    - `DownloadAndInstall` may be quite jarring to your users if you don't tell them the software is about to quit to restart. Use `AboutToExitForInstallerRun` or `AboutToExitForInstallerRunAsync` to monitor for these events.
    - For proper `DownloadNoInstall` use, monitor the `DownloadedFileReady` event to know when things are ready. At some later time, call `_sparkle.ShowUpdateNeededUI(true);` to show the software update window. You may want to monitor other events as well to keep your user from performing another update check while a software update is downloading.
- `TmpDownloadFilePath` to redirect the download location. This should be a folder, not a full path. Note that you still need to manually delete files that are downloaded here.

### Changed
- Deprecated EnableSilentMode in lieu of SilentMode
- Stopped the software from redownloading the installer if it already exists on disk (saves bandwidth and time on the user's part)
    - Note that NetSparkle does not perform resumable downloads in between software instances
- Fixed potential infinite software update download loop if the software keeps downloading corrupted files (corrupt files or ones that don't pass the DSA check).

## Older
For older changes, see [HISTORY.md](HISTORY.md).

[Unreleased]: https://github.com/Deadpikle/NetSparkle/compare/28ceb84...develop
[0.19.0]: https://github.com/Deadpikle/NetSparkle/compare/2701e54...28ceb84
[0.18.2]: https://github.com/Deadpikle/NetSparkle/compare/592fc70...2701e54
[0.18.1]: https://github.com/Deadpikle/NetSparkle/compare/af0c797...592fc70
[0.18.0]: https://github.com/Deadpikle/NetSparkle/compare/bc91f54...af0c797
[0.17.0]: https://github.com/Deadpikle/NetSparkle/compare/a3df35b...bc91f54
[0.16.2]: https://github.com/Deadpikle/NetSparkle/compare/b1bb3d1...a3df35b
[0.16.1]: https://github.com/Deadpikle/NetSparkle/compare/9298e3c...b1bb3d1
[0.16.0]: https://github.com/Deadpikle/NetSparkle/compare/6b30321...9298e3c
[0.15.0]: https://github.com/Deadpikle/NetSparkle/compare/6b30321...ee65d3e
[0.14.0]: https://github.com/Deadpikle/NetSparkle/compare/b442795...6b30321
[0.13.0]: https://github.com/Deadpikle/NetSparkle/compare/85a50da...b442795
[0.12.0]: https://github.com/Deadpikle/NetSparkle/compare/8a8b393...85a50da
[0.11.0]: https://github.com/Deadpikle/NetSparkle/compare/d2740a4...8a8b393
[0.10.0]: https://github.com/Deadpikle/NetSparkle/compare/c5e1e49...d2740a4
[0.9.1.1]: https://github.com/Deadpikle/NetSparkle/compare/e0f5004...c5e1e49
[0.9.1]: https://github.com/Deadpikle/NetSparkle/compare/7d679f0...e0f5004
[0.9]: https://github.com/Deadpikle/NetSparkle/compare/8034ec2...7d679f0

## Other History

June 2016 Steffen Xonna: Forked the library to raise the security and change some handling for an better usability for the user.  

 - Introduce an SecurityMode
   - SecurityMode.Unsafe = Is nearly the same like before. If there isn't an Signature or an DSA Key the files will be accepted. If both is present the signature has to be valid. *i don't recommend this mode.* It has critical security issues.
   - SecurityMode.UseIfPossible = If an local DSA key is available then an signature is neccessary. If both is present the signature has to be valid.
   - SecurityMode.Strict = The default-mode. This mode enforce the avaibility of an local DSA Key. If none is available the updatefiles will be rejected. *I strongly recommend this mode.*
 - Added support for DSA public key as parameter for sparkle.
 - Added support for appcast sidecar file which contains the DSA signature. Depends on the same rules (SecurityMode) like all other files.
 - Added support for DSA signatures of release note links. Depends on the same rules (SecurityMode) like all other files.
 - Added support for embedded release note.
 - Added support for relative downloads and relative external release notes.
 - Added the possibility to show the form synchronized with the main form. I hope for better handling for the user. So no window in background or multiple windows will be shown.
 - Added better handling of ssl connections (all downloads should use the same code and not 3 different)
 - Added support for custom registry path of configuration
 - Fix: don't accept ssl connections with policy errors.
 - only show versions they match security mode (if signature is needed than only accept versions with signatures or without downloads)
 - Fix: download don't stop if download windows will be closed

23 December 2014 Bluewalk: Since the development seems very quiet, I have forked this project and started adding fixes from other forks into this fork. Also I started to add improvements to the library as well including:

 - Cleaning up the project files
 - Set proxy to use default credentials if available (jimgraham)
 - Prettied up the release notes overview
 - Fixed not showing the installed version and app name in the Softare Update window
 - Added option to make the toast optional
 - Added option to use the NetSparkle icon if wanted
 - Stopped using .NET 4 Client profile
 - Added options RelaunchAfterUpdate (which defines if the app needs to be relaunced after update) and CustomInstallerArguments which allows you to add arguments to the update command line (as per https://github.com/hatton/NetSparkle/commit/2dbaa197633b2624b765ff86b3a619501200580d)
 - Added support for Windows installer patches (MSP)

8 March 2013 John Hatton: Looking for something with a different approach, but found a good start in NetSparkle. Forked with these goals:

 - Reduce (maybe remove) the current "loop" background orientation. Most apps just want to check on launch.
 - Remove dependency on NLog, in fact just removed the whole separate diagnostic window. Now just uses Debug.Writeline().
 - Make UI use application icon based on a single icon input, rather than requiring a separate Image input
 - Make UI less obtrusive, fitting with apps that are updated frequent (like, daily)
 - Enable Markdown for release notes, in addition to the existing html
 - Made the application shutdown event cancellable

3 Jan 2012 Jim Graham: forked NetSparkle to GitHub because I had some requirements for a client program that weren't easily met by the standard version.

 - This version allows for a custom UI instead of the two Windows Forms dialog
 - Allows for one-time check for updates instead of running on a loop
 - Allows for custom configuration objects instead of reading/writing to the registry
 - Refactored the diagnostic to use [NLog] instead of a custom filestream
 - Refactored existing the application to fire an event for shutdown instead of `Environment.Exit`

October 2010 dei79 created SVN repository on [CodePlex]:

NetSparkle is an easy-to-use software update framework for .NET developers on Windows, MAC or Linux. It was inspired by the Sparkle (http://bit.ly/HWyJd) project for Cocoa developers and the WinSparkle (http://bit.ly/cj5kP5) project (a Win32 port).
