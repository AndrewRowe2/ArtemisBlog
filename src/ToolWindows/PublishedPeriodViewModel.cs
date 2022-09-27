using ArtemisBlog.Feed;
using ArtemisBlog.Resources;

namespace ArtemisBlog.ToolWindows
{
    class PublishedPeriodViewModel : ViewModel
    {
        internal PublishedPeriodViewModel(PublishedPeriod publishedPeriod)
        {
            PublishedPeriod = Legends.ResourceManager.GetString(publishedPeriod.ToString());
        }

        public string PublishedPeriod { get; }
    }
}
