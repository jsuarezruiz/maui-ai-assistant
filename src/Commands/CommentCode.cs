namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.Comment)]
    internal sealed class CommentCode : MAUIAIBaseCommand<CommentCode>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = "Refactor this code adding comments. Return only the refactored code.";
            CommandBehavior = CommandBehavior.Replace;

            await base.ExecuteAsync(e);
        }
    }
}