using DevExpress.Xpf.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenDL.Service
{
    public class FileBrowserService
    {

        public string SelectFolder()
        {
            DXFolderBrowserDialog dialog = new DXFolderBrowserDialog();
            if(dialog.ShowDialog() == false)
                return "";

            return dialog.SelectedPath;
        }

        public string SaveOneFile(string filter)
        {
            DXSaveFileDialog dialog = new DXSaveFileDialog();
            dialog.Filter = filter;
            
            if (dialog.ShowDialog() == false)
                return "" ;

            return dialog.FileName;
        }



        public string[] ImageListFromFolder(string _folderPath)
        {

            if (_folderPath.Length <= 0)
                return null;

            string[] files = Directory.GetFiles(_folderPath, "*.*", SearchOption.AllDirectories)
                             .Where(s => s.EndsWith(".bmp") || s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".png")).ToArray();

            return files;
        }


    }
}
