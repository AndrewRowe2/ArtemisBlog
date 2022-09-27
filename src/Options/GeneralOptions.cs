using System.Runtime.InteropServices;
using System.Windows;

namespace ArtemisBlog.Options
{
    [ComVisible(true)]
    [Guid("02356BCA-C49B-4959-8CF6-4FADC0F9D020")]
    public class GeneralOptions : UIElementDialogPage
    {
        private GeneralUserControl generalPage;

        protected override UIElement Child => generalPage;

        public override void LoadSettingsFromStorage()
        {
            generalPage ??= new GeneralUserControl();
            generalPage.DataContext = new GeneralViewModel();
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            ArtemisBlogPackage.FeedMonitor.RefreshOptions = generalPage.DataContext as IRefreshOptions;
        }
    }
}
