using System;
using System.Diagnostics;
using System.IO;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.AssemblyAccessors
{
    /// <summary>
    /// An assembly accessor that uses <seealso cref="System.Diagnostics.FileVersionInfo"/>
    /// to get information on the assembly with a given name.
    /// AssemblyTitle is the same as AssemblyProduct with this assembly accessor.
    /// </summary>
    public class AssemblyDiagnosticsAccessor : IAssemblyAccessor
    {
        private string? _fileVersion;
        private string? _productVersion;
        private string? _productName;
        private string? _companyName;
        private string? _legalCopyright;
        private string? _fileDescription;

        /// <summary>
        /// Create a new diagnostics accessor and parse the assembly's information
        /// using <seealso cref="System.Diagnostics.FileVersionInfo"/>.
        /// <seealso cref="Path.GetFullPath(string)"/> is used to get the full file
        /// path of the given assembly.
        /// </summary>
        /// <param name="assemblyPath">the path to the assembly to read/access</param>
        /// <exception cref="FileNotFoundException">Thrown when the path to the assembly with the given name doesn't exist or null is sent to the constructor</exception>
        public AssemblyDiagnosticsAccessor(string? assemblyPath)
        {
            if (assemblyPath == null)
            {
                // https://github.com/dotnet/corert/issues/6947#issuecomment-460204830
                assemblyPath = Path.Combine(System.AppContext.BaseDirectory, Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            }
            string absolutePath = Path.GetFullPath(assemblyPath);
            if (!File.Exists(absolutePath))
            {
                throw new FileNotFoundException();
            }
            FileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);
            _fileVersion = FileVersionInfo.FileVersion;
            _productVersion = FileVersionInfo.ProductVersion;
            _productName = FileVersionInfo.ProductName;
            _companyName = FileVersionInfo.CompanyName;
            _legalCopyright = FileVersionInfo.LegalCopyright;
            _fileDescription = FileVersionInfo.Comments;
        }

        #region Assembly Attribute Accessors

        /// <summary>
        /// <seealso cref="FileVersionInfo"/> for this assembly accessor in case there's something
        /// you want to look at outside of the other shortcut properties that are the same
        /// as other <seealso cref="IAssemblyAccessor"/> objects
        /// </summary>
        public FileVersionInfo FileVersionInfo { get; protected set; }

        /// <summary>
        /// Title of the assembly, e.g. "My Best Product". 
        /// NOTE: Returns ProductName data, not Title data, due to limitations in Diagnostics accessor!
        /// </summary>
        public string AssemblyTitle
        {
            get => AssemblyProduct;
        }

        /// <inheritdoc/>
        public string AssemblyVersion
        {
             get => _productVersion ?? "";
        }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string AssemblyDescription
        {
             get => _fileDescription ?? "";
        }

        /// <inheritdoc/>
        public string AssemblyProduct
        {
             get => _productName ?? "";
        }

        /// <inheritdoc/>
        public string AssemblyCopyright
        {
             get => _legalCopyright ?? "";
        }

        /// <inheritdoc/>
        public string AssemblyCompany
        {
             get => _companyName ?? "";
        }

        #endregion
    }
}