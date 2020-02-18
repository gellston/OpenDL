using DevExpress.Xpf.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Service
{
    public class FolderBrowserService
    {

        public string SelectFolder()
        {

            DXFolderBrowserDialog dialog = new DXFolderBrowserDialog();
            dialog.ShowDialog();


            return "";
            
        }
    }
}
