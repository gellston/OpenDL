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
using OpenDL.Model;
using OpenDL.Service;

namespace OpenDL.ViewModel
{
    public class SegmentationLabelViewModel: ViewModelBase
    {
        private readonly FolderBrowserService folderBrowserService;
        private readonly LabelLoaderService labelLoaderService;

        public SegmentationLabelViewModel(FolderBrowserService _folderBrowserService,
                                          LabelLoaderService _labelLoaderService)
        {
            this.folderBrowserService = _folderBrowserService;
            this.labelLoaderService = _labelLoaderService;

            this.LabelCollection = new ObservableCollection<SegmentLabelPolygon>();
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
                if(_AddLabelCommand == null)
                {
                    _AddLabelCommand = new RelayCommand(() => {
                        this.LabelCollection.Add(new SegmentLabelPolygon()
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
                            foreach (SegmentLabelPolygon polygon in temp.OfType<SegmentLabelPolygon>().ToList())
                            {
                                this.LabelCollection.Remove(polygon);
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
                            foreach (SegmentLabelPolygon polygon in temp.OfType<SegmentLabelPolygon>().ToList())
                            {
                                this.PolygonCollection.Remove(polygon);
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
                if(_OpenLabelCommand == null)
                {
                    _OpenLabelCommand = new RelayCommand(() =>
                    {
                        string folderPath = folderBrowserService.SelectFolder();
                        if (folderPath.Length <= 0)
                            return;

                        string[] files = folderBrowserService.ImageListFromFolder(folderPath);
                        ObservableCollection<SegmentLabelUnit> labels = labelLoaderService.LoadSegmentedLabel(files);
                        this.LabelUnitCollection = labels;
                        this.PolygonCollection = null;
                        this.SelectedLabelUnit = null;
                        this.SelectedPolygon = null;
                    });
                }

                return _OpenLabelCommand;
            }
        }



        private SegmentLabelPolygon _SelectedPolygon = null;
        public SegmentLabelPolygon SelectedPolygon
        {
            get => _SelectedPolygon;
            set
            {

                if(value != null)
                {
                    SegmentLabelPolygon polygon = value;
                    polygon.IsSelected = true;
                }

                if(_SelectedPolygon != null && value != _SelectedPolygon)
                {
                    _SelectedPolygon.IsSelected = false;
                }


                Set<SegmentLabelPolygon>(nameof(SelectedPolygon), ref _SelectedPolygon, value);
            }
        }


        private ObservableCollection<SegmentLabelPolygon> _PolygonCollection = null;
        public ObservableCollection<SegmentLabelPolygon> PolygonCollection
        {
            get
            {
                if (_PolygonCollection == null)
                    _PolygonCollection = new ObservableCollection<SegmentLabelPolygon>();

                return _PolygonCollection;
            }
            set => Set<ObservableCollection<SegmentLabelPolygon>>(nameof(PolygonCollection), ref _PolygonCollection, value);
        }



        private ObservableCollection<SegmentLabelPolygon> _LabelCollection = null;
        public ObservableCollection<SegmentLabelPolygon> LabelCollection
        {
            get
            {
                if (_LabelCollection == null)
                    _LabelCollection = new ObservableCollection<SegmentLabelPolygon>();

                return _LabelCollection;
            }
            set => Set<ObservableCollection<SegmentLabelPolygon>>(nameof(LabelCollection), ref _LabelCollection, value);
        }

        private SegmentLabelPolygon _SelectedTargetLabel = null;
        public SegmentLabelPolygon SelectedTargetLabel
        {
            get => _SelectedTargetLabel;
            set => Set<SegmentLabelPolygon>(nameof(SelectedTargetLabel), ref _SelectedTargetLabel, value);
        }




        private ObservableCollection<SegmentLabelUnit> _LabelUnitCollection = null;
        public ObservableCollection<SegmentLabelUnit> LabelUnitCollection
        {
            get => _LabelUnitCollection;
            set => Set<ObservableCollection<SegmentLabelUnit>>(nameof(LabelUnitCollection), ref _LabelUnitCollection, value);
        }


        private SegmentLabelUnit _SelectedLabelUnit = null;
        public SegmentLabelUnit SelectedLabelUnit
        {
            get => _SelectedLabelUnit;

            set
            {
                if(value != null)
                {
                    SegmentLabelUnit unit = value as SegmentLabelUnit;
                    this.PolygonCollection = unit.Polygons;
                    this.CurrentImage = new BitmapImage(new Uri(unit.FilePath));
                    this.SelectedPolygon = null;
                }

                Set<SegmentLabelUnit>(nameof(SelectedLabelUnit), ref _SelectedLabelUnit, value);
            }
        }
    }
}
