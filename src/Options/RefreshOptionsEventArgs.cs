namespace ArtemisBlog.Options
{
    public class RefreshOptionsEventArgs
    {
        public RefreshOptionsEventArgs(IRefreshOptions options)
        {
            Options = options;
        }

        public IRefreshOptions Options { get; set; }
    }
}
