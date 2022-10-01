using System.IO;

using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace ArtemisBlog.Options
{
    class SettingsManager
    {
        private static async Task<ShellSettingsManager> GetSettingsManagerAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return new ShellSettingsManager(ServiceProvider.GlobalProvider);
        }

        private static async Task<SettingsStore> GetReadOnlySettingsAsync()
        {
            ShellSettingsManager settingsManager = await GetSettingsManagerAsync();
            return settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
        }

        private static async Task<WritableSettingsStore> GetWritableSettingsAsync()
        {
            ShellSettingsManager settingsManager = await GetSettingsManagerAsync();
            return settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        internal async Task LoadSettingsAsync()
        {
            SettingsStore settingsStore = await GetReadOnlySettingsAsync();

            if (settingsStore != null)
            {
                LastRefreshed = new DateTime(settingsStore.GetInt64(Vsix.Name, nameof(LastRefreshed), 0));

                RefreshOptions = new RefreshOptions(
                    settingsStore.GetBoolean(Vsix.Name, nameof(RefreshOptions.RefreshAutomatically), true),
                    settingsStore.GetBoolean(Vsix.Name, nameof(RefreshOptions.RefreshOnStartup), true),
                    settingsStore.GetInt32(Vsix.Name, nameof(RefreshOptions.RefreshInterval), 120));
            }
        }

        internal static async Task RemoveAllAsync()
        {
            WritableSettingsStore settingsStore = await GetWritableSettingsAsync();

            if (settingsStore != null)
            {
                if (!settingsStore.PropertyExists(@"ExtensionManager\EnabledExtensions", $"{Vsix.Id},{Vsix.Version}"))
                {
                    // We're not listed as an enabled extension anymore: remove the folder containing the blog feed...
                    DirectoryInfo blogFolder = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Vsix.Name));

                    if (blogFolder.Exists)
                    {
                        blogFolder.Delete(true);
                    }

                    // ...and remove the registry settings
                    settingsStore.DeleteCollection(Vsix.Name);
                }
            }
        }

        internal static async Task WriteLastRefreshedAsync(DateTime lastRefreshed)
        {
            WritableSettingsStore settingsStore = await GetWritableSettingsAsync();

            if (settingsStore != null)
            {
                if (!settingsStore.CollectionExists(Vsix.Name))
                {
                    settingsStore.CreateCollection(Vsix.Name);
                }

                settingsStore.SetInt64(Vsix.Name, nameof(LastRefreshed), lastRefreshed.Ticks);
            }
        }

        internal static async Task WriteRefreshOptionsAsync(IRefreshOptions refreshOptions)
        {
            WritableSettingsStore settingsStore = await GetWritableSettingsAsync();

            if (settingsStore != null)
            {
                if (!settingsStore.CollectionExists(Vsix.Name))
                {
                    settingsStore.CreateCollection(Vsix.Name);
                }

                settingsStore.SetBoolean(Vsix.Name, nameof(refreshOptions.RefreshOnStartup), refreshOptions.RefreshOnStartup);
                settingsStore.SetBoolean(Vsix.Name, nameof(refreshOptions.RefreshAutomatically), refreshOptions.RefreshAutomatically);
                settingsStore.SetInt32(Vsix.Name, nameof(refreshOptions.RefreshInterval), refreshOptions.RefreshInterval);
            }
        }

        internal DateTime LastRefreshed { get; private set; }

        internal IRefreshOptions RefreshOptions { get; private set; }
    }
}
