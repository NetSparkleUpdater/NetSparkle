using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Interfaces;

namespace NetSparkleUnitTests
{
    public class EmptyTestDataConfiguration : Configuration
    {
        public EmptyTestDataConfiguration(IAssemblyAccessor accessor) : base(accessor) 
        {
        }

        public override void Reload()
        {
        }
    }
}