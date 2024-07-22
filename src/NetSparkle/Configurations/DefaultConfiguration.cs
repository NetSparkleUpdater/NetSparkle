#nullable enable

using NetSparkleUpdater.Interfaces;

namespace NetSparkleUpdater.Configurations
{
    /// <summary>
    /// A default configuration that really does nothing. 
    /// Only used if unable to create another <see cref="Configuration"/>.
    /// </summary>    
    public class DefaultConfiguration : Configuration
    {
        /// <summary>
        /// Constructor that just sends on the <see cref="IAssemblyAccessor"/> to the parent.
        /// </summary>
        /// <param name="accessor">IAssemblyAccessor for this <see cref="Configuration"/></param>
        public DefaultConfiguration(IAssemblyAccessor accessor) : base(accessor) 
        {
        }

        /// <inheritdoc/>
        public override void Reload()
        {
        }
    }
}