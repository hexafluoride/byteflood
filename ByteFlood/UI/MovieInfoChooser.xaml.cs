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
using ByteFlood.Services.MoviesDatabases;
using System.Threading.Tasks;
using System.Threading;
namespace ByteFlood.UI
{
    /// <summary>
    /// Interaction logic for MovieInfoChooser.xaml
    /// </summary>
    public partial class MovieInfoChooser : Window
    {

        public string SearchQuery
        {
            get { return (string)GetValue(SearchQueryProperty); }
            set { SetValue(SearchQueryProperty, value); }
        }

        public static readonly DependencyProperty SearchQueryProperty =
            DependencyProperty.Register("SearchQuery", typeof(string), typeof(MovieInfoChooser), new PropertyMetadata(null));

        public ImdbSearchResult[] Items
        {
            get { return (ImdbSearchResult[])GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ImdbSearchResult[]), typeof(MovieInfoChooser), new PropertyMetadata(null));

        public Visibility SearchMessageVisible
        {
            get { return (Visibility)GetValue(SearchMessageVisibleProperty); }
            set { SetValue(SearchMessageVisibleProperty, value); }
        }

        public static readonly DependencyProperty SearchMessageVisibleProperty =
            DependencyProperty.Register("SearchMessageVisible", typeof(Visibility), typeof(MovieInfoChooser), new PropertyMetadata(Visibility.Collapsed));

        public bool ControlsEnabled
        {
            get { return (bool)GetValue(ControlsEnabledProperty); }
            set { SetValue(ControlsEnabledProperty, value); }
        }

        public static readonly DependencyProperty ControlsEnabledProperty =
            DependencyProperty.Register("ControlsEnabled", typeof(bool), typeof(MovieInfoChooser), new PropertyMetadata(true));

        public bool SearchCancelled { get; set; }

        public TorrentInfo Torrent { get; set; }

        public MovieInfoChooser()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.Commands_Search(null, null);
        }

        private void Commands_Search(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.SearchQuery))
            {
                DisableControls();
                SearchCancelled = false;
                string sq = this.SearchQuery;
                Task.Factory.StartNew(new Action(() =>
                {
                    var res = Imdb.Search(sq);

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

        private void Commands_CancelSearch(object sender, ExecutedRoutedEventArgs e)
        {
            this.SearchCancelled = true;
            EnableControls();
        }

        private void Commands_Pick(object sender, ExecutedRoutedEventArgs e)
        {
            Torrent.PickedMovieData = e.Parameter as ImdbSearchResult;
            Torrent.LoadMovieDataIntoFolder();
            this.Close();
        }
    }
}
