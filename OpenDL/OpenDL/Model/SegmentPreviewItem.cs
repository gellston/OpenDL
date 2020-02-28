using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Media.Imaging;

namespace OpenDL.Model
{
    public class SegmentPreviewItem : INotifyPropertyChanged
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

        public SegmentPreviewItem()
        {

        }


        private BitmapSource _Image = null;
        public BitmapSource Image
        {
            get => _Image;
            set => Set<BitmapSource>(nameof(Image), ref _Image, value);
        }

        private double _Alpha = 1;
        public double Alpha
        {
            get => _Alpha;
            set => Set<double>(nameof(Alpha), ref _Alpha, value);
        }

    }
}
