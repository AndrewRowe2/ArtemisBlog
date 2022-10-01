global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using System.Threading.Tasks;

using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using ArtemisBlog.Feed;
using ArtemisBlog.Options;
using ArtemisBlog.ToolWindows;
using Microsoft.VisualStudio;

namespace ArtemisBlog
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(BlogWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.ArtemisBlogString)]
    [ProvideOptionPage(typeof(GeneralOptions), Vsix.Name, "General", 0, 0, true)]
    public sealed class ArtemisBlogPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Vsix.Name, "Feed.xml");
            SettingsManager settingsManager = new();
            await settingsManager.LoadSettingsAsync();

            FeedMonitor = new FeedMonitor(
                new HttpFeedReader("https://blogs.nasa.gov/artemis/feed/"),
                new FeedFile(fileName),
                settingsManager.RefreshOptions,
                settingsManager.LastRefreshed);

            FeedMonitor.OnRefreshing += FeedMonitor_OnRefreshing;
            FeedMonitor.OnRefreshOptionsChanged += FeedMonitor_OnRefreshOptionsChanged;
            FeedMonitor.Start();
        }

        private void FeedMonitor_OnRefreshing(object sender, FeedEventArgs e)
        {
            if (!e.IsUpdating) // Not updating = updated
            {
                SettingsManager.WriteLastRefreshedAsync(DateTime.Now).FireAndForget();
            }
        }

        private void FeedMonitor_OnRefreshOptionsChanged(object sender, RefreshOptionsEventArgs e)
        {
            SettingsManager.WriteRefreshOptionsAsync(e.Options).FireAndForget();
        }

        protected override int QueryClose(out bool canClose)
        {
            FeedMonitor.Stop();
            JoinableTaskFactory.Run(SettingsManager.RemoveAllAsync);

            return base.QueryClose(out canClose);
        }

        internal static FeedMonitor FeedMonitor { get; private set; }
    }
}