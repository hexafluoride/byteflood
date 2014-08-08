/* 
    ByteFlood - A BitTorrent client.
    Copyright (C) 2014 Burak Öztunç

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Common;
using Microsoft.Win32;
using System.Threading;
using System.Net;

namespace ByteFlood
{
    class GraphDrawer
    {
        Canvas graph;
        public GraphDrawer(Canvas canvas)
        {
            graph = canvas;
        }
        public Thickness GetSize() // This returns thickness just because.
        {
            // thickness.left = left
            // thickness.top = top
            // thickness.right = the rightest point of the graph, subtract left to get width
            // thickness.bottom = the lowest point of the graph, subtract top to get height
            double xmin = 0;
            double ymin = 0;
            double xmax = 0;
            double ymax = 0;
            bool first = true;
            foreach (UIElement element in graph.Children)
            {
                if (element is Line)
                {
                    Line line = element as Line;
                    if (first)
                    {
                        xmin = line.X1;
                        ymin = line.Y1;
                        xmax = line.X2;
                        ymax = line.Y2;
                        first = false;
                        continue;
                    }
                    Utility.SetIfHigherThan(ref xmax, line.X1); // ha ha ha, oh wow
                    Utility.SetIfHigherThan(ref xmax, line.X2);
                    Utility.SetIfLowerThan(ref xmin, line.X1);
                    Utility.SetIfLowerThan(ref xmin, line.X2);
                    Utility.SetIfHigherThan(ref ymax, line.Y1);
                    Utility.SetIfHigherThan(ref ymax, line.Y2);
                    Utility.SetIfLowerThan(ref ymin, line.Y1);
                    Utility.SetIfLowerThan(ref ymin, line.Y2);
                }
            }
            return new Thickness(xmin, ymin, xmax, ymax);
        }
        public void Clear()
        {
            graph.Children.Clear();
        }
        public void DrawGrid(double left, double top, double width, double height)
        {
            for (int i = 0; i <= 5; i++) // horizontal lines
            {
                double y = Utility.CalculateLocation(height / 5, i);
                y += top; // compensate for the margins
                Line line = Utility.GenerateLine(left, y, width, y, Brushes.LightGray, 1);
                graph.Children.Add(line);
                Canvas.SetZIndex(line, -1); // this ensures the grid stays under the graph lines
            }
            for (int i = 1; i <= 50; i++) // vertical lines
            {
                double x = Utility.CalculateLocation(width / 50, i);
                x += left; // compensate for the margins
                Line line = Utility.GenerateLine(x, top, x, height, Brushes.LightGray, 1);
                graph.Children.Add(line);
                Canvas.SetZIndex(line, -1); // this ensures the grid stays under the graph lines
            }
        }
        public void Draw(float[] down, float[] up, bool drawdown, bool drawup) // I don't like parts of this
        {
            float[] highest_data = drawdown && drawup ? // Ugly, but compact
                ((down.Max() > up.Max()) ? down : up) :
                drawdown ? down : up;
            float[] lowest_data = drawdown && drawup ? // same
                ((down.Min() < up.Min()) ? down : up) :
                drawdown ? down : up;
            double width = graph.ActualWidth;
            double height = graph.ActualHeight;
            double highest = highest_data.Max();
            double lowest = lowest_data.Min();
            double spp = height / (highest - lowest); // The number of vertical pixels to increase per graph unit
            if (highest == 0) // this prevents spp from going NaN
            {
                spp = 0;
            }
            double xpp = (width - 60) / down.Length;
            double h_margin = Utility.CalculateLocation(spp, lowest); // we subtract the lowest point of the graph from all points so the lowest point corresponds to 0
            if (drawdown)
                DrawData(down, xpp, spp, h_margin, height, Brushes.Green); // download data
            if (drawup)
                DrawData(up, xpp, spp, h_margin, height, Brushes.Red); // upload data
            Thickness size = GetSize();
            double left_margin = size.Right + 2;
            double right_margin = 10;

            double up_margin_max = -5; // very very ugly
            double up_margin_mid = (height - 25) / 2;
            double up_margin_low = height - 20;

            Label l_max = Utility.GenerateLabel(
                Utility.PrettifySpeed((int)highest),
                new Thickness(left_margin, up_margin_max, right_margin, 0));
            Label l_mid = Utility.GenerateLabel(
                Utility.PrettifySpeed((int)(highest + lowest) / 2),
                new Thickness(left_margin, up_margin_mid, right_margin, 0));
            Label l_low = Utility.GenerateLabel(
                Utility.PrettifySpeed((int)lowest),
                new Thickness(left_margin, up_margin_low, right_margin, 0));
            graph.Children.Add(l_max);
            graph.Children.Add(l_mid);
            graph.Children.Add(l_low);
        }
        public void DrawData(float[] data, double xpp, double spp, double h_margin, double height, Brush color)
        {
            double xprev = 0;
            double yprev = height;
            for (int i = 0; i < data.Length; i++)
            {
                if (i > data.Length - 1)
                    break;
                float d = data[i];
                double w_loc = i * xpp;
                double h_loc = Utility.CalculateLocation(spp, d);
                h_loc -= h_margin;
                h_loc = height - h_loc;
                graph.Children.Add(Utility.GenerateLine(xprev, yprev, w_loc, h_loc, color));
                if (yprev == height)
                    yprev = h_loc;
                xprev = w_loc;
                yprev = h_loc;
            }
        }
    }
}
