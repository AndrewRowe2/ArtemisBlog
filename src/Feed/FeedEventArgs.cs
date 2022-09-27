using System.ServiceModel.Syndication;

namespace ArtemisBlog.Feed
{
    class FeedEventArgs : EventArgs
    {
        internal FeedEventArgs(SyndicationFeed feed)
        {
            Feed = feed;
        }

        internal FeedEventArgs(SyndicationFeed feed, bool isUpdating)
            : this(feed)
        {
            IsUpdating = isUpdating;
        }

        internal SyndicationFeed Feed { get; }
        internal bool IsUpdating { get; }
    }
}
