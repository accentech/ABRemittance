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
        public Nullable<System.DateTime> PwdExpiryDate { get; set; }
        public Nullable<int> BranchId { get; set; }
        public Nullable<int> AgentId { get; set; }
        public Nullable<int> SubAgentId { get; set; }
        public Nullable<int> ExchangeHouseCode { get; set; }
        public string EmailAddress { get; set; }
        public string FullName { get; set; }
        public string NID { get; set; }
        public string Phone { get; set; }
        public bool IsHeadOffice { get; set; }
        public Nullable<System.DateTime> UserDeActivationDate { get; set; }
        public string ReasonForDeactivation { get; set; }
        public virtual ICollection<ActionLog> ActionLogs { get; set; }
        public virtual Agent Agent { get; set; }
        public virtual Branch Branch { get; set; }
        public virtual ExchangeHouse ExchangeHouse { get; set; }
        public virtual SubAgent SubAgent { get; set; }
        public virtual Role Role { get; set; }
    }
}
