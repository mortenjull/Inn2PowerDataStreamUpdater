using System;
using System.Collections.Generic;
using System.Text;

namespace Inn2PowerDataStreamUpdater.Misc.Entities
{
    public class SupplyChainCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal SupplyChainCategoryCode { get; set; }
        public int SuperCategory { get; set; }
    }
}
