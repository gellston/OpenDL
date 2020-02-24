using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace OpenDL.Model
{
    public class SegmentLabelUnit : INotifyPropertyChanged
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


        public SegmentLabelUnit()
        {
            this.Polygons = new ObservableCollection<SegmentationPolygon>();
        }

        private string _FileName = "";
        public string FileName
        {
            set => Set<string>(nameof(FileName), ref _FileName, value);
            get => _FileName;
        }


        private ObservableCollection<SegmentationPolygon> _Polygons = null;
        public ObservableCollection<SegmentationPolygon> Polygons
        {
            get => _Polygons;
            set => Set<ObservableCollection<SegmentationPolygon>>(nameof(Polygons), ref _Polygons, value);
        }
    }
}
