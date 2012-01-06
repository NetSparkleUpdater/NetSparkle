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

- [NLog] (included in lib/, you may want to update). 
 
## Differences with [CodePlex]

I forked NetSparkle because I had some requirements for a client program that weren't easily met by the standard version.

 - This version allows for a custom UI instead of the two Windows Forms dialog
 - Allows for one-time check for updates instead of running on a loop
 - Allows for custom configuration objects instead of reading/writing to the registry
 - Refactored the diagnostic to use [NLog] instead of a custom filestream (that tends to lock)
 - Refactored exiting the application to fire an event for shutdown instead of `Environment.Exit`

## Other Projects

An incomplete list of other projects that might be more suitable for you:
 
 - [Sparkle Dot Net]

[CodePlex]: http://netsparkle.codeplex.com
[Mit License]: http://netsparkle.codeplex.com/license
[NLog]: http://nlog-project.org/
[Sparkle Dot Net]: https://github.com/iKenndac/SparkleDotNET
