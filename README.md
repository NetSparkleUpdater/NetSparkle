## Description

Simple .NET update checker & installer downloader. You provide, somewhere on the internet, an XML file with version history. You also provide release notes as HTML or Markdown files. This library then checks for an update in the background, shows the user the release notes, and offers to download the new installer.

- [About This Fork](#about-this-fork)
- Sparkle class
    - [Basic Usage](#basic-usage)
    - [Public Methods](#public-methods)
- [Appcast](#appcast)
- [License](#license)
- [Requirements](#requirements)
- [Other Options](#other-options)

## About This Fork

This is a fork of NetSparkle, which has been forked by various people at various times. As of June 2017, this is the "latest" fork. I, Deadpikle, am not actively working on or maintaining this repo outside of issues or features I experience using it for work, but I welcome any and all bug reports, pull requests, and other feature changes. In other words, I'm happy to help maintain the code so we don't have a million forks floating around.

I highly recommend checking out [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows), which is a more Chrome-like software updater. That repo is actively maintained. You could also check out [WinSparkle](https://github.com/vslavik/winsparkle), but there isn't a merged .NET binding yet.

Honestly, this library needs a serious clean up and rewrite. Some of it is nice, such as the UIFactory separation to allow you to create your own UI, but some of the code is not so nice, such as the lack of better WPF support/GUI and the code organization. Perhaps someone reading this readme would like to help?

Some things TODO if you want to help:

- Address some of the TODO in the code
- Delete the downloaded installer once we're done? (Perhaps using START /wait?)
- I'm pretty sure there's a bug where `SecurityMode.Strict` doesn't properly check the DSA on update files
- Better WPF support. WPF app updates work right now, but this project has obviously been created for Forms instead. There's already been work to keep the UI separate from the actual updater, which is good, but better WPF support/documentation/etc. would be helpful.
- Could we do something neat to tie this in with [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)?
- Clean up the code (this is needed quite a bit)
- Update the example files and demo project (as of 2016-10-27, the NetSparkleTestAppWPF and SampleApplication projects work!)
- Allow for a WPF window to show changelog/download progress/etc. instead of forcing the user to make their own and implement the UIFactory and IDownloadProgress/IUpdateAvailable/etc. interfaces.
- Make demos more extensive to show off features

## Basic Usage

```csharp
_sparkle = new Sparkle(
    "http://example.com/appcast.xml",
    this.Icon,
    SecurityMode.Strict,
    "<DSAKeyValue>...</DSAKeyValue>",
);
_sparkle.CheckOnFirstApplicationIdle();
```

On the first Application.Idle event, your appcast.xml will be read and compared to the currently running version. If it's newer, the user will be notified with a little "toast" box if enabled, otherwise with the update dialog containing your release notes (if defined). The user can then ignore the update, ask to be reminded later, or download it now.

If you want to add a manual update in the background, do

```csharp
_sparkle.CheckForUpdatesQuietly();
```

If you want have a menu item for the user to check for updates, use

```csharp
_sparkle.CheckForUpdatesAtUserRequest();
```

If you have files that need saving, subscribe to the AboutToExitForInstallerRun event:

```csharp
_sparkle.AboutToExitForInstallerRun += ((x, cancellable) =>
{
	// ask the user to save, whatever else is needed to close down gracefully
});
```

## Public Methods

### Sparkle(string appcastUrl)

Initializes a new instance of the Sparkle class with the given appcast URL.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |

### Sparkle(string appcastUrl, System.Drawing.Icon applicationIcon)

Initializes a new instance of the Sparkle class with the given appcast URL and an Icon for the update UI.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| applicationIcon | *System.Drawing.Icon*<br>Icon to be displayed in the update UI. If you're invoking this from a form, this would be `this.Icon`. |

### Sparkle(string appcastUrl, System.Drawing.Icon applicationIcon, NetSparkle.SecurityMode securityMode)

Initializes a new instance of the Sparkle class with the given appcast URL, an Icon for the update UI, and the security mode for DSA signatures.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| applicationIcon | *System.Drawing.Icon*<br>Icon to be displayed in the update UI. If you're invoking this from a form, this would be `this.Icon`. |
| securityMode | *NetSparkle.SecurityMode*<br>the security mode to be used when checking DSA signatures |

### Sparkle(string appcastUrl, System.Drawing.Icon applicationIcon, NetSparkle.SecurityMode securityMode, string dsaPublicKey)

Initializes a new instance of the Sparkle class with the given appcast URL, an Icon for the update UI, the security mode for DSA signatures, and the DSA public key.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| applicationIcon | *System.Drawing.Icon*<br>Icon to be displayed in the update UI. If you're invoking this from a form, this would be `this.Icon`. |
| securityMode | *NetSparkle.SecurityMode*<br>the security mode to be used when checking DSA signatures |
| dsaPublicKey | *System.String*<br>the DSA public key for checking signatures, in XML Signature (`<DSAKeyValue>`) format<br>if null, a file named "NetSparkle_DSA.pub" is used instead |

### Sparkle(string appcastUrl, System.Drawing.Icon applicationIcon, NetSparkle.SecurityMode securityMode, string dsaPublicKey, string referenceAssembly)

Initializes a new instance of the Sparkle class with the given appcast URL, an Icon for the update UI, the security mode for DSA signatures, the DSA public key, and the name of the assembly to use when comparing update versions.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| applicationIcon | *System.Drawing.Icon*<br>Icon to be displayed in the update UI. If you're invoking this from a form, this would be `this.Icon`. |
| securityMode | *NetSparkle.SecurityMode*<br>the security mode to be used when checking DSA signatures |
| dsaPublicKey | *System.String*<br>the DSA public key for checking signatures, in XML Signature (`<DSAKeyValue>`) format<br>if null, a file named "NetSparkle_DSA.pub" is used instead |
| referenceAssembly | *System.String*<br>the name of the assembly to use for comparison when checking update versions |

### Sparkle(string appcastUrl, System.Drawing.Icon applicationIcon, NetSparkle.SecurityMode securityMode, string dsaPublicKey, string referenceAssembly, NetSparkle.Interfaces.IUIFactory factory)

Initializes a new instance of the Sparkle class with the given appcast URL, an Icon for the update UI, the security mode for DSA signatures, the DSA public key, the name of the assembly to use when comparing update versions, and a UI factory to use in place of the default UI.

| Name | Description |
| ---- | ----------- |
| appcastUrl | *System.String*<br>the URL of the appcast file |
| applicationIcon | *System.Drawing.Icon*<br>Icon to be displayed in the update UI. If you're invoking this from a form, this would be `this.Icon`. |
| securityMode | *NetSparkle.SecurityMode*<br>the security mode to be used when checking DSA signatures |
| dsaPublicKey | *System.String*<br>the DSA public key for checking signatures, in XML Signature (`<DSAKeyValue>`) format<br>if null, a file named "NetSparkle_DSA.pub" is used instead |
| referenceAssembly | *System.String*<br>the name of the assembly to use for comparison when checking update versions |
| factory | *NetSparkle.Interfaces.IUIFactory*<br>a UI factory to use in place of the default UI |

### void CancelFileDownload()

Cancels an in-progress download and deletes the temporary file.

### Task<SparkleUpdateInfo> CheckForUpdatesAtUserRequest()

Check for updates, using interaction appropriate for if the user just said "check for updates".

### Task<SparkleUpdateInfo> CheckForUpdatesQuietly()

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

### void ReportDiagnosticMessage(string message)

This method reports a message in the diagnostic window.

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

## Appcast

NetSparkle uses Sparkle-compatible appcasts. Here is a sample appcast:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<rss xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:sparkle="http://www.andymatuschak.org/xml-namespaces/sparkle" version="2.0">
    <channel>
        <title>NetSparkle Test App</title>
        <link>https://deadpikle.github.io/NetSparkle/files/sample-app/appcast.xml</link>
        <description>Most recent changes with links to updates.</description>
        <language>en</language>
        <item>
            <title>Version 2.0 (2 bugs fixed; 3 new features)</title>
            <sparkle:releaseNotesLink>
            https://deadpikle.github.io/NetSparkle/files/sample-app/2.0-release-notes.md
            </sparkle:releaseNotesLink>
            <pubDate>Thu, 27 Oct 2016 10:30:00 +0000</pubDate>
            <enclosure url="https://deadpikle.github.io/NetSparkle/files/sample-app/NetSparkleUpdate.exe"
                       sparkle:version="2.0"
                       length="12288"
                       type="application/octet-stream"
                       sparkle:dsaSignature="NSG/eKz9BaTJrRDvKSwYEaOumYpPMtMYRq+vjsNlHqRGku/Ual3EoQ==" />
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
        - `sparkle:dsaSignature`, optional: the DSA signature of the document; if present, notes will only be displayed if the DSA signature is valid
- `<pubDate>`
    - The date this update was published
- `<enclosure>`
    - This tag describes the update file that NetSparkle will download.
    - **Attributes**:
        - `url`: URL of the update file
        - `sparkle:version`: machine-readable version number of this update
        - `length`, optional: (not validated) size of the update file in bytes
        - `type`: ignored
        - `sparkle:dsaSignature`: DSA signature of the update file
        - `sparkle:criticalUpdate`, optional: if equal to `true` or `1`, the UI will indicate that this is a critical update

By default, you need 2 DSA signatures (DSA Strict mode):

1. One in the enclosure tag for your download file (`sparkle:dsaSignature="..."`)
1. Another on your web server to secure the actual appcast file. **This file must be located at [CastURL].dsa**. In other words, if the appcast URL is http://example.com/awesome-software.xml, you need a valid DSA signature for that file at http://example.com/awesome-software.xml.dsa.

## License

NetSparkle is available under the [MIT License](LICENSE).

## Requirements

- .NET 4.5

## Other Options

An incomplete list of other projects related to updating:

- [NAppUpdate](https://github.com/synhershko/NAppUpdate)
- [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows)
- [WinSparkle](https://github.com/vslavik/winsparkle)
