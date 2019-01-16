using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Module
    {
        public Module()
        {
            this.SubModules = new List<SubModule>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public Nullable<byte> Ordering { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public virtual ICollection<SubModule> SubModules { get; set; }
    }
}
