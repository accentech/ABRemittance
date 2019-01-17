using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class SubModule
    {
        public SubModule()
        {
            this.SubModuleItems = new List<SubModuleItem>();
        }

        public int Id { get; set; }
        public Nullable<int> ModuleId { get; set; }
        public string Name { get; set; }
        public Nullable<byte> Ordering { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public virtual Module Module { get; set; }
        public virtual ICollection<SubModuleItem> SubModuleItems { get; set; }
    }
}
