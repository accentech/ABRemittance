using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Role
    {
        public Role()
        {
            this.BusinessUsers = new List<BusinessUser>();
            this.RoleSubModuleItems = new List<RoleSubModuleItem>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<BusinessUser> BusinessUsers { get; set; }
        public virtual ICollection<RoleSubModuleItem> RoleSubModuleItems { get; set; }
    }
}
