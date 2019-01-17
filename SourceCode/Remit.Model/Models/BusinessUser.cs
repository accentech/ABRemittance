using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class BusinessUser
    {
        public BusinessUser()
        {
            this.ActionLogs = new List<ActionLog>();
        }

        public int Id { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public Nullable<int> RoleId { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> PwdTimeStamp { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public string BranchCode { get; set; }
        public string AgentCode { get; set; }
        public virtual ICollection<ActionLog> ActionLogs { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Role Role { get; set; }
    }
}
