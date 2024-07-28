using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Interfaces;

namespace NetSparkle.Tests.AppCastGenerator
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