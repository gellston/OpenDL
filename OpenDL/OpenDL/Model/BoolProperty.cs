using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Model
{
    public class BoolProperty : BaseProperty
    {

        public BoolProperty()
        {

        }

        private bool _Value = false;
        public bool Value
        {

            get => _Value;
            set => Set<bool>(nameof(Value), ref _Value, value);
        }
    }
}
