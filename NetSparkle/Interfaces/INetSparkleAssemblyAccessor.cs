using System;
namespace NetSparkle.Interfaces
{
    interface INetSparkleAssemblyAccessor
    {
        string AssemblyCompany { get; }
        string AssemblyCopyright { get; }
        string AssemblyDescription { get; }
        string AssemblyProduct { get; }
        string AssemblyTitle { get; }
        string AssemblyVersion { get; }
    }
}
