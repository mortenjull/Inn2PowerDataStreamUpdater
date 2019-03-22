using System;
using System.Collections.Generic;
using System.Text;

namespace Inn2PowerDataStreamUpdater.Misc.Entities
{
    public class PotentialDiplicate
    {
        public PotentialDiplicate(DataStreamCompany duplicate, DataStreamCompany match, APICompany match2)
        {
            this.Duplicate = duplicate;
            this.Match = match;
            this.Match2 = match2;
        }
        public DataStreamCompany Duplicate { get; set; }
        public DataStreamCompany Match { get; set; }
        public APICompany Match2 { get; set; }
    }
}
