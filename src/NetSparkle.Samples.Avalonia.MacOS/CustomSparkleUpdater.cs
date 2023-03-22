using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.Samples.Avalonia
{
    public class CustomSparkleUpdater : NetSparkleUpdater.SparkleUpdater
    {
        public CustomSparkleUpdater(string appcastUrl, ISignatureVerifier signatureVerifier)
            : base(appcastUrl, signatureVerifier, null)
        { }

        protected override string GetInstallerCommand(string downloadFilePath)
        {
            // get the file type
            string installerExt = Path.GetExtension(downloadFilePath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsInstallerCommand(downloadFilePath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (DoExtensionsMatch(installerExt, ".pkg"))
                {
                    return "open \"" + downloadFilePath + "\"";
                }
                else if (DoExtensionsMatch(installerExt, ".dmg"))
                {
                    return $@"
                        listing=$(sudo hdiutil attach """ + downloadFilePath + $@""" | grep Volumes)
                        volume=$(echo ""$listing"" | cut -f 3)
                        if [ -e ""$volume""/*.app ]; then
                            sudo cp -rf ""$volume""/*.app """ + RestartExecutablePath + $@"""
                        elif [ -e ""$volume""/*.pkg ]; then
                            package=$(ls -1 ""$volume"" | grep .pkg | head -1)
                            sudo installer -pkg ""$volume""/""$package"" -target /
                        fi
                        sudo hdiutil unmount ""$volume""/*.app
                        #sudo hdiutil detach ""$(echo \""$listing\"" | cut -f 1)""
                    ";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (DoExtensionsMatch(installerExt, ".deb"))
                {
                    return "sudo dpkg -i \"" + downloadFilePath + "\"";
                }
                if (DoExtensionsMatch(installerExt, ".rpm"))
                {
                    return "sudo rpm -i \"" + downloadFilePath + "\"";
                }
            }
            return downloadFilePath;
        }

        

        /// <summary>
        /// Updates the application via the file at the given path. Figures out which command needs
        /// to be run, sets up the application so that it will start the downloaded file once the
        /// main application stops, and then waits to start the downloaded update.
        /// </summary>
        /// <param name="downloadFilePath">path to the downloaded installer/updater</param>
        /// <returns>the awaitable <see cref="Task"/> for the application quitting</returns>
        protected override async Task RunDownloadedInstaller(string downloadFilePath)
        {
            LogWriter.PrintMessage("Running downloaded installer");
            // get the options for restarting the application
            string executableName = RestartExecutableName;
            string workingDir = RestartExecutablePath;

            // generate the batch file path
#if NETFRAMEWORK
            bool isWindows = true;
            bool isMacOS = false;
#else
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
            var extension = isWindows ? ".cmd" : ".sh";
            string batchFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
            string installerCmd;
            try
            {
                installerCmd = GetInstallerCommand(downloadFilePath);
                if (!string.IsNullOrEmpty(CustomInstallerArguments))
                {
                    installerCmd += " " + CustomInstallerArguments;
                }
            }
            catch (InvalidDataException)
            {
                UIFactory?.ShowUnknownInstallerFormatMessage(this, downloadFilePath);
                return;
            }

            // generate the batch file                
            LogWriter.PrintMessage("Generating batch in {0}", Path.GetFullPath(batchFilePath));

            string processID = Process.GetCurrentProcess().Id.ToString();
            string relaunchAfterUpdate = "";
            if (RelaunchAfterUpdate)
            {
                relaunchAfterUpdate = $@"
                    cd ""{workingDir}""
                    ""{executableName}""";
            }

            using (FileStream stream = new FileStream(batchFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, true))
            using (StreamWriter write = new StreamWriter(stream, new UTF8Encoding(false))/*new StreamWriter(batchFilePath, false, new UTF8Encoding(false))*/)
            {
                if (isWindows)
                {
                    // We should wait until the host process has died before starting the installer.
                    // This way, any DLLs or other items can be replaced properly.
                    // Code from: http://stackoverflow.com/a/22559462/3938401

                    string output = $@"
                        @echo off
                        chcp 65001 > nul
                        set /A counter=0                       
                        setlocal ENABLEDELAYEDEXPANSION
                        :loop
                        set /A counter=!counter!+1
                        if !counter! == 90 (
                            exit /b 1
                        )
                        tasklist | findstr ""\<{processID}\>"" > nul
                        if not errorlevel 1 (
                            timeout /t 1 > nul
                            goto :loop
                        )
                        :install
                        {installerCmd}
                        :afterinstall
                        {relaunchAfterUpdate.Trim()}
                        endlocal";
                    await write.WriteAsync(output);
                    write.Close();
                }
                else
                {
                    // We should wait until the host process has died before starting the installer.
                    var waitForFinish = $@"
                        COUNTER=0;
                        while ps -p {processID} > /dev/null;
                            do sleep 1;
                            COUNTER=$((++COUNTER));
                            if [ $COUNTER -eq 90 ] 
                            then
                                exit -1;
                            fi;
                        done;
                    ";
                    if (IsZipDownload(downloadFilePath)) // .zip on macOS or .tar.gz on Linux
                    {
                        // waiting for finish based on http://blog.joncairns.com/2013/03/wait-for-a-unix-process-to-finish/
                        // use tar to extract
                        var tarCommand = isMacOS ? $"tar -x -f {downloadFilePath} -C \"{workingDir}\"" 
                            : $"tar -xf {downloadFilePath} -C \"{workingDir}\" --overwrite ";
                        var output = $@"
                            {waitForFinish}
                            {tarCommand}
                            {relaunchAfterUpdate}";
                        await write.WriteAsync(output.Replace("\r\n", "\n"));
                    }
                    else
                    {
                        string installerExt = Path.GetExtension(downloadFilePath);
                        if (DoExtensionsMatch(installerExt, ".pkg"))
                        {
                            relaunchAfterUpdate = ""; // relaunching not supported for pkg download
                        }
                        else if (DoExtensionsMatch(installerExt, ".dmg"))
                        {
                            relaunchAfterUpdate = $@"
                                cd ""{workingDir}""
                                open -a ""{executableName}"""; // results in error, No application knows how to open URL
                        }
                        var output = $@"
                            {waitForFinish}
                            {installerCmd}
                            {relaunchAfterUpdate}";
                        LogWriter.PrintMessage("Installer script is: {0}", output);
                        await write.WriteAsync(output.Replace("\r\n", "\n"));
                    }
                    write.Close();
                    // if we're on unix, we need to make the script executable!
                    Exec($"chmod +x {batchFilePath}"); // this could probably be made async at some point
                }
            }

            // report
            LogWriter.PrintMessage("Going to execute script at path: {0}", batchFilePath);

            // init the installer helper
            if (isWindows)
            {
                _installerProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = batchFilePath,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                // start the installer process. the batch file will wait for the host app to close before starting.
                LogWriter.PrintMessage("Starting the installer process at {0}", batchFilePath);
                _installerProcess.Start();
            }
            else
            {
                // on macOS need to use bash to execute the shell script
                LogWriter.PrintMessage("Starting the installer script process at {0} via shell exec", batchFilePath);
                if (isMacOS)
                {
                    ExecMacOSSudo(batchFilePath, false); // run sudo on macOS
                }
                else
                {
                    Exec(batchFilePath, false);
                }
            }
            await QuitApplication();
        }

        private void ExecMacOSSudo(string cmd, bool waitForExit = true)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            LogWriter.PrintMessage("{0}", $"-e 'do shell script \"{cmd}\" with administrator privileges'");
            _installerProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "osascript",
                    Arguments = $"-e \"do shell script \\\"{escapedArgs}\\\" with administrator privileges\""
                }
            };
            _installerProcess.Start();
            if (waitForExit)
            {
                LogWriter.PrintMessage("Waiting for exit...");
                _installerProcess.WaitForExit();
            }
        }
    }
}