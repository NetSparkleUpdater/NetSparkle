# Changelog
All notable changes to this project will be documented in this file.

## [Unreleased]
### Added

### Changed
- Moved `UpdateStatus` enum to `NetSparkle.Enums`
- Moved `UpdateInfo` class to its own file

### Removed

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

[Unreleased]: https://github.com/Deadpikle/NetSparkle/compare/8a8b393...develop
[0.11.0]: https://github.com/Deadpikle/NetSparkle/compare/d2740a4...8a8b393
[0.10.0]: https://github.com/Deadpikle/NetSparkle/compare/c5e1e49...d2740a4
[0.9.1.1]: https://github.com/Deadpikle/NetSparkle/compare/e0f5004...c5e1e49
[0.9.1]: https://github.com/Deadpikle/NetSparkle/compare/7d679f0...e0f5004
[0.9]: https://github.com/Deadpikle/NetSparkle/compare/8034ec2...7d679f0
