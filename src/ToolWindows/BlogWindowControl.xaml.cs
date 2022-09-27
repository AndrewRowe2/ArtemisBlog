using System.Windows.Controls;

namespace ArtemisBlog.ToolWindows
{
    public partial class BlogWindowControl : UserControl
    {
        internal BlogWindowControl()
        {
            InitializeComponent();
        }

        internal BlogWindowControl(BlogWindowViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }
    }
}