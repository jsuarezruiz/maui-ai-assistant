namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.ConvertFromXamarinForms)]
    internal sealed class ConvertFromXamarinForms : MAUIAIBaseCommand<ConvertFromXamarinForms>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = "Refactor this code converting it from Xamarin.Forms to .NET MAUI based on https://learn.microsoft.com/en-us/dotnet/maui/migration/multi-project-to-multi-project. Return only the refactored code.";
            CommandBehavior = CommandBehavior.Replace;

            await base.ExecuteAsync(e);
        }
    }
}