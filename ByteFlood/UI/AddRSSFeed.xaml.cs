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
    /// Interaction logic for AddRSSFeed.xaml
    /// </summary>
    public partial class AddRSSFeed : Window
    {
        
        #region Properties

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(string), typeof(AddRSSFeed), new PropertyMetadata(null));

        public string CustomAlias
        {
            get { return (string)GetValue(CustomAliasProperty); }
            set { SetValue(CustomAliasProperty, value); }
        }

        public static readonly DependencyProperty CustomAliasProperty =
            DependencyProperty.Register("CustomAlias", typeof(string), typeof(AddRSSFeed), new PropertyMetadata(null));

        public bool? AutoDownload
        {
            get { return (bool?)GetValue(AutoDownloadProperty); }
            set { SetValue(AutoDownloadProperty, value); }
        }

        public static readonly DependencyProperty AutoDownloadProperty =
            DependencyProperty.Register("AutoDownload", typeof(bool?), typeof(AddRSSFeed), new PropertyMetadata(false));

        public string FilterExpression
        {
            get { return (string)GetValue(FilterExpressionProperty); }
            set { SetValue(FilterExpressionProperty, value); }
        }

        public static readonly DependencyProperty FilterExpressionProperty =
            DependencyProperty.Register("FilterExpression", typeof(string), typeof(AddRSSFeed), new PropertyMetadata(null));

        public int FilterAction
        {
            get { return (int)GetValue(FilterActionProperty); }
            set { SetValue(FilterActionProperty, value); }
        }

        public static readonly DependencyProperty FilterActionProperty =
            DependencyProperty.Register("FilterAction", typeof(int), typeof(AddRSSFeed), new PropertyMetadata(1));

        #endregion

        public AddRSSFeed()
        {
            InitializeComponent();
        }

        #region Commands

        private void Commands_Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false; this.Close();
        }

        private void Commands_Add(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = true; this.Close();
        }

        #endregion


    }
}
