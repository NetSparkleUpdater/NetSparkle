# NetSparkle

 [![Gitter](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/NetSparkleUpdater/NetSparkle?utm_campaign=pr-badge&utm_content=badge&utm_medium=badge&utm_source=badge) [![Issues](https://img.shields.io/github/issues/NetSparkleUpdater/NetSparkle.svg?style=flat-square)](https://github.com/NetSparkleUpdater/NetSparkle/issues)

NetSparkle is a software update framework for C# that is compatible with .NET Core 3+ and .NET Framework 4.5.2+, has pre-built UIs for .NET Framework (WinForms, WPF) and .NET Core (WinForms, WPF, Avalonia), uses Ed25519 or other signatures, and even allows for custom UIs or no UI at all! You provide, somewhere on the internet, an [app cast](#appcast) with update and version information, along with release notes in Markdown or HTML format. This library then helps you check for an update, show the user the release notes, and offer to download/install the new version of the software. 

The `develop` branch has changed significantly from `master` and represents a major 2.0 version update. NetSparkle 2.0, currently in beta, brings the ability to customize most of NetSparkle -- custom UIs are easy, you can have custom app cast downloaders and handlers (e.g. for FTP download or JSON appcasts), and more! No more big changes to the API are planned (but we cannot guarantee it before 2.0 RC), but smaller API changes may occur if bugs are found between now and the official 2.0 release.

Built-in supported update download types:
* Windows -- .exe, .msi, .msp
* macOS -- .zip, .pkg, .dmg
* Linux -- .tar.gz, .deb, .rpm

_README and other documentation updates for version 2.0 are in progress. If you have specific questions or need help even after looking at the samples, please file an issue or message me on [Gitter](https://gitter.im/NetSparkleUpdater/NetSparkle). You should be able to implement your own handlers and/or UI for most operations, so things like a custom update process, downloading/parsing JSON rather than XML, downloading things from FTP, using your own file signature verification method, etc. are all now possible. Some extra features aren't built-in out of the box yet, such as JSON app cast feeds -- contributions are welcome and benefit the whole community!_

## Contributing

Contributions are ALWAYS welcome! If you see a new feature you'd like to add, please open an issue to talk about it first, then open a PR for that implementation. If there's a bug you find, please open a PR with the fix or file an issue! Thank you!! :) You can also join us in our [Gitter chat room](https://gitter.im/NetSparkleUpdater/NetSparkle)!

## Installing NetSparkle

NetSparkle is available via NuGet. To choose a NuGet package to use:

* Reference the core NetSparkle build if you don't care about having a built-in UI and can manage things yourself
* Choose one of the other packages if you want a built-in UI or want to create your UI based on one of the other UIs

| Package | Use Case | Release | Preview | Downloads |
| ------- | -------- | ------- | ------- | --------- |
| NetSparkle | Core package; No UI or 100% custom UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkle.New.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkle.New/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkle.New.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkle.New/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkle.New.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkle.New/) |
| WinForms UI (.NET Framework) | NetSparkle with built-in WinForms UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) |
| WinForms UI (.NET Core) | NetSparkle with built-in WinForms UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) |
| WPF UI (.NET Framework and Core) | NetSparkle with built-in WPF UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WPF.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WPF.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WPF.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) |
| [Avalonia](https://github.com/AvaloniaUI/Avalonia) UI | NetSparkle with built-in Avalonia UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) |
| Command Line Tools | DSA helper; AppCast generator (incl. Ed25519 helpers) | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.Tools.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.Tools.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.Tools.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) |

All notable changes to this project will be documented in the [changelog](CHANGELOG.md).

- [How Updates Work](#how-updates-work)
- [Basic Usage](#basic-usage)
- [Appcast](#appcast)
- [Updating from 0.x or 1.x](#updating-from-0x-or-1x)
- SparkleUpdater class
    - [Public Methods](#public-methods)
    - [Public Properties](#public-properties)
    - [Public Events](#public-events)
- [License](#license)
- [Requirements](#requirements)
- [Other Options](#other-options)

## How updates work

A typical software update path for a stereotypical piece of software might look like this:

1. Compile application so it can be run on other computers (e.g. `dotnet publish`)
2. Programmer puts app in some sort of installer/zip/etc. for distribution (e.g. InnoSetup for Windows)
3. Programmer creates app cast file (see the [appcast](#appcast) section of this document for more info on how to create this)
4. Programmer uploads files for distribution (installer, app cast file, appcast-file.signature file) to their download site.
5. Client opens app and is automatically notified of an available update (or the software otherwise detects there is an update)
6. Client chooses to update (or update is downloaded if the software downloads it automatically)
7. Update is downloaded and sitting on the user's disk
8. User is asked to close the software so the update can run. User closes the software.
9. Downloaded file/installer is run (or the update is otherwise performed)

Right now, NetSparkle **does not** help you with 1., 2., or 4. "Why not?", you might ask:

* 1. We can't compile your application for you since we don't know (or care) how you are compiling or packaging your application! :)
* 2. A cross-platform installer package/system would be difficult and may not feel normal to end users, although a system that uses [Avalonia](https://github.com/AvaloniaUI/Avalonia) could maybe work I suppose (might take a lot of work though and make downloads large!). We do not provide support for getting your installer/distribution ready. To generate your installer/distribution, we recommend the following:
  * Windows: [InnoSetup](https://jrsoftware.org/isinfo.php) or [NSIS](https://nsis.sourceforge.io/Main_Page) or [WiX](https://wixtoolset.org/)
  * macOS: If you have a .app to distribute, use [dotnet-bundle](https://github.com/egramtel/dotnet-bundle) with [create-dmg](https://github.com/sindresorhus/create-dmg). If you want an installer, create a .pkg installer with [macos-installer-builder](https://github.com/KosalaHerath/macos-installer-builder) (tutorial [here](https://medium.com/swlh/the-easiest-way-to-build-macos-installer-for-your-application-34a11dd08744)), [Packages](http://s.sudre.free.fr/Software/Packages/about.html), or [your terminal](https://www.techrepublic.com/article/pro-tip-use-terminal-to-create-packages-for-software-deployment/). Otherwise, plop things in a zip file.
  * Linux: Use [dotnet-packaging](https://github.com/qmfrederik/dotnet-packaging/) to create an rpm, deb, or tar.gz file for your users.
* 4. We don't know where your files will live on the internet, so you need to be responsible for uploading these files and putting them online somewhere.

To create your app cast file, see the [appcast](#appcast) section of this document.

We are open to contributions that might make the overall install/update process easier for the user. Please file an issue first with your idea before starting work so we can talk about it.

## Basic Usage

**Please look at the sample projects in this repository for basic, runnable usage samples!!** There are samples on using each of the built-in UIs as well as a "do it yourself in your own UI" sample!

```csharp
_sparkle = new SparkleUpdater(
    "http://example.com/appcast.xml", // link to your app cast file
    new Ed25519Checker(SecurityMode.Strict, // security mode -- use .Unsafe to ignore all signature checking (NOT recommended or secure!!)
                       "base_64_public_key") // your base 64 public key -- generate this with the NetSparkleUpdater.Tools AppCastGenerator on any OS
) {
    UIFactory = new NetSparkleUpdater.UI.WPF.UIFactory(icon) // or null or choose some other UI factory or build your own!
};
_sparkle.StartLoop(true); // `true` to run an initial check online -- only call StartLoop once for a given SparkleUpdater instance!
```

On the first Application.Idle event, your App Cast XML file will be downloaded, read, and compared to the currently running version. If it has a software update inside, the user will be notified with a little toast notification (if supported by the UI and enabled) or with an update dialog containing your release notes. The user can then ignore the update, ask to be reminded later, or download/install it now.

If you want to check for an update in the background without the user seeing anything, use

```csharp
_sparkle.CheckForUpdatesQuietly();
```

If you want to have a menu item for the user to check for updates so the user can see the UI while NetSparkle looks for updates, use

```csharp
_sparkle.CheckForUpdatesAtUserRequest();
```

If you have files that need saving, subscribe to the PreparingToExit event:

```csharp
_sparkle.PreparingToExit += ((x, cancellable) =>
{
	// ask the user to save, whatever else is needed to close down gracefully
});
```

Note that if you do _not_ use a `UIFactory`, you **must** use the `CloseApplication` or `CloseApplicationAsync` events to close your application; otherwise, your downloaded update file will never be executed/read! The only exception to this is if you want to handle all aspects of installing the update package yourself.

The file that launches your downloaded update executable only waits for 90 seconds before giving up! Make sure that your software closes within 90 seconds of [CloseApplication](#closeapplication)/[CloseApplicationAsync](#closeapplicationasync) being called if you implement those events! If you need an event that can be canceled, such as when the user needs to be asked if it's OK to close (e.g. to save their work), use [AboutToExitForInstallerRun](#abouttoexitforinstallerrun)/[AboutToExitForInstallerRunAsync](#abouttoexitforinstallerrunasync).

## App Cast

NetSparkle uses [Sparkle](https://github.com/sparkle-project/Sparkle)-compatible app casts _for the most part_. NetSparkle uses `sparkle:signature` rather than `sparkle:dsaSignature` so that you can choose how to sign your files/app cast. NetSparkle is compatible with and uses Ed25519 signatures by default, but the framework can handle a different implementation of the `ISignatureVerifier` class to check different kinds of signatures without a major version bump/update.

_Note: if your app has DSA signatures, the app cast generator uses Ed25519 signatures by default starting with preview 2.0.0-20200607001. To transition to Ed25519 signatures, create an update where the software has your new Ed25519 public key and a NEW url for a NEW app cast that uses Ed25519 signatures. Upload this update with an app cast that has DSA signatures so your old DSA-enabled app can download the Ed25519-enabled update. Then, future updates and app casts should all use Ed25519._

Here is a sample app cast:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<rss xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle" version="2.0">
    <channel>
        <title>NetSparkle Test App</title>
        <link>https://netsparkleupdater.github.io/NetSparkle/files/sample-app/appcast.xml</link>
        <description>Most recent changes with links to updates.</description>
        <language>en</language>
        <item>
            <title>Version 2.0 (2 bugs fixed; 3 new features)</title>
            <sparkle:releaseNotesLink>
            https://netsparkleupdater.github.io/NetSparkle/files/sample-app/2.0-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Thu, 27 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url="https://netsparkleupdater.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe"
                       sparkle:version="2.0"
                       sparkle:os="windows"
                       length="12288"
                       type="application/octet-stream"
                       sparkle:signature="NSG/eKz9BaTJrRDvKSwYEaOumYpPMtMYRq+vjsNlHqRGku/Ual3EoQ==" />
        </item>
    </channel>
</rss>
```

NetSparkle reads the `<item>` tags to determine whether updates are available.

The important tags in each `<item>` are:

- `<description>`
    - A description of the update in HTML or Markdown.
    - Overrides the `<sparkle:releaseNotesLink>` tag.
- `<sparkle:releaseNotesLink>`
    - The URL to an HTML or Markdown document describing the update.
    - If the `<description>` tag is present, it will be used instead.
    - **Attributes**:
        - `sparkle:signature`, optional: the DSA signature of the document; NetSparkle does not check this DSA signature for you unless you set `ReleaseNotesGrabber.ChecksReleaseNotesSignature` to `true`, but you may manually verify changelog DSA signatures if you like or set `ReleaseNotesGrabber.ChecksReleaseNotesSignature = true` in your UI.
- `<pubDate>`
    - The date this update was published
- `<enclosure>`
    - This tag describes the update file that NetSparkle will download.
    - **Attributes**:
        - `url`: URL of the update file
        - `sparkle:version`: machine-readable version number of this update
        - `length`, optional: (not validated) size of the update file in bytes
        - `type`: ignored
        - `sparkle:signature`: DSA signature of the update file
        - `sparkle:criticalUpdate`, optional: if equal to `true` or `1`, the UI will indicate that this is a critical update
        - `sparkle:os`: Operating system for the app cast item. Defaults to Windows if not supplied. For Windows, use "win" or "windows"; for macOS, use "macos" or "osx"; for Linux, use "linux".

By default, you need 2 (DSA) signatures (`SecurityMode.Strict`):

1. One in the enclosure tag for the update file (`sparkle:signature="..."`)
2. Another on your web server to secure the actual app cast file. **This file must be located at [AppCastURL].signature**. In other words, if the app cast URL is http://example.com/awesome-software.xml, you need a valid (DSA) signature for that file at http://example.com/awesome-software.xml.signature. 

### Ed25519 Signatures

You can generate Ed25519 signatures using the `AppCastGenerator` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.AppCastGenerator)). If you need to generate an Ed25519 keypair, use the tool like this:

```bash
generate_appcast.exe --generate-keys
```

Then you can use the tool like this:

```bash
generate_appcast.exe -a directory/for/appcast/ -e ext -b directory/with/binaries/ -o windows
```

You can use the `AppCastGenerator` tool to verify your binaries:

```bash
generate_appcast.exe --verify path/to/binary.exe --signature base_64_signature
```

If you want to make a signature for a binary, you can do so like this:

```bash
generate_appcast.exe --generate-signature path/to/binary.exe
```

By default, your Ed25519 signatures are stored on disk in your local application data folder in a subdirectory called `netsparkle`. If you want to export your keys, you can do:

```bash
generate_appcast.exe --export
```

If you want to use keys dynamically, you can set the `SPARKLE_PRIVATE_KEY` and `SPARKLE_PUBLIC_KEY` environment variables before running `generate_appcast`. The tool prioritizes environment keys over keys sitting on disk!

If your keys are sitting on disk somewhere (`NetSparkle_Ed25519.priv` and `NetSparkle_Ed25519.pub` -- both in base 64 and both on disk in the same folder!), you can pass in the path to these keys like this:

```bash
generate_appcast.exe --key-path path/to/keys/
```


### DSA Signatures

DSA signatures are not recommended for 2.0. They are insecure!

You can generate these signatures using the `DSAHelper` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.DSAHelper)). If you need to generate a DSA public/private key, please use the same tool on Windows like this: 

```
NetSparkle.DSAHelper.exe /genkey_pair
```

This only works on Windows because .NET Core 3 does not have the proper implementation to generate DSA keys on macOS/Linux.

On any platform, you can use the DSAHelper to get a signature like this:

```
NetSparkle.DSAHelper.exe /sign_update {YourInstallerPackage.msi} {NetSparkle_PrivateKey_DSA.priv}
```

### How can I make the app cast?

* Use the `AppCastGenerator` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.AppCastGenerator)) to easily create your app cast file.
* Rig up a script that generates the app cast for you in python or some other language (`string.Format` or similar is a wonderful thing).
* Or you can just copy/paste the above example app cast into your own file and tweak the signatures/download info yourself, then generate the (DSA) signature for the app cast file manually! :)

## Updating from 0.X or 1.X

This section is still WIP, but major changes include:

* Minimum .NET requirement is now .NET Framework 4.5.2 instead of 4.5.1
* Change of base namespace from `NetSparkle` to `NetSparkleUpdater`
* `Sparkle` renamed to `SparkleUpdater` for clarity
* UIs are now in different namespaces. If you want to use a UI, you must pass in a `UIFactory` that implements `IUIFactory` and handles showing/handling all user interface elements
  * `SparkleUpdater` no longer holds its own `Icon`
  * `HideReleaseNotes`, `HideRemindMeLaterButton`, and `HideSkipButton` are all handled by the `UIFactory` objects
* Added built-in UIs for [Avalonia](https://github.com/AvaloniaUI/Avalonia) and WPF
* Localization capabilities are non-functional and are expected to come back in a later version. See [this issue](https://github.com/NetSparkleUpdater/NetSparkle/issues/92).
* Most `SparkleUpdater` elements are now configurable. For example, you can implement `IAppCastHandler` to implement your own app cast parsing and checking.
  * `IAppCastDataDownloader` to implement downloading of your app cast file
  * `IAppCastHandler` to implement your own app cast parsing
  * `ISignatureVerifier` to implement your own download/app cast signature checking. NetSparkle has built-in DSA and Ed25519 signature verifiers.
  * `IUIFactory` to implement your own UI
* Samples have been updated and improved
  * Sample apps for [Avalonia](https://github.com/AvaloniaUI/Avalonia), WinForms, and WPF UIs
  * Sample app to demonstrate how to handle events yourself with your own UI
* Many delegates, events, and functions have been renamed, removed, and/or tweaked for clarity and better use
  * `DownloadEvent` now has the `AppCastItem` that is being downloaded rather than being just the download path
  * `AboutToExitForInstallerRun`/`AboutToExitForInstallerRunAsync` has been renamed to `PreparingToExit`/``PreparingToExitAsync`, respectively
  * The `UserSkippedVersion` event has been removed. Use `UserRespondedToUpdate` instead.
  * The `RemindMeLaterSelected` event has been removed. Use `UserRespondedToUpdate` instead.
  * The `FinishedDownloading`/`DownloadedFileReady` events have been removed. Use `DownloadFinished` instead.
* By default, the app cast signature file now has a `.signature` extension. The app cast downloader will look for a file with the old `.dsa` signature if data is not available or found in a `appcast.xml.signature` on your server.
* `sparkle:dsaSignature` is now `sparkle:signature` instead. If no `sparkle:signature` is found, `sparkle:dsaSignature` will be used (if available).
* By default, the app cast generator tool now uses Ed25519 signatures. If you don't want to use files on disk to store your keys, set the `SPARKLE_PRIVATE_KEY` and `SPARKLE_PUBLIC_KEY` environment variables before running the app cast generator tool.

## Public Methods

### This section still needs to be updated for 2.0!

### SparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier)

Initializes a new instance of the Sparkle class with the given appcast URL and signature verifier.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| signatureVerifier | *NetSparkle.Interfaces.ISIgnatureVerifier*<br>the object that will validate your app cast signatures |


### SparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier, string referenceAssembly)

Initializes a new instance of the Sparkle class with the given appcast URL, signature verifier, and the name of the assembly to use when comparing update versions.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| signatureVerifier | *NetSparkle.Interfaces.ISIgnatureVerifier*<br>the object that will validate your app cast signatures |
| referenceAssembly | *System.String*<br>the name of the assembly to use for comparison when checking update versions |

### SparkleUpdater(string appcastUr, ISignatureVerifier signatureVerifier, string referenceAssembly, NetSparkle.Interfaces.IUIFactory factory)

Initializes a new instance of the Sparkle class with the given appcast URL, signature verifier, the name of the assembly to use when comparing update versions, and a UI factory to use in place of the default UI.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| signatureVerifier | *NetSparkle.Interfaces.ISIgnatureVerifier*<br>the object that will validate your app cast signatures |
| referenceAssembly | *System.String*<br>the name of the assembly to use for comparison when checking update versions |
| factory | *NetSparkle.Interfaces.IUIFactory*<br>a UI factory to use in place of the default UI |

### void CancelFileDownload()

Cancels an in-progress download and deletes the temporary file.

### Task<NetSparkle.SparkleUpdateInfo> CheckForUpdatesAtUserRequest()

Check for updates, using interaction appropriate for if the user just said "check for updates".

### Task<NetSparkle.SparkleUpdateInfo> CheckForUpdatesQuietly()

Check for updates, using interaction appropriate for where the user doesn't know you're doing it, so be polite.

### void CheckOnFirstApplicationIdle()

(WinForms only) Schedules an update check to happen on the first Application.Idle event.

### void Dispose()

Inherited from IDisposable. Stops all background activities.

### System.Uri GetAbsoluteUrl(string)

Creates a System.Uri from a URL string. If the URL is relative, converts it to an absolute URL based on the appcast URL.

| Name | Description |
| ---- | ----------- |
| url | *System.String*<br>relative or absolute URL |

### NetSparkle.Configuration GetApplicationConfig()

Reads the local Sparkle configuration for the given reference assembly.

### Task<NetSparkle.SparkleUpdateInfo> GetUpdateStatus(NetSparkle.Configuration config)

This method checks if an update is required. During this process the appcast will be downloaded and checked against the reference assembly. Ensure that the calling process has read access to the reference assembly. This method is also called from the background loops.

| Name | Description |
| ---- | ----------- |
| config | *NetSparkle.Configuration*<br>the NetSparkle configuration for the reference assembly |

**Returns**: NetSparkle.SparkleUpdateInfo with information on whether there is an update available or not.

### System.Net.WebResponse GetWebContentResponse(string url)

Used by NetSparkle.AppCast to fetch the appcast and DSA signature.

### System.IO.Stream GetWebContentStream(string url)

Used by NetSparkle.AppCast to fetch the appcast and DSA signature as a System.IO.Stream.

### void ShowUpdateNeededUI(bool isUpdateAlreadyDownloaded)

Shows the update UI with the latest downloaded update information.

| Name | Description |
| ---- | ----------- |
| isUpdateAlreadyDownloaded | *System.Boolean*<br>If true, make sure UI text shows that the user is about to install the file instead of download it. |

### void ShowUpdateNeededUI(NetSparkle.AppCastItem[], bool)

Shows the update needed UI with the given set of updates.

| Name | Description |
| ---- | ----------- |
| updates | *NetSparkle.AppCastItem[]*<br>updates to show UI for |
| isUpdateAlreadyDownloaded | *System.Boolean*<br>If true, make sure UI text shows that the user is about to install the file instead of download it. |

### void StartLoop(bool doInitialCheck)

Starts a NetSparkle background loop to check for updates every 24 hours.

You should only call this function when your app is initialized and shows its main window.

| Name | Description |
| ---- | ----------- |
| doInitialCheck | *System.Boolean*<br>whether the first check should happen before or after the first interval |

### void StartLoop(bool doInitialCheck, bool forceInitialCheck)

Starts a NetSparkle background loop to check for updates every 24 hours.

You should only call this function when your app is initialized and shows its main window.

| Name | Description |
| ---- | ----------- |
| doInitialCheck | *System.Boolean*<br>whether the first check should happen before or after the first interval |
| forceInitialCheck | *System.Boolean*<br>if doInitialCheck is true, whether the first check should happen even if the last check was less than 24 hours ago |

### void StartLoop(bool doInitialCheck, bool forceInitialCheck, System.TimeSpan checkFrequency)

Starts a NetSparkle background loop to check for updates on a given interval.

You should only call this function when your app is initialized and shows its main window.

| Name | Description |
| ---- | ----------- |
| doInitialCheck | *System.Boolean*<br>whether the first check should happen before or after the first period |
| forceInitialCheck | *System.Boolean*<br>if doInitialCheck is true, whether the first check should happen even if the last check was within the last checkFrequency interval |
| checkFrequency | *System.TimeSpan*<br>the interval to wait between update checks |

### void StartLoop(bool doInitialCheck, System.TimeSpan checkFrequency)

Starts a NetSparkle background loop to check for updates on a given interval.

You should only call this function when your app is initialized and shows its main window.

| Name | Description |
| ---- | ----------- |
| doInitialCheck | *System.Boolean*<br>whether the first check should happen before or after the first interval |
| checkFrequency | *System.TimeSpan*<br>the interval to wait between update checks |

### void StopLoop()

Stops the Sparkle background loop. Called automatically by [Dispose](#void-dispose).

## Public Properties

### This section still needs to be updated for 2.0!

- string [AppcastUrl](#string-appcasturl--get-set-) { get; set; }
- NetSparkle.CheckingForUpdatesWindow [CheckingForUpdatesWindow](#netsparklecheckingforupdateswindow-checkingforupdateswindow--get-set-) { get; set; }
- System.Action [ClearOldInstallers](#systemaction-clearoldinstallers--get-set-) { get; set; }
- NetSparkle.Configuration [Configuration](#netsparkleconfiguration-configuration--get-set-) { get; set; }
- string [CustomInstallerArguments](#string-custominstallerarguments--get-set-) { get; set; }
- NetSparkle.DSAChecker [DSAChecker](#netsparkledsachecker-dsachecker--get-set-) { get; set; }
- NetSparkle.LogWriter [LogWriter](#netsparklelogwriter-logwriter--get-set-) { get; set; }
- string [ExtraJsonData](#string-extrajsondata--get-set-) { get; set; }
- bool [HideReleaseNotes](#bool-hidereleasenotes--get-set-) { get; set; }
- bool [HideRemindMeLaterButton](#bool-hideremindmelaterbutton--get-set-) { get; set; }
- bool [HideSkipButton](#bool-hideskipbutton--get-set-) { get; set; }
- bool [IsUpdateLoopRunning](#bool-isupdatelooprunning--get-) { get; }
- NetSparkle.AppCastItem[] [LatestAppCastItems](#netsparkleappcastitem-latestappcastitems--get-) { get; }
- [PrintDiagnosticToConsole](#printdiagnostictoconsole--get-set-) { get; set; }
- NetSparkle.Interfaces.IDownloadProgress [ProgressWindow](#netsparkleinterfacesidownloadprogress-progresswindow--get-set-) { get; set; }
- bool [RelaunchAfterUpdate](#bool-relaunchafterupdate--get-set-) { get; set; }
- bool [ShowsUIOnMainThread](#bool-showsuionmainthread--get-set-) { get; set; }
- NetSparkle.Sparkle.UserInteractionMode [UserInteractionMode](#netsparklesparkleuserinteractionmode-silentmode--get-set-) { get; set; }
- string [TmpDownloadFilePath](#string-tmpdownloadfilepath--get-set-) { get; set; }
- bool [TrustEverySSLConnection](#bool-trusteverysslconnection--get-set-) { get; set; }
- NetSparkle.Interfaces.IUIFactory [UIFactory](#netsparkleinterfacesiuifactory-uifactory--get-set-) { get; set; }
- bool [UpdateMarkedCritical](#bool-updatemarkedcritical--get-) { get; }
- bool [UseNotificationToast](#bool-usenotificationtoast--get-set-) { get; set; }
- NetSparkle.Interfaces.IUpdateAvailable [UserWindow](#netsparkleinterfacesiupdateavailable-userwindow--get-set-) { get; set; }
- NetSparkle.SecurityProtocolType [SecurityProtocolType](#netsparklesecurityprotocoltype--get-set-) { get; set; }

### string AppcastUrl { get; set; }

Gets or sets the appcast URL

### NetSparkle.CheckingForUpdatesWindow CheckingForUpdatesWindow { get; set; }

The user interface window that shows the 'Checking for Updates...' form. TODO: Make this an interface so user can config their own UI

### System.Action ClearOldInstallers { get; set; }

Function that is called asynchronously to clean up old installers that have been downloaded with UserInteractionMode.DownloadNoInstall or UserInteractionMode.DownloadAndInstall.

### NetSparkle.Configuration Configuration { get; set; }

The NetSparkle configuration object for the current assembly.

### string CustomInstallerArguments { get; set; }

Run the downloaded installer with these arguments

### NetSparkle.DSAChecker DSAChecker { get; set; }

The DSA checker that verifies/validates downloaded files

### NetSparkle.LogWriter LogWriter { get; set; }

Logs diagnostic information to `Console.WriteLine` or `Debug.WriteLine` or wherever else the child class wants to report diagnostic information

### string ExtraJsonData { get; set; }

If not "", sends extra JSON via POST to server with the web request for update information and for the DSA signature.

### bool HideReleaseNotes { get; set; }

Hides the release notes view when an update is found.

### bool HideRemindMeLaterButton { get; set; }

Hides the remind me later button when an update is found.

### bool HideSkipButton { get; set; }

Hides the skip button view when an update is found.

### bool IsUpdateLoopRunning { get; }

Whether or not the update loop is running

### NetSparkle.AppCastItem[] LatestAppCastItems { get; }

Returns the latest appcast items to the caller. Might be null.

### PrintDiagnosticToConsole { get; set; }

If true, prints diagnostic messages to Console.WriteLine rather than Debug.WriteLine

### NetSparkle.Interfaces.IDownloadProgress ProgressWindow { get; set; }

The user interface window that shows a download progress bar, and then asks to install and relaunch the application

### bool RelaunchAfterUpdate { get; set; }

Defines if the application needs to be relaunched after executing the downloaded installer

### bool ShowsUIOnMainThread { get; set; }

WinForms only. If true, tries to run UI code on the main thread using System.Threading.SynchronizationContext.

### NetSparkle.Sparkle.UserInteractionMode SilentMode { get; set; }

Set the silent mode type for Sparkle to use when there is a valid update for the software

### string TmpDownloadFilePath { get; set; }

If set, downloads files to this path. If the folder doesn't already exist, creates the folder. Note that this variable is a path, not a full file name.

### bool TrustEverySSLConnection { get; set; }

If true, don't check the validity of SSL certificates

### NetSparkle.Interfaces.IUIFactory UIFactory { get; set; }

Factory for creating UI forms like progress window, etc.

### bool UpdateMarkedCritical { get; }

Loops through all of the most recently grabbed app cast items and checks if any of them are marked as critical

### bool UseNotificationToast { get; set; }

Specifies if you want to use the notification toast

### NetSparkle.Interfaces.IUpdateAvailable UserWindow { get; set; }

The user interface window that shows the release notes and asks the user to skip, later or update

### NetSparkle.SecurityProtocolType { get; set; }

The security protocol (`System.Net.SecurityProtocolType`) used by NetSparkle. Setting this property will also set this property for the current AppDomain of the caller. Needs to be set to `SecurityProtocolType.Tls12` for some cases, such as downloading something over HTTPS for a GitHub pages site.

## Public Events

### This section still needs to be updated for 2.0!

- [AboutToExitForInstallerRun](#abouttoexitforinstallerrun)
- [AboutToExitForInstallerRunAsync](#abouttoexitforinstallerrunasync)
- [CloseApplication](#closeapplication)
- [CloseApplicationAsync](#closeapplicationasync)
- [CheckLoopFinished](#checkloopfinished)
- [CheckLoopStarted](#checkloopstarted)
- [DownloadCanceled](#downloadcanceled)
- [DownloadedFileIsCorrupt](#downloadedfileiscorrupt)
- [DownloadedFileReady](#downloadedfileready)
- [DownloadError](#downloaderror)
- [FinishedDownloading](#finisheddownloading)
- [StartedDownloading](#starteddownloading)
- [UpdateCheckFinished](#updatecheckfinished)
- [UpdateCheckStarted](#updatecheckstarted)
- [UpdateDetected](#updatedetected)
- [UserSkippedVersion](#userskippedversion)
- [RemindMeLaterSelected](#remindmelaterselected)

### AboutToExitForInstallerRun

**Delegate**: void System.ComponentModel.CancelEventHandler(object sender, System.ComponentModel.CancelEventArgs e)

Subscribe to this to get a chance to shut down gracefully before quitting. If [AboutToExitForInstallerRunAsync](#abouttoexitforinstallerrunasync) is set, this has no effect.

### AboutToExitForInstallerRunAsync

**Delegate**: Task CancelEventHandlerAsync(object sender, System.ComponentModel.CancelEventArgs e)

Subscribe to this to get a chance to asynchronously shut down gracefully before quitting. This overrides [AboutToExitForInstallerRun](#abouttoexitforinstallerrun).

### CloseApplication

**Delegate**: void CloseApplication()

Event for custom shutdown logic. If this is set, it is called instead of Application.Current.Shutdown or Application.Exit. If [CloseApplicationAsync](#closeapplicationasync) is set, this has no effect.

**Warning**: The batch file that launches your executable only waits for 90 seconds before giving up! Make sure that your software closes within 90 seconds if you implement this event! If you need an event that can be canceled, use [AboutToExitForInstallerRun](#abouttoexitforinstallerrun).

### CloseApplicationAsync

**Delegate**: Task CloseApplicationAsync()

Event for custom shutdown logic. If this is set, it is called instead of Application.Current.Shutdown or Application.Exit. This overrides [CloseApplication](#closeapplication).

**Warning**: The batch file that launches your executable only waits for 90 seconds before giving up! Make sure that your software closes within 90 seconds if you implement this event! If you need an event that can be canceled, use [AboutToExitForInstallerRunAsync](#abouttoexitforinstallerrunasync).

### CheckLoopFinished

**Delegate**: void NetSparkle.LoopFinishedOperation(object sender, bool updateRequired)

This event will be raised when a check loop is finished

### CheckLoopStarted

**Delegate**: void NetSparkle.LoopStartedOperation(object sender)

This event will be raised when a check loop will be started

### DownloadCanceled

**Delegate**: void NetSparkle.DownloadEvent(string path)

Called when the download has been canceled

### DownloadedFileIsCorrupt

**Delegate**: void NetSparkle.DownloadedFileIsCorrupt(NetSparkle.AppCastItem item, string downloadPath)

Called when the downloaded file is downloaded (or at least partially on disk) and the DSA signature doesn't match. When this is called, Sparkle is not taking any further action to try to download the install file during this instance of the software. In order to make Sparkle try again, you must delete the file off disk yourself. Sparkle will try again after the software is restarted.

### DownloadedFileReady

**Delegate**: void NetSparkle.DownloadedFileReady(NetSparkle.AppCastItem item, string downloadPath)

Called when the downloaded file is fully downloaded and verified regardless of the value for SilentMode. Note that if you are installing fully silently, this will be called before the install file is executed, so don't manually initiate the file or anything.

### DownloadError

**Delegate**: void NetSparkle.DownloadEvent(string path)

Called when the download has downloaded but has an error other than corruption

### FinishedDownloading

**Delegate**: void NetSparkle.DownloadEvent(string path)

Called when the download has finished successfully

### StartedDownloading

**Delegate**: void NetSparkle.DownloadEvent(string path)

Called when the download has just started

### UpdateCheckFinished

**Delegate**: void NetSparkle.UpdateCheckFinished(object sender, NetSparkle.UpdateStatus status)

Called when update check is all done. May or may not have called [UpdateDetected](#updatedetected) in the middle.

### UpdateCheckStarted

**Delegate**: void NetSparkle.UpdateCheckStarted(object sender)

Called when update check has just started

### UpdateDetected

**Delegate**: void NetSparkle.UpdateDetected(object sender, NetSparkle.UpdateDetectedEventArgs e)

This event can be used to override the standard user interface process when an update is detected

### UserSkippedVersion

**Delegate**: void NetSparkle.UserSkippedVersion(NetSparkle.AppCastItem item, string downloadPath)

Called when the user skips some version of the application.

### RemindMeLaterSelected

**Delegate**: void NetSparkle.RemindMeLaterSelected(AppCastItem item);

Called when the user skips some version of the application by clicking the 'Remind Me Later' button

## License

NetSparkle is available under the [MIT License](LICENSE).

## Requirements

- .NET Framework 4.5.1+ OR .NET Core 3+

## Acknowledgements

* The original NetSparkle library, found at [dei79/netsparkle](https://github.com/dei79/netsparkle)
* A function for finding the base directory was taken from MIT-licensed [WalletWasabi](https://github.com/zkSNACKs/WalletWasabi/)
* MarkdownSharp is from [here](https://github.com/StackExchange/MarkdownSharp)

## Other Options

An incomplete list of other projects related to software updating:

- [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)
- [WinSparkle](https://github.com/vslavik/winsparkle)
- [NAppUpdate](https://github.com/synhershko/NAppUpdate)
- [AutoUpdater.NET](https://github.com/ravibpatel/AutoUpdater.NET)
