using NetSparkle.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetSparkle.Interfaces
{
    public interface IAppCastHandler
    {
        void SetupAppCastHandler(IAppCastDataDownloader dataDownloader, string castUrl, Configuration config, DSAChecker dsaChecker, LogWriter logWriter = null);
        bool DownloadAndParse();
        List<AppCastItem> GetNeededUpdates();
    }
}
