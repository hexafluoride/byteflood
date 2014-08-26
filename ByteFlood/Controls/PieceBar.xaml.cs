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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteFlood.Controls
{
    /// <summary>
    /// Interaction logic for PieceBar.xaml
    /// </summary>
    public partial class PieceBar : UserControl
    {

        public PieceInfo[] PieceList
        {
            get { return (PieceInfo[])GetValue(PieceListProperty); }
            set { SetValue(PieceListProperty, value); }
        }

        public static readonly DependencyProperty PieceListProperty =
            DependencyProperty.Register("PieceList", typeof(PieceInfo[]), typeof(PieceBar), new PropertyMetadata(null));

        int PieceCount 
        {
            get { if (PieceList != null) return PieceList.Length;  else return 0; }
        }

        public PieceBar()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (PieceCount > 0)
            {
                //clear in uncomplete color;
                drawingContext.DrawRectangle(Brushes.AliceBlue, new Pen(Brushes.Black, 1), new Rect(new Size(this.ActualWidth, this.ActualHeight)));

                for (int i = 0; i < this.PieceList.Length; i++)
                {
                    double offset = i / PieceCount * this.ActualWidth;

                    if (this.PieceList[i] != null)
                    {
                        PieceInfo p = this.PieceList[i];
                        if (p.Finished) 
                        {
                            drawingContext.DrawLine(new Pen(Brushes.Violet, 1), new Point(offset, 0), new Point(offset, this.ActualHeight));
                        }
                    }

                }
            }
            else { base.OnRender(drawingContext); }

           
        }
    }
}
