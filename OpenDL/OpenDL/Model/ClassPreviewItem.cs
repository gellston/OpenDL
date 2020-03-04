using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Media.Imaging;

namespace OpenDL.Model
{
    public class ClassPreviewItem : INotifyPropertyChanged
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

        public ClassPreviewItem()
        {

        }


        private BitmapSource _Image = null;
        public BitmapSource Image
        {
            get => _Image;
            set => Set<BitmapSource>(nameof(Image), ref _Image, value);
        }

        private double _Score = 1;
        public double Score
        {
            get => _Score;
            set => Set<double>(nameof(Score), ref _Score, value);
        }

        private string _Name = "";
        public string Name
        {
            get => _Name;
            set => Set<string>(nameof(Name), ref _Name, value);
        }

    }
}
