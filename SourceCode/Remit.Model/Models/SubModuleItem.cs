using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class SubModuleItem
    {
        public SubModuleItem()
        {
            this.RoleSubModuleItems = new List<RoleSubModuleItem>();
            this.SubModuleItem1 = new List<SubModuleItem>();
            this.WorkflowactionSettings = new List<WorkflowactionSetting>();
        }

        public int Id { get; set; }
        public Nullable<int> SubModuleId { get; set; }
        public string Name { get; set; }
        public string UrlPath { get; set; }
        public Nullable<byte> Ordering { get; set; }
        public Nullable<bool> IsBaseItem { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> BaseItemId { get; set; }
        public virtual ICollection<RoleSubModuleItem> RoleSubModuleItems { get; set; }
        public virtual SubModule SubModule { get; set; }
        public virtual ICollection<SubModuleItem> SubModuleItem1 { get; set; }
        public virtual SubModuleItem SubModuleItem2 { get; set; }
        public virtual ICollection<WorkflowactionSetting> WorkflowactionSettings { get; set; }
    }
}
