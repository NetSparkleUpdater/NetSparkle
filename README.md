## Description

Simple .net update checker & installer downloader. You provide, somewhere on the net, an rss xml file that advertises latest version. You also provide release notes. This library then checks for an update in the background, shows the user the release notes, and offers to download the new installer.

## Basic Usage

    _sparkleApplicationUpdater = new Sparkle(@"http://example.com/appcast.xml", this.Icon);
	_sparkleApplicationUpdater.CheckOnFirstApplicationIdle();

On the first Application.Idle event, your appcast.xml will be read and compared to the currently running version. If it's newer, the user will be notified with a little "toast" box if enabled, otherwise with the update dialog containing your release notes (if defined). The user can then ignore the update, ask to be reminded later, or download it now.

If you want to add a manual update in the background, do

    _sparkleApplicationUpdater.CheckForUpdatesQuietly();

If you want have a menu item for the user to check for updates, use

    _sparkleApplicationUpdater.CheckForUpdatesAtUserRequest();

If you have files that need saving, subscribe to the AboutToExitForInstallerRun event:

    _sparkle.AboutToExitForInstallerRun += ((x, cancellable) =>
    {
    	// ask the user to save, whatever else is needed to close down gracefully   
    });  

## License

NetSparkle is [MIT Licensed]

## Requirements

- .NET 4.0

## History
23 December 2014 Bluewalk: Since the development seems very quiet, I have forked this project and started adding fixes from other forks into this fork. Also I started to add improvements to the library as well including:

 - Cleaning up the project files
 - Set proxy to use default credentials if available (jimgraham)
 - Prettied up the release notes overview
 - Fixed not showing the installed version and app name in the Softare Update window
 - Added option to make the toast optional
 - Added option to use the NetSparkle icon if wanted
 - Stopped using .NET 4 Client profile

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

## Other Options

An incomplete list of other .net projects related to updating:
 
 - [Sparkle Dot Net]
 - [NAppUpdate]

[CodePlex]: http://netsparkle.codeplex.com
[MIT Licensed]: http://netsparkle.codeplex.com/license
[Sparkle Dot Net]: https://github.com/iKenndac/SparkleDotNET
[NAppUpdate]: https://github.com/synhershko/NAppUpdate
