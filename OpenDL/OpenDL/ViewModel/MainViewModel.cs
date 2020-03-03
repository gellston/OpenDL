
using DevExpress.Utils.Design;
using DevExpress.Utils.Svg;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

using static Tensorflow.Binding;
using Tensorflow;



namespace OpenDL.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        public MainViewModel()
        {

            this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<ReleaseNoteViewModel>();

            this.WorkType = this._NonSelection;

            MenuItems.Add(new MainMenu()
            {
                Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Business Objects/BO_Note.svg")), 1d, null, null, true),
                Name = "릴리즈 노트",
                MenuAction = new RelayCommand(() =>
                {
                    this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<ReleaseNoteViewModel>();
                })
            });

            MenuItems.Add(new MainMenu()
            {
                Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Icon Builder/Actions_Label.svg")), 1d, null, null, true),
                Name = "라벨링",
                MenuAction = new RelayCommand(() =>
                {
                    this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<LabelViewModel>();
                })
            });

            MenuItems.Add(new MainMenu()
            {
                Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Business Objects/BO_Opportunity.svg")), 1d, null, null, true),
                Name = "트레이닝",
                MenuAction = new RelayCommand(() =>
                {
                    this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<TrainViewModel>();
                })

            });

            MenuItems.Add(new MainMenu()
            {
                Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Spreadsheet/ManageRelations.svg")), 1d, null, null, true),
                Name = "모델 관리",
                MenuAction = new RelayCommand(() =>
                {
                    this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<ModelManagementViewModel>();
                })

            });



            Messenger.Default.Register<SelectWorkTypeMessage>(this, SelectWorkTypeCallback);
            Messenger.Default.Register<SegmentAugmentationMessage>(this, SegmentAugmentationCallback);
            Messenger.Default.Register<ClassificationAugmentationMessage>(this, ClassificationAugmentationCallback);
        }


        private void SelectWorkTypeCallback(SelectWorkTypeMessage message)
        {
            switch (message.Message)
            {
                case "Cancel":
                    this.IsPopup = false;
                    break;

                case "Clear":
                    this.CurrentContentViewModel = SimpleIoc.Default.GetInstance<ReleaseNoteViewModel>();
                    this.WorkType = this._NonSelection;
                    this.IsOpenLeftMenu = false;
                    this.IsPopup = false;
                    this.IsAblePopup = false;
                    break;

                case "Select":
                    this.WorkType = message.Type;
                    this.IsPopup = false;
                    this.IsOpenLeftMenu = true;
                    this.IsAblePopup = true;
                    break;
            };
        }


        private void SegmentAugmentationCallback(SegmentAugmentationMessage message)
        {
            switch (message.Message)
            {
                case "Open":
                    this.PopupWidth = 500;
                    this.PopupHeight = 500;
                    this.CurrentPopupViewModel = SimpleIoc.Default.GetInstance<SegmentationAugmentViewModel>();
                    this.IsPopup = true;
                    this.IsStayOpen = true;
                    break;

                case "Cancel":
                    this.IsPopup = false;
                    this.IsStayOpen = false;
                    break;

            };
        }

        private void ClassificationAugmentationCallback(ClassificationAugmentationMessage message)
        {
            switch (message.Message)
            {
                case "Open":
                    this.PopupWidth = 500;
                    this.PopupHeight = 500;
                    this.CurrentPopupViewModel = SimpleIoc.Default.GetInstance<ClassificationAugmentViewModel>();
                    this.IsPopup = true;
                    this.IsStayOpen = true;
                    break;

                case "Cancel":
                    this.IsPopup = false;
                    this.IsStayOpen = false;
                    break;

            };
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

        private bool _IsStayOpen = false;
        public bool IsStayOpen
        {
            get => _IsStayOpen;
            set => Set<bool>(nameof(IsStayOpen), ref _IsStayOpen, value);
        }


        private bool _IsAblePopup = false;
        public bool IsAblePopup
        {
            get => _IsAblePopup;
            set => Set<bool>(nameof(IsAblePopup), ref _IsAblePopup, value);
        }

        private bool _IsOpenLeftMenu = false;
        public bool IsOpenLeftMenu
        {
            get => _IsOpenLeftMenu;
            set => Set<bool>(nameof(IsOpenLeftMenu), ref _IsOpenLeftMenu, value);
        }


        private int _PopupWidth = 0;
        public int PopupWidth
        {
            get => _PopupWidth;
            set => Set<int>(nameof(PopupWidth), ref _PopupWidth, value);
        }


        private int _PopupHeight = 0;
        public int PopupHeight
        {
            get => _PopupHeight;
            set => Set<int>(nameof(PopupHeight), ref _PopupHeight, value);
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



        private ICommand _OpenSelectWorkTypeCommand = null;
        public ICommand OpenSelectWorkTypeCommand
        {
            get
            {
                if (_OpenSelectWorkTypeCommand == null)
                {
                    _OpenSelectWorkTypeCommand = new RelayCommand(() =>
                    {
                        this.CurrentPopupViewModel = SimpleIoc.Default.GetInstance<SelectWorkTypeViewModel>();
                        this.PopupWidth = 300;
                        this.PopupHeight = 300;
                        this.IsPopup = !this.IsPopup;

                    });
                }

                return _OpenSelectWorkTypeCommand;
            }
        }


        private WorkTypeMenu _NonSelection
        {

            get
            {
                return new WorkTypeMenu()
                {
                    Name = "선택되지 않음",
                    Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Spreadsheet/ChartLegend_None.svg")), 1d, null, null, true)
                };
            }
        }

        private WorkTypeMenu _WorkType = null;
        public WorkTypeMenu WorkType
        {
            get => _WorkType;
            set => Set<WorkTypeMenu>(nameof(WorkType), ref _WorkType, value);
        }
    }
}
