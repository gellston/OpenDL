using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.ViewModel
{
    public class PopupControlViewModel : ViewModelBase
    {

        public PopupControlViewModel()
        {

        }


        private ViewModelBase _CurrentViewModel = null;
        public ViewModelBase CurrentViewModel
        {
            set => Set<ViewModelBase>(nameof(CurrentViewModel), ref _CurrentViewModel, value);
            get => _CurrentViewModel;
        }
    }
}
