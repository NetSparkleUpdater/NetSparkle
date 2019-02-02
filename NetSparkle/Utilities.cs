using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSparkle
{
    public class Utilities
    {
        public static string GetVersionString(Version version)
        {
            if (version.Revision != 0)
                return version.ToString();
            if (version.Build != 0)
                return version.ToString(3);
            return version.ToString(2);
        }
    }
}
