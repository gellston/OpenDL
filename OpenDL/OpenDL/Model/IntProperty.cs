using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Model
{
    public class IntProperty : BaseProperty
    {

        public IntProperty()
        {


        }

        private int _Value = 0;
        public int Value
        {
            get => _Value;
            set => Set<int>(nameof(Value), ref _Value, value);
        }
    }
}
