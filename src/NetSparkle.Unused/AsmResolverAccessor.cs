#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using NetSparkleUpdater.Interfaces;
using AsmResolver;
using AsmResolver.DotNet;

namespace NetSparkleUpdater.AssemblyAccessors
{
    /// <summary>
    /// An assembly accessor that uses the AsmResolver library to learn information
    /// on an assembly with a given name.
    /// </summary>
    public class AsmResolverAccessor : IAssemblyAccessor
    {
        private IList<CustomAttribute>? _assemblyAttributes;

        /// <summary>
        /// Load the assembly with a given assembly name's attributes. All pertinent attributes
        /// that are needed are read during object construction.
        /// </summary>
        /// <param name="assemblyName">The path to the assembly to read</param>
        /// <exception cref="BadImageFormatException">Thrown when the assembly cannot be read</exception>
        /// <exception cref="FileNotFoundException">Thrown when the path to the assembly with the given name doesn't exist</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the assembly doesn't have any readable attributes</exception>
        public AsmResolverAccessor(string? assemblyName)
        {
            var path = "";
            if (assemblyName != null)
            {
                path = Path.GetFullPath(assemblyName);
            }
            else
            {
                // https://github.com/dotnet/corert/issues/6947#issuecomment-460204830
                path = Path.Combine(System.AppContext.BaseDirectory, Path.GetFileName(Environment.GetCommandLineArgs()[0]));
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }
            
            var module = ModuleDefinition.FromFile(path);
            _assemblyAttributes = module.Assembly?.CustomAttributes;
            if (_assemblyAttributes == null || _assemblyAttributes.Count == 0)
            {
                throw new ArgumentOutOfRangeException("Unable to load assembly attributes from " + path);
            }
        }

        private string? FindAttributeData(IList<CustomAttribute> attributes, System.Type type)
        {
            var attr = attributes.FirstOrDefault(c => c.Constructor?.DeclaringType?.FullName == type.ToString());
            if (attr != null) 
            {
                if (attr.Signature?.FixedArguments.Count > 0)
                {
                    return attr.Signature?.FixedArguments[0].Element as Utf8String;
                }
            }
            return null;
        }


        /// <inheritdoc/>
        public string AssemblyTitle
        {
            get
            {
                return _assemblyAttributes != null 
                    ? FindAttributeData(_assemblyAttributes, typeof(AssemblyTitleAttribute)) ?? "" 
                    : "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyVersion
        {
            get
            {
                return _assemblyAttributes != null 
                    ? FindAttributeData(_assemblyAttributes, typeof(AssemblyInformationalVersionAttribute)) ?? FileVersion
                    : "";
            }
        }

        /// <summary>
        /// Version of assembly that does not include more than Major.Minor.Revision info
        /// (aka does not include things like the alpha portion of a version like 1.0.1-alpha1)
        /// </summary>
        public string FileVersion
        {
            get
            {
                return _assemblyAttributes != null 
                    ? FindAttributeData(_assemblyAttributes, typeof(AssemblyFileVersionAttribute)) ?? "" 
                    : "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyDescription
        {
            get
            {
                return _assemblyAttributes != null 
                    ? FindAttributeData(_assemblyAttributes, typeof(AssemblyDescriptionAttribute)) ?? "" 
                    : "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyProduct
        {
            get
            {
                return _assemblyAttributes != null 
                    ? FindAttributeData(_assemblyAttributes, typeof(AssemblyProductAttribute)) ?? "" 
                    : "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyCopyright
        {
            get
            {
                return _assemblyAttributes != null 
                    ? FindAttributeData(_assemblyAttributes, typeof(AssemblyCopyrightAttribute)) ?? "" 
                    : "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyCompany
        {
            get
            {
                return _assemblyAttributes != null 
                    ? FindAttributeData(_assemblyAttributes, typeof(AssemblyCompanyAttribute)) ?? "" 
                    : "";
            }
        }
    }
}
