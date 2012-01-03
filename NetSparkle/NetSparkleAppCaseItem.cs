using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppLimit.NetSparkle
{
    public class NetSparkleAppCastItem : IComparable<NetSparkleAppCastItem>
    {
        public String AppName;
        public String AppVersionInstalled;

        public String Version;
        public String ReleaseNotesLink;
        public String DownloadLink;

        public String DSASignature;

        #region IComparable<NetSparkleAppCastItem> Members

        public int CompareTo(NetSparkleAppCastItem other)
        {
            Version v1 = new Version(this.Version);
            Version v2 = new Version(other.Version);

            return v1.CompareTo(v2);            
        }

        #endregion
    }    
}
