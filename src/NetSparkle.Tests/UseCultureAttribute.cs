using System.Globalization;
using System.Reflection;
using Xunit.Sdk;

namespace NetSparkleUnitTests
{
    public sealed class UseCultureAttribute : BeforeAfterTestAttribute
    {
        private CultureInfo _originalCulture;
        private CultureInfo _originalUICulture;

        private readonly CultureInfo _culture;
        private readonly CultureInfo _uiCulture;

        public UseCultureAttribute(string culture)
        {
            _culture = new CultureInfo(culture);
            _uiCulture = new CultureInfo(culture);
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUICulture = CultureInfo.CurrentUICulture;

            CultureInfo.CurrentCulture = _culture;
            CultureInfo.CurrentUICulture = _uiCulture;
        }

        public override void After(MethodInfo methodUnderTest)
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUICulture;
        }
    }
}
