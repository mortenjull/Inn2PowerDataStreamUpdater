﻿using Inn2PowerDataStreamUpdater.Misc.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inn2PowerDataStreamUpdater.Misc
{
    public class APICompany
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool SME { get; set; }
        public DateTime Created { get; set; }
        public List<SupplyChainCategory> SupplyChainCategories { get; set; }
        public List<SupplyChainRole> SupplyChainRoles { get; set; }
        public string CompanyDirectoryEntryReffNumber { get; set; }
      
        /// <summary>
        /// This is not a part of the entity. This is for diplay purpose only.
        /// </summary>
        public string Status { get; set; }
    }
}
