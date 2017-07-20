## History

**This is an old file. For an up-to-date changelog, see [CHANGELOG.md](CHANGELOG.md).**

June 2016 Steffen Xonna: Forked the library to raise the security and change some handling for an better usability for the user.  

 - Introduce an SecurityMode
   - SecurityMode.Unsafe = Is nearly the same like before. If there isn't an Signature or an DSA Key the files will be accepted. If both is present the signature has to be valid. *i don't recommend this mode.* It has critical security issues.
   - SecurityMode.UseIfPossible = If an local DSA key is available then an signature is neccessary. If both is present the signature has to be valid.
   - SecurityMode.Strict = The default-mode. This mode enforce the avaibility of an local DSA Key. If none is available the updatefiles will be rejected. *I strongly recommend this mode.*
 - Added support for DSA public key as parameter for sparkle.
 - Added support for appcast sidecar file which contains the DSA signature. Depends on the same rules (SecurityMode) like all other files.
 - Added support for DSA signatures of release note links. Depends on the same rules (SecurityMode) like all other files.
 - Added support for embedded release note.
 - Added support for relative downloads and relative external release notes.
 - Added the possibility to show the form synchronized with the main form. I hope for better handling for the user. So no window in background or multiple windows will be shown.
 - Added better handling of ssl connections (all downloads should use the same code and not 3 different)
 - Added support for custom registry path of configuration
 - Fix: don't accept ssl connections with policy errors.
 - only show versions they match security mode (if signature is needed than only accept versions with signatures or without downloads)
 - Fix: download don't stop if download windows will be closed

23 December 2014 Bluewalk: Since the development seems very quiet, I have forked this project and started adding fixes from other forks into this fork. Also I started to add improvements to the library as well including:

 - Cleaning up the project files
 - Set proxy to use default credentials if available (jimgraham)
 - Prettied up the release notes overview
 - Fixed not showing the installed version and app name in the Softare Update window
 - Added option to make the toast optional
 - Added option to use the NetSparkle icon if wanted
 - Stopped using .NET 4 Client profile
 - Added options RelaunchAfterUpdate (which defines if the app needs to be relaunced after update) and CustomInstallerArguments which allows you to add arguments to the update command line (as per https://github.com/hatton/NetSparkle/commit/2dbaa197633b2624b765ff86b3a619501200580d)
 - Added support for Windows installer patches (MSP)

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
