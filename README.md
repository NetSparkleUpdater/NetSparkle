## Description

Simple .net update checker & installer downloader. Based on NetSparkle [CodePlex], which is based on Sparkle for the Mac. You provide, somewhere on the net, an xml file that advertises latest version. You also provide release notes. This library then checks for an update in the background, shows the user the release notes, and offers to download the new installer.

## Basic Usage

    _sparkleApplicationUpdater = new Sparkle(@"http://example.com/appcast.xml")
    {
        ApplicationWindowIcon = this.Icon
    };

There are several (too many) other options, but this will give you a reasonable default. On the first Application.Idle event, your appcast.xml will be read and compared to the currently running version. If it's newer, a little "toast" box will slide up in the corner of the screen, announcing the new version. If the user clicks on it, then a larger dialog box will open, showing your release notes, again, from a server somewhere. The user can then ignore the update, ask to be reminded later, or download it now.

If you want to add a manual update, do

    _sparkleApplicationUpdater.CheckForUpdates();

If you want your own UI, just subscribe to this event, and no UI will be shown:

    _sparkleApplicationUpdater.UpdateDetected+=new UpdateDetected(MyCustomUpdateHandler);


## More about the original CodePlex project
From the [CodePlex] page:

NetSparkle is an easy-to-use software update framework for .NET developers on Windows, MAC or Linux. It was inspired by the Sparkle (http://bit.ly/HWyJd) project for Cocoa developers and the WinSparkle (http://bit.ly/cj5kP5) project (a Win32 port).


## License

NetSparkle is [Mit Licensed]

## Requirements

- Visual Studio 2010 

## Dependencies

 
## History
8 March 2013 John Hatton: Goals:

 - Reduce/remove the current "loop" background orientation. Most apps just want to check on launch.
 - Remove dependency on NLog
 - Provide greater separation between the logic/download vs. the Sparkle-provided UI
 - Make UI less obtrusive, fitting with apps that are updated frequent (like, daily)

3 Jan 2012 Jim Graham: forked NetSparkle to GitHub because I had some requirements for a client program that weren't easily met by the standard version.

 - This version allows for a custom UI instead of the two Windows Forms dialog
 - Allows for one-time check for updates instead of running on a loop
 - Allows for custom configuration objects instead of reading/writing to the registry
 - Refactored the diagnostic to use [NLog] instead of a custom filestream
 - Refactored exiting the application to fire an event for shutdown instead of `Environment.Exit`

## Other Projects

An incomplete list of other projects that might be more suitable for you:
 
 - [Sparkle Dot Net]

[CodePlex]: http://netsparkle.codeplex.com
[Mit Licensed]: http://netsparkle.codeplex.com/license
[Sparkle Dot Net]: https://github.com/iKenndac/SparkleDotNET
