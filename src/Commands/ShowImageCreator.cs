namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.ImageCreator)]
    internal sealed class ShowImageCreator : BaseCommand<ShowImageCreator>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ImageCreator.ShowAsync();
        }
    }
}