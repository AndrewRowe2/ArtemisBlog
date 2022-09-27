using ArtemisBlog.ToolWindows;

namespace ArtemisBlog.Commands
{
    [Command(PackageIds.MyCommand)]
    internal sealed class BlogWindowCommand : BaseCommand<BlogWindowCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return BlogWindow.ShowAsync();
        }
    }
}
