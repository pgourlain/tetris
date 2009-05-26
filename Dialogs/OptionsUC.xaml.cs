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
using System.Configuration;
using GeniusTetris.Multiplayer;

namespace GeniusTetris.Dialogs
{
    /// <summary>
    /// Interaction logic for OptionsUC.xaml
    /// </summary>

    public partial class OptionsUC : System.Windows.Controls.UserControl
    {
        public OptionsUC(GameOptions options)
        {
            this.DataContext = options;
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        void CloseClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
        }

        void CloseOkClick(object sender, RoutedEventArgs e)
        {
            ((GameOptions)this.DataContext).Save();
            ModalService.CloseModal();
        }
    }
}