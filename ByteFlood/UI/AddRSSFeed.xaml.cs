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
using System.Collections.ObjectModel;

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

        public bool AllowUrlChange
        {
            get { return (bool)GetValue(AllowUrlChangeProperty); }
            set { SetValue(AllowUrlChangeProperty, value); }
        }

        public static readonly DependencyProperty AllowUrlChangeProperty =
            DependencyProperty.Register("AllowUrlChange", typeof(bool), typeof(AddRSSFeed), new PropertyMetadata(true));



        public string DownloadPath
        {
            get { return (string)GetValue(DownloadPathProperty); }
            set { SetValue(DownloadPathProperty, value); }
        }

        public static readonly DependencyProperty DownloadPathProperty =
            DependencyProperty.Register("DownloadPath", typeof(string), typeof(AddRSSFeed), new PropertyMetadata(null));

        public ObservableCollection<Services.RSS.RssFilter> Filters
        {
            get { return (ObservableCollection<Services.RSS.RssFilter>)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }

        public static readonly DependencyProperty FiltersProperty =
            DependencyProperty.Register("Filters", typeof(ObservableCollection<Services.RSS.RssFilter>), typeof(AddRSSFeed), new PropertyMetadata(null));


        public int UpdateIntervalType
        {
            get { return (int)GetValue(UpdateIntervalTypeProperty); }
            set { SetValue(UpdateIntervalTypeProperty, value); }
        }

        public static readonly DependencyProperty UpdateIntervalTypeProperty =
            DependencyProperty.Register("UpdateIntervalType", typeof(int), typeof(AddRSSFeed), new PropertyMetadata(0));

        public int CustomIntervalSeconds;
        public string ManualUpdateIntervalSeconds
        {
            get { return this.CustomIntervalSeconds.ToString(); }
            set
            {
                Int32.TryParse(value, out this.CustomIntervalSeconds);
                if (this.CustomIntervalSeconds < 0)
                {
                    this.CustomIntervalSeconds = -this.CustomIntervalSeconds;
                }
                else if (this.CustomIntervalSeconds == 0)
                {
                    this.CustomIntervalSeconds = 15 * 60;
                }
                this.ManualUpdateIntervalSecondsText = Formatters.HMSFormatter.GetReadableTimespan(new TimeSpan(0, 0, this.CustomIntervalSeconds));
            }
        }

        public string ManualUpdateIntervalSecondsText
        {
            get { return (string)GetValue(ManualUpdateIntervalSecondsTextProperty); }
            set { SetValue(ManualUpdateIntervalSecondsTextProperty, value); }
        }

        public static readonly DependencyProperty ManualUpdateIntervalSecondsTextProperty =
            DependencyProperty.Register("ManualUpdateIntervalSecondsText", typeof(string), typeof(AddRSSFeed), new PropertyMetadata(null));

        public TorrentProperties DefaultTorrentProperties { get; set; }

        #endregion

        public AddRSSFeed()
        {
            InitializeComponent();
            this.Filters = new ObservableCollection<Services.RSS.RssFilter>();
            this.RemoveEvent = new RoutedEventHandler(this.Filters_Remove);
            this.DownloadPath = App.Settings.DefaultDownloadPath;
            this.ManualUpdateIntervalSeconds = "120";
            this.DefaultTorrentProperties = Utility.CloneObject(TorrentProperties.DefaultTorrentProperties);
            this.DataContext = new NiceDataContext<AddRSSFeed>(this);
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

        private void Commands_Browse(object sender, ExecutedRoutedEventArgs e)
        {
            string path = Utility.PromptFolderSelection("Choose RSS save directory", null, this);
            if (path != null)
            {
                this.DownloadPath = path;
            }
        }

        private RoutedEventHandler RemoveEvent;

        private void Filters_Add(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.FilterExpression))
            {
                var action = this.FilterAction == 0 ? Services.RSS.RssFilter.FilterActionEnum.Download : Services.RSS.RssFilter.FilterActionEnum.Skip;
                this.Filters.Add(new Services.RSS.RssFilter()
                {
                    FilterAction = action,
                    FilterText = this.FilterExpression,
                    RemoveAction = RemoveEvent
                });
                this.FilterExpression = "";
            }
        }

        private void Filters_Remove(object sender, RoutedEventArgs e)
        {
            Controls.ClickableLabel label = sender as Controls.ClickableLabel;
            Services.RSS.RssFilter filter = label.Tag as Services.RSS.RssFilter;
            this.Filters.Remove(filter);
        }

        private void Commands_ChangeDefaultTorrentSettings(object sender, ExecutedRoutedEventArgs e)
        {
            var editor = new TorrentPropertiesEditor(this.DefaultTorrentProperties) 
            { Icon = this.Icon, Owner = this };
            editor.ShowDialog();
        }

        #endregion

        public void LoadFilters(Services.RSS.RssFilter[] filters)
        {
            foreach (var filter in filters)
            {
                filter.RemoveAction = this.RemoveEvent;
                this.Filters.Add(filter);
            }
        }




    }
}
