using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using NetSparkle.Interfaces;

namespace NetSparkle.AssemblyAccessors
{
    /// <summary>
    /// An assembly accessor
    /// </summary>
    public class AssemblyAccessor : IAssemblyAccessor
    {
        IAssemblyAccessor _internalAccessor = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">the assembly name</param>
        /// <param name="isReflectionAccesorUsed"><c>true</c> if reflection is used to access the attributes.</param>
        public AssemblyAccessor(string assemblyName, bool isReflectionAccesorUsed)
        {
            if ( isReflectionAccesorUsed )
                _internalAccessor = new AssemblyReflectionAccessor(assemblyName);
            else
                _internalAccessor = new AssemblyDiagnosticsAccessor(assemblyName);
        }

        #region IAssemblyAccessor Members

        /// <summary>
        /// Gets the company
        /// </summary>
        public string AssemblyCompany
        {
            get { return _internalAccessor.AssemblyCompany; }
        }

        /// <summary>
        /// Gets the copyright
        /// </summary>
        public string AssemblyCopyright
        {
            get { return _internalAccessor.AssemblyCopyright; }
        }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string AssemblyDescription
        {
            get { return _internalAccessor.AssemblyDescription; }
        }

        /// <summary>
        /// Gets the product
        /// </summary>
        public string AssemblyProduct
        {
            get { return _internalAccessor.AssemblyProduct; }
        }

        /// <summary>
        /// Gets the title
        /// </summary>
        public string AssemblyTitle
        {
            get { return _internalAccessor.AssemblyTitle; }
        }

        /// <summary>
        /// Gets the version
        /// </summary>
        public string AssemblyVersion
        {
            get { return _internalAccessor.AssemblyVersion; }
        }

        #endregion
    }
}
