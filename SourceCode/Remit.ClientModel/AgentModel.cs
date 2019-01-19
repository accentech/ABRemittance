using Remit.ClientModel;
using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class AgentModel
    {
        public AgentModel()
        {
            this.BusinessUsers = new List<BusinessUserModel>();
        }

        public int Id { get; set; }
        public string AgentName { get; set; }
        public Nullable<System.DateTime> DateOfOperationStart { get; set; }
        public string AgentAddress { get; set; }
        public string ContactPhone { get; set; }
        public string Email { get; set; }
        public virtual ICollection<BusinessUserModel> BusinessUsers { get; set; }
    }
}
