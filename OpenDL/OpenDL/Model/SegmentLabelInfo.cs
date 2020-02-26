using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace OpenDL.Model
{
    public class SegmentLabelInfo : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        public void Set<T>(string _name, ref T _reference, T _value)
        {
            if (!Equals(_reference, _value))
            {
                _reference = _value;
                OnPropertyRaised(_name);
            }
        }


        public SegmentLabelInfo()
        {
            this.Labels = new ObservableCollection<SegmentLabelPolygon>();
        }


        private ObservableCollection<SegmentLabelPolygon> _Labels = null;
        public ObservableCollection<SegmentLabelPolygon> Labels
        {
            get => _Labels;
            set => Set<ObservableCollection<SegmentLabelPolygon>>(nameof(Labels), ref _Labels, value);
        }

        private int _LabelSize = 0;
        public int LabelSize
        {
            get => _LabelSize;
            set => Set<int>(nameof(LabelSize), ref _LabelSize, value);
        }

        private int _ImageWidth = 0;
        public int ImageWidth
        {
            get => _ImageWidth;
            set => Set<int>(nameof(ImageWidth), ref _ImageWidth, value);
        }

        private int _ImageHeight = 0;
        public int ImageHeight
        {
            get => _ImageHeight;
            set => Set<int>(nameof(ImageHeight), ref _ImageHeight, value);
        }

    }
}
