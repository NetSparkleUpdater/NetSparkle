using System;
using Foundation;

namespace NetSparkle.Xamarin
{
    public class PListConfiguration
    {
        public PListConfiguration()
        {
        }

        public string AssemblyVersion => GetInfoPlistValue("CFBundleShortVersionString");

        private string GetInfoPlistValue(string identifier)
        {
            return NSBundle.MainBundle.ObjectForInfoDictionary(identifier).ToString();
        }
    }
}
