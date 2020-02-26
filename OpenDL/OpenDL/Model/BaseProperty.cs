using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OpenDL.Model
{
    public class BaseProperty : INotifyPropertyChanged
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

        public BaseProperty()
        {

        }

        private string _Name = "";
        public string Name
        {
            get => _Name;
            set => Set<string>(nameof(Name), ref _Name, value);
        }
    }
}
