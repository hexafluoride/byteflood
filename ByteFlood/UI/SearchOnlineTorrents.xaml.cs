using System;
using System.Windows;
using System.Windows.Input;
using ByteFlood.Services.WebSearch;
using System.Threading.Tasks;
using System.Web;

namespace ByteFlood.UI
{
    /// <summary>
    /// Interaction logic for SearchOnlineTorrents.xaml
    /// </summary>
    public partial class SearchOnlineTorrents : Window
    {
        public string SearchQuery { get; set; }

        public SearchResult[] Items
        {
            get { return (SearchResult[])GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(SearchResult[]), typeof(SearchOnlineTorrents), new PropertyMetadata(null));

        public Visibility SearchMessageVisible
        {
            get { return (Visibility)GetValue(SearchMessageVisibleProperty); }
            set { SetValue(SearchMessageVisibleProperty, value); }
        }

        public static readonly DependencyProperty SearchMessageVisibleProperty =
            DependencyProperty.Register("SearchMessageVisible", typeof(Visibility), typeof(SearchOnlineTorrents), new PropertyMetadata(Visibility.Collapsed));

        public bool ControlsEnabled
        {
            get { return (bool)GetValue(ControlsEnabledProperty); }
            set { SetValue(ControlsEnabledProperty, value); }
        }

        public static readonly DependencyProperty ControlsEnabledProperty =
            DependencyProperty.Register("ControlsEnabled", typeof(bool), typeof(SearchOnlineTorrents), new PropertyMetadata(true));

        public bool SearchCancelled { get; set; }

        public SearchOnlineTorrents()
        {
            InitializeComponent();
        }

        private void Commands_Search(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.SearchQuery))
            {
                DisableControls();
                SearchCancelled = false;
                Task.Factory.StartNew(new Action(() =>
                {
                    var res = TorrentzEu.Search(this.SearchQuery);

                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (SearchCancelled) { return; }
                        this.Items = res;
                        EnableControls();
                    }));
                }));
            }
        }

        private void DisableControls()
        {
            this.SearchMessageVisible = System.Windows.Visibility.Visible;
            this.ControlsEnabled = false;
        }

        private void EnableControls()
        {
            this.SearchMessageVisible = System.Windows.Visibility.Collapsed;
            this.ControlsEnabled = true;
        }

        private void Commands_AddTorrent(object sender, ExecutedRoutedEventArgs e)
        {
            SearchResult ser = (SearchResult)e.Parameter;

            (App.Current.MainWindow as MainWindow).state.AddTorrentByMagnet(string.Format("magnet:?xt=urn:btih:{0}&dn={1}", 
                ser.InfoHash, HttpUtility.UrlEncode(ser.Name)));
        }

        private void Commands_CancelSearch(object sender, ExecutedRoutedEventArgs e)
        {
            this.SearchCancelled = true;
            EnableControls();
        }



    }
}
