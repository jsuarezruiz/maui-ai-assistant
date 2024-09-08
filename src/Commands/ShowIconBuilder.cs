namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.IconBuilder)]
    internal sealed class ShowIconBuilder : BaseCommand<ShowIconBuilder>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await IconBuilder.ShowAsync();
        }
    }
}