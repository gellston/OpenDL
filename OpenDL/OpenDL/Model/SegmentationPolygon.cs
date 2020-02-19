using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace OpenDL.Model
{
    public class SegmentationPolygon : INotifyPropertyChanged
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

        public SegmentationPolygon()
        {
            this.Points = new PointCollection();

        }


        private string _Name = "Temp";
        public string Name
        {
            get => _Name;
            set => Set<string>(nameof(Name), ref _Name, value);
        }

        private PointCollection _Points = null;
        public PointCollection Points
        {
            get => _Points;
            set => Set<PointCollection>(nameof(Points), ref _Points, value);
        }

        //public void Add(Point _point)
        //{
        //    if (this.Points != null)
        //    {
        //        this.Points.Add(_point);
        //        OnPropertyRaised(nameof(Points));
        //    }
        //}

        public double _X = 0;
        public double X
        {
            get => _X;
            set => Set<double>(nameof(X), ref _X, value);
        }

        public double _Y = 0;
        public double Y
        {
            get => _Y;
            set => Set<double>(nameof(Y), ref _Y, value);
        }

        public double _Z = 0;
        public double Z
        {
            get => _Z;
            set => Set<double>(nameof(Z), ref _Z, value);
        }

        public double _Width = 0;
        public double Width
        {
            get => _Width;
            set => Set<double>(nameof(Width), ref _Width, value);
        }

        public double _Height = 0;
        public double Height
        {
            get => _Height;
            set => Set<double>(nameof(Height), ref _Height, value);
        }

        public bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;
            set => Set<bool>(nameof(IsSelected), ref _IsSelected, value);
        }
    }
}
