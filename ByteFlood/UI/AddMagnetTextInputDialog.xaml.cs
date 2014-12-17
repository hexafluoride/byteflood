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

namespace ByteFlood.UI
{
    /// <summary>
    /// Interaction logic for AddMagnetTextInputDialog.xaml
    /// </summary>
    public partial class AddMagnetTextInputDialog : Window
    {
        public AddMagnetTextInputDialog()
        {
            InitializeComponent();
            this.DataContext = new NiceDataContext<AddMagnetTextInputDialog>(this);
        }

        public string Input
        {
            get { return (string)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            DependencyProperty.Register("Input", typeof(string), typeof(AddMagnetTextInputDialog), null);


        #region Commands

        private void Commands_PasteFromClipboard(object sender, ExecutedRoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                if (Utility.IsMagnetLink(text))
                {
                    this.Input = Clipboard.GetText();
                }
            }
        }

        private void Commands_OK(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true; this.Close();
        }

        private void Commands_Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false; this.Close();
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Commands_PasteFromClipboard(null, null);
        }
    }
}
