using DevExpress.Xpf.Core;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace OpenDL.Service
{
    public class DialogService
    {
        public DialogService()
        {

        }

        public void ShowErrorMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DXMessageBox.Show(message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            });
            
        }

        public void ShowConfirmMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DXMessageBox.Show(message, "Confirm", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            });
        }
    }
}
