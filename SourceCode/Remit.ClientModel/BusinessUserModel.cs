using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class BusinessUserModel
    {
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
        public string EmployeeFullName { get; set; }
        public string RoleName { get; set; }
    }
}
