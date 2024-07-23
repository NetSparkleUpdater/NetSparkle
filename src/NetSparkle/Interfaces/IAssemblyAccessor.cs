#nullable enable

namespace NetSparkleUpdater.Interfaces
{
    /// <summary>
    /// An assembly accessor grabs details on the current version
    /// and publisher information for a C# application (presumably
    /// the one that is currently running). This is intended to be
    /// used in conjunction with a Configuration subclass in order 
    /// to retrieve and store info on the latest version of the app
    /// that was used, whether the app has run 1x or not, etc.
    /// These fields may or may not be required based on the specific 
    /// Configuration that you use. At the very least, make sure
    /// your IAssemblyAccessor can grab valid info for the title of the 
    /// application, the product name (which may or may not be the same
    /// as the title), and the version of the application (e.g. 3.1.2.0).
    /// </summary>
    public interface IAssemblyAccessor
    {
        /// <summary>
        /// The publisher of the application.
        /// From &lt; Company &gt; tag in .csproj file
        /// </summary>
        string AssemblyCompany { get; }
        /// <summary>
        /// The copyright for the application (e.g. © 2020)
        /// From &lt; Copyright &gt; tag in .csproj file
        /// </summary>
        string AssemblyCopyright { get; }
        /// <summary>
        /// Description of the assembly.
        /// From &lt; Description &gt; tag in .csproj file
        /// </summary>
        string AssemblyDescription { get; }
        /// <summary>
        /// Title of the assembly, e.g. "My Best Product"
        /// From &lt; AssemblyTitle &gt; tag in .csproj file.
        /// NOTE: AssemblyDiagnosticsAccessor returns AssemblyProduct here!
        /// </summary>
        string AssemblyTitle { get; }
        /// <summary>
        /// Product for the assembly. Might be the same as
        /// the title, but also could be more specific than the
        /// title.
        /// From &lt; Product &gt; tag in .csproj file
        /// </summary>
        string AssemblyProduct { get; }
        /// <summary>
        /// Version of the item that's running. E.g. 3.1.2.1 or 3.1.2.1-beta1.
        /// From &lt; Version &gt; tag in .csproj file
        /// </summary>
        string AssemblyVersion { get; }
    }
}
