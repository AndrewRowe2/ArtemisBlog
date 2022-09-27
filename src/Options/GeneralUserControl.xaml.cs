using System.Windows.Controls;

namespace ArtemisBlog.Options
{
    /// <summary>
    /// Interaction logic for GeneralUserControl.xaml
    /// </summary>
    public partial class GeneralUserControl : UserControl
    {
        public GeneralUserControl()
        {
            InitializeComponent();
        }

        internal GeneralUserControl(GeneralViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }
    }
}
