using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using OpenDL.Model;
using OpenDL.Service;

namespace OpenDL.ViewModel
{
    public class ClassificationLabelViewModel : ViewModelBase
    {
        private readonly FileBrowserService folderBrowserService;
        private readonly LabelingService labelLoaderService;
        private readonly DialogService dialogService;

        private string openFolderLocation;

        public ClassificationLabelViewModel(FileBrowserService _folderBrowserService,
                                          LabelingService _labelLoaderService,
                                          DialogService _dialogService)
        {
            this.folderBrowserService = _folderBrowserService;
            this.labelLoaderService = _labelLoaderService;
            this.dialogService = _dialogService;

            this.LabelCollection = new ObservableCollection<ClassificationLabelBox>();
        }


        private BitmapImage _CurrentImage = null;
        public BitmapImage CurrentImage
        {
            get => _CurrentImage;
            set => Set<BitmapImage>(nameof(CurrentImage), ref _CurrentImage, value);
        }


        private ICommand _AddLabelCommand = null;
        public ICommand AddLabelCommand
        {
            get
            {
                if (_AddLabelCommand == null)
                {
                    _AddLabelCommand = new RelayCommand(() => {
                        this.LabelCollection.Add(new ClassificationLabelBox()
                        {
                            Name = "이름 미지정"
                        });
                    });
                }

                return _AddLabelCommand;
            }
        }


        private ICommand _DeleteLabelCommand = null;
        public ICommand DeleteLabelCommand
        {
            get
            {
                if (_DeleteLabelCommand == null)
                {
                    _DeleteLabelCommand = new RelayCommand<object>((_collection) =>
                    {
                        System.Collections.IList temp = _collection as System.Collections.IList;

                        if (temp != null)
                        {
                            foreach (ClassificationLabelBox box in temp.OfType<ClassificationLabelBox>().ToList())
                            {
                                this.LabelCollection.Remove(box);
                            }
                        }
                    });

                }

                return _DeleteLabelCommand;
            }
        }


        private ICommand _DeletePolygonCommand = null;
        public ICommand DeletePolygonCommand
        {
            get
            {
                if (_DeletePolygonCommand == null)
                {
                    _DeletePolygonCommand = new RelayCommand<object>((_collection) =>
                    {
                        System.Collections.IList temp = _collection as System.Collections.IList;

                        if (temp != null)
                        {
                            foreach (ClassificationLabelBox box in temp.OfType<ClassificationLabelBox>().ToList())
                            {
                                this.BoxCollection.Remove(box);
                            }
                        }
                    });

                }

                return _DeletePolygonCommand;
            }
        }


        private ICommand _OpenLabelCommand = null;
        public ICommand OpenLabelCommand
        {
            get
            {
                if (_OpenLabelCommand == null)
                {
                    _OpenLabelCommand = new RelayCommand(async () =>
                    {
                        string folderPath = folderBrowserService.SelectFolder();
                        if (folderPath.Length <= 0)
                            return;

                        this.openFolderLocation = folderPath;


                        string[] files = folderBrowserService.ImageListFromFolder(folderPath);
                        var labelResult = await labelLoaderService.LoadClassLabelAsync(folderPath, files);
                        this.LabelCollection = labelResult.Item1;
                        this.LabelUnitCollection = labelResult.Item2;
                        this.BoxCollection = null;
                        this.SelectedLabelUnit = null;
                        this.SelectedBox = null;

                    });
                }

                return _OpenLabelCommand;
            }
        }

        private ICommand _SaveLabelCommand = null;
        public ICommand SaveLabelCommand
        {
            get
            {
                if (_SaveLabelCommand == null)
                {
                    _SaveLabelCommand = new RelayCommand(async () =>
                    {

                        try
                        {
                            await labelLoaderService.SaveClassLabelInformationAsync(this.openFolderLocation, this.LabelCollection, this.LabelUnitCollection);
                        }
                        catch (Exception e)
                        {
                            this.dialogService.ShowErrorMessage(e.ToString());
                        }


                    });
                }

                return _SaveLabelCommand;
            }
        }


        private ICommand _KeyDownEventCommand = null;
        public ICommand KeyDownEventCommand
        {
            get
            {
                if (_KeyDownEventCommand == null)
                {
                    _KeyDownEventCommand = new RelayCommand<object>((args) =>
                    {
                        if (Keyboard.IsKeyDown(Key.LeftAlt) && Keyboard.IsKeyDown(Key.Enter))
                        {
                            if (this.SelectedLabelUnit == null) return;

                            int index = this.LabelUnitCollection.IndexOf(this.SelectedLabelUnit);
                            index += 1;
                            if (index >= this.LabelUnitCollection.Count)
                                index -= 1;

                            this.SelectedLabelUnit = this.LabelUnitCollection[index];
                        }

                    });
                }

                return _KeyDownEventCommand;
            }
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
                        Messenger.Default.Send<ClassificationAugmentationMessage>(new ClassificationAugmentationMessage()
                        {
                            Message = "Open"
                        });
                    });
                }

                return _OpenAugmentationCommand;
            }
        }



        private ClassificationLabelBox _SelectedBox = null;
        public ClassificationLabelBox SelectedBox
        {
            get => _SelectedBox;
            set
            {

                if (value != null)
                {
                    ClassificationLabelBox box = value;
                    box.IsSelected = true;
                }

                if (_SelectedBox != null && value != _SelectedBox)
                {
                    _SelectedBox.IsSelected = false;
                }


                Set<ClassificationLabelBox>(nameof(SelectedBox), ref _SelectedBox, value);
            }
        }


        private ObservableCollection<ClassificationLabelBox> _BoxCollection = null;
        public ObservableCollection<ClassificationLabelBox> BoxCollection
        {
            get
            {
                if (_BoxCollection == null)
                    _BoxCollection = new ObservableCollection<ClassificationLabelBox>();

                return _BoxCollection;
            }
            set => Set<ObservableCollection<ClassificationLabelBox>>(nameof(BoxCollection), ref _BoxCollection, value);
        }



        private ObservableCollection<ClassificationLabelBox> _LabelCollection = null;
        public ObservableCollection<ClassificationLabelBox> LabelCollection
        {
            get
            {
                if (_LabelCollection == null)
                    _LabelCollection = new ObservableCollection<ClassificationLabelBox>();

                return _LabelCollection;
            }
            set => Set<ObservableCollection<ClassificationLabelBox>>(nameof(LabelCollection), ref _LabelCollection, value);
        }

        private ClassificationLabelBox _SelectedTargetLabel = null;
        public ClassificationLabelBox SelectedTargetLabel
        {
            get => _SelectedTargetLabel;
            set => Set<ClassificationLabelBox>(nameof(SelectedTargetLabel), ref _SelectedTargetLabel, value);
        }




        private ObservableCollection<ClassLabelUnit> _LabelUnitCollection = null;
        public ObservableCollection<ClassLabelUnit> LabelUnitCollection
        {
            get => _LabelUnitCollection;
            set => Set<ObservableCollection<ClassLabelUnit>>(nameof(LabelUnitCollection), ref _LabelUnitCollection, value);
        }


        private ClassLabelUnit _SelectedLabelUnit = null;
        public ClassLabelUnit SelectedLabelUnit
        {
            get => _SelectedLabelUnit;

            set
            {
                if (value != null)
                {
                    ClassLabelUnit unit = value as ClassLabelUnit;
                    this.BoxCollection = unit.Boxes;
                    this.CurrentImage = new BitmapImage(new Uri(unit.FilePath));
                    this.SelectedBox = null;
                }

                Set<ClassLabelUnit>(nameof(SelectedLabelUnit), ref _SelectedLabelUnit, value);
            }
        }
    }
}
