using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.AssemblyAccessors
{
    /// <summary>
    /// An assembly accessor that uses <seealso cref="System.Diagnostics.FileVersionInfo"/>
    /// to get information on the assembly with a given name
    /// </summary>
    public class AssemblyDiagnosticsAccessor : IAssemblyAccessor
    {
        private string _fileVersion;
        private string _productVersion;
        private string _productName;
        private string _companyName;
        private string _legalCopyright;
        private string _fileDescription;

        /// <summary>
        /// Create a new diagnostics accessor and parse the assembly's information
        /// using <seealso cref="System.Diagnostics.FileVersionInfo"/>.
        /// <seealso cref="Path.GetFullPath(string)"/> is used to get the full file
        /// path of the given assembly.
        /// </summary>
        /// <param name="assemblyName">the assembly name</param>
        /// <exception cref="FileNotFoundException">Thrown when the path to the assembly with the given name doesn't exist</exception>
        public AssemblyDiagnosticsAccessor(string assemblyName)
        {
            if (assemblyName != null)
            {
                var absolutePath = Path.GetFullPath(assemblyName);
                if (!File.Exists(absolutePath))
                {
                    throw new FileNotFoundException();
                }
                var info = FileVersionInfo.GetVersionInfo(assemblyName);
                _fileVersion = info.FileVersion;
                _productVersion = info.ProductVersion;
                _productName = info.ProductName;
                _companyName = info.CompanyName;
                _legalCopyright = info.LegalCopyright;
                _fileDescription = info.FileDescription;
            }
        }

        #region Assembly Attribute Accessors

        /// <inheritdoc/>
        public string AssemblyTitle
        {
            get => _productName ?? "";
        }

        /// <inheritdoc/>
        public string AssemblyVersion
        {
            get => _fileVersion;
        }        

        /// <summary>
        /// Gets the description
        /// </summary>
        public string AssemblyDescription
        {
            get => _fileDescription;
        }

        /// <inheritdoc/>
        public string AssemblyProduct
        {
            get => _productName;
        }

        /// <inheritdoc/>
        public string AssemblyCopyright
        {
            get => _legalCopyright;
        }

        /// <inheritdoc/>
        public string AssemblyCompany
        {
            get => _companyName;
        }

        #endregion
    }
}