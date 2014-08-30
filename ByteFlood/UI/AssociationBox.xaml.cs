using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ByteFlood
{
    /// <summary>
    /// Interaction logic for AssociationBox.xaml
    /// </summary>
    public partial class AssociationBox : Window
    {
        public bool MagnetLinks;
        public bool Files;
        public bool Remind;
        public AssociationBox()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MagnetLinks = magnets.IsChecked.Value;
            Files = files.IsChecked.Value;
            remind.IsEnabled = !(MagnetLinks || Files);
            if (!remind.IsEnabled)
                remind.IsChecked = false;
            Remind = remind.IsChecked.Value;
        }
    }
}
