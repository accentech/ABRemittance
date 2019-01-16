using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Currency
    {
        public Currency()
        {
            this.Companies = new List<Company>();
            this.Companies1 = new List<Company>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public virtual ICollection<Company> Companies { get; set; }
        public virtual ICollection<Company> Companies1 { get; set; }
    }
}
