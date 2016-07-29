using System.Diagnostics;
using System.Windows;

namespace MindBotConnector
{
    /// <summary>
    /// Interaction logic for instructions.xaml
    /// </summary>
    public partial class instructions : Window
    {
        public instructions()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
