using Azure;
using Azure.AI.OpenAI;
using EnvDTE;
using MAUI_AI_Assistant.Options;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using OpenAI.Chat;
using System.Linq;
using System.Text.RegularExpressions;

namespace MAUI_AI_Assistant.Commands
{
    internal class MAUIAIBaseCommand<T> : BaseCommand<T> where T : class, new()
    {
        public string SystemMessage { get; set; }
        public string ChatMessage { get; set; }
        public CommandBehavior CommandBehavior { get; set; }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var generalOptions = await General.GetLiveInstanceAsync();

            if (string.IsNullOrEmpty(generalOptions.ApiKey))
            {
                await VS.MessageBox.ShowAsync("API Key is missing, go to Tools/Options/.NET MAUI AI Assistant/General and add the API Key.",
                    buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);

                Package.ShowOptionPage(typeof(General));

                return;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var fac = (IVsThreadedWaitDialogFactory)await VS.Services.GetThreadedWaitDialogAsync();
            IVsThreadedWaitDialog4 twd = fac.CreateInstance();

            twd.StartWaitDialog(".NET MAUI AI Assistant", "Working...", "", null, "", 1, false, true);

            var docView = await VS.Documents.GetActiveDocumentViewAsync(); 
            var selectedCode = string.IsNullOrEmpty(ChatMessage) ? docView.TextView.Selection.StreamSelectionSpan.GetText() : ChatMessage;

            if (string.IsNullOrEmpty(selectedCode))
            {
                twd.EndWaitDialog();
                await VS.MessageBox.ShowAsync("No text selected.", buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }

            string endpoint = generalOptions.ApiEndpoint;
            string key = generalOptions.ApiKey;
            string model =  generalOptions.ChatModel; // Example: gpt-4o

            var azureClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new AzureKeyCredential(key));

            var chatClient = azureClient.GetChatClient(model);

            string systemChatMessage = SystemMessage;
            string userChatMessage = selectedCode;

            try
            {
                ChatCompletion completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(systemChatMessage),
                    new UserChatMessage(userChatMessage),
                ]);

                string result = SanitizeResult(completion.Content[0].Text);

                twd.EndWaitDialog();

                var selection = docView.TextView.Selection.SelectedSpans.FirstOrDefault();

                switch (CommandBehavior)
                {
                    case CommandBehavior.Dialog:
                        await VS.MessageBox.ShowAsync(result, buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
                        break;
                    case CommandBehavior.Insert:
                        docView.TextBuffer.Insert(selection.End, Environment.NewLine + result);
                        break;
                    case CommandBehavior.Replace:
                        docView.TextBuffer.Replace(selection, result);
                        break;
                } 
            }
            catch (Exception ex)
            {
                twd.EndWaitDialog();
                await VS.MessageBox.ShowAsync(ex.Message, buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }

            var hasSelection = docView.TextView.Selection.SelectedSpans.FirstOrDefault().Length != 0;

            if (!hasSelection)
            {
                var selection = docView.TextView.Selection.SelectedSpans.FirstOrDefault();
                int selectionStartLineNumber = docView.TextView.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(selection.Start.Position);

                var startLine = docView.TextView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(selectionStartLineNumber);
                var endLine = docView.TextView.TextBuffer.CurrentSnapshot.GetLineFromPosition(selection.End);
                var snapshotSpan = new SnapshotSpan(startLine.Start, endLine.End);

                docView.TextView.Selection.Select(snapshotSpan, false);
            }

            (await VS.GetServiceAsync<DTE, DTE>()).ExecuteCommand("Edit.FormatSelection");
        }
        
        string SanitizeResult(string response)
        {
            var regex = new Regex(@"```.*\r?\n?");
            return regex.Replace(response, "");
        }
    }
}