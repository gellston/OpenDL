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

            this.CurrentImage = new BitmapImage(new Uri("pack://application:,,,/OpenDL;component/Image/Orange.jpg"));
            this.LabelCollection = new ObservableCollection<SegmentationPolygon>();
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
                        this.LabelCollection.Add(new SegmentationPolygon()
                        {
                            Name = "Temp"
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
                            foreach (SegmentationPolygon polygon in temp.OfType<SegmentationPolygon>().ToList())
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
                            foreach (SegmentationPolygon polygon in temp.OfType<SegmentationPolygon>().ToList())
                            {
                                this.CurrentPolyconCollection.Remove(polygon);
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
                        


                    });
                }

                return _OpenLabelCommand;
            }
        }



        private SegmentationPolygon _SelectedPolygon = null;
        public SegmentationPolygon SelectedPolygon
        {
            get => _SelectedPolygon;
            set
            {

                if(value != null)
                {
                    SegmentationPolygon polygon = value;
                    polygon.IsSelected = true;
                }

                if(_SelectedPolygon != null && value != _SelectedPolygon)
                {
                    _SelectedPolygon.IsSelected = false;
                }


                Set<SegmentationPolygon>(nameof(SelectedPolygon), ref _SelectedPolygon, value);
            }
        }


        private ObservableCollection<SegmentationPolygon> _CurrentPolyconCollection = null;
        public ObservableCollection<SegmentationPolygon> CurrentPolyconCollection
        {
            get
            {
                if (_CurrentPolyconCollection == null)
                    _CurrentPolyconCollection = new ObservableCollection<SegmentationPolygon>();

                return _CurrentPolyconCollection;
            }
            set => Set<ObservableCollection<SegmentationPolygon>>(nameof(CurrentPolyconCollection), ref _CurrentPolyconCollection, value);
        }



        private ObservableCollection<SegmentationPolygon> _LabelCollection = null;
        public ObservableCollection<SegmentationPolygon> LabelCollection
        {
            get
            {
                if (_LabelCollection == null)
                    _LabelCollection = new ObservableCollection<SegmentationPolygon>();

                return _LabelCollection;
            }
            set => Set<ObservableCollection<SegmentationPolygon>>(nameof(LabelCollection), ref _LabelCollection, value);
        }

        private SegmentationPolygon _SelectedTargetLabel = null;
        public SegmentationPolygon SelectedTargetLabel
        {
            get => _SelectedTargetLabel;
            set => Set<SegmentationPolygon>(nameof(SelectedTargetLabel), ref _SelectedTargetLabel, value);
        }
    }
}
