using Azure;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using Azure.AI.OpenAI;
using MAUI_AI_Assistant.Options;
using Microsoft.VisualStudio.Shell.Interop;
using OpenAI.Chat;
using System.ClientModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenAI.Images;

namespace MAUI_AI_Assistant
{
    public class DesignToCodeViewModel : ObservableObject
    {
        // TODO: Enable this option in Options.
        const bool GenerateImages = true;

        const string INSTRUCTIONS = @"
        You are an expert developer specialized in implementing .NET MAUI apps using XAML.
        I will provide you with an image of a reference design and some instructions and it will be your job to implement the corresponding app using .NET MAUI and XAML.
        - Pay close attention to background color, text color, font size, font family, padding, margin, border, gradients, etc. in the design. Match the colors and sizes exactly.
        - If it contains text, use the exact text in the design.
        - Repeat elements as needed to match the screenshot. For example, if there are 10 items, the code should have 10 items. 
        - DO NOT LEAVE comments like 'Repeat for each item'.
        - For images, use placeholder images from https://placehold.co with 100x100 size and include a detailed description of the image in a `description` query parameter so that an image generation AI can generate the image later (e.g. https://placehold.co/100x100?description=An%20image%20of%20a%20cat). Add as many details as possible to the description.
        
        Try your best to figure out what the designer and product owner want and make it happen. If there are any questions or underspecified features, use what you know about applications, user experience, and app design patterns to 'fill in the blanks'. 
        If you're unsure of how the designs should work, take a guess—it's better for you to get it wrong than to leave things incomplete.

        Technical details:
        - Use .NET MAUI XAML.
        - Use only official .NET MAUI packages unless otherwise specified.
        - RETURN ONLY THE CODE FOR THE `MainPage.xaml` FILE.
        - Generate the CollectionView ItemsSource data in XAML using x:Array.
        - Don't include any explanations or comments.

        Remember: you love your designers and POs and want them to be happy. The more complete and impressive your app, the happier they will be. Let's think step by step. Good luck, you've got this!.";

        const string QUESTION = "Here are the latest designs. Implement a new .NET MAUI app based on these designs and instructions."; // Set your question here
        
        bool _isBusy; 
        string _preview;
        string _result;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public string Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }

        public string Result
        {
            get { return _result; }
            set { SetProperty(ref _result, value); }
        }

        public ICommand PickImageCommand => new DelegateCommand(async () => await PickImageAsync());

        async Task PickImageAsync()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    DefaultExt = ".png",
                    Filter = "Image files (.png)|*.png"
                };

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    IsBusy = true;

                    string imagePath = openFileDialog.FileName;

                    Preview = imagePath;

                    Result = await GenerateCodeFromScreenshotAsync(imagePath);

                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = "XAML files (*.xaml)|*.xaml",
                        FilterIndex = 2
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        File.WriteAllText(saveFileDialog.FileName, Result);
                        await VS.MessageBox.ShowAsync("Generated code saved successfully.", buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    }
                }
            }
            catch(Exception ex)
            {
                await VS.MessageBox.ShowAsync(ex.Message, buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        async Task<string> GenerateCodeFromScreenshotAsync(string imagePath)
        {
            try
            {
                var generalOptions = await General.GetLiveInstanceAsync();

                string endpoint = generalOptions.ApiEndpoint;
                string key = generalOptions.ApiKey;
                string model = generalOptions.CompletionsModel; // Example: gpt4 vision-preview

                var azureClient = new AzureOpenAIClient(
                    new Uri(endpoint),
                    new AzureKeyCredential(key));

                var chatClient = azureClient.GetChatClient(model);

                using Stream imageStream = File.OpenRead(imagePath);
                BinaryData imageBytes = BinaryData.FromStream(imageStream);

                List<ChatMessage> messages = new List<ChatMessage>()
                {
                    new SystemChatMessage(INSTRUCTIONS),
                     new UserChatMessage(
                        ChatMessageContentPart.CreateTextMessageContentPart(QUESTION),
                        ChatMessageContentPart.CreateImageMessageContentPart(imageBytes, "image/png"))
                };

                ChatCompletionOptions chatCompletionsOptions = new ChatCompletionOptions
                {
                    Temperature = 0.7f,
                    MaxTokens = 2048,
                    FrequencyPenalty = 0,
                    PresencePenalty = 0
                };

                ClientResult<ChatCompletion> result = await chatClient.CompleteChatAsync(messages, chatCompletionsOptions);

                if (result is not null)
                {
                    var xaml = result.Value.Content[0].Text;

                    if (GenerateImages)
                    {
                        var images = ParseHelper.GetImages(xaml);
                        xaml = await UpdateCodeImagesAsync(xaml, images);
                    }

                    return SanitizeResult(xaml);
                }

                return string.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        async Task<string> UpdateCodeImagesAsync(string xaml, List<string> images)
        {
            var generalOptions = await General.GetLiveInstanceAsync();

            string endpoint = generalOptions.ApiEndpoint;
            string key = generalOptions.ApiKey;
            string imagesModel = generalOptions.ImagesModel; // Example: Dall-E 3

            var azureClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));

            var imageGenerationOptions = new ImageGenerationOptions
            {
                Quality = GeneratedImageQuality.Standard
            };

            var imageClient = azureClient.GetImageClient(imagesModel);

            Dictionary<string, string> mappedImages = new Dictionary<string, string>();
            int counter = 1;

            foreach (var image in images)
            {
                var prompt = ParseHelper.GetImageDescription(image);
                var clientResult = await imageClient.GenerateImageAsync(prompt, imageGenerationOptions);
                var generatedImage = clientResult.Value.ImageUri.AbsoluteUri;
                mappedImages.Add(image, generatedImage);
                counter++;
            }

            var result = ParseHelper.UpdateImages(xaml, mappedImages);

            return result;
        }

        string SanitizeResult(string response)
        {
            var regex = new Regex(@"```.*\r?\n?");
            var result = regex.Replace(response, "");
            result = result.Replace("&", "&amp;");

            return result;
        }
    }
}