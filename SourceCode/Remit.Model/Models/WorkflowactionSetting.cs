using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class WorkflowactionSetting
    {
        public System.Guid Id { get; set; }
        public Nullable<int> SubMouduleItemId { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public Nullable<int> WorkflowactionId { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual SubModuleItem SubModuleItem { get; set; }
        public virtual Workflowaction Workflowaction { get; set; }
    }
}
