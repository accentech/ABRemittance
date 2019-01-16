using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class RoleSubModuleItem
    {
        public int Id { get; set; }
        public Nullable<int> RoleId { get; set; }
        public Nullable<int> SubModuleItemId { get; set; }
        public Nullable<bool> CreateOperation { get; set; }
        public Nullable<bool> ReadOperation { get; set; }
        public Nullable<bool> UpdateOperation { get; set; }
        public Nullable<bool> DeleteOperation { get; set; }
        public virtual Role Role { get; set; }
        public virtual SubModuleItem SubModuleItem { get; set; }
    }
}
