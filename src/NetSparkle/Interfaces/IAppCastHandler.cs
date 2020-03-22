using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkle.Interfaces
{
    public interface IAppCastHandler
    {
        void SetupAppCast(IAppCastDataDownloader dataDownloader, string castUrl, Configuration config, DSAChecker dsaChecker, LogWriter logWriter = null);
        bool Read();
        List<AppCastItem> GetUpdates();
    }
}
