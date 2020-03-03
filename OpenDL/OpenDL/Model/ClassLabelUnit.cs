using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace OpenDL.Model
{
    public class ClassLabelUnit : INotifyPropertyChanged
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


        public ClassLabelUnit()
        {
            this.Boxes = new ObservableCollection<ClassificationLabelBox>();
        }

        private string _FileName = "";
        public string FileName
        {
            set => Set<string>(nameof(FileName), ref _FileName, value);
            get => _FileName;
        }

        private string _FileNameWithExtension = "";
        public string FileNameWithExtension
        {
            set => Set<string>(nameof(FileNameWithExtension), ref _FileNameWithExtension, value);
            get => _FileNameWithExtension;
        }

        private string _FilePath = "";
        public string FilePath
        {
            set => Set<string>(nameof(FilePath), ref _FilePath, value);
            get => _FilePath;
        }

        private int _ImageWidth = 0;
        public int ImageWidth
        {
            set => Set<int>(nameof(ImageWidth), ref _ImageWidth, value);
            get => _ImageWidth;
        }


        private int _ImageHeight = 0;
        public int ImageHeight
        {
            set => Set<int>(nameof(ImageHeight), ref _ImageHeight, value);
            get => _ImageHeight;
        }


        private ObservableCollection<ClassificationLabelBox> _Boxes = null;
        public ObservableCollection<ClassificationLabelBox> Boxes
        {
            get => _Boxes;
            set => Set<ObservableCollection<ClassificationLabelBox>>(nameof(Boxes), ref _Boxes, value);
        }
    }
}
