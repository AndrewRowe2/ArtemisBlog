using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Windows;

using ArtemisBlog.Commands;
using ArtemisBlog.Feed;
using ArtemisBlog.Options;
using ArtemisBlog.Resources;

namespace ArtemisBlog.ToolWindows
{
    class BlogWindowViewModel : ViewModel
    {
        private readonly FeedMonitor feedMonitor;
        private int itemCount;
        private Visibility progressBarVisibility = Visibility.Hidden;
        private ObservableCollection<ViewModel> items;

        internal BlogWindowViewModel(FeedMonitor feedMonitor)
        {
            this.feedMonitor = feedMonitor ?? throw new ArgumentNullException(nameof(feedMonitor));
            feedMonitor.OnRefreshOptionsChanged += FeedMonitor_OnRefreshOptionsChanged;
            feedMonitor.OnRefreshing += FeedMonitor_OnFeedUpdating;
            feedMonitor.OnUpdated += FeedMonitor_OnFeedUpdated;

            Refresh = new DelegateCommand(() =>
            {
                feedMonitor.RefreshFeedAsync(new CancellationToken()).FireAndForget();
            });

            Update(feedMonitor.Feed);
        }

        private void FeedMonitor_OnRefreshOptionsChanged(object sender, RefreshOptionsEventArgs e)
        {
            RaisePropertyChanged(nameof(RefreshStatus));
        }

        private void FeedMonitor_OnFeedUpdating(object sender, FeedEventArgs e)
        {
            if (e.IsUpdating)
            {
                Refresh.ExecuteEnabled = false;
                ProgressBarVisibility = Visibility.Visible;
            }
            else
            {
                ProgressBarVisibility = Visibility.Hidden;
                RaisePropertyChanged(nameof(RefreshStatus));
                Refresh.ExecuteEnabled = true;
            }
        }

        private void Update(SyndicationFeed feed)
        {
            if (feed == null)
            {
                return;
            }

            ItemCount = feed.Items.Count();
            Items = new ObservableCollection<ViewModel>();
            
            PublishedPeriod currentPeriod = (PublishedPeriod)(-1);

            foreach (SyndicationItem item in feed.Items)
            {
                PublishedPeriod itemPeriod = item.PublishedPeriod();

                if (currentPeriod != itemPeriod)
                {
                    currentPeriod = itemPeriod;
                    Items.Add(new PublishedPeriodViewModel(itemPeriod));
                }

                Items.Add(new PostViewModel(item));
            }
        }

        private void FeedMonitor_OnFeedUpdated(object sender, FeedEventArgs e)
        {
            Update(e.Feed);
            ProgressBarVisibility = Visibility.Hidden;
        }

        public int ItemCount
        {
            get { return itemCount; }
            set {  SetProperty(ref itemCount, value); }
        }

        public string RefreshStatus
        {
            get
            {
                string lastRefreshed;

                if (feedMonitor.LastRefreshed.Date == DateTime.Today)
                {
                    lastRefreshed = feedMonitor.LastRefreshed.ToString("t");
                }
                else
                {
                    lastRefreshed = Legends.NoneToday;
                }

                string nextRefresh;

                if (feedMonitor.RefreshAutomatically)
                {
                    nextRefresh = feedMonitor.NextRefresh.ToString("t");
                }
                else
                {
                    nextRefresh = Legends.Manual;
                }

                return $"{Legends.LastRefreshed}: {lastRefreshed}\r\n{Legends.NextRefresh}: {nextRefresh}";
            }
        }

        public Visibility ProgressBarVisibility
        {
            get { return progressBarVisibility; }
            set
            {
                SetProperty(ref progressBarVisibility, value);
            }
        }

        public ObservableCollection<ViewModel> Items
        {
            get { return items; }
            set { SetProperty(ref items, value); }
        }

        public DelegateCommand Refresh { get; private set; }
    }
}
