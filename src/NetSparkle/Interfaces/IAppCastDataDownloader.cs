using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetSparkle.Interfaces
{
    public interface IAppCastDataDownloader
    {
        Stream DownloadAndGetContentStream(string url);
    }
}
