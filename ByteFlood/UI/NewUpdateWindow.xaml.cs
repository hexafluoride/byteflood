using System.Windows;
using ByteFlood.Services;
using System.Diagnostics;

namespace ByteFlood.UI
{
    /// <summary>
    /// Interaction logic for NewUpdateWindow.xaml
    /// </summary>
    public partial class NewUpdateWindow : Window
    {
        private NewUpdateInfo Info 
        {
            get 
            {
                return (NewUpdateInfo)this.DataContext;
            }
        }

        public NewUpdateWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(this.Info.Link);
        }
    }
}
