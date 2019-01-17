using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Bank
    {
        public Bank()
        {
            this.Branches = new List<Branch>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Branch> Branches { get; set; }
    }
}
