using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using OpenDL.Model;

namespace OpenDL.ViewModel
{
    public class SegmentationLabelViewModel: ViewModelBase
    {

        public SegmentationLabelViewModel()
        {
            this.CurrentImage = new BitmapImage(new Uri("pack://application:,,,/OpenDL;component/Image/Orange.jpg"));
            this.LabelCollection = new ObservableCollection<SegmentationPolygon>();
        }


        private BitmapImage _CurrentImage = null;
        public BitmapImage CurrentImage
        {
            get => _CurrentImage;
            set => Set<BitmapImage>(nameof(CurrentImage), ref _CurrentImage, value);
        }


        private ObservableCollection<SegmentationPolygon> _LabelCollection = null;
        public ObservableCollection<SegmentationPolygon> LabelCollection
        {
            get => _LabelCollection;
            set => Set<ObservableCollection<SegmentationPolygon>>(nameof(LabelCollection), ref _LabelCollection, value);
        }
    }
}
