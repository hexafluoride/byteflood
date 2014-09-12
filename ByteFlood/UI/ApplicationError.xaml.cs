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
    /// Interaction logic for ApplicationErro.xaml
    /// </summary>
    public partial class ApplicationError : Window
    {

        public Visibility WillClose
        {
            get { return (Visibility)GetValue(WillCloseProperty); }
            set { SetValue(WillCloseProperty, value); }
        }

        public static readonly DependencyProperty WillCloseProperty =
            DependencyProperty.Register("WillClose", typeof(Visibility), typeof(ApplicationError), new PropertyMetadata(Visibility.Collapsed));


        public Exception ExceptionData
        {
            get { return (Exception)GetValue(ExceptionDataProperty); }
            set { SetValue(ExceptionDataProperty, value); }
        }

        public static readonly DependencyProperty ExceptionDataProperty =
            DependencyProperty.Register("ExceptionData", typeof(Exception), typeof(ApplicationError), new PropertyMetadata(null));

        public ApplicationError()
        {
            InitializeComponent();
        }

        private void Commands_Close(object sender, ExecutedRoutedEventArgs e)
        {
            if (WillClose == Visibility.Visible)
            {
                //try save (Shutdown already saves the state and config)
                State state = ((MainWindow)App.Current.MainWindow).state;
                state.uiContext.Send(x => state.Shutdown(), null); 
                Environment.Exit(-1);
            }

            this.Close();
        }

        private void Commands_Copy(object sender, ExecutedRoutedEventArgs e)
        {
            if (ExceptionData == null) { return; }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(ExceptionData.Message);

            sb.AppendLine("Stacktrace:");
            sb.AppendLine(ExceptionData.StackTrace);
            sb.AppendLine();

            sb.AppendLine("Source:");
            sb.AppendLine(ExceptionData.Source);

            Clipboard.SetText(sb.ToString());
        }
    }
}
