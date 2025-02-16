using MAUI_AI_Assistant.Options;

namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.CreateUnitTest)]
    internal sealed class CreateUnitTest : MAUIAIBaseCommand<CreateUnitTest>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var opts = await UnitTest.GetLiveInstanceAsync();
            var framework = opts.Framework;

            SystemMessage = $"Create {framework} Unit Test methods. Return only the refactored code.";
            CommandBehavior = CommandBehavior.Insert;

            await base.ExecuteAsync(e);
        }
    }
}