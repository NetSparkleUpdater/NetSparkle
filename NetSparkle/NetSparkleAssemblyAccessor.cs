using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using AppLimit.NetSparkle.Interfaces;

namespace AppLimit.NetSparkle
{
    public class NetSparkleAssemblyAccessor : INetSparkleAssemblyAccessor
    {
        INetSparkleAssemblyAccessor _internalAccessor = null;

        public NetSparkleAssemblyAccessor(String assemblyName, Boolean bUseReflectionAccesor)
        {
            if ( bUseReflectionAccesor )
                _internalAccessor = new NetSparkleAssemblyReflectionAccessor(assemblyName);
            else
                _internalAccessor = new NetSparkleAssemblyDiagnosticsAccessor(assemblyName);
        }

        #region INetSparkleAssemblyAccessor Members

        public string AssemblyCompany
        {
            get { return _internalAccessor.AssemblyCompany; }
        }

        public string AssemblyCopyright
        {
            get { return _internalAccessor.AssemblyCopyright; }
        }

        public string AssemblyDescription
        {
            get { return _internalAccessor.AssemblyDescription; }
        }

        public string AssemblyProduct
        {
            get { return _internalAccessor.AssemblyProduct; }
        }

        public string AssemblyTitle
        {
            get { return _internalAccessor.AssemblyTitle; }
        }

        public string AssemblyVersion
        {
            get { return _internalAccessor.AssemblyVersion; }
        }

        #endregion
    }
}
