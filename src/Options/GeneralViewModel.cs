using System.Collections.Generic;
using ArtemisBlog.Feed;
using ArtemisBlog.ToolWindows;

namespace ArtemisBlog.Options
{
    class GeneralViewModel : ViewModel, IRefreshOptions
    {
        internal GeneralViewModel()
        {
            RefreshIntervals = new List<RefreshInterval>
            {
                RefreshInterval.FiveMinutes,
                RefreshInterval.ThirtyMinutes,
                RefreshInterval.SixtyMinutes,
                RefreshInterval.TwoHours,
                RefreshInterval.FourHours
            };

            Initialise(ArtemisBlogPackage.FeedMonitor);
        }

        private void Initialise(FeedMonitor feedMonitor)
        {
            RefreshAutomatically = feedMonitor.RefreshAutomatically;
            RefreshOnStartup = feedMonitor.RefreshOnStartup;

            foreach (RefreshInterval refreshInterval in RefreshIntervals)
            {
                if (refreshInterval.Minutes == feedMonitor.RefreshInterval)
                {
                    RefreshInterval = refreshInterval;
                    break;
                }
            }
        }

        public bool RefreshAutomatically { get; set; }
        public bool RefreshOnStartup { get; set; }
        int IRefreshOptions.RefreshInterval => RefreshInterval.Minutes;
        public RefreshInterval RefreshInterval { get; set; }
        public IEnumerable<RefreshInterval> RefreshIntervals { get; }
    }
}
