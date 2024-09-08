namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.Suggestion)]
    internal sealed class CodeSuggestions : MAUIAIBaseCommand<CodeSuggestions>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = "Refactor this code with best practices. Apply the best option taking into account good practices and performance. Return only the refactored code.";
            CommandBehavior = CommandBehavior.Replace;

            await base.ExecuteAsync(e);
        }
    }
}