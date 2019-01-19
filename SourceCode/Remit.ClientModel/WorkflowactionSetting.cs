using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public partial class WorkflowactionSettingModel
    {
        public System.Guid Id { get; set; }
        public Nullable<int> SubMouduleItemId { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public Nullable<int> WorkflowactionId { get; set; }
    }
}
