using System;
using Foundation;

namespace NetSparkle.Xamarin
{
    internal class Data
    {
        private NSUserDefaults _storage { get; set; }

        public Data()
        {
            _storage = NSUserDefaults.StandardUserDefaults;
        }

        private string ReadValue(string key) => NSBundle.MainBundle.ObjectForInfoDictionary(key).ToString();

        public string ApplicationName =>  ReadValue("CFBundleName");
        public string InstalledVersion => ReadValue("CFBundleShortVersionString");

        public bool DidRunOnce
        {
            get
            {
                return _storage.BoolForKey("DidRunOnce");
            }

            set
            {
                _storage.SetBool(value, "DidRunOnce");
            }
        }

        public string LastVersionSkipped
        {
            get
            {
                return _storage.StringForKey("LastVersionSkipped");
            }

            set
            {
                _storage.SetString(value, "LastVersionSkipped");
            }
        }

        public string PreviousVersionOfSoftwareRan
        {
            get
            {
                return _storage.StringForKey("PreviousVersionOfSoftwareRan");
            }

            set
            {
                _storage.SetString(value, "PreviousVersionOfSoftwareRan");
            }
        }

        public DateTime LastCheckTime
        {
            // Xamarin.mac does not suport SetObject, so we can't use (NSDate)DateTime
            get
            {
                var lastCheckTime = _storage.StringForKey("LastCheckTime");
                return DateTime.Parse(lastCheckTime);
            }

            set
            {
                _storage.SetString("LastCheckTime", value.ToString());
            }
        }

        public DateTime LastConfigUpdate
        {
            // Xamarin.mac does not suport SetObject, so we can't use (NSDate)DateTime
            get
            {
                var lastCheckTime = _storage.StringForKey("LastConfigUpdate");
                return DateTime.Parse(lastCheckTime);
            }

            set
            {
                _storage.SetString("LastConfigUpdate", value.ToString());
            }
        }
    }
}
