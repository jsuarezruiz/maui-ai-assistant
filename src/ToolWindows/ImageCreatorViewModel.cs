﻿using Azure.AI.OpenAI;
using Azure;
using MAUI_AI_Assistant.Options;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using OpenAI.Images;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MAUI_AI_Assistant
{
    internal class ImageCreatorViewModel : ObservableObject
    {
        const int ImagesCount = 4;

        bool _isBusy;
        string _prompt;
        ObservableCollection<Icon> _images;

        public ImageCreatorViewModel()
        {
            Images = new ObservableCollection<Icon>();
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

        public ObservableCollection<Icon> Images
        {
            get { return _images; }
            set { SetProperty(ref _images, value); }
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

                for (int i = 0; i < ImagesCount; i++)
                {
                    var result = await imageClient.GenerateImagesAsync(Prompt, 1, imageGenerationOptions);

                    if (result is not null)
                    {
                        var icon = result.Value[0];

                        Images.Add(new Icon
                        {
                            Image = icon.ImageUri.AbsoluteUri,
                            Prompt = icon.RevisedPrompt,
                        });
                    }
                }
            }
            catch (Exception ex)
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

                        await VS.MessageBox.ShowAsync("Image saved successfully.", buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
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