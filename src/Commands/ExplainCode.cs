namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.Explain)]
    internal sealed class ExplainCode : MAUIAIBaseCommand<ExplainCode>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = "Explain this code.";
            CommandBehavior = CommandBehavior.Dialog;

            await base.ExecuteAsync(e);
        }
    }
}
