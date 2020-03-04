using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Model
{
    public class FreezedClassModelInfo
    {

        public FreezedClassModelInfo()
        {

        }

        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

        public bool IsGray
        {
            get; set;
        }

        public int LabelCount
        {
            get; set;
        }

        public string FreezeOutNodeName
        {
            get; set;
        }

        public string FreezeInputNodeName
        {
            get; set;
        }

        public string FreezePhaseNodeName
        {
            get; set;
        }
    }
}
