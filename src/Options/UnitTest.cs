using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MAUI_AI_Assistant.Options
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class UnitTestOptions : BaseOptionPage<UnitTest> { }
    }

    public class UnitTest : BaseOptionModel<UnitTest>
    {
        [Category("UnitTest")]
        [DisplayName("Framework")]
        [Description("Indicates the framework used when generating unit tests.")]
        [DefaultValue(UnitTestFramework.nUnit)]
        [TypeConverter(typeof(EnumConverter))]
        public UnitTestFramework Framework { get; set; } = UnitTestFramework.nUnit;
    }
}
