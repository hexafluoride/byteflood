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
using System.IO;

namespace ByteFlood.Controls
{
    public class PieceBar : Image
    {
        public PieceBar()
        {
            this.SizeChanged += (s, e) =>
            {
                Redraw();
            };
        }

        private MonoTorrent.Common.BitField bit = null;

        private System.Drawing.Bitmap bi = null;

        private TorrentInfo ti;

        public void AttachTorrent(TorrentInfo ti)
        {
            this.ti = ti;
            CleanUP();
            //this.bit = ti.Torrent.Bitfield;
            //ti.Torrent.PieceHashed += Torrent_PieceHashed;
        }

        public void DetachTorrent()
        {
            CleanUP();
            this.bit = null;
            //if (ti != null) { ti.Torrent.PieceHashed -= this.Torrent_PieceHashed; }
        }

        void Torrent_PieceHashed(object sender, MonoTorrent.Client.PieceHashedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                Redraw();
            }));
        }

        void Redraw()
        {
            if (bit != null)
            {
                bi = new System.Drawing.Bitmap(Convert.ToInt32(this.Width), Convert.ToInt32(this.Height));

                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Brushes.Black);

                pen.Width = Convert.ToSingle(this.Width / bit.Length);

                using (var g = System.Drawing.Graphics.FromImage(bi))
                {
                    g.Clear(System.Drawing.Color.White);

                    for (int index = 0; index < bit.Length; index++)
                    {
                        if (bit[index])
                        {
                            g.DrawLine(pen, index, 0, index, Convert.ToSingle(this.Height));
                        }
                    }
                }

                MemoryStream m = new MemoryStream();

                ((System.Drawing.Image)bi).Save(m, System.Drawing.Imaging.ImageFormat.Png);

                BitmapImage wbi = new BitmapImage();

                wbi.BeginInit();
                wbi.CacheOption = BitmapCacheOption.Default;
                wbi.StreamSource = m;
                wbi.EndInit();
                wbi.Freeze();
                this.Source = wbi;

                bi.Dispose();
            }
            else 
            {
                CleanUP();
            }
        }

        private void CleanUP()
        {
            if (bi != null)
            {
                base.Source = null;
                bi.Dispose();
            }
        }


    }
}
