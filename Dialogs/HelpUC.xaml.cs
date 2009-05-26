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

namespace GeniusTetris.Dialogs
{
    /// <summary>
    /// Interaction logic for Help.xaml
    /// </summary>

    public partial class HelpUC : System.Windows.Controls.UserControl
    {
        public HelpUC()
        {
            InitializeComponent();
        }

        public void CloseClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
        }

    }
}