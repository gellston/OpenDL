using GalaSoft.MvvmLight;
using OpenDL.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.ViewModel
{
    public class LabelViewModel : ViewModelBase
    {

        private readonly FolderBrowserService folderDialogService;
        public LabelViewModel(FolderBrowserService _folderDialogService)
        {
            folderDialogService = _folderDialogService;

        }
    }
}
