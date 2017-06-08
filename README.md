## Description

Simple .NET update checker & installer downloader. You provide, somewhere on the internet, an XML file with version history. You also provide release notes as HTML or Markdown files. This library then checks for an update in the background, shows the user the release notes, and offers to download the new installer.

- [About This Fork](#about-this-fork)
- [Basic Usage](#basic-usage)
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
