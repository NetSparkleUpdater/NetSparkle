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

NetSparkle is a software update framework for C# that is compatible with .NET Core 3+ and .NET Framework 4.5.2+, has pre-built UIs for .NET Framework (WinForms, WPF) and .NET Core (WinForms, WPF, Avalonia), uses Ed25519 or other signatures, and even allows for custom UIs or no UI at all! You provide, somewhere on the internet, an [app cast](#app-cast) with update and version information, along with release notes in Markdown or HTML format. This library then helps you check for an update, show the user the release notes, and offer to download/install the new version of the software. 

The `develop` branch has changed significantly from `master` and represents a major 2.0 version update. NetSparkle 2.0, currently in beta, brings the ability to customize most of NetSparkle -- custom UIs are easy, you can have custom app cast downloaders and handlers (e.g. for FTP download or JSON appcasts), and more!

Built-in supported update download types:
* Windows -- .exe, .msi, .msp
* macOS -- .zip, .pkg, .dmg
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

* Reference the core NetSparkle build if you don't care about having a built-in UI and can manage things yourself
* Choose one of the other packages if you want a built-in UI or want to create your UI based on one of the other UIs

| Package | Use Case | Release | Preview | Downloads |
| ------- | -------- | ------- | ------- | --------- |
| NetSparkle | Core package; No built-in UI or 100% custom UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkle.New.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkle.New/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkle.New.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkle.New/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkle.New.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkle.New/) |
| WinForms UI (.NET Framework) | NetSparkle with built-in WinForms UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WinForms.NetFramework.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetFramework/) |
| WinForms UI (.NET Core) | NetSparkle with built-in WinForms UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WinForms.NetCore.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WinForms.NetCore/) |
| WPF UI (.NET Framework and Core) | NetSparkle with built-in WPF UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.WPF.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.WPF.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.WPF.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.WPF/) |
| [Avalonia](https://github.com/AvaloniaUI/Avalonia) UI | NetSparkle with built-in Avalonia UI | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.UI.Avalonia.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.UI.Avalonia/) |
| App Cast Generator Tool | `netsparkle-generate-appcast` CLI tool (incl. Ed25519 helpers) | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.Tools.AppCastGenerator.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.Tools.AppCastGenerator.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.Tools.AppCastGenerator.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) |
| DSA Helper Tool | `netsparkle-dsa` CLI tool (DSA helpers) | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.Tools.DSAHelper.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.DSAHelper/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.Tools.DSAHelper.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.DSAHelper/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.Tools.DSAHelper.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools.DSAHelper/) |
Command Line Tools [DEPRECATED] | DSA helper; AppCast generator (incl. Ed25519 helpers) [DEPRECATED] | [![NuGet](https://img.shields.io/nuget/v/NetSparkleUpdater.Tools.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) | [![NuGet](https://img.shields.io/nuget/vpre/NetSparkleUpdater.Tools.svg?style=flat-square&label=nuget-pre)](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) | [![NuGet](https://img.shields.io/nuget/dt/NetSparkleUpdater.Tools.svg?style=flat-square)](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) |

All notable changes to this project will be documented in the [changelog](CHANGELOG.md).


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

Right now, NetSparkleUpdater **does not** help you with 1., 2., or 4. "Why not?", you might ask:

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
    new Ed25519Checker(SecurityMode.Strict, // security mode -- use .Unsafe to ignore all signature checking (NOT recommended!!)
                       "base_64_public_key") // your base 64 public key -- generate this with the NetSparkleUpdater.Tools AppCastGenerator on any OS
) {
    UIFactory = new NetSparkleUpdater.UI.WPF.UIFactory(icon) // or null or choose some other UI factory or build your own!
};
_sparkle.StartLoop(true); // `true` to run an initial check online -- only call StartLoop once for a given SparkleUpdater instance!
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

#### Installing the App Cast command-line tool

1. `dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator`
2. The tool is now available on your command line as the `netsparkle-generate-appcast` command

### Ed25519 Signatures

You can generate Ed25519 signatures using the `AppCastGenerator` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.AppCastGenerator)). **This tool requires the .NET 5 Desktop Runtime to be installed.** Please see below sections for options and examples on generating the Ed25519 keys and for using them when creating an app cast.

### DSA Signatures

DSA signatures are not recommended for 2.0+. They are insecure!

You can still generate these signatures, however, using the `DSAHelper` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.DSAHelper)). Key generation only works on Windows because .NET Core 3 does not have the proper implementation to generate DSA keys on macOS/Linux; however, you can get DSA signatures for a file on any platform. If you need to generate a DSA public/private key, please use this tool on Windows like this: 

```
NetSparkle.DSAHelper.exe /genkey_pair
```

You can use the DSAHelper to get a signature like this:

```
NetSparkle.DSAHelper.exe /sign_update {YourInstallerPackage.msi} {NetSparkle_PrivateKey_DSA.priv}
```

#### Installing the DSA Helper command-line tool

1. `dotnet tool install --global NetSparkleUpdater.Tools.DSAHelper`
2. The tool is now available on your command line as the `netsparkle-dsa` command

### How can I make the app cast?

* Use the `AppCastGenerator` tool (from [this NuGet package](https://www.nuget.org/packages/NetSparkleUpdater.Tools.AppCastGenerator/) or in the [source code here](https://github.com/NetSparkleUpdater/NetSparkle/tree/develop/src/NetSparkle.Tools.AppCastGenerator)) to easily create your app cast file. Available options are described below. You can install it on your CLI via `dotnet tool install --global NetSparkleUpdater.Tools.AppCastGenerator`.
* Rig up a script that generates the app cast for you in python or some other language (`string.Format` or similar is a wonderful thing).
* Or you can just copy/paste the above example app cast into your own file and tweak the signatures/download info yourself, then generate the (Ed25519/DSA) signature for the app cast file manually! :)

### App Cast Generator Options

_Missing some option you'd like to see? File an issue on this repo or add it yourself and send us a pull request!_

* `--show-examples`: Print examples of usage to the console.
* `--help`: Show all options and their descriptions.

#### General Options When Generating App Cast
* `-a`/`--appcast-output-directory`: Directory in which to write the output `appcast.xml` file. Example use: `-a ./Output`
* `-e`/`--ext`: When looking for files to add to the app cast, use the given extension. Defaults to `exe`. Example use: `-e exe`
* `-b`/`--binaries`: File path to directory that should be searched through when looking for files to add to the app cast. Defaults to `.`. Example use: `-b my/build/directory`
* `-r`/`--search-binary-subdirectories`: True to search the binary directory recursively for binaries; false to only search the top directory. Defaults to `false`. Example use: `-r`.
* `-f`/`--file-extract-version`: Whether or not to extract the version of the file from the file's name rather than the file itself. Defaults to false. Use when your files that will be downloaded by NetSparkleUpdater will have the version number in the file name, e.g. "My App 1.3.2.exe". Example use: `-f true`
* `-o`/`--os`: Operating system that the app cast items belong to. Must be one of the following: `windows`, `mac`, `linux`. Defaults to `windows`. Example use: `-o linux`
* `-u`/`--base-url`: Beginning portion of the URL to use for downloads. The file name that will be downloaded will be put after this portion of the URL. Example use: `-u https://myawesomecompany.com/downloads`
* `-l`/`--change-log-url`: Beginning portion of the URL to use for your change log files. The change log file that will be downloaded will be put after this portion of the URL. If this option is not specified, then the change log data will be put into the app cast itself. Example use: `-l https://myawesomecompany.com/changes`
* `-p`/`--change-log-path`: Path to the change log files for your software. These are expected to be in markdown format with an extension of `.md`. The file name of the change log files must contain the version of the software, e.g. `MyApp 1.3.2.md`. Example use: `-p path/to/change/logs`
* `-n`/`--product-name`: Product name for your software. Used when setting the title for your app cast and its items. Defaults to `Application`. Example use: `-n "My Awesome App"`
* `-x`/`--url-prefix-version`: Add the version number as a prefix to the file name for the download URL. Defaults to false. For example, if `--base-url` is `www.example.com/downloads`, your version is `1.4.2`, and your app name is `MyApp.exe`, your download URL will become `www.example.com/downloads/1.4.2/MyApp.exe`. Example use: `-x true`. 
* `--key-path`: Path to `NetSparkle_Ed25519.priv` and `NetSparkle_Ed25519.pub` files, which are your private and public Ed25519 keys for your software updates, respectively.  Example use: `--key-path my/path/to/keys`
  * If you want to use keys dynamically, you can set the `SPARKLE_PRIVATE_KEY` and `SPARKLE_PUBLIC_KEY` environment variables before running `generate_appcast`. The tool prioritizes environment keys over keys sitting on disk!

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
generate_appcast.exe --generate-keys
# Store keys in a custom location
generate_appcast.exe --key-path path/to/store/keys

# By default, your Ed25519 signatures are stored on disk in your local 
# application data folder in a subdirectory called `netsparkle`. 
# If you want to export your keys to the console, you can do:
generate_appcast.exe --export

#### Generate a signature for a binary without creating an app cast:
generate_appcast.exe --generate-signature path/to/binary.exe

#### Verifying Binaries
generate_appcast.exe --verify path/to/binary.exe --signature base_64_signature

#### Using a custom key location:
# If your keys are sitting on disk somewhere
# (`NetSparkle_Ed25519.priv` and `NetSparkle_Ed25519.pub` -- both 
# in base 64 and both on disk in the same folder!), you can pass in 
# the path to these keys like this:
generate_appcast.exe --key-path path/to/keys/

#### Generating an app cast

# Generate an app cast for Windows executables that are sitting in a 
# specific directory
generate_appcast.exe -a directory/for/appcast/output/ -e exe -b directory/with/binaries/ -o windows

# Add change log info to your app cast
generate_appcast.exe -b binary/folder -p change/log/folder

# Customize download URL for binaries and change logs
generate_appcast.exe -b binary/folder -p change/log/folder -u https://example.com/downloads -p https://example.com/downloads/changelogs

# Set your application name for the app cast
generate_appcast.exe -n "My Awesome App" -b binary/folder

# Use file versions in file names, e.g. for apps like "My App 1.2.1.dmg"
generate_appcast.exe -n "macOS version" -o macos -f true -b binary/folder -e dmg
```

## Updating from 0.X or 1.X

This section holds info on major changes when moving from versions 0.X or 1.Y. If we've forgotten something important, please [file an issue](https://github.com/NetSparkleUpdater/NetSparkle/issues).

* Minimum .NET Framework requirement, if running on .NET Framework, is now .NET Framework 4.5.2 instead of 4.5.1. Otherwise, the core library is .NET Standard 2.0 compatible, which means you can use it in your .NET Core and .NET 5+ projects.
* Change of base namespace from `NetSparkle` to `NetSparkleUpdater`
* `Sparkle` renamed to `SparkleUpdater` for clarity
* More logs are now written via `LogWriter` to help you debug and figure out any issues that are going on
* The default `NetSparkleUpdater` package (`NetSparkle.New`/`NetSparkleUpdater.NetSparkle`) **has no built-in UI**. Please use one of the NetSparkleUpdater packages with a UI if you want a built-in UI that is provided for you.
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
* Added built-in UIs for WPF and [Avalonia](https://github.com/AvaloniaUI/Avalonia) 0.9.X.
* Localization capabilities are now non-functional and are expected to come back in a later version. See [this issue](https://github.com/NetSparkleUpdater/NetSparkle/issues/92). (Contributions are welcome!)
* Most `SparkleUpdater` elements are now configurable. For example, you can implement `IAppCastHandler` to implement your own app cast parsing and checking.
  * `IAppCastDataDownloader` to implement downloading of your app cast file
  * `IUpdateDownloader` to implement downloading of your actual binary/installer as well as getting file names from the server (if `CheckServerFileName` is `true`)
  * `IAppCastHandler` to implement your own app cast parsing
  * `ISignatureVerifier` to implement your own download/app cast signature checking. NetSparkleUpdater has built-in DSA and Ed25519 signature verifiers.
  * `IUIFactory` to implement your own UI
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
* Samples have been updated and improved
  * Sample apps for [Avalonia](https://github.com/AvaloniaUI/Avalonia), WinForms, and WPF UIs
  * Sample app to demonstrate how to handle events yourself with your own, custom UI
* By default, the app cast signature file now has a `.signature` extension. The app cast downloader will look for a file with the old `.dsa` signature if data is not available or found in a `appcast.xml.signature` on your server.
* `sparkle:dsaSignature` is now `sparkle:signature` instead. If no `sparkle:signature` is found, `sparkle:dsaSignature` will be used (if available). If `sparkle:dsaSignature` is not found, `sparkle:edSignature` will be used (if available). This is to give us as much compatibility with old versions of `NetSparkle` as well as the macOS `Sparkle` library.
* An entirely new app cast generator tool is now available for use.
* By default, the app cast generator tool now uses Ed25519 signatures. If you don't want to use files on disk to store your keys, set the `SPARKLE_PRIVATE_KEY` and `SPARKLE_PUBLIC_KEY` environment variables before running the app cast generator tool. You can also store these signatures in a custom location with the `--key-path` flag.
  * You can still use DSA signatures via the DSAHelper tool and the `DSAChecker` class. This is not recommended.
  * `Ed25519Checker` is the class responsible for handling Ed25519 signatures. `DSAChecker` sticks around for verifying DSA signatures if they're still used.
* Removed `AssemblyAccessor` class in lieu of `IAssemblyAccessor` implementors
* The server file name for each app cast download is now checked before doing any downloads or showing available updates to the client. To disable this behavior and use the name in the app cast, set `SparkleUpdater.CheckServerFileName` to `false`.
* `bool ignoreSkippedVersions = false` has been added to `CheckForUpdatesAtUserRequest`, `CheckForUpdatesQuietly`, and `GetUpdateStatus` to make ignoring skipped versions easier.
* **Breaking change**: `CheckForUpdatesQuietly` now shows no UI ever. It could show a UI before, which didn't make a lot of sense based on the function name. Make sure that if you use this function that you handle showing a UI yourself if necessary. (See the HandleEventsYourself sample if you want help.) You can always trigger the built-in `SparkleUpdater` by calling `_sparkle.ShowUpdateNeededUI(updateInfo.Updates)`. 
* **We now rely on Portable.BouncyCastle** (BouncyCastle.Crypto.dll) for the ed25519 implementation. This means there is another DLL to reference when you use NetSparkle!
* **We now rely on System.Text.Json (netstandard2.0) OR Newtonsoft.Json (.NET Framework 4.5.2)** for the JSON items. This means there is another DLL to reference when you use NetSparkle, and it will change depending on if the `System.Text.Json` or `Newtonsoft.Json` item is used!

## FAQ

### Am I required to use a UI with NetSparkleUpdater?

Nope. You can just reference the core library and handle everything yourself, including any custom UI. Check out the code samples for an example of doing that!

### How can I use NetSparkleUpdater with [AppCenter](https://appcenter.ms/)?

1. Make sure you've read over the documentation [here](https://docs.microsoft.com/en-us/appcenter/distribution/sparkleupdates)
2. Decide if you want to generate signatures for your files. If so, make sure that works, and then use NetSparkleUpdater as normal.
3. If you don't want to generate signatures because you trust your AppCenter builds, use `SecurityMode.Unsafe` or the following `IAppCastHandler` override:

```csharp
public bool DownloadAndParse()
{
    try
    {
        _logWriter.PrintMessage("Downloading app cast data...");

        var appcast = _dataDownloader.DownloadAndGetAppCastData(_castUrl);
        if (!string.IsNullOrEmpty(appcast))
        {
            ParseAppCast(appcast);
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

### Does this work with Avalonia 0.10?

Right now, the Avalonia UI is compatible with Avalonia 0.9. Please see [#122](https://github.com/NetSparkleUpdater/NetSparkle/issues/122) for details on this issue -- basically, when 0.10 is officially released, we'll update the Avalonia build. For now, you can use your own `IUIFactory` implementation to fix any issues that come up.

### Things aren't working. Help!

Here are some things you can do to figure out how to get your app running:

* Make sure you have enabled and debugged your application thoroughly. A great way to do this is to set `SparkleUpdater.LogWriter = new LogWriter(true)` and then watch your console output while debugging.
* Look at the NetSparkleUpdater samples by downloading this repo and running the samples. You can even try putting your app cast URL in there and using your public key to debug with the source code!
* Ask for help in our [Gitter](https://gitter.im/NetSparkleUpdater/NetSparkle)
* Post an issue and wait for someone to respond with assistance

### Are you accepting contributions?

Yes! Please help us make this library awesome!

## Requirements

- .NET Framework 4.5.2+ | .NET Core 3+ | .NET 5+

## License

NetSparkle is available under the [MIT License](LICENSE).

## Contributing

Contributions are ALWAYS welcome! If you see a new feature you'd like to add, please open an issue to talk about it first, then open a PR for that implementation. If there's a bug you find, please open a PR with the fix or file an issue! Thank you!! :) You can also join us in our [Gitter chat room](https://gitter.im/NetSparkleUpdater/NetSparkle)!

### Areas where we could use help/contributions

* Unit tests for all parts of the project
* Extensive testing on macOS/Linux
* More built-in app cast parsers (e.g. natively support using/creating JSON feeds) -- possible via interfaces but not built-in yet
* More options in the app cast generator
* See the [issues list](https://github.com/NetSparkleUpdater/NetSparkle/issues) for more

## Acknowledgements

* The original NetSparkle library, found at [dei79/netsparkle](https://github.com/dei79/netsparkle)
* A function for finding the base directory was taken from MIT-licensed [WalletWasabi](https://github.com/zkSNACKs/WalletWasabi/)
* MarkdownSharp is from [here](https://github.com/StackExchange/MarkdownSharp)
* We got our starting README layout from [MahApps.Metro](https://github.com/MahApps/MahApps.Metro), an awesome UI framework for WPF 

## Other Options

An incomplete list of other projects related to software updating that you might want to look at if NetSparkleUpdater doesn't work for you:

- [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)
- [WinSparkle](https://github.com/vslavik/winsparkle)
- [NAppUpdate](https://github.com/synhershko/NAppUpdate)
- [AutoUpdater.NET](https://github.com/ravibpatel/AutoUpdater.NET)
