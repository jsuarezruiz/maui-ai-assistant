using Azure;
using MAUI_AI_Assistant.Options;
using Microsoft.VisualStudio.PlatformUI;
using OpenAI.Images;
using System.Windows.Input;
using Azure.AI.OpenAI;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Microsoft.Win32;

namespace MAUI_AI_Assistant
{
    public class IconBuilderViewModel : ObservableObject
    {
        const int IconsCount = 4;

        bool _isBusy;
        string _prompt;
        ObservableCollection<Icon> _icons;

        public IconBuilderViewModel()
        {
            Icons = new ObservableCollection<Icon>();
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public string Prompt
        {
            get { return _prompt; }
            set { SetProperty(ref _prompt, value); }
        }

        public ObservableCollection<Icon> Icons
        {
            get { return _icons; }
            set { SetProperty(ref _icons, value); }
        }

        public ICommand GenerateCommand => new DelegateCommand(async () => await GenerateAsync());
        public ICommand DownloadCommand => new DispatchedDelegateCommand<string>(async (parameter) => await DownloadAsync(parameter));
       
        async Task GenerateAsync()
        {
            try
            {
                IsBusy = true;

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

                for (int i = 0; i < IconsCount; i++)
                {
                    string prompt = $"Create an icon for a mobile app, {Prompt}";
                    var result = await imageClient.GenerateImagesAsync(prompt, 1, imageGenerationOptions);

                    if (result is not null)
                    {
                        var icon = result.Value[0];

                        Icons.Add(new Icon
                        {
                            Image = icon.ImageUri.AbsoluteUri,
                            Prompt = icon.RevisedPrompt,
                        });
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

        async Task DownloadAsync(string imageUrl)
        {
            try
            {
                Stream fileStream = await GetFileStreamAsync(imageUrl);

                if (fileStream is not null)
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Image files (*.png)|*.png",
                        FilterIndex = 2
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string fileName = saveFileDialog.FileName;
                        string destinationFolder = Path.GetDirectoryName(fileName);
                        string destinationFileName = Path.GetFileName(fileName);
                        await SaveStreamAsync(fileStream, destinationFolder, destinationFileName);

                        await VS.MessageBox.ShowAsync("Icon saved successfully.", buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
                    }
                }
            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowAsync(ex.Message, buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
        }

        async Task<Stream> GetFileStreamAsync(string fileUrl)
        {
            HttpClient httpClient = new HttpClient();
            try
            {
                Stream fileStream = await httpClient.GetStreamAsync(fileUrl);
                return fileStream;
            }
            catch (Exception)
            {
                return Stream.Null;
            }
        }

        async Task SaveStreamAsync(Stream fileStream, string destinationFolder, string destinationFileName)
        {
            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            string path = Path.Combine(destinationFolder, destinationFileName);

            using (FileStream outputFileStream = new FileStream(path, FileMode.CreateNew))
            {
                await fileStream.CopyToAsync(outputFileStream);
            }
        }
    }
}