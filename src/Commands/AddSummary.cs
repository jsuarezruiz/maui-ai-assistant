namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.Summary)]
    internal sealed class AddSummary : MAUIAIBaseCommand<AddSummary>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = "Refactor this code adding the summary. Return only the refactored code. Use the recommendations from https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags";
            CommandBehavior = CommandBehavior.Replace;

            await base.ExecuteAsync(e);
        }
    }
}
