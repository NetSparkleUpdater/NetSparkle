using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private IList<CustomAttribute> _assemblyAttributes = null;

        /// <summary>
        /// Load the assembly with a given assembly name's attributes. All pertinent attributes
        /// that are needed are read during object construction.
        /// </summary>
        /// <param name="assemblyName">the assembly name. 
        /// If null is passed, <seealso cref="Assembly.GetEntryAssembly"/> is used to get info on the assembly.</param>
        /// <exception cref="BadImageFormatException">Thrown when the assembly cannot be read</exception>
        /// <exception cref="FileNotFoundException">Thrown when the path to the assembly with the given name doesn't exist</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the assembly doesn't have any readable attributes</exception>
        public AsmResolverAccessor(string assemblyName)
        {
            var path = "";
            if (assemblyName != null)
            {
                path = Path.GetFullPath(assemblyName);
            }
            else
            {
                path =  Assembly.GetEntryAssembly().Location;
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

        private string FindAttributeData(IList<CustomAttribute> attributes, System.Type type)
        {
            var attr = attributes.FirstOrDefault(c => c.Constructor?.DeclaringType?.FullName == type.ToString());
            if (attr != null) {

                var utf8Value = (Utf8String)attr.Signature.FixedArguments[0].Element;
                return (string)utf8Value;
            }
            return null;
        }


        /// <inheritdoc/>
        public string AssemblyTitle
        {
            get
            {
                return FindAttributeData(_assemblyAttributes, typeof(AssemblyTitleAttribute)) ?? "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyVersion
        {
            get
            {
                return FindAttributeData(_assemblyAttributes, typeof(AssemblyInformationalVersionAttribute)) ?? "";
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
                return FindAttributeData(_assemblyAttributes, typeof(AssemblyFileVersionAttribute)) ?? "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyDescription
        {
            get
            {
                return FindAttributeData(_assemblyAttributes, typeof(AssemblyDescriptionAttribute)) ?? "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyProduct
        {
            get
            {
                return FindAttributeData(_assemblyAttributes, typeof(AssemblyProductAttribute)) ?? "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyCopyright
        {
            get
            {
                return FindAttributeData(_assemblyAttributes, typeof(AssemblyCopyrightAttribute)) ?? "";
            }
        }

        /// <inheritdoc/>
        public string AssemblyCompany
        {
            get
            {
                return FindAttributeData(_assemblyAttributes, typeof(AssemblyCompanyAttribute)) ?? "";
            }
        }
    }
}
