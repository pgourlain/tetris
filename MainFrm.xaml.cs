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
using System.Windows.Shapes;
using GeniusTetris.Core;
using System.Collections.ObjectModel;
using GeniusTetris.Services;
/**********************************************************************************************************
 * Author : Pierrick Gourlain
 * December 2006
 * 
 * http://blogs.developpeur.org/pierrick/
 * 
 *********************************************************************************************************/

namespace GeniusTetris
{
    /// <summary>
    /// Interaction logic for MainFrm.xaml
    /// </summary>

    public partial class MainFrm : System.Windows.Window
    {
        public MainFrm()
        {
            InitializeComponent();
            ModalService.ModalHost = this.ModalHost;
            ModalService.ModalHostContent = this.ModalContent;
            ModalService.RootParent = this.game;
            ExceptionsManager.ConnectToApplicationExceptions();
        }

        void SizeWindow(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            int xChange = (int)e.HorizontalChange;
            int yChange = (int)e.VerticalChange;

            double x = this.Left;
            double y = this.Top;
            double width = this.Width;
            double height = this.Height;

            switch (((FrameworkElement)sender).Name)
            {
                case "sizeWindowUpLeft":
                    x += xChange;
                    y += yChange;
                    width += -xChange;
                    height += -yChange;
                    break;
                case "sizeWindowUp":
                    y += yChange;
                    height += -yChange;
                    break;
                case "sizeWindowUpRight":
                    y += yChange;
                    width += xChange;
                    height += -yChange;
                    break;
                case "sizeWindowLeft":
                    x += xChange;
                    width += -xChange;
                    break;
                case "sizeWindowRight":
                    width += xChange;
                    break;
                case "sizeWindowDownLeft":
                    x += xChange;
                    width += -xChange;
                    height += yChange;
                    break;
                case "sizeWindowDown":
                    height += yChange;
                    break;
                case "sizeWindowDownRight":
                    width += xChange;
                    height += yChange;
                    break;
            }
            this.Left = x;
            this.Top = y;
            this.Height = height;
            this.Width = width;
        }

        void MoveWindow(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            int xChange = (int)e.HorizontalChange;
            int yChange = (int)e.VerticalChange;
            this.Left += xChange;
            this.Top += yChange;
        }

        void CloseApplication(object sender, System.Windows.RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        void MinimizeApplication(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        void MaximizeApplication(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        void OnModalClose(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ModalContent.Children.Count == 1)
            {
                IModalDialog dlg = ModalContent.Children[0] as IModalDialog;
                if (dlg != null)
                    dlg.CloseButtonClick();
            }
            ModalService.CloseModal();
            //this.ModalHost.Visibility = Visibility.Collapsed;
            //foreach (UIElement element in this.ModalContent.Children)
            //{
            //    if (element is IDisposable)
            //        ((IDisposable)element).Dispose();
            //}
            //this.ModalContent.Children.Clear();
        }



        private void CanClose(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OnQuit(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }


    }
}