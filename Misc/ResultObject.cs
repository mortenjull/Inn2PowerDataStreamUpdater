using System;
using System.Collections.Generic;
using System.Text;

namespace Inn2PowerDataStreamUpdater.Misc
{
    public class ResultObject
    {
        public bool IsSuccesFull { get; set; }
        public object Payload { get; set; }

        public string ErrorMessage { get; set; }
    }
}
