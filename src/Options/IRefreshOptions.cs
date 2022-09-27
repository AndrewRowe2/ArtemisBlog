namespace ArtemisBlog.Options
{
    public interface IRefreshOptions
    {
        bool RefreshAutomatically { get; }
        bool RefreshOnStartup { get; }
        int RefreshInterval { get; }
    }

    class RefreshOptions : IRefreshOptions
    {
        internal RefreshOptions(bool refreshAutomatically, bool refreshOnStartup, int refreshInterval)
        {
            RefreshAutomatically = refreshAutomatically;
            RefreshOnStartup = refreshOnStartup;
            RefreshInterval = refreshInterval;
        }

        public bool RefreshAutomatically { get; }

        public bool RefreshOnStartup { get; }

        public int RefreshInterval { get; }
    }
}
