using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using GeniusTetris.Services;
using GeniusTetris.Dialogs;
using System.Threading;

namespace GeniusTetris
{
    /// <summary>
    /// Exception management
    /// </summary>
    static class ExceptionsManager
    {
        public static void ConnectToApplicationExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowException(e.ExceptionObject as Exception);
        }

        static void ShowException(Exception ex)
        {
            ExceptionDlgUC dlg = new ExceptionDlgUC(ex);
            ModalService.ShowModal(dlg);
        }

        static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            ShowException(e.Exception);
        }
    }
}
