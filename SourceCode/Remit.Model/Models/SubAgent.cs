using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class SubAgent
    {
        public SubAgent()
        {
            this.BusinessUsers = new List<BusinessUser>();
        }

        public int Id { get; set; }
        public string LocationName { get; set; }
        public Nullable<int> AgentId { get; set; }
        public virtual ICollection<BusinessUser> BusinessUsers { get; set; }
    }
}
