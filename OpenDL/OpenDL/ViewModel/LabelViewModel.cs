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
    public class LabelViewModel : ViewModelBase
    {

        private readonly FolderBrowserService folderDialogService;
        public LabelViewModel(FolderBrowserService _folderDialogService)
        {
            folderDialogService = _folderDialogService;


            Messenger.Default.Register<SelectWorkTypeMessage>(this, SelectWorkTypeCallback);
        }


        private ViewModelBase _LabelViewModelContent = null;
        public ViewModelBase LabelViewModelContent
        {
            get => _LabelViewModelContent;
            set => Set<ViewModelBase>(nameof(LabelViewModelContent), ref _LabelViewModelContent, value);
        }


        private void SelectWorkTypeCallback(SelectWorkTypeMessage message)
        {
            if (message.Type == null) return;
            switch (message.Type.Name)
            {

                case "Segmentation":
                    this.LabelViewModelContent = SimpleIoc.Default.GetInstance<SegmentationLabelViewModel>();
                    break;

                case "Classification":
                    this.LabelViewModelContent = SimpleIoc.Default.GetInstance<ClassificationLabelViewModel>();
                    break;
            };
        }
    }
}
