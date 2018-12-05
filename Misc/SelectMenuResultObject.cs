using System;
using System.Collections.Generic;
using System.Text;

namespace Inn2PowerDataStreamUpdater.Misc
{
    public class SelectMenuResultObject
    {
        public bool IsSuccesFull { get; set; }
        public List<APICompany> SelectedCompanies { get; set; }
        public List<APICompany> Companies { get; set; }
        public string ErrorMessage { get; set; }
    }
}
