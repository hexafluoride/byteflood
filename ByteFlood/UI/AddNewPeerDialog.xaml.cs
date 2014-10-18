using System.Windows;

namespace ByteFlood.UI
{
    /// <summary>
    /// Interaction logic for AddNewPeerDialog.xaml
    /// </summary>
    public partial class AddNewPeerDialog : Window
    {
        public AddNewPeerDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string IP { get; set; }

        private void Commands_Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; this.Close();
        }

        private void Commands_OK(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; this.Close();
        }
    }
}
