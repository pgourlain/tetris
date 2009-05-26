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
    /// Interaction logic for ExceptionDlgUC.xaml
    /// </summary>

    public partial class ExceptionDlgUC : System.Windows.Controls.UserControl
    {
        public ExceptionDlgUC(Exception ex)
        {
            InitializeComponent();
            ExceptionToString(ex);
        }

        private void ExceptionToString(Exception ex)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(string.Format("Exception : {0}", i++));
                sb.AppendLine(ex.ToString());
                ex = ex.InnerException;
            }
            tbLog.Text = sb.ToString();
        }

        void OkClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
        }
    }
}