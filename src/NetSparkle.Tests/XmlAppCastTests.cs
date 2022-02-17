using Xunit;

namespace NetSparkleUnitTests
{
    [Collection(XmlAppCastFixture.CollectionName)]
    public class XmlAppCastTests
    {
        private XmlAppCastFixture _fixture;

        public XmlAppCastTests(XmlAppCastFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CanFilterOutBetaItems()
        {
            // using an XMLAppCast, use a filter to remove a certain item and check that its gone 
            
        }
        
    }
}