namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.DesignToCode)]
    internal sealed class ShowDesignToCode : BaseCommand<ShowDesignToCode>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await DesignToCode.ShowAsync();
        }
    }
}