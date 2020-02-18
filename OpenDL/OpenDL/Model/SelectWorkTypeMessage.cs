using System;
using System.Collections.Generic;
using System.Text;

namespace OpenDL.Model
{
    public class SelectWorkTypeMessage
    {

        public string Message
        {
            get;set;
        }

        public WorkTypeMenu Type
        {
            get;set;
        }
    }
}
