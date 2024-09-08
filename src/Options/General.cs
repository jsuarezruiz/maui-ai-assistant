using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MAUI_AI_Assistant.Options
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<General> { }
    }

    public class General : BaseOptionModel<General>
    {
        [Category("General")]
        [DisplayName("API Endpoint")]
        [Description("URL containing the Azure OpenAI API endpoint ({0}=name). This value can be found in the Keys & Endpoint section when examining your resource from the Azure portal. Alternatively, you can find the value in the Azure OpenAI Studio > Playground > Code View.")]
        [DefaultValue("https://{0}.openai.azure.com")]
        public string ApiEndpoint { get; set; } = "https://{0}.openai.azure.com"; 
        
        [Category("General")]
        [DisplayName("API Key")]
        [Description("This value can be found in the Keys & Endpoint section when examining your resource from the Azure portal. You can use either KEY1 or KEY2.")]
        [DefaultValue("")]
        public string ApiKey { get; set; } = "";

        [Category("General")]
        [DisplayName("Chat Model Deployment Name")]
        [Description("Chat Model Name. Azure OpenAI Service is powered by a diverse set of models with different capabilities and price points.")]
        [DefaultValue("")]
        public string ChatModel { get; set; } = "";

        [Category("General")]
        [DisplayName("Completions Model Deployment Name")]
        [Description("Completions Model Name. Azure OpenAI Service is powered by a diverse set of models with different capabilities and price points.")]
        [DefaultValue("")]
        public string CompletionsModel { get; set; } = "";
        
        [Category("General")]
        [DisplayName("Images Model Deployment Name")]
        [Description("Images Model Name. Azure OpenAI Service is powered by a diverse set of models with different capabilities and price points.")]
        [DefaultValue("")]
        public string ImagesModel { get; set; } = "";

        public General() : base()
        {
            Saved += delegate { VS.StatusBar.ShowMessageAsync("Options Saved").FireAndForget(); };
        }
    }
}