using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Microsoft.Samples.KMoore.WPFSamples.AnimatingTilePanel;
using System.Windows.Shapes;

namespace GeniusTetris.Dialogs
{
    static class DialogStringAnimation
    {
        private static Grid NewLetter(char c, Brush foreground, double fontsize)
        {
            Grid g = new Grid();
            TextBlock tb = new TextBlock();
            tb.Margin = new Thickness(2, 2, 0, 0);
            tb.Text = new string(c, 1);
            tb.TextAlignment = TextAlignment.Center;
            tb.FontSize = fontsize;
            g.Children.Add(tb);
            tb = new TextBlock();
            tb.Text = new string(c, 1);
            tb.TextAlignment = TextAlignment.Center;
            tb.FontSize = fontsize;
            tb.Foreground = foreground;
            g.Children.Add(tb);
            return g;
        }

        public static void AddStringAnimation(string st, AniTilePanel aContent, double aFontSize)
        {
            Random rnd = new Random(Environment.TickCount);

            LinearGradientBrush br = new LinearGradientBrush(Color.FromRgb(0xc0, 0xc0, 0xc0), Color.FromRgb(0, 0, 200), new Point(0.0110642, 0.499497), new Point(1, 0.499681));
            foreach (char c in st)
            {
                StackPanel pnl = new StackPanel();

                pnl.Children.Add(NewLetter(c, br, aFontSize));
                Grid g = NewLetter(c, br, aFontSize);

                LinearGradientBrush maskBr = new LinearGradientBrush();
                maskBr.GradientStops.Add(new GradientStop(Color.FromArgb(0xff, 0x88, 0x88, 0x88), 0));
                maskBr.GradientStops.Add(new GradientStop(Color.FromArgb(0x4c, 0x88, 0x88, 0x88), 0.5));
                maskBr.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0x88, 0x88, 0x88), 0.6));
                maskBr.StartPoint = new Point(0.5, 1);
                maskBr.EndPoint = new Point(0.5, 0);
                g.RenderTransformOrigin = new Point(0.5, 0.5);
                g.RenderTransform = new ScaleTransform(1, -1);
                g.Margin = new Thickness(0, -(aFontSize / 3), 0, 0);
                g.OpacityMask = maskBr;
                pnl.Children.Add(g);

                //pnl.Background = Brushes.Yellow;
                AniTilePanel.SetChildLocation(pnl, new Point(-rnd.Next(300, 600), -rnd.Next(300, 600)));
                aContent.Children.Add(pnl);
            }
        }
    }
}
