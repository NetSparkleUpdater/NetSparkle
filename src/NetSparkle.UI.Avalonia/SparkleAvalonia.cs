using NetSparkle.Enums;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkle.UI.Avalonia
{
    public class SparkleAvalonia : Sparkle
    {

        public SparkleAvalonia(string appcastUrl)
            : this(appcastUrl, SecurityMode.Strict, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        public SparkleAvalonia(string appcastUrl, SecurityMode securityMode)
            : this(appcastUrl, securityMode, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url
        /// </summary>
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        /// <param name="dsaPublicKey">the DSA public key for checking signatures, in XML Signature (&lt;DSAKeyValue&gt;) format.
        /// If null, a file named "NetSparkle_DSA.pub" is used instead.</param>
        public SparkleAvalonia(string appcastUrl, SecurityMode securityMode, string dsaPublicKey)
            : this(appcastUrl, securityMode, dsaPublicKey, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        /// <param name="dsaPublicKey">the DSA public key for checking signatures, in XML Signature (&lt;DSAKeyValue&gt;) format.
        /// If null, a file named "NetSparkle_DSA.pub" is used instead.</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison when checking update versions</param>
        public SparkleAvalonia(string appcastUrl, SecurityMode securityMode, string dsaPublicKey, string referenceAssembly)
            : this(appcastUrl, securityMode, dsaPublicKey, referenceAssembly, null)
        { }

        /// <summary>
        /// ctor which needs the appcast url and a referenceassembly
        /// </summary>        
        /// <param name="appcastUrl">the URL of the appcast file</param>
        /// <param name="securityMode">the security mode to be used when checking DSA signatures</param>
        /// <param name="dsaPublicKey">the DSA public key for checking signatures, in XML Signature (&lt;DSAKeyValue&gt;) format.
        /// If null, a file named "NetSparkle_DSA.pub" is used instead.</param>
        /// <param name="referenceAssembly">the name of the assembly to use for comparison when checking update versions</param>
        /// <param name="factory">a UI factory to use in place of the default UI</param>
        public SparkleAvalonia(string appcastUrl, SecurityMode securityMode, string dsaPublicKey, string referenceAssembly, IUIFactory factory)
            : base(appcastUrl, securityMode, dsaPublicKey, referenceAssembly, factory) { }


        private bool DoExtensionsMatch(string extension, string otherExtension)
        {
            return extension.Equals(otherExtension, StringComparison.CurrentCultureIgnoreCase);
        }

        protected override string GetInstallerCommand(string downloadFilePath)
        {
            // get the file type
            string installerExt = Path.GetExtension(downloadFilePath);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (DoExtensionsMatch(installerExt, ".exe"))
                {
                    return "\"" + downloadFilePath + "\"";
                }
                if (DoExtensionsMatch(installerExt, ".msi"))
                {
                    return "msiexec /i \"" + downloadFilePath + "\"";
                }
                if (DoExtensionsMatch(installerExt, ".msp"))
                {
                    return "msiexec /p \"" + downloadFilePath + "\"";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (DoExtensionsMatch(installerExt, ".pkg") ||
                    DoExtensionsMatch(installerExt, ".dmg"))
                {
                    return "open \"" + downloadFilePath + "\"";
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (DoExtensionsMatch(installerExt, ".deb"))
                {
                    return "sudo apt install \"" + downloadFilePath + "\"";
                }
                if (DoExtensionsMatch(installerExt, ".rpm"))
                {
                    return "sudo rpm -i \"" + downloadFilePath + "\"";
                }
            }
            return downloadFilePath;
        }

        private bool IsZipDownload(string downloadFilePath)
        {
            string installerExt = Path.GetExtension(downloadFilePath);
            bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if ((isMacOS && DoExtensionsMatch(installerExt, ".zip")) ||
                (isLinux && DoExtensionsMatch(installerExt, ".tar.gz")))
            {
                return true;
            }
            return false;
        }

        protected override async Task RunDownloadedInstaller(string downloadFilePath)
        {
            LogWriter.PrintMessage("Running downloaded installer");
            // get the commandline 
            string cmdLine = Environment.CommandLine;
            string workingDir = Environment.CurrentDirectory;

            // generate the batch file path
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
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
                UIFactory?.ShowUnknownInstallerFormatMessage(downloadFilePath);
                return;
            }

            // generate the batch file                
            LogWriter.PrintMessage("Generating batch in {0}", Path.GetFullPath(batchFilePath));

            string processID = Process.GetCurrentProcess().Id.ToString();

            using (StreamWriter write = new StreamWriter(batchFilePath))
            {
                if (isWindows)
                {
                    write.WriteLine("@echo off");
                    // We should wait until the host process has died before starting the installer.
                    // This way, any DLLs or other items can be replaced properly.
                    // Code from: http://stackoverflow.com/a/22559462/3938401
                    string relaunchAfterUpdate = "";
                    if (RelaunchAfterUpdate)
                    {
                        relaunchAfterUpdate = $@"
                        cd {workingDir}
                        {cmdLine}";
                    }

                    string output = $@"
                        set /A counter=0                       
                        setlocal ENABLEDELAYEDEXPANSION
                        :loop
                        set /A counter=!counter!+1
                        if !counter! == 90 (
                            goto :afterinstall
                        )
                        tasklist | findstr ""\<{processID}\>"" > nul
                        if not errorlevel 1 (
                            timeout /t 1 > nul
                            goto :loop
                        )
                        :install
                        {installerCmd}
                        {relaunchAfterUpdate}
                        :afterinstall
                        endlocal";
                    write.Write(output);
                    write.Close();
                }
                else
                {
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
                    string relaunchAfterUpdate = "";
                    if (RelaunchAfterUpdate)
                    {
                        relaunchAfterUpdate = $@"{Process.GetCurrentProcess().MainModule.FileName}";
                    }
                    if (IsZipDownload(downloadFilePath)) // .zip on macOS or .tar.gz on Linux
                    {
                        // waiting for finish based on http://blog.joncairns.com/2013/03/wait-for-a-unix-process-to-finish/
                        // use tar to extract
                        var tarCommand = isMacOS ? $"tar -x -f {downloadFilePath}" : $"tar -xf {downloadFilePath} --overwrite ";
                        var output = $@"
                            {waitForFinish}
                            cd {workingDir}
                            cd ..
                            {tarCommand}
                            {relaunchAfterUpdate}";
                        write.Write(output);
                    }
                    else
                    {
                        var output = $@"
                            {waitForFinish}
                            {installerCmd}
                            {relaunchAfterUpdate}";
                        write.Write(output);
                    }
                    write.Close();
                }
            }

            // report
            LogWriter.PrintMessage("Going to execute batch: {0}", batchFilePath);

            // init the installer helper
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
            _installerProcess.Start();
            await QuitApplication();
        }
    }
}
