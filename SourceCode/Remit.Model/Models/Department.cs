using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Department
    {
        public Department()
        {
            this.Designations = new List<Designation>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Information { get; set; }
        public virtual ICollection<Designation> Designations { get; set; }
    }
}
