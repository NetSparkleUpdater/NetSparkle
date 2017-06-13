# NetSparkle

NetSparkle is a C# .NET update checker that allows you to easily download installer files and update your WinForms or C# WPF software. You provide, somewhere on the net, an rss xml file that advertises latest version. You also provide release notes. This library then checks for an update in the background, shows the user the release notes, and offers to download the new installer. The original NetSparkle library can be found [here](https://github.com/dei79/netsparkle).

**Please make sure to check the `develop` branch for the latest updates! It will be the default branch soon.**

## About This Fork

This is a fork of NetSparkle, which has been forked by various people at various times. As of 2017-01-16, this is the "latest" fork. I, Deadpikle, am not actively working on or maintaining this repo outside of issues or features I experience using it for work, but I welcome any and all bug reports, pull requests, and other feature changes. In other words, I'm happy to help maintain the code so we don't have a million forks floating around. I've contacted [dei79](https://github.com/dei79), the original author of NetSparkle who migrated his code from CodePlex to GitHub a few months ago, to see if we can collaborate on a single NetSparkle repo/organization, but haven't heard back.

I highly recommend checking out https://github.com/Squirrel/Squirrel.Windows, which is a more Chrome-like software updater. That repo is actively maintained. You could also check out WinSparkle at https://github.com/vslavik/winsparkle, but there isn't a merged .NET binding yet.

## Basic Usage

    _sparkleApplicationUpdater = new Sparkle(
        @"http://example.com/appcast.xml", 
        this.Icon, 
        SecurityMode.Strict,
        @"<DSAKeyValue>...</DSAKeyValue>",
    );
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

If you're using NetSparkle from WPF, make sure to set `RunningFromWPF` to `true`!

## App Cast

[TODO -- this section]

By default, you need 2 DSA signatures (DSA Strict mode):

1. One in the enclosure section for your download file (sparkle:dsaSignature="...")
2. Another on your web server to secure the actual app cast file. **This file must be located at [CastURL].dsa** -- in other words, if your download URL for the app cast is http://someurl.com/awesome-software.xml, you need a valid DSA signature for that file stored at http://someurl.com/awesome-software.xml.dsa .

## License

NetSparkle is [MIT Licensed]

## Requirements

- .NET 4.5

## Other Options

An incomplete list of other .net projects related to updating:
 
 - [Sparkle Dot Net]
 - [NAppUpdate]

[CodePlex]: http://netsparkle.codeplex.com
[MIT Licensed]: http://netsparkle.codeplex.com/license
[Sparkle Dot Net]: https://github.com/iKenndac/SparkleDotNET
[NAppUpdate]: https://github.com/synhershko/NAppUpdate
