using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using OpenDL.Model;
using OpenDL.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.ViewModel
{
    public class ModelManagementViewModel : ViewModelBase
    {

        private readonly FileBrowserService folderDialogService;
        public ModelManagementViewModel(FileBrowserService _folderDialogService)
        {
            folderDialogService = _folderDialogService;


            Messenger.Default.Register<SelectWorkTypeMessage>(this, SelectWorkTypeCallback);
        }


        private ViewModelBase _ManagementViewModel = null;
        public ViewModelBase ManagementViewModel
        {
            get => _ManagementViewModel;
            set => Set<ViewModelBase>(nameof(ModelManagementViewModel), ref _ManagementViewModel, value);
        }


        private void SelectWorkTypeCallback(SelectWorkTypeMessage message)
        {
            if (message.Type == null) return;
            switch (message.Type.Name)
            {

                case "Segmentation":
                    this.ManagementViewModel = SimpleIoc.Default.GetInstance<SegmentationModelManagementViewModel>();
                    break;

                case "Classification":
                    this.ManagementViewModel = SimpleIoc.Default.GetInstance<ClassificationModelManagementViewModel>();
                    break;

                default:
                    this.ManagementViewModel = null;
                    break;
            };
        }
    }
}
