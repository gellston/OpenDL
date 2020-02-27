using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Model
{
    public class StringProperty : BaseProperty
    {

        public StringProperty()
        {

        }

        private string _Value = "";
        public string Value
        {
            get => _Value;
            set => Set<string>(nameof(Value), ref _Value, value);
        }
    }
}
