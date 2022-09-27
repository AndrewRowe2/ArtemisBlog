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
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Settings;

namespace ArtemisBlog
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(BlogWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(PackageGuids.ArtemisBlogString)]
    [ProvideOptionPage(typeof(GeneralOptions), Vsix.Name, "General", 0, 0, true)]
    [ProvideCustomUnregistrationBehaviour]
    public sealed class ArtemisBlogPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Vsix.Name, "Feed.xml");
            IRefreshOptions refreshOptions = await ReadSettingsAsync();

            FeedMonitor = new FeedMonitor(
                new HttpFeedReader("https://blogs.nasa.gov/artemis/feed/"),
                new FeedFile(fileName),
                refreshOptions);

            FeedMonitor.OnRefreshOptionsChanged += FeedMonitor_OnRefreshOptionsChanged;
            FeedMonitor.Start();
        }

        private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return new ShellSettingsManager(ServiceProvider.GlobalProvider);
        }

        private void FeedMonitor_OnRefreshOptionsChanged(object sender, RefreshOptionsEventArgs e)
        {
            WriteSettingsAsync(e.Options).FireAndForget();
        }

        private async Task<IRefreshOptions> ReadSettingsAsync()
        {
            ShellSettingsManager settingsManager = await GetSettingsManagerAsync();
            SettingsScope scope = SettingsScope.UserSettings;
            SettingsStore settingsStore = settingsManager.GetReadOnlySettingsStore(scope);

            if (settingsStore != null)
            {
                return new RefreshOptions(
                    settingsStore.GetBoolean(Vsix.Name, nameof(RefreshOptions.RefreshAutomatically), true),
                    settingsStore.GetBoolean(Vsix.Name, nameof(RefreshOptions.RefreshOnStartup), true),
                    settingsStore.GetInt32(Vsix.Name, nameof(RefreshOptions.RefreshInterval), 120));
            }

            return null;
        }

        private async Task WriteSettingsAsync(IRefreshOptions options)
        {
            ShellSettingsManager settingsManager = await GetSettingsManagerAsync();
            SettingsScope scope = SettingsScope.UserSettings;
            WritableSettingsStore settingsStore = settingsManager.GetWritableSettingsStore(scope);

            if (settingsStore != null)
            {
                if (!settingsStore.CollectionExists(Vsix.Name))
                {
                    settingsStore.CreateCollection(Vsix.Name);
                }

                settingsStore.SetBoolean(Vsix.Name, nameof(options.RefreshOnStartup), options.RefreshOnStartup);
                settingsStore.SetBoolean(Vsix.Name, nameof(options.RefreshAutomatically), options.RefreshAutomatically);
                settingsStore.SetInt32(Vsix.Name, nameof(options.RefreshInterval), options.RefreshInterval);
            }
        }

        protected override int QueryClose(out bool canClose)
        {
            FeedMonitor.Stop();

            return base.QueryClose(out canClose);
        }

        internal static FeedMonitor FeedMonitor { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ProvideCustomUnregistrationBehaviourAttribute : RegistrationAttribute
    {
        public override void Register(RegistrationContext context)
        {
            // Nothing to do
        }

        public override void Unregister(RegistrationContext context)
        {
            DirectoryInfo blogFolder = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Vsix.Name));

            if (blogFolder.Exists)
            {
                context.Log.WriteLine($"Attempting to delete: {blogFolder.FullName}");
                blogFolder.Delete(true);
            }

            context.Log.WriteLine("Deleting feed");
            File.Delete(@"C:\ProgramData\Artemis Blog\Feed.xml");
        }
    }
}