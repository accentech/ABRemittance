using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Country
    {
        public Country()
        {
            this.Branches = new List<Branch>();
            this.Cities = new List<City>();
            this.ExchangeHouses = new List<ExchangeHouse>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public virtual ICollection<Branch> Branches { get; set; }
        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<ExchangeHouse> ExchangeHouses { get; set; }
    }
}
