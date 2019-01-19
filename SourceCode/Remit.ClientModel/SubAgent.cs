using Remit.ClientModel;
using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class SubAgentModel
    {
        public SubAgentModel()
        {
            this.BusinessUsers = new List<BusinessUserModel>();
        }

        public int Id { get; set; }
        public string LocationName { get; set; }
        public Nullable<int> AgentId { get; set; }
        public string AgentName { get; set; }
        public virtual ICollection<BusinessUserModel> BusinessUsers { get; set; }
    }
}
