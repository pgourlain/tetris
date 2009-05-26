using System;
using System.Collections.Generic;
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
using GeniusTetris.Services;
using System.Diagnostics;
using GeniusTetris._3D;
using Microsoft.Samples.KMoore.WPFSamples.AnimatingTilePanel;

namespace GeniusTetris.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutUC.xaml
    /// </summary>

    public partial class AboutUC : System.Windows.Controls.UserControl
    {
        public AboutUC()
        {
            InitializeComponent();
        }

        #region private class
        public sealed class W : GeniusTetris.Core.Shape
        {
            public W()
                : base(5, 5)
            {
                this[0, 1] = this[0, 2] = this[0, 3] = this[1, 0] = this[2, 1] = this[2, 2] = 1;
                this[2, 3] = this[3, 0] = this[4, 1] = this[4, 2] = this[4, 3] = 1;
            }
        }
        public sealed class P : GeniusTetris.Core.Shape
        {
            public P()
                : base(4,5)
            {
                this[0, 0] = 2;
                this[1, 4] = this[2, 4] = this[3, 3] = 2;
                this[0, 1] = this[0, 2] = this[0, 3] = this[0, 4] = 2;
                this[1, 2] = this[2, 2] = 2;
            }
        }
        public sealed class F : GeniusTetris.Core.Shape
        {
            public F()
                : base(4, 5)
            {
                this[0, 0] = 3;
                this[1, 4] = this[2, 4] = this[3, 4] = 3;
                this[0, 1] = this[0, 2] = this[0, 3] = this[0, 4] = 3;
                this[1, 2] = this[2, 2] = 3;
            }
        }

        #endregion

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            String st = "Genius Tetris";

            DialogStringAnimation.AddStringAnimation(st, this.tilePanel, 40);
            Random rnd = new Random(Environment.TickCount);
            View3DUC uc = new View3DUC();
            uc.Shape = new W();
            this.tilePanel1.Children.Add(uc);

            uc = new View3DUC();
            uc.Shape = new P();
            this.tilePanel1.Children.Add(uc);

            uc = new View3DUC();
            uc.Shape = new F();
            this.tilePanel1.Children.Add(uc);
        }

        void CloseClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
        }

        void OnGotoSite(object sender, RoutedEventArgs e)
        {
            string url = ((Hyperlink)sender).NavigateUri.OriginalString;
            Process.Start(new ProcessStartInfo(url));
        }
    }
}