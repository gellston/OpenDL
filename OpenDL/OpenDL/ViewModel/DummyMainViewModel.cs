using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace OpenDL.ViewModel
{
    public class DummyMainViewModel : ViewModelBase
    {

        public DummyMainViewModel()
        {

            this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<ReleaseNoteViewModel>();

            MenuItems.Add(new MainMenu()
            {
                Name = "릴리즈 노트",
                MenuAction = new RelayCommand(() =>
                {
                    this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<DummyLabelViewModel>();
                })
            });



            MenuItems.Add(new MainMenu()
            {
                Name = "라벨링",
                MenuAction = new RelayCommand(() =>
                {
                    this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<DummyLabelViewModel>();
                })
            });


        }


        private ObservableCollection<MainMenu> _MenuItems = null;
        public ObservableCollection<MainMenu> MenuItems
        {
            get
            {
                if (_MenuItems == null)
                    _MenuItems = new ObservableCollection<MainMenu>();

                return _MenuItems;
            }
        }


        private ViewModelBase _CurrentPopupViewModel = null;
        public ViewModelBase CurrentPopupViewModel
        {
            get => _CurrentPopupViewModel;
            set => Set<ViewModelBase>(nameof(CurrentPopupViewModel), ref _CurrentPopupViewModel, value);
        }

        private ViewModelBase _CurrentContentViewModel = null;
        public ViewModelBase CurrentContentViewModel
        {
            get => _CurrentContentViewModel;
            set => Set<ViewModelBase>(nameof(CurrentContentViewModel), ref _CurrentContentViewModel, value);
        }



        private bool _IsPopup = false;
        public bool IsPopup
        {
            get => _IsPopup;
            set => Set<bool>(nameof(IsPopup), ref _IsPopup, value);
        }

        private bool _IsOpenLeftMenu = false;
        public bool IsOpenLeftMenu
        {
            get => _IsOpenLeftMenu;
            set => Set<bool>(nameof(IsOpenLeftMenu), ref _IsOpenLeftMenu, value);
        }


        private ICommand _OpenLeftMenuCommand = null;
        public ICommand OpenLeftMenuCommand
        {
            get
            {
                if (_OpenLeftMenuCommand == null)
                {
                    _OpenLeftMenuCommand = new RelayCommand(() =>
                    {

                        this.IsOpenLeftMenu = !this.IsOpenLeftMenu;

                    });
                }

                return _OpenLeftMenuCommand;
            }
        }

        private string _WorkType = "선택안됨";
        public string WorkType
        {
            get => _WorkType;
            set => Set<string>(nameof(WorkType), ref _WorkType, value);
        }
    }
}
