using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OpenDL.Model;

namespace OpenDL.ViewModel
{
    public class AnomalyLabelViewModel : ViewModelBase
    {
        public AnomalyLabelViewModel()
        {

        }

        private ICommand _OpenAugmentationCommand = null;
        public ICommand OpenAugmentationCommand
        {
            get
            {
                if (_OpenAugmentationCommand == null)
                {
                    _OpenAugmentationCommand = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<AugmentationMessage>(new AugmentationMessage()
                        {
                            Message = "Open",
                            ViewModelType = "AnomalyDetection"
                        });
                    });
                }

                return _OpenAugmentationCommand;
            }
        }
    }
}
