using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace OpenDL.Model
{
    public class SegmentTrainSample : INotifyPropertyChanged
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

        public SegmentTrainSample()
        {
            this.OutputLabelCollection = new ObservableCollection<string>();
        }

        private string _InputImagePath = "";
        public string InputImagePath
        {
            get => _InputImagePath;
            set => Set<string>(nameof(InputImagePath), ref _InputImagePath, value);
        }

        private ObservableCollection<string> _OutputLabelCollection = null;
        public ObservableCollection<string> OutputLabelCollection
        {
            get => _OutputLabelCollection;
            set => Set<ObservableCollection<string>>(nameof(OutputLabelCollection), ref _OutputLabelCollection, value);
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
            set => Set<int>(nameof(ImageWidth), ref _ImageHeight, value);
        }

        private bool _IsGray = false;
        public bool IsGray
        {
            get => _IsGray;
            set => Set<bool>(nameof(IsGray), ref _IsGray, value);
        }

        public string _Name = "";
        public string Name
        {
            get => _Name;
            set => Set<string>(nameof(Name), ref _Name, value);
        }
    }
}
