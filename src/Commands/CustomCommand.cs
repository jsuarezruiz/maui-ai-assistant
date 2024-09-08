using Microsoft.VisualStudio.Shell.Interop;

namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.CustomCommand)]
    internal class CustomCommand : MAUIAIBaseCommand<CustomCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var customCommandOptions = await Options.CustomCommand.GetLiveInstanceAsync();
            
            
            if (string.IsNullOrEmpty(customCommandOptions.Prompt))
            {
                await VS.MessageBox.ShowAsync("The Prompt for the custom Command is missing, go to Tools/Options/.NET MAUI AI Assistant/Custom Command and add the Prompt.",
                    buttons: OLEMSGBUTTON.OLEMSGBUTTON_OK);

                Package.ShowOptionPage(typeof(CustomCommand));

                return;
            }

            SystemMessage = customCommandOptions.Prompt;
            CommandBehavior = customCommandOptions.Action;

            await base.ExecuteAsync(e);
        }
    }
}