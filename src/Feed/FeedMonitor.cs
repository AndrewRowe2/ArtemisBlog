using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading;

using ArtemisBlog.Options;
using ArtemisBlog.Resources;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell.Interop;

namespace ArtemisBlog.Feed
{
    class FeedMonitor : IRefreshOptions
    {
        private readonly HttpFeedReader onlineFeed;
        private readonly FeedFile localFeed;
        private SyndicationFeed feed;
        private bool refreshAutomatically = true;
        private int refreshInterval = 120;
        private IVsTask monitorTask;
        private bool started;

        internal FeedMonitor(HttpFeedReader onlineFeed, FeedFile localFeed, IRefreshOptions options, DateTime lastRefreshed)
        {
            this.onlineFeed = onlineFeed;
            this.localFeed = localFeed;

            if (options != null)
            {
                RefreshOnStartup = options.RefreshOnStartup;
                refreshAutomatically = options.RefreshAutomatically;
                refreshInterval = options.RefreshInterval;
            }

            LastRefreshed = lastRefreshed;
        }

        private void DoRefreshOptionsChanged()
        {
            if (OnRefreshOptionsChanged != null)
            {
                try
                {
                    OnRefreshOptionsChanged(this, new RefreshOptionsEventArgs(this));
                }
                catch { }
            }
        }

        private void DoRefreshing(bool isUpdating)
        {
            if (OnRefreshing != null)
            {
                try
                {
                    OnRefreshing(this, new FeedEventArgs(feed, isUpdating));
                }
                catch { }
            }
        }

        private void DoUpdated()
        {
            if (OnUpdated != null)
            {
                try
                {
                    OnUpdated(this, new FeedEventArgs(feed));
                }
                catch { }
            }
        }

        private SyndicationItem GetLatestItem()
        {
            if (feed?.Items != null)
            {
                foreach (SyndicationItem item in feed.Items)
                {
                    return item;
                }
            }

            return null;
        }

        private void Restart()
        {
            if (monitorTask != null)
            {
                monitorTask.Cancel();
                Start();
            }
        }

        private async Task<int> MonitorAsync(CancellationToken cancellationToken)
        {
            if (!started)
            {
                started = true;

                if (RefreshOnStartup && DateTime.Now.Date > LastRefreshed.Date)
                {
                    await RefreshFeedAsync(cancellationToken);
                }
                else
                {
                    Feed = localFeed.GetFeed();
                }
            }

            while (RefreshAutomatically && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(NextBoundary(RefreshInterval), cancellationToken);
                await RefreshFeedAsync(cancellationToken);
            }            

            return 0;
        }

        private int NextBoundary(int minutes)
        {
            //Return number of milliseconds to the next boundary
            DateTime now = DateTime.Now;
            int minutesRemaining = now.Minute % minutes;

            return (minutes * 60000) - ((minutesRemaining * 60000) + (now.Second * 1000));
        }

        private async Task DisplayLatestItemAsync(CancellationToken cancellationToken)
        {
            SyndicationItem item = GetLatestItem();

            if (item != null)
            {
                InfoBarModel infoBarModel = new(
                    new[]
                    {
                        new InfoBarTextSpan(WebUtility.HtmlDecode(item.Title.Text)),
                        new InfoBarTextSpan("   "),
                        new InfoBarHyperlink(Legends.ViewInBrowser, item.Id)
                    },
                    KnownMonikers.StatusInformation);

                InfoBar infoBar;

                // If VS is displaying the Start Window then infoBar will be null. Wait 10s and try again...
                while ((infoBar = await VS.InfoBar.CreateAsync(infoBarModel)) == null)
                {
                    await Task.Delay(10000, cancellationToken);
                }

                infoBar.ActionItemClicked += InfoBar_ActionItemClicked;
                await infoBar.TryShowInfoBarUIAsync();
            }
        }

        private void InfoBar_ActionItemClicked(object sender, InfoBarActionItemEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Process.Start(e.ActionItem.ActionContext.ToString());
        }

        internal async Task RefreshFeedAsync(CancellationToken cancellationToken)
        {
            DoRefreshing(true);
            try
            {
                if (await onlineFeed.GetLastModifiedAsync() > localFeed.GetLastModified())
                {
                    using Stream feedStream = await onlineFeed.GetFeedStreamAsync();
                    localFeed.Save(feedStream);
                    Feed = new StreamFeedReader(feedStream).GetFeed();
                    await DisplayLatestItemAsync(cancellationToken);
                }
                else
                {
                    Feed ??= localFeed.GetFeed();
                }

                LastRefreshed = DateTime.Now;
            }
            finally
            {
                DoRefreshing(false);
            }
        }

        internal void Start()
        {
            monitorTask = ThreadHelper.JoinableTaskFactory.RunAsyncAsVsTask(VsTaskRunContext.UIThreadIdlePriority, MonitorAsync);
        }

        internal void Stop()
        {
            if (monitorTask != null)
            {
                monitorTask.Cancel();
                monitorTask = null;
            }
        }

        internal SyndicationFeed Feed
        {
            get { return feed; }
            set
            {
                feed = value;

                // Don't raise the event if the feed has become null
                if (feed != null)
                {
                    DoUpdated();
                }
            }
        }

        internal DateTime LastRefreshed { get; private set; }

        internal DateTime NextRefresh => DateTime.Now.AddMilliseconds(NextBoundary(RefreshInterval));

        internal event EventHandler<RefreshOptionsEventArgs> OnRefreshOptionsChanged;

        internal event EventHandler<FeedEventArgs> OnRefreshing;

        internal event EventHandler<FeedEventArgs> OnUpdated;

        public bool RefreshAutomatically
        {
            get { return refreshAutomatically; }
            private set
            {
                if (refreshAutomatically != value)
                {
                    refreshAutomatically = value;

                    if (refreshAutomatically)
                    {
                        if (monitorTask == null)
                        {
                            Start();
                        }
                    }
                    else
                    {
                        Stop();
                    }
                }
            }
        }

        public bool RefreshOnStartup { get; private set; } = true;

        public int RefreshInterval
        {
            get { return refreshInterval; }
            private set
            {
                if (refreshInterval != value)
                {
                    refreshInterval = value;                    
                    Restart();
                }
            }
        }

        public IRefreshOptions RefreshOptions
        {
            get { return this; }
            set
            {
                if (value != null)
                {
                    if (RefreshAutomatically != value.RefreshAutomatically ||
                        RefreshOnStartup != value.RefreshOnStartup ||
                        RefreshInterval != value.RefreshInterval)
                    {
                        RefreshAutomatically = value.RefreshAutomatically;
                        RefreshOnStartup = value.RefreshOnStartup;
                        RefreshInterval = value.RefreshInterval;
                        DoRefreshOptionsChanged();
                    }
                }
            }
        }
    }
}
