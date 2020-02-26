using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Model
{
    public class DoubleProperty : BaseProperty
    {

        public DoubleProperty()
        {

        }

        private double _Value = 0;
        public double Value
        {
            get => _Value;
            set => Set<double>(nameof(Value), ref _Value, value);
        }
    }
}
