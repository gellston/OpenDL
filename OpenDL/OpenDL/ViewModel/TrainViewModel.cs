using System;
using System.Collections.Generic;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using OpenDL.Model;

namespace OpenDL.ViewModel
{
    public class TrainViewModel : ViewModelBase
    {

        public TrainViewModel()
        {

            Messenger.Default.Register<SelectWorkTypeMessage>(this, SelectWorkTypeCallback);
        }

        private void SelectWorkTypeCallback(SelectWorkTypeMessage message)
        {
            if (message.Type == null) return;
            switch (message.Type.Name)
            {
                case "Segmentation":
                    this.TrainViewModelContent = SimpleIoc.Default.GetInstance<SegmentationTrainViewModel>();
                    break;
                case "Classification":
                    this.TrainViewModelContent = SimpleIoc.Default.GetInstance<ClassificationTrainViewModel>();
                    break;
            };
        }


        private ViewModelBase _TrainViewModelContent = null;
        public ViewModelBase TrainViewModelContent
        {
            get => _TrainViewModelContent;
            set => Set<ViewModelBase>(nameof(TrainViewModelContent), ref _TrainViewModelContent, value);
        }
    }
}
