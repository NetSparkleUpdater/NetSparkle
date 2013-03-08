NetSparkle
============

http://github.com/jimgraham/NetSparkle

## Description

A fork of the NetSparkle [CodePlex] repository

From the [CodePlex] page

NetSparkle is an easy-to-use software update framework for .NET developers on Windows, MAC or Linux. It was inspired by the Sparkle (http://bit.ly/HWyJd) project for Cocoa developers and the WinSparkle (http://bit.ly/cj5kP5) project (a Win32 port).

The main project's webpage is on [CodePlex].

## License

NetSparkle is MIT-Licensed [Mit License]

## Requirements

- Visual Studio 2010 

## Dependencies

 
## History
8 March 2013 John Hatton: Goals: Redesign to be less obtrusive, both for the user and the client application. Initial goals are to remove dependency on NLog, then provide greater separation between the logic/download vs. the Sparkle-provided UI.

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
[Mit License]: http://netsparkle.codeplex.com/license
[Sparkle Dot Net]: https://github.com/iKenndac/SparkleDotNET
