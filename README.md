## Description

Simple .net update checker & installer downloader. You provide, somewhere on the net, an rss xml file that advertises latest version. You also provide release notes. This library then checks for an update in the background, shows the user the release notes, and offers to download the new installer.

## Basic Usage

    _sparkleApplicationUpdater = new Sparkle(@"http://example.com/appcast.xml",this.Icon);
	_sparkleApplicationUpdater.CheckOnFirstApplicationIdle();

On the first Application.Idle event, your appcast.xml will be read and compared to the currently running version. If it's newer, a little "toast" box will slide up in the corner of the screen, announcing the new version. If the user clicks on it, then a larger dialog box will open, showing your release notes, again, from a server somewhere. The user can then ignore the update, ask to be reminded later, or download it now.

If you want to add a manual update in the background, do

    _sparkleApplicationUpdater.CheckForUpdatesQuietly();

If you want have a menu item for the user to check for updates, use

    _sparkleApplicationUpdater.CheckForUpdatesAtUserRequest();

## License

NetSparkle is [MIT Licensed]

## Requirements

- Visual Studio 2010 

## History
8 March 2013 John Hatton: Looking for something with a different approach, but found a good start in NetSparkle. Forked with these goals:

 - Reduce (maybe remove) the current "loop" background orientation. Most apps just want to check on launch.
 - Remove dependency on NLog, in fact just removed the whole separate diagnostic window. Now just uses Debug.Writeline().
 - Provide greater separation between the logic/download vs. the Sparkle-provided UI
 - Make UI less obtrusive, fitting with apps that are updated frequent (like, daily)
 - Enable Markdown for release notes, in addition to the existing html

3 Jan 2012 Jim Graham: forked NetSparkle to GitHub because I had some requirements for a client program that weren't easily met by the standard version.

 - This version allows for a custom UI instead of the two Windows Forms dialog
 - Allows for one-time check for updates instead of running on a loop
 - Allows for custom configuration objects instead of reading/writing to the registry
 - Refactored the diagnostic to use [NLog] instead of a custom filestream
 - Refactored existing the application to fire an event for shutdown instead of `Environment.Exit`

October 2010 dei79 created SVN repository on [CodePlex]:

NetSparkle is an easy-to-use software update framework for .NET developers on Windows, MAC or Linux. It was inspired by the Sparkle (http://bit.ly/HWyJd) project for Cocoa developers and the WinSparkle (http://bit.ly/cj5kP5) project (a Win32 port).

## Other Options

An incomplete list of other .net projects related to updating:
 
 - [Sparkle Dot Net]
 - [NAppUpdate]

[CodePlex]: http://netsparkle.codeplex.com
[MIT Licensed]: http://netsparkle.codeplex.com/license
[Sparkle Dot Net]: https://github.com/iKenndac/SparkleDotNET
[NAppUpdate]: https://github.com/synhershko/NAppUpdate