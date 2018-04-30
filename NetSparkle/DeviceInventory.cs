using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Management;
using System.Threading;

namespace NetSparkle
{
    internal class DeviceInventory
    {
        public bool x64System { get; set; }
        public uint ProcessorSpeed { get; set; }
        public long MemorySize { get; set; }
        public string OsVersion { get; set; }
        public int CPUCount { get; set; }

        private Configuration _config;

        public DeviceInventory(Configuration config)
        {
            _config = config;
        }

        public void CollectInventory()
        {
            // x64
            CollectProcessorBitnes();

            // cpu speed
            CollectCPUSpeed();

            // cpu count
            CollectCPUCount();

            // ram size
            CollectRamSize();

            // windows
            CollectWindowsVersion();
        }

        public string BuildRequestUrl(string baseRequestUrl)
        {
            string retValue = baseRequestUrl;
            
            // x64 
            retValue += "cpu64bit=" + (x64System ? "1" : "0") + "&";

            // cpu speed
            retValue += "cpuFreqMHz=" + ProcessorSpeed + "&";

            // ram size
            retValue += "ramMB=" + MemorySize + "&";

            // Application name (as indicated by CFBundleName)
            retValue += "appName=" + _config.ApplicationName + "&";

            // Application version (as indicated by CFBundleVersion)
            retValue += "appVersion=" + _config.InstalledVersion + "&";

            // User’s preferred language
            retValue += "lang=" + Thread.CurrentThread.CurrentUICulture.ToString() + "&";

            // Windows version
            retValue += "osVersion=" + OsVersion + "&";

            // CPU type/subtype (see mach/machine.h for decoder information on this data)
            // ### TODO: cputype, cpusubtype ###

            // Mac model
            // ### TODO: model ###

            // Number of CPUs (or CPU cores, in the case of something like a Core Duo)
            // ### TODO: ncpu ###
            retValue += "ncpu=" + CPUCount + "&";

            // sanitize url
            retValue = retValue.TrimEnd('&');            

            // go ahead
            return retValue;
        }

        private void CollectWindowsVersion()
        {
            OperatingSystem osInfo = Environment.OSVersion;
            OsVersion = string.Format("{0}.{1}.{2}.{3}", osInfo.Version.Major, osInfo.Version.Minor, osInfo.Version.Build, osInfo.Version.Revision);
        }

        private void CollectProcessorBitnes()
        {
             if (Marshal.SizeOf(typeof(IntPtr)) == 8)
                x64System = true;
            else
                x64System = false;
        }

        private void CollectCPUCount()
        {
            CPUCount = Environment.ProcessorCount;
        }

        private void CollectCPUSpeed()
        {
            ManagementObject Mo = new ManagementObject("Win32_Processor.DeviceID='CPU0'");
            ProcessorSpeed = (uint)(Mo["CurrentClockSpeed"]);
            Mo.Dispose();            
        }

        private void CollectRamSize()
        {
            MemorySize = 0;

            // RAM size
            ManagementScope oMs = new ManagementScope();
            ObjectQuery oQuery = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);
            ManagementObjectCollection oCollection = oSearcher.Get();
            
            Int64 mCap = 0;

            // In case more than one Memory sticks are installed
            foreach (ManagementObject mobj in oCollection)
            {
                mCap = Convert.ToInt64(mobj["Capacity"]);
                MemorySize += mCap;
            }

            MemorySize = (MemorySize / 1024) / 1024;
        }
    }
}
