using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MAUI_AI_Assistant.Options
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class CustomCommandOptions : BaseOptionPage<CustomCommand> { }
    }

    public class CustomCommand : BaseOptionModel<CustomCommand>
    {
        [Category("CustomCommand")]
        [DisplayName("Prompt")]
        [Description("Add a custom prompt here to use in your custom Command.")]
        public string Prompt { get; set; }

        [Category("CustomCommand")]
        [DisplayName("Action")]
        [Description("Indicates whether to insert text, replace selected text, or display a dialog.")]
        [DefaultValue(CommandBehavior.Insert)]
        [TypeConverter(typeof(EnumConverter))]
        public CommandBehavior Action { get; set; } = CommandBehavior.Insert;
    }
}