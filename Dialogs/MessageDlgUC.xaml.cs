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
using System.ComponentModel;
using GeniusTetris.Services;

namespace GeniusTetris.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageDlgUC.xaml
    /// </summary>

    public partial class MessageDlgUC : System.Windows.Controls.UserControl, INotifyPropertyChanged, IModalDialog
    {
        private RoutedEventHandler _OkClick;
        private RoutedEventHandler _CancelClick;
        private string _Message;

        public MessageDlgUC(RoutedEventHandler OkClick, RoutedEventHandler CancelClick, bool okVisible, bool cancelVisible)
        {
            InitializeComponent();
            _OkClick = OkClick;
            _CancelClick = CancelClick;
            this.bOk.Visibility = okVisible ? Visibility.Visible : Visibility.Collapsed;
            this.bCancel.Visibility = cancelVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Message to display
        /// </summary>
        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                if (value != _Message)
                {
                    _Message = value;
                    DoChanged("Message");
                }
            }
        }
        private string _YesText = "Yes";
        public string YesText
        {
            get
            {
                return _YesText;
            }
            set
            {
                _YesText = value;
                DoChanged("YesText");
            }
        }

        private string _NoText = "No";
        public string NoText
        {
            get
            {
                return _NoText;
            }
            set
            {
                _NoText = value;
                DoChanged("NoText");
            }
        }



        private void DoChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        void YesClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
            if (_OkClick != null)
                _OkClick(this, e);
        }

        void NoClick(object sender, RoutedEventArgs e)
        {
            ModalService.CloseModal();
            if (_CancelClick != null)
                _CancelClick(this, e);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IModalDialog Members

        public void CloseButtonClick()
        {
            if (_CancelClick != null)
                _CancelClick(this, null);
        }

        #endregion
    }
}