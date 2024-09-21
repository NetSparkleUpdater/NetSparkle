<div align="center">
  <img alt="NetSparkleUpdater logo" height="200" src="src/NetSparkle/ArtWork/software-update-available.png">
  <h1>NetSparkleUpdater</h1>
  <p>An easily customizable software update framework for C# .NET projects with built-in UIs for WinForms, WPF, and Avalonia</p>
  <a href="https://gitter.im/NetSparkleUpdater/NetSparkle?utm_campaign=pr-badge&utm_content=badge&utm_medium=badge&utm_source=badge">
    <img alt="Gitter" src="https://badges.gitter.im/Join%20Chat.svg">
  </a>
  <a href="https://github.com/NetSparkleUpdater/NetSparkle/issues">
    <img alt="Gitter" src="https://img.shields.io/github/issues/NetSparkleUpdater/NetSparkle.svg?style=flat-square">
  </a>
</div>

# 

NetSparkle is a highly-configurable software update framework for C# that is compatible with .NET 6+ and .NET Framework 4.6.2+, has pre-built UIs for .NET Framework (WinForms, WPF) and .NET 6+ (WinForms, WPF, Avalonia), uses Ed25519 or other cryptographic signatures, and even allows for custom UIs or no built-in UI at all! You provide, somewhere on the internet, an [app cast](#app-cast) with update and version information, along with release notes in Markdown or HTML format. This library then helps you check for an update, show the user the release notes, and offer to download/install the new version of the software. 

NetSparkle 2.0 brings the ability to customize most of NetSparkle -- custom UIs are now possible, you can have custom app cast downloaders and handlers (e.g. for FTP download or JSON app casts), and many more enhancements are available!

_Note: NetSparkle 3.0 development is in progress and includes: built-in JSON app cast reading/writing instead of just XML, built-in ability to use different channels for your apps (e.g. beta, alpha, preview), semver compatibility, a reworked app cast serializing/deserializing API, a new assembly accessor, trimming/AOT compatibility, smaller file size due to fewer dependencies, and more! See the Issues list for more information about what's left. Updates to this README are pending, so ask questions on Gitter or on GitHub discussions if you're using a newer preview/building from source._

Built-in supported update download types:
* Windows -- .exe, .msi, .msp
* macOS -- .tar, .tar.gz, .zip, .pkg, .dmg
* Linux -- .tar.gz, .deb, .rpm

## Getting Started

- [Installing NetSparkle](#installing-netsparkle)
- [How Updates Work](#how-updates-work)
- [Basic Usage](#basic-usage)
- [App Cast](#app-cast)
- [Updating from 0.x or 1.x](#updating-from-0x-or-1x)
- [FAQ](#faq)
- [Requirements](#requirements)
- [License](#license)
- [Contributing](#contributing)
- [Acknowledgements](#acknowledgements)
- [Other Options](#other-options)

## Installing NetSparkle

NetSparkle is available via NuGet. To choose a NuGet package to use:

* Reference the core `NetSparkleUpdater.SparkleUpdater package` if you don't care about having a built-in UI and can manage things yourself
* Choose one of the other packages if you want a built-in UI or want to create your UI based on one of the other UIs

| Package | Use Case | Release | Preview | Downloads |
| ------- | -------- | ------- | ------- | --------- |
| NetSparkleUpdater.SparkleUpdater | Core package; Use a 100% custom UI or no UI (nothing built-in) | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.SparkleUpdater.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.SparkleUpdater/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.SparkleUpdater.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.SparkleUpdater/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.SparkleUpdater.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.SparkleUpdater/) |
| WinForms UI (.NET Framework) | NetSparkle with built-in WinForms UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) |
| WinForms UI (.NET 6+) | NetSparkle with built-in WinForms UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) |
| WPF UI (.NET Framework and .NET 6+) | NetSparkle with built-in WPF UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WPF.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WPF.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WPF.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) |
| [Avalonia](https://github.com/AvaloniaUI/Avalonia) UI | NetSparkle with built-in Avalonia UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) |
| App Cast Generator Tool | `netsparkle-generate-appcast` CLI tool (incl. Ed25519 helpers) | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.Tools.AppCastGenerator.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.Tools.AppCastGenerator.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.Tools.AppCastGenerator.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) |
| DSA Helper Tool | `netsparkle-dsa` CLI tool (DSA helpers) | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.Tools.DSAHelper.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.DSAHelper/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.Tools.DSAHelper.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.DSAHelper/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.Tools.DSAHelper.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.DSAHelper/) |

Quick info for tool installations:
* App cast generator -- `dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator`; available as `netsparkle-generate-appcast` on your command line after installation
* DSA Helper -- `dotnet tool install --global NetSparkleUpdater.Tools.DSAHelper`; available as `netsparkle-dsa` on your command line after installation

## How updates work

A typical software update path for a stereotypical piece of software might look like this:

1. Compile application so it can be run on other computers (e.g. `dotnet publish`)
2. Programmer puts app in some sort of installer/zip/etc. for distribution (e.g. InnoSetup for Windows)
3. Programmer creates app cast file (see the [app cast](#app-cast) section of this document for more info on how to create this)
4. Programmer uploads files for distribution (installer, app cast file, appCast-file.signature file) to their download site.
5. Client opens app and is automatically notified of an available update (or the software otherwise detects there is an update)
6. Client chooses to update (or update is downloaded if the software downloads it automatically)
7. Update is downloaded and sitting on the user's disk
8. User is asked to close the software so the update can run. User closes the software.
9. Downloaded file/installer is run (or the update is otherwise performed)

Right now, NetSparkleUpdater **does not** help you with 1., 2., or 4. "Why not?", you might ask:

* 1. We can't compile your application for you since we don't know (or care) how you are compiling or packaging your application! :)
* 2. A cross-platform installer package/system would be difficult and may not feel normal to end users, although a system that uses [Avalonia](https://github.com/AvaloniaUI/Avalonia) could maybe work I suppose (might take a lot of work though and make downloads large!). We do not provide support for getting your installer/distribution ready. To generate your installer/distribution, we recommend the following:
  * Windows: [InnoSetup](https://jrsoftware.org/isinfo.php) or [NSIS](https://nsis.sourceforge.io/Main_Page) or [WiX](https://wixtoolset.org/)
  * macOS: If you have a .app to distribute, use [dotnet-bundle](https://github.com/egramtel/dotnet-bundle) with [create-dmg](https://github.com/sindresorhus/create-dmg). If you want an installer, create a .pkg installer with [macos-installer-builder](https://github.com/KosalaHerath/macos-installer-builder) (tutorial [here](https://medium.com/swlh/the-easiest-way-to-build-macos-installer-for-your-application-34a11dd08744)), [Packages](http://s.sudre.free.fr/Software/Packages/about.html), or [your terminal](https://www.techrepublic.com/article/pro-tip-use-terminal-to-create-packages-for-software-deployment/). Otherwise, plop things in a zip file. If you need to run with `sudo` for whatever reason, there is an example of doing that in the macOS `Avalonia` sample.
  * Linux: Use [dotnet-packaging](https://github.com/qmfrederik/dotnet-packaging/) to create an rpm, deb, or tar.gz file for your users.
* 4. We don't know where your files will live on the internet, so you need to be responsible for uploading these files and putting them online somewhere.

To create your app cast file, see the [app cast](#app-cast) section of this document.

We are open to contributions that might make the overall install/update process easier for the user. Please file an issue first with your idea before starting work so we can talk about it.

## Basic Usage

**Please look at the sample projects in this repository for basic, runnable usage samples!!** There are samples on using each of the built-in UIs as well as a "do it yourself in your own UI" sample!

### Project file

In your project file, make sure you set up a few things so that the library can read in the pertinent details later. _Note: You can use your own `IAssemblyAccessor` to load version information from somewhere else. However, setting things up in your project file is easy, and NetSparkleUpdater can read that in natively!_

```xml
<PropertyGroup>
    <Version>1.0.2-beta1</Version> <!-- accepts semver -->
    <AssemblyVersion>1.0.2</AssemblyVersion> <!-- only accepts Major.Minor.Patch.Revision -->
    <AssemblyTitle>My Best App</AssemblyTitle>
    <Description>My app is cool (not required)</Description>
    <Company>My Company Name (required unless you set the IAssemblyAccessor save path yourself)</Company>
    <Product>My Product (required unless you set the IAssemblyAccessor save path yourself; set to product name e.g. MyBestApp)</Product>
    <Copyright>2024 MyCompanyName</Copyright>
</PropertyGroup>
```

IMPORTANT NOTE: In .NET 8+, a change was made that causes your git/source code commit hash to be included in your app's `<Version>` number. This behavior cannot be avoided by NetSparkleUpdater at this time as we rely on `AssemblyInformationalVersionAttribute`, and this attribute's behavior was changed. Your users may be told that they are currently running `1.0.0+commitHashHere` by NetSparkleUpdater (and your native app itself!). We also recommend adding the following lines to your project file (in a new `<PropertyGroup>` or an existing one):

```xml
<IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>
```

### Code

```csharp
// NOTE: Under most, if not all, circumstances, SparkleUpdater should be initialized on your app's main UI thread.
// This way, if you're using a built-in UI with no custom adjustments, all calls to UI objects will automatically go to the UI thread for you.
// Basically, SparkleUpdater's background loop will make calls to the thread that the SparkleUpdater was created on via SyncronizationContext.
// So, if you start SparkleUpdater on the UI thread, the background loop events will auto-call to the UI thread for you.
_sparkle = new SparkleUpdater(
    "http://example.com/appcast.xml", // link to your app cast file
    new Ed25519Checker(SecurityMode.Strict, // security mode -- use .Unsafe to ignore all signature checking (NOT recommended!!)
                       "base_64_public_key") // your base 64 public key -- generate this with the NetSparkleUpdater.Tools.AppCastGenerator .NET CLI tool on any OS
) {
    UIFactory = new NetSparkleUpdater.UI.WPF.UIFactory(icon), // or null, or choose some other UI factory, or build your own IUIFactory implementation!
    RelaunchAfterUpdate = false, // default is false; set to true if you want your app to restart after updating (keep as false if your installer will start your app for you)
    CustomInstallerArguments = "", // set if you want your installer to get some command-line args
};
_sparkle.StartLoop(true); // `true` to run an initial check online -- only call StartLoop **once** for a given SparkleUpdater instance!
```

On the first Application.Idle event, your App Cast XML file will be downloaded, read, and compared to the currently running version. If it has a software update inside, the user will be notified with a little toast notification (if supported by the UI and enabled) or with an update dialog containing your release notes. The user can then ignore the update, ask to be reminded later, or download/install it now.

If you want to check for an update in the background without the user seeing anything, use

```csharp
var updateInfo = _sparkle.CheckForUpdatesQuietly();
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

The file that launches your downloaded update executable only waits for 90 seconds before giving up! Make sure that your software closes within 90 seconds of [CloseApplication](#closeapplication)/[CloseApplicationAsync](#closeapplicationasync) being called if you implement those events! If you need an event that can be canceled, such as when the user needs to be asked if it's OK to close (e.g. to save their work), use `PreparingForExit` or `PreparingToExitAsync`.

#### What interfaces and classes can I utilitize to configure functionality for my own software's needs?

##### Interfaces 

* If you want to use your own UI, implement `IUIFactory`; set `SparkleUpdater.UIFactory` to utilize an instance of your object.
  * Implement `ICheckingForUpdates` for your UI that tells the user that `SparkleUpdater` is checking for updates
  * Impelement `IDownloadProgress` for your UI that shows the user that an update is being downloaded
  * Implement `IUpdateAvailable` for your UI that shows the user that an update is available along with release notes
* Implement `IAppCastDataDownloader` to setup your own methods for downloading app cast data; set `SparkleUpdater.AppCastDataDownloader` to utilize an instance of your object.. NetSparkle includes two implementations by default: `WebRequestAppCastDataDownloader` for downloading app cast information from the internet at large, and `LocalFileAppCastDownloader` for copying/"downloading" an app cast from a given path
* Implement `IAppCastFilter` to do custom filtering on the `AppCastItem` objects in your downloaded app cast, e.g. to only consider a given subset of items as valid updates for your application; set `AppCastHelper.AppCastFilter` (`SparkleUpdater.AppCastHelper.AppCastFilter`) to utilize an instance of your object. NetSparkle includes the `ChannelAppCastFilter` class, which you can use to filter out items by a given product channel (e.g. alpha, beta) if your application utilizes those features.
* Implement `IAppCastGenerator` to control how app casts are serialized and deserialized; set `SparkleUpdater.AppCastGenerator` to utilize an instance of your object. NetSparkle includes two implementations: `XMLAppCastGenerator`, for XML serialization/deserialization; and `JsonAppCastGenerator`, for JSON serialization/deserialization. The app cast generator CLI tool can also output both XML and JSON app casts.
* Implement `IAssemblyAccessor` to control how version, copyright, and other product details are loaded for your application; set `Configuration.AssemblyAccessor` (`SparkleUpdater.Configuration.AssemblyAccessor`) to utilize an instance of your object. NetSparkle contains a default implementation, `AssemblyDiagnosticsAccessor`, which should work in the general case of loading data from a given assembly.
* To log information to a file or to your console, implement `ILogger` and set `SparkleUpdater.LogWriter`. By default, the `LogWriter` class is used (which has the `LogWriterOutputMode` property to control whether the logs are written to `Console`, `Trace`, etc.)
* Implement `ISignatureVerifier` to change how your signatures for the app cast, downloads, etc. are handled; set `SparkleUpdater.SignatureVerifier` to utilize an instance of your object.
* Implement `IUpdateDownloader` to setup your own methods for downloading and sending progress on app update files (e.g. installers) for a given app cast item; set `SparkleUpdater.UpdateDownloader` to utilize an instance of your object.. NetSparkle includes two implementations by default: `WebFileDownloader` (default) to download files from the web/internet, and `LocalFileDownloader` for copying/"downloading" a file from a given path.

##### Subclassing

* Subclass `Configuration` to change how certain NetSparkle information is saved and loaded - e.g., skipped version information. This class is the one that utilizes an `IAssemblyAccessor` instance to save and load version information, product name, etc. NetSparkle contains three implementations: `RegistryConfiguration`, which saves and loads info to the Windows registry (default on Windows); `JSONConfiguration`, which saves and loads info to a JSON file (default on macOS/Linux); and `DefaultConfiguration`, which does nothing and serves as a fallback in case `JSONConfiguration` cannot find a valid file location to save and load data. To use the instance of your class, set `SparkleUpdater.Configuration`.
  * Subclassing `RegistryConfiguration` lets you quickly change the registry path where items are saved via `BuildRegistryPath`
  * Subclassing `JSONConfiguration` lets you quickly change the file path where data is saved via `GetSavePath`
* Subclass `AppCastHelper` if you want full control over the app cast downloading and parsing process. Note that you can probably do everything you need to do via the `AppCastHelper` properties (including `IAppCastFilter AppCastFilter`), but subclassing will give you full, absolute control over the whole process. To use the instance of your class, set `SparkleUpdater.AppCastHelper`.
* Subclass `ReleaseNotesGrabber` to control the release notes downloading (and therefore display) process. To use an instance of your class, set `UIFactory.ReleaseNotesGrabberOverride`.
* Override `WebFileDownloader` if you don't want to implement `IUpdateDownloader` yourself and just want to override a function or two such as `CreateHttpClient`. To use an instance of your class, set `SparkleUpdater.UpdateDownloader`.
* Override `WebRequestAppCastDataDownloader` if you don't want to implement `IAppCastDataDownloader` and just want to override a function or two such as `CreateHttpClient`. To use an instance of your class, set `SparkleUpdater.AppCastDataDownloader`.
* Override `LogWriter` to implement the `PrintMessage` function; because `ILogger` is a pretty simple interface, you can probably just implement that interface yourself if your needs are complex. To use an instance of your class, set `SparkleUpdater.LogWriter`.
* Override `SparkleUpdater` to implement some different installation-related functions, including:
  * `GetWindowsInstallerCommand`
  * `GetInstallerCommand`
  * `RunDownloadedInstaller`
* Override `UIFactory` if you don't want to implement the entirety of the `IUIFactory` interface yourself and just want to configure a function or two. To use an instance of your class, set `SparkleUpdater.UIFactory`.

#### Using `IAppCastFilter`

You can change how your app cast items are filtered through the `AppCastHelper.AppCastFilter` property (via the `IAppCastFilter` interface). This allows you to change what items are made available to your end users.

NetSparkle contains a built-in `IAppCastFilter` implementation for channel-based filtering called `ChannelAppCastFilter`. For some examples on how to use that class, see [the unit tests here](https://github.com/NetSparkleUpdater/NetSparkle/blob/develop/src/NetSparkle.Tests/ChannelAppCastFilterTests.cs). Basically, set the `List<string> ChannelSearchNames` property to the channels you want to filter by. If you want to keep items with no channel info (e.g. `1.2.3`) in it, set `KeepItemsWithNoChannelInfo` to `true`.

To actually set channels on your app cast items / in your app cast, use the `--channel` property of the app cast CLI tool, or set the `<Version>` property of your project file to the applicable semver-compatible version (e.g. `<Version>1.0.2-beta1</Version>`), and the app cast CLI tool will pick up on this automatically. Or, if you're building your app cast manually, set the `<sparkle:channel>YourChannelHere</sparkle:channel>` property on your `<item>` (or, if using JSON, the `channel` property).

#### Using/building your own UI

NetSparkleUpdater does not have to be used with a UI at all. You can do everything yourself or even have the library run your downloaded update automatically by setting the `SparkleUpdater.UserInteractionMode = UserInteractionMode.DownloadAndInstall`. This repo has a sample on doing things yourself without any pre-built UI in [src/NetSparkle.Samples.HandleEventsYourself](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Samples.HandleEventsYourself).

If you want a UI, we offer pre-built UIs in different NuGet packages with a small number of customizable options for WinForms, WPF, and Avalonia. The UIs are triggered via a `IUIFactory` implementation, called `UIFactory` in each of the built-in options. Most methods in the `UIFactory` can be overridden if you want to tweak behavior, and the `ProcessWindowAfterInit` lets you customize each window after it is made.

If you want to roll your own UI entirely, just implement the `IUIFactory` interface with whatever UI library you want to use. You can copy or reuse view models, code, etc. from NetSparkleUpdater's prebuilt options, and copy+pasting code from this repo into your own is probably a good, quick way to start. Don't forget to set the `SparkleUpdater.UIFactory` property with an instance of your `IUIFactory` implementation, though!

Please note: NetSparkle basically makes no attempts to worry about threading (e.g. calling to the main thread) except for the background loop calling to the main thread that started the `SparkleUpdater` instance. In other words, generally speaking, NetSparkle will do everything on the thread that originally created the `SparkleUpdater` instance. For most apps, this will be fine as they are just using their main UI thread. When in doubt, for your own UI needs, make sure to check `InvokeRequired` on WinForms, and on WPF/Avalonia, marshal things to the UI thread (unless you're using data binding in which case it's handled for you!).

Passing your own `IUIFactory` implementation that starts windows/things on new threads into `SparkleUpdater` is not a supported configuration. If you want to run your own UI on multiple threads (e.g. for WinForms to not have NetSparkleUpdater's windows close when the main form closes), do so using `SparkleUpdater`'s events and not the `UIFactory`; please also see the [src/NetSparkle.Samples.Forms.Multithread](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Samples.Forms.Multithread) sample for a practical example of how to do this.

## App cast

The app cast is just an XML or JSON file.  It contains fields such as the title and description of your product as well as a definition per release of your software.

We strongly recommend that you make use of the [netsparkle-generate-appcast](#install-appcast-generator-tool) tool to create (and later, re-create/update) the file because it can help take care of all signing requirements for you.

### Install AppCast Generator Tool

1. This tool requires the .NET 6, 7, or 8 Desktop Runtime to be installed.
2. `dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator`
3. The tool is now available on your command line as the `netsparkle-generate-appcast` command. You can use `netsparkle-generate-appcast --help` to see a full list of options for this tool.

### Sparkle Compatibility 

By default, NetSparkle uses [Sparkle](https://github.com/sparkle-project/Sparkle)-compatible XML app casts _for the most part_. NetSparkle uses `sparkle:signature` rather than `sparkle:edSignature` so that you can choose how to sign your files/app cast. (If you want to use `sparkle:edSignature`, pass `--use-ed25519-signature-attribute` to the app cast generator.) Note that NetSparkle is compatible with and uses Ed25519 signatures by default, but the framework can handle a different implementation of the `ISignatureVerifier` class to check different kinds of signatures without a major version bump/update.

### Sample App Cast

Here is a sample XML app cast:

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

### App Cast Items

NetSparkle reads the `<item>` tags to determine whether updates are available.

The important tags in each `<item>` are:

- `<description>`
    - A description of the update in HTML or Markdown.
    - Overrides the `<sparkle:releaseNotesLink>` tag.
- `<sparkle:releaseNotesLink>`
    - The URL to an HTML or Markdown document describing the update.
    - If the `<description>` tag is present, it will be used instead.
    - **Attributes**:
        - `sparkle:signature`, optional: the DSA/Ed25519 signature of the document; NetSparkle does not check this signature for you unless you set `ReleaseNotesGrabber.ChecksReleaseNotesSignature` to `true`, but you may manually verify changelog signatures if you like or set `ReleaseNotesGrabber.ChecksReleaseNotesSignature = true` in your UI.
- `<pubDate>`
    - The date this update was published
- `<enclosure>`
    - This tag describes the update file that NetSparkle will download.
    - **Attributes**:
        - `url`: URL of the update file
        - `sparkle:version`: machine-readable version number of this update
        - `length`, optional: (not validated) size of the update file in bytes
        - `type`: ignored
        - `sparkle:signature`: DSA/Ed25519 signature of the update file
        - `sparkle:criticalUpdate`, optional: if equal to `true` or `1`, the UI will indicate that this is a critical update
        - `sparkle:os`: Operating system for the app cast item. Defaults to Windows if not supplied. For Windows, use "win" or "windows"; for macOS, use "macos" or "osx"; for Linux, use "linux".

By default, you need 2 signatures (`SecurityMode.Strict`):

1. One in the enclosure tag for the update file (`sparkle:signature="..."`)
2. Another on your web server to secure the actual app cast file. **This file must be located at [AppCastURL].signature**. In other words, if the app cast URL is http://example.com/awesome-software.xml, you need a valid (DSA/Ed25519) signature for that file at http://example.com/awesome-software.xml.signature. 

_Note:_ the app cast generator tool creates both of these signatures for you when it recreates the appcast.xml file.

### Ed25519 Signatures

You can generate Ed25519 signatures using the `AppCastGenerator` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.AppCastGenerator)). **This tool requires the .NET 6, 7, or 8 Desktop Runtime to be installed.** Please see below sections for options and examples on generating the Ed25519 keys and for using them when creating an app cast.

### How can I make the app cast?

* Use the `AppCastGenerator` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.AppCastGenerator)) to easily create your app cast file. Available options are described below. You can install it on your CLI via `dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator`.
* Rig up a script that generates the app cast for you in python or some other language (`string.Format` or similar is a wonderful thing).
* Or you can just copy/paste the above example app cast into your own file and tweak the signatures/download info yourself, then generate the (Ed25519/DSA) signature for the app cast file manually! :)

### Using JSON app casts

If you'd like to use a JSON app cast rather than XML:

* Use `--output-type json` when generating your app cast file via the app cast generator
* Set `SparkleUpdater.AppCastGenerator` to `new JsonAppCastGenerator(mySparkleUpdater.LogWriter)`.
* By default, the output will be human-readable. If you want to turn this off, set the `JsonAppCastGenerator.HumanReadableOutput` property to `false`.

### App Cast Generator Options

_Missing some option you'd like to see? File an issue on this repo or add it yourself and send us a pull request!_

* `--show-examples`: Print examples of usage to the console.
* `--help`: Show all options and their descriptions.

#### General Options When Generating App Cast

* `-a`/`--appcast-output-directory`: Directory in which to write the output `appcast.xml` file. Example use: `-a ./MyAppCastOutput`
* `-e`/`--ext`: When looking for files to add to the app cast, use the given extension(s) when looking for files. Defaults to `exe`. Example use: `-e exe,msi`
* `-b`/`--binaries`: File path to directory that should be searched through when looking for files to add to the app cast. Defaults to `.`. Example use: `-b my/build/directory`
* `-r`/`--search-binary-subdirectories`: True to search the binary directory recursively for binaries; false to only search the top directory. Defaults to `false`. Example use: `-r`.
* `-f`/`--file-extract-version`: Whether or not to extract the version of the file from the file's name rather than the file (e.g. dll) itself. Defaults to `false`. Use when your files that will be downloaded by NetSparkleUpdater will have the version number in the file name, e.g. "My App 1.3.2-alpha1.exe". Note that this only searches the last four directory items/folders. Example use: `-f true`
* `--file-version`: Use to set the version for a binary going into an app cast. Note that this version can only be set once, so when generating an app cast, make sure you either: A) have only one binary in your app cast | B) Utilize the `--reparse-existing` parameter so that old items get picked up. If the generator finds 2 binaries without any known version and `--file-version` is set, then an error will be emitted. Example use: `--file-version 1.3.2`
* `-o`/`--os`: Operating system that the app cast items belong to. String must include one of the following: `windows`, `mac`, `linux`. Defaults to `windows`. Example use: `-o macos-arm64`; `-o windows-x64`
* `--description-tag`: Text to put in the app cast description tag/information. Defaults to "Most recent changes with links to updates". Example use: `--description-tag "Hello I am a Cool App"`
* `--link-tag`: Text to put in the app cast `link` tag/information. Should be your app cast download URL if you use this. Example use: `--link-tag https://mysite.com/coolapp/appcast.xml`
* `-u`/`--base-url`: Beginning portion of the URL to use for downloads. The file name that will be downloaded will be put after this portion of the URL. Example use: `-u https://myawesomecompany.com/downloads`
* `-l`/`--change-log-url`: Beginning portion of the URL to use for your change log files. The change log file that will be downloaded will be put after this portion of the URL. If this option is not specified, then the change log data will be put into the app cast itself. Example use: `-l https://myawesomecompany.com/changes`
* `-p`/`--change-log-path`: Path to the change log files for your software. These are expected to be in markdown format with an extension of `.md`. The file name of the change log files must contain the version of the software, e.g. `1.3.2.md`. Example use: `-p path/to/change/logs`. (Note: The generator will also attempt to find change logs whose file names are formatted like so: `MyApp 1.3.2.md`.)
* `--change-log-name-prefix`: Prefix for change log file names. By default, the generator searches for file names with the format "[Version].md". If you set this parameter to (for example) "My App Change Log", it will search for file names with the format "My App Change Log [Version].md" as well as "[Version].md".
* `-n`/`--product-name`: Product name for your software. Used when setting the title for your app cast and its items. Defaults to `Application`. Example use: `-n "My Awesome App"`
* `-x`/`--url-prefix-version`: Add the version number as a prefix to the file name for the download URL. Defaults to false. For example, if `--base-url` is `www.example.com/downloads`, your version is `1.4.2`, and your app name is `MyApp.exe`, your download URL will become `www.example.com/downloads/1.4.2/MyApp.exe`. Example use: `-x true`. 
* `--key-path`: Path to `NetSparkle_Ed25519.priv` and `NetSparkle_Ed25519.pub` files, which are your private and public Ed25519 keys for your software updates, respectively.  Example use: `--key-path my/path/to/keys`
  * If you want to use keys dynamically, you can set the `SPARKLE_PRIVATE_KEY` and `SPARKLE_PUBLIC_KEY` environment variables before running `generate_appcast`. The tool prioritizes environment keys over keys sitting on disk!
* `--signature-file-extension`: Extension (WITHOUT the `.`) to use for the app cast signature file. Defaults to `signature`. Example use: `--signature-file-extension txt`.
* `--output-file-name`: Output file name for the app cast with the `.` or the extension. Extension is controlled by whether it is an xml or json output and is not configurable. Defaults to 'appcast'. Of course, you can always change this later on your own after the app cast has been generated; this option is only for convenience. Example use: `--output-file-name super_app_download_info`.
* `--use-ed25519-signature-attribute`: If true and doing XML output, the output signature attribute in the XML will be `edSignature` rather than `signature` to match the original [Sparkle](https://github.com/sparkle-project/Sparkle) library. No effect on JSON app casts.
* `--file-version`: Use to set the version for a binary going into an app cast. Note that this version can only be set once, so when generating an app cast, make sure you either: A) have only one binary in your app cast | B) Utilize the `--reparse-existing` parameter so that old items get picked up. If the generator finds 2 binaries without any known version and `--file-version` is set, then an error will be emitted.
* `--critical-versions`: Comma-separated list of versions to mark as critical in the app cast. Must match version text exactly. E.g., "1.0.2,1.2.3.1".
* `--reparse-existing`: Re-parse an existing app cast rather than overriding it and creating it anew. Skips versions already in the app cast, so if you deploy a new binary with the same version, you will need to manually edit your app cast to remove the old listing for the version you are re-deploying. Example use: `--reparse-existing true`
* `--overwrite-old-items`: Causes app cast items to be rewritten in the app cast if the a binary on disk with the same version number is found. In other words, if 1.0.1 is in the app cast already (either from reparsing or from another binary), and another 1.0.1 is found on disk, then the 1.0.1 data in the app cast will be rewritten based on the binary found. Note that this means that if you have multiple 1.0.1 versions on disk (which you shouldn't do...), the last one found will be the one in your app cast! Example use: `--overwrite-old-items`
* `--human-readable`: If true, makes the output app cast file human readable (newslines, indents). Example use: `--human-readable true`
* `--channel`: Name of release channel for any items added into the app cast. Should be a single channel; does not support multiple channels at once, e.g. `beta,gamma`. Do not set if you want to use your release channel - if you set this to `release` or `stable`, those names/words will be treated as special channels and not as the stable channel. (Unless you want all your items to be in a specific channel, of course.) Example use: `--channel beta`
* `--output-type`: Output type for the app cast file (`xml` or `json`). Defaults to `xml`. Example use: `--output-type json`

#### Overriding public/private keys

* `--public-key-override`: Public key override (ignores whatever is in the public key file) for signing binaries. This overrides ALL other public keys set when verifying binaries, INCLUDING public key set via environment variables! If not set, uses `--key-path` (if set) or the default SignatureManager location. Not used in `--generate-keys` or `--export`. Example use: `--public-key-override asoj341ljsdflj`
* `--private-key-override`: Private key override (ignores whatever is in the private key file) for signing binaries. This overrides ALL other public keys set when verifying binaries, INCLUDING private key set via environment variables! If not set, uses `--key-path` (if set) or the default SignatureManager location. Not used in `--generate-keys` or `--export`. Example use: `--private-key-override asoj341ljsdflj`

#### Options for Key Generation

* `--generate-keys`: If set, will attempt to generate NEW Ed25519 keys for you. Can be used in conjunction with `--key-path`. Once keys are successfully (or unsuccessfully) generated, the program ends without generating an app cast. By default, existing keys are not overwritten. This option defaults to `false`.
* `--force`: If set to `true`, will overwrite existing keys on disk. **WARNING: THIS COULD RESULT IN A LOSS OF YOUR PUBLIC AND PRIVATE KEYS. USE WITH CAUTION. DO NOT USE IF YOU DO NOT KNOW WHAT YOU ARE DOING! THIS WILL MAKE NO ATTEMPT TO BACK UP YOUR DATA.** This option defaults to `false`. Example use: `--generate-keys --force true`.
* `--export`: Export keys as base 64 strings to the console. Defaults to `false`. Example use: `--export true`. Output format:
```
Private Key:
2o34usledjfs0
Public Key:
sdljflase;ru2u3
```

#### Options for Generating Signatures Without App Cast

* `--generate-signature`: Generate a signature for a file and output it to the console. Example use: `--generate-signature path/to/app/MyApp.exe`. Outputs in format: `Signature: seljr13412zpdfj`. 

#### Options for Verifying Signatures

Note that these options are only for verifying Ed25519 signatures. For DSA signatures, please use the `DSAHelper` tool. Both of the following options must be used together. You must have keys already generated in order to verify file signatures.

* `--verify`: Path to the file that has a signature you want to verify.
* `--signature`: Base 64 signature of the file.

Example use: `--verify my/path/MyApp.exe --signature 123l4ijsdfzderu23`.

This will return either `Signature valid` (signature is good!) or `Signature invalid` (signature does not match file).

#### App Cast Generator Examples

```bash

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
netsparkle-generate-appcast -n "My Awesome App" -b binary/folder

# Use file versions in file names, e.g. for apps like "My App 1.2.1.dmg"
netsparkle-generate-appcast -n "macOS version" -o macos -f true -b binary_folder -e dmg

# Don't overwrite the entire app cast file
netsparkle-generate-appcast --reparse-existing

# Don't overwrite the entire app cast file, but do overwrite items that are still on disk
netsparkle-generate-appcast --reparse-existing --overwrite-old-items
```

## Upgrading between major versions

Please see the [UPGRADING.md](UPGRADING.md) file for information on breaking changes and fixes between major versions.

## FAQ

### Am I required to use a UI with NetSparkleUpdater?

Nope. You can just reference the core library and handle everything yourself, including any custom UI. Check out the code samples for an example of doing that!

### Can I run my UI on another thread besides my main UI thread?

This isn't a built-in feature, as NetSparkleUpdater assumes that it can safely make calls/events to the UI on the thread that started the `SparkleUpdater` instance. However, if you'd like to do this, we have a sample on how to do this: `NetSparkle.Samples.Forms.Multithread`. Basically, instead of passing in a `UIFactory` to `SparkleUpdater`, you handle `SparkleUpdater`'s events yourself and show the UI however you want to show it - and yes, you can still use the built-in UI objects for this!

(Note that on Avalonia, the answer is always "No" since they only support one UI thread at this time.)

### On WinForms, can I let the user close the main window and still keep the updater forms around?

Yes. You need to start the `NetSparkleUpdater` forms on a new thread(s). See the `NetSparkle.Samples.Forms.Multithread` sample for how to do this by handling events yourself and still using the built-in WinForms `UIFactory`.

### How do I make my .NET Framework WinForms app high DPI aware?

See #238 [and this documentation](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/high-dpi-support-in-windows-forms?view=netframeworkdesktop-4.8#configuring-your-windows-forms-app-for-high-dpi-support) for the fix for making this work on the sample application. Basically, you need to use an app config file and manifest file to let Windows know that your application is DPI-aware. If that doesn't work for you, try some of the tips at [this SO post](https://stackoverflow.com/questions/4075802/creating-a-dpi-aware-application).

### What's all this about trimming?

[Trimming](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trimming-options) is a great way to reduce the file size of your application when it is self-published and/or built as a self-contained application. In short, trimming removes unused code from your applications, including external libraries, so you can ship your application with a reduced file size. To trim your application on publish, add `<PublishTrimmed>true</PublishTrimmed>` to your `csproj` file. If you want to trim all assemblies (including those that may not have specified they are compatible with trimming), add `<TrimMode>full</TrimMode>` to your `csproj` file; to only trim those that have opted-in, use `<TrimMode>partial</TrimMode>`. To enable warnings for trimming, add `<SuppressTrimAnalysisWarnings>false</SuppressTrimAnalysisWarnings>`.

There are other options to use, which you can learn more about on Microsoft's documentation [here](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trimming-options). For those applications that may not work with the built-in trimming options, please try [Zack.DotNetTrimmer](https://github.com/yangzhongke/Zack.DotNetTrimmer) or other solutions you may find.

We recommend that you trim your application before publishing it and distributing it to your users. Some of NetSparkle's default dependencies are rather large, but the file size can be drastically reduced by the trim process. If you choose to trim your application, don't forget to test it after trimming and make sure you fix any warnings that come up!

You can also read more about trimming libraries [here](https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/prepare-libraries-for-trimming).

### Is this library compatible with AOT compilation?

Yes.

### Is this library nullable aware?

Yes.

### Can I use relative paths for my app cast item download links?

Yes. In the app cast generator, you can do things like, `-u ../` to make NetSparkle check the directory above the server's `appcast.xml` file for download files.

### NuGet has lots of packages when I search for "NetSparkle", which one do I use?

`NetSparkleUpdater.SparkleUpdater` is the right package if you want the library with no built-in UI. Otherwise, use `NetSparkleUpdater.UI.{YourChoiceOfUI}`, which will give you a built-in UI and the core library. Previous to 2.0, the UI libraries reference `NetSparkle.New`, which is now deprecated.

Here is the full list of deprecated packages:

* [`com.pikleproductions.netsparkle`](https://www.nuget.org/packages/com.pikleproductions.netsparkle/) -- replaced by `NetSparkleUpdater.SparkleUpdater`
* [`com.pikleproductions.netsparkle.tools`](https://www.nuget.org/packages/com.pikleproductions.netsparkle.tools/) -- replaced by `NetSparkleUpdater.Tools.AppCastGenerator` and `NetSparkleUpdater.Tools.DSAHelper`
* [`NetSparkle.New`](https://www.nuget.org/packages/NetSparkle.New/) -- replaced by `NetSparkleUpdater.SparkleUpdater`
* [`NetSparkle.New.Tools`](https://www.nuget.org/packages/NetSparkle.New.Tools/) -- replaced by `NetSparkleUpdater.Tools.AppCastGenerator` and `NetSparkleUpdater.Tools.DSAHelper`
* [`NetSparkleUpdater.Tools`](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) -- replaced by `NetSparkleUpdater.Tools.AppCastGenerator` and `NetSparkleUpdater.Tools.DSAHelper`

### Must I put all my release versions into a single app cast file?

No. If your app is just using NetSparkle to work out if there is a later release - and you are not using the app cast as a way to refer to historical versions of your app in any way - then you don't need to add all the released versions into the app cast file.  

Having just the latest version of your software in the app cast has the added side effect that you won't need all the binaries & changelogs of all the versions to be available to the app cast generator tool.  For example, this might make an automated release build easier via GitHub Actions - because the only data required is the generated .exe and changelogs from your git repository.

### How can I use NetSparkleUpdater with [AppCenter](https://appcenter.ms/)?

#### Note: [AppCenter is scheduled for retirement on March 31, 2025.](https://learn.microsoft.com/en-us/appcenter/retirement)

1. Make sure you've read over the documentation [here](https://docs.microsoft.com/en-us/appcenter/distribution/sparkleupdates)
2. Decide if you want to generate signatures for your files. If so, make sure that works, and then use NetSparkleUpdater as normal.
3. If you don't want to generate signatures because you trust your AppCenter builds, use `SecurityMode.Unsafe` or the following `IAppCastHandler` override:

```csharp
public override bool DownloadAndParse()
{
    try
    {
        _logWriter.PrintMessage("Downloading app cast data...");

        var appCast = _dataDownloader.DownloadAndGetAppCastData(_castUrl);
        if (!string.IsNullOrWhiteSpace(appCast))
        {
            Items.Clear();
            Items.AddRange(ParseAppCast(appcast));
            return true;
        }
    }
    catch (Exception e)
    {
        _logWriter.PrintMessage("Error reading app cast {0}: {1} ", _castUrl, e.Message);
    }

    return false;
}
```

### Is reverting your application version supported?

The answer is both yes and no. No, because that is not the default behavior. Yes, because if you use installers for each of your versions, you can use your app cast to see which previous versions are available and download those versions. If your installers are standalone, they should install an old version just fine. Just keep in mind that if you install an old version and then there is a newer version in your app cast, after opening the older software, it will ask them if they want to update to the newer version!

Here's a summary of what you can do:

1. Setup your `SparkleUpdater` object
2. Call `_updateInfo = await _sparkle.CheckForUpdatesQuietly();` (no UI shown) or `_sparkle.CheckForUpdatesAtUserRequest()` (shows UI). I would recommend checking quietly because the UI method will always show the latest version. You can always show your own UI.
3. Look in `_updateInfo.Updates` for the available versions in your app cast. You can compare it with your currently installed version to see which ones are new and which ones are old.
4. Call `await _sparkle.InitAndBeginDownload(update);` with the update you want to download. The download path is provided in the `DownloadFinished` event.
5. When it's done downloading, call `_sparkle.InstallUpdate(update, _downloadPath);`

The "Handle Events Yourself" sample will be very helpful to you: https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Samples.HandleEventsYourself

### I want to have custom properties in my app cast. Can I handle serializing/deserializing the app cast file myself?

Yes. Implement `IAppCastGenerator` and set the `SparkleUpdater.AppCastGenerator` property to an instance of your class. You will have to implement the following methods:

```csharp
AppCast DeserializeAppCast(string appCastString);
Task<AppCast> DeserializeAppCastAsync(string appCastString);
AppCast DeserializeAppCastFromFile(string filePath);
Task<AppCast> DeserializeAppCastFromFileAsync(string filePath);

string SerializeAppCast(AppCast appCast);
Task<string> SerializeAppCastAsync(AppCast appCast);
void SerializeAppCastToFile(AppCast appCast, string outputPath);
Task SerializeAppCastToFileAsync(AppCast appCast, string outputPath);
```

As you can see, many of those functions are small variants of the core serialization and deserialization processes that you want to accomplish. You can look at the implementation of `JsonAppCastGenerator` and `XMLAppCastGenerator` for implementation examples.

### Can I use an app cast format other than XML or JSON?

Yes. Implement `IAppCastGenerator` and set the `SparkleUpdater.AppCastGenerator` property to an instance of your class. You'll have to make the actual app cast file yourself, though, since the app cast generator is only currently compatible with XML and JSON.

### Does this work with Avalonia version XYZ?

Right now, we are compatible with version 11. If you need to make changes, you can use your own `IUIFactory` implementation to fix any issues that come up.

### Can I use DSA signatures still?

DSA signatures are not recommended when using NetSparkleUpdater 2.0+. They are considered insecure!

You can still generate/use these signatures, however, using the `DSAHelper` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools.DSAHelper/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.DSAHelper)). Key generation only works on Windows because .NET Core 3 does not have the proper implementation to generate DSA keys on macOS/Linux; however, you can get DSA signatures for a file on any platform. If you need to generate a DSA public/private key, please use the DSAHelper tool on Windows like this: 

```
netsparkle-dsa /genkey_pair
```

You can use the DSAHelper to get a signature like this:

```
netsparkle-dsa /sign_update {YourInstallerPackage.msi} {NetSparkle_PrivateKey_DSA.priv}
```

#### Installing the DSA Helper command-line tool

1. `dotnet tool install --global NetSparkleUpdater.Tools.DSAHelper`
2. The tool is now available on your command line as the `netsparkle-dsa` command

#### DSA Code

Pass a `DSAChecker` into your `SparkleUpdater` constructor rather than an `Ed25519Checker`.

#### How do I transition from DSA to ed25519 signatures?

If your app has DSA signatures, the app cast generator uses Ed25519 signatures by default starting with preview `2.0.0-20200607001`. To transition to Ed25519 signatures, create an update where the software has your new Ed25519 public key and a NEW url for a NEW app cast that uses Ed25519 signatures. Upload this update with an app cast that has DSA signatures so your old DSA-enabled/containing app can download the Ed25519-enabled update. Then, future updates and app casts should all use Ed25519.

### Things aren't working. Help!

Here are some things you can do to figure out how to get your app running:

* Make sure you have enabled and debugged your application thoroughly. A great way to do this is to set `SparkleUpdater.LogWriter = new LogWriter(LogWriterOutputMode.Console)` and then watch your console output while debugging.
* Look at the NetSparkleUpdater samples by downloading this repo and running the samples. You can even try putting your app cast URL in there and using your public key to debug with the source code!
* Ask for help in our [Gitter](https://gitter.im/NetSparkleUpdater/NetSparkle)
* Post an issue and wait for someone to respond with assistance

### Are you accepting contributions?

Yes! Please help us make this library awesome!

### What's the tagging scheme, here?

* Major.Minor.Patch (Core)
* Major.Minor.Patch-app-cast-generator
* Major.Minor.Patch-dsa-helper
* Major.Minor.Patch-UI-Avalonia
* Major.Minor.Patch-UI-WinForms
* Major.Minor.Patch-UI-WPF

## Requirements

- .NET Framework 4.6.2+ | .NET 6+

## License

NetSparkle is available under the [MIT License](LICENSE).

## Contributing

Contributions are ALWAYS welcome! If you see a new feature you'd like to add, please open an issue to talk about it first, then open a PR for that implementation. If there's a bug you find, please open a PR with the fix or file an issue! Thank you!! :) You can also join us in our [Gitter chat room](https://gitter.im/NetSparkleUpdater/NetSparkle)!

### Areas where we could use help/contributions

* Unit tests for all parts of the project, including UI unit tests, full download tests, etc.
* Extensive testing/upgrades on macOS/Linux
* More options in the app cast generator
* See the [issues list](https://github.com/NetSparkleUpdater/NetSparkle/issues) for more

## Acknowledgements

* The original NetSparkle library, found at [dei79/netsparkle](https://github.com/dei79/netsparkle)
* A function for finding the base directory was taken from MIT-licensed [WalletWasabi](https://github.com/zkSNACKs/WalletWasabi/)
* MarkdownSharp is from [here](https://github.com/StackExchange/MarkdownSharp)
* We got our starting README layout from [MahApps.Metro](https://github.com/MahApps/MahApps.Metro), an awesome UI framework for WPF 

## Other Options

An incomplete list of other projects related to software updating that you might want to look at if NetSparkleUpdater doesn't work for you:

- [Velopack](https://github.com/velopack/velopack)
- [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)
- [WinSparkle](https://github.com/vslavik/winsparkle)
- [NAppUpdate](https://github.com/synhershko/NAppUpdate)
- [AutoUpdater.NET](https://github.com/ravibpatel/AutoUpdater.NET)
