using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

using ArtemisBlog.Resources;
using Microsoft.VisualStudio.Imaging;

namespace ArtemisBlog.ToolWindows
{
    public class BlogWindow : BaseToolWindow<BlogWindow>
    {
        public override string GetTitle(int toolWindowId) => Legends.Title;

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new BlogWindowControl(new BlogWindowViewModel(ArtemisBlogPackage.FeedMonitor)));
        }

        [Guid("9e1bf92a-db9c-493e-bcc8-1b5ac90d2c50")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}