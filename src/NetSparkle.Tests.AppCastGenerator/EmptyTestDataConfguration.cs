using NetSparkleUpdater.Configurations;
using NetSparkleUpdater.Interfaces;

namespace NetSparkle.Tests.AppCastGenerator
{
    public class EmptyTestDataConfguration : Configuration
    {
        public EmptyTestDataConfguration(IAssemblyAccessor accessor) : base(accessor) 
        {
        }

        public override void Reload()
        {
        }
    }
}