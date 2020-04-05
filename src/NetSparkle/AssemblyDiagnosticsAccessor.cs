using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using NetSparkle.Interfaces;

namespace NetSparkle
{
    /// <summary>
    /// A diagnostic accessor
    /// </summary>
    public class AssemblyDiagnosticsAccessor : IAssemblyAccessor
    {
        private string fileVersion;
        private string productVersion;
        private string productName;
        private string companyName;
        private string legalCopyright;
        private string fileDescription;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">the assembly name</param>
        public AssemblyDiagnosticsAccessor(string assemblyName)
        {
            if (assemblyName != null)
            {
                string absolutePath = Path.GetFullPath(assemblyName);
                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException();
                }
                var info = FileVersionInfo.GetVersionInfo(assemblyName);
                fileVersion = info.FileVersion;
                productVersion = info.ProductVersion;
                productName = info.ProductName;
                companyName = info.CompanyName;
                legalCopyright = info.LegalCopyright;
                fileDescription = info.FileDescription;
            }
        }

        #region Assembly Attribute Accessors

        /// <summary>
        /// Gets the Title
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                return productName ?? "";                
            }
        }

        /// <summary>
        /// Gets the version
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return fileVersion;
            }
        }        

        /// <summary>
        /// Gets the description
        /// </summary>
        public string AssemblyDescription
        {
            get { return fileDescription; }
        }

        /// <summary>
        /// gets the product
        /// </summary>
        public string AssemblyProduct
        {
            get
            {
                return productName;                                
            }
        }

        /// <summary>
        /// Gets the copyright
        /// </summary>
        public string AssemblyCopyright
        {
            get { return legalCopyright; }
        }

        /// <summary>
        /// Gets the company
        /// </summary>
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