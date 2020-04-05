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
    /// Assembly reflection accessor
    /// </summary>
    public class AssemblyReflectionAccessor : IAssemblyAccessor
    {
        private Assembly _assembly;
        private List<Attribute> _assemblyAttributes = new List<Attribute>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">the assembly name</param>
        public AssemblyReflectionAccessor(string assemblyName)
        {
            if (assemblyName == null)
                _assembly = Assembly.GetEntryAssembly();
            else
            {
                string absolutePath = Path.GetFullPath(assemblyName);
                if (!File.Exists(absolutePath))
                    throw new FileNotFoundException();

                _assembly = Assembly.ReflectionOnlyLoadFrom(absolutePath);

                if (_assembly == null)
                    throw new ArgumentNullException("Unable to load assembly " + absolutePath);                
            }

            // read the attributes            
            foreach (CustomAttributeData data in _assembly.GetCustomAttributesData())
            {
                Attribute a = CreateAttribute(data);
                if (a != null)
                {
                    _assemblyAttributes.Add(a);
                }
            }

            if (_assemblyAttributes == null || _assemblyAttributes.Count == 0)
                throw new ArgumentOutOfRangeException("Unable to load assembly attributes from " + _assembly.FullName);                                    
        }

        /// <summary>
        /// This methods creates an attribute instance from the attribute data 
        /// information
        /// </summary>
        private Attribute CreateAttribute(CustomAttributeData data)
        {
            try
            {
                var arguments = from arg in data.ConstructorArguments
                                select arg.Value;

                var attribute = data.Constructor.Invoke(arguments.ToArray())
                  as Attribute;

                foreach (var namedArgument in data.NamedArguments)
                {
                    var propertyInfo = namedArgument.MemberInfo as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(attribute, namedArgument.TypedValue.Value, null);
                    }
                    else
                    {
                        var fieldInfo = namedArgument.MemberInfo as FieldInfo;
                        if (fieldInfo != null)
                        {
                            fieldInfo.SetValue(attribute, namedArgument.TypedValue.Value);
                        }
                    }
                }

                return attribute;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Attribute FindAttribute(Type AttributeType)
        {            
            foreach (Attribute attr in _assemblyAttributes)
            {
                if (attr.GetType().Equals(AttributeType))
                    return attr;                                
            }

            throw new Exception("Attribute of type " + AttributeType.ToString() + " does not exists in the assembly " + _assembly.FullName);
        }

        #region Assembly Attribute Accessors

        /// <summary>
        /// Gets the assembly title
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                AssemblyTitleAttribute a = FindAttribute(typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
                return a.Title;                
            }
        }

        /// <summary>
        /// Gets the version
        /// </summary>
        public string AssemblyVersion
        {
            get
            {
                return _assembly.GetName().Version.ToString();
            }
        }        

        /// <summary>
        /// Gets the description
        /// </summary>
        public string AssemblyDescription
        {
            get
            {
                AssemblyDescriptionAttribute a = FindAttribute(typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
                return a.Description;                                
            }
        }

        /// <summary>
        /// Gets the product
        /// </summary>
        public string AssemblyProduct
        {
            get
            {
                AssemblyProductAttribute a = FindAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
                return a.Product;                                
            }
        }

        /// <summary>
        /// Gets the copyright
        /// </summary>
        public string AssemblyCopyright
        {
            get
            {
                AssemblyCopyrightAttribute a = FindAttribute(typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
                return a.Copyright;                                                
            }
        }

        /// <summary>
        /// Gets the company
        /// </summary>
        public string AssemblyCompany
        {
            get
            {
                AssemblyCompanyAttribute a = FindAttribute(typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
                return a.Company;                  
            }
        }
        #endregion
    }
}
