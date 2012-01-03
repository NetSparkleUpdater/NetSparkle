using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using AppLimit.NetSparkle.Interfaces;

namespace AppLimit.NetSparkle
{
    public class NetSparkleAssemblyDiagnosticsAccessor : INetSparkleAssemblyAccessor
    {
        private string fileVersion;
        private string productVersion;
        private string productName;
        private string companyName;
        private string legalCopyright;
        private string fileDescription;

        public NetSparkleAssemblyDiagnosticsAccessor(String assemblyName)
        {
            if (assemblyName != null)
            {
                fileVersion = FileVersionInfo.GetVersionInfo(assemblyName).FileVersion;
                productVersion = FileVersionInfo.GetVersionInfo(assemblyName).ProductVersion;
                productName = FileVersionInfo.GetVersionInfo(assemblyName).ProductName;
                companyName = FileVersionInfo.GetVersionInfo(assemblyName).CompanyName;
                legalCopyright = FileVersionInfo.GetVersionInfo(assemblyName).LegalCopyright;
                fileDescription = FileVersionInfo.GetVersionInfo(assemblyName).FileDescription; 
            }
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                return productName;                
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return fileVersion;
            }
        }        

        public string AssemblyDescription
        {
            get { return fileDescription; }
        }

        public string AssemblyProduct
        {
            get
            {
                return productVersion;                                
            }
        }

        public string AssemblyCopyright
        {
            get { return legalCopyright; }
        }

        public string AssemblyCompany
        {
            get
            {
                return companyName;                  
            }
        }
        #endregion
    }
}