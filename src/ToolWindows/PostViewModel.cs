using System.Diagnostics;
using System.Net;
using System.ServiceModel.Syndication;
using System.Windows.Input;

using ArtemisBlog.Commands;

namespace ArtemisBlog.ToolWindows
{
    class PostViewModel : ViewModel
    {
        private readonly SyndicationItem post;

        public PostViewModel(SyndicationItem post)
        {
            this.post = post ?? throw new ArgumentNullException(nameof(post));
            Title = WebUtility.HtmlDecode(post.Title.Text);
            Summary = WebUtility.HtmlDecode(post.Summary.Text);
            Published = post.PublishDate.ToLocalTime().DateTime;
            ToolTip = $"{Title}\r\n{Published:ddd, MMMM dd · HH:mm}";
            ViewInBrowser = new DelegateCommand(ViewInBrowserAction);

            foreach (SyndicationCategory category in post.Categories)
            {
                if (category.Name != "Uncategorized")
                {
                    Category = category.Name;
                    break;
                }
            }
        }

        private void ViewInBrowserAction()
        {
            Process.Start(Url.AbsoluteUrl);
        }

        public Url Url => new(post.Id);
        public string ToolTip { get; }
        public string Title { get; }
        public string Summary { get; }
        public DateTime Published { get; }
        public string Category { get; }
        public ICommand ViewInBrowser { get; }
    }
}
