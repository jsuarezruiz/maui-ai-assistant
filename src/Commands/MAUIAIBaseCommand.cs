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
            var selectedSpan = docView.TextView.Selection.SelectedSpans.FirstOrDefault();

            if (selectedSpan.Length == 0)
            {
                var textBuffer = docView.TextView.TextBuffer;
                var position = selectedSpan.Start.Position;
                var line = textBuffer.CurrentSnapshot.GetLineFromPosition(position);
                SelectText(docView, line.Start, line.End);
                selectedSpan = docView.TextView.Selection.SelectedSpans.FirstOrDefault();
            }

            var selectedCode = docView.TextView.Selection.StreamSelectionSpan.GetText();
            int selectedStartLineNumber = docView.TextView.TextBuffer.CurrentSnapshot.GetLineNumberFromPosition(selectedSpan.Start.Position);

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

                switch (CommandBehavior)
                {
                    case CommandBehavior.Dialog:
                        await VS.MessageBox.ShowAsync(result, buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
                        break;
                    case CommandBehavior.Insert:
                        docView.TextBuffer.Insert(selectedSpan.End, Environment.NewLine + result);
                        break;
                    case CommandBehavior.Replace:
                        docView.TextBuffer.Replace(selectedSpan, result);
                        break;
                } 
            }
            catch (Exception ex)
            {
                twd.EndWaitDialog();
                await VS.MessageBox.ShowAsync(ex.Message, buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }

            if (CommandBehavior != CommandBehavior.Dialog) // Format selected code
            {
                selectedSpan = docView.TextView.Selection.SelectedSpans.FirstOrDefault();
              
                if (selectedSpan.Length == 0)
                {
                    var startLine = docView.TextView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(selectedStartLineNumber);
                    var endLine = docView.TextView.TextBuffer.CurrentSnapshot.GetLineFromPosition(selectedSpan.End);
                    SelectText(docView, startLine.Start, endLine.End);
                }
            }

            (await VS.GetServiceAsync<DTE, DTE>()).ExecuteCommand("Edit.FormatSelection");
        }
        

        void SelectText(DocumentView docView, SnapshotPoint start, SnapshotPoint end)
        {
            var snapshotSpan = new SnapshotSpan(start, end);
            docView.TextView.Selection.Select(snapshotSpan, false);
        }

        string SanitizeResult(string response)
        {
            var regex = new Regex(@"```.*\r?\n?");
            return regex.Replace(response, "");
        }
    }
}