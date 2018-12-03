using System;
using System.Collections.Generic;
using System.Text;

namespace Inn2PowerDataStreamUpdater.Misc
{
    public class DataStreamCompany
    {
        public string entry_reference_number { get; set; }
        public string company_name { get; set; }
        public string country { get; set; }
        public string status { get; set; }
        public bool display_on_map { get; set; }
        public List<string> supply_chain_roles { get; set; }
        public List<string> supply_chain_categories { get; set; }
        public string website { get; set; }
        public bool sme_status { get; set; }
        public List<Office> offices { get; set; }
    }

    public class Office
    {
        public string entry_reference_number { get; set; }
        public string address { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public bool include_in_map { get; set; }
        public Updated last_updated { get; set; }
    }

    public class Updated
    {
        public string date { get; set; }
        public int timezone_type { get; set; }
        public string timezone { get; set; }
    }
}
