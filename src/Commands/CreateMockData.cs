namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.CreateMockData)]
    internal sealed class CreateMockData : MAUIAIBaseCommand<CreateMockData>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = $"Create Mock Data. Return only the refactored code.";
            CommandBehavior = CommandBehavior.Insert;

            await base.ExecuteAsync(e);
        }
    }
}