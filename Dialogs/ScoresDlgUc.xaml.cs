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
    /// Interaction logic for ScoresDlgUC.xaml
    /// </summary>

    public partial class ScoresDlgUC : System.Windows.Controls.UserControl
    {
        public ScoresDlgUC()
        {
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.lbScores.Focus();
        }

        void OkClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
        }
    }
}