using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Workflowaction
    {
        public Workflowaction()
        {
            this.WorkflowactionSettings = new List<WorkflowactionSetting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<WorkflowactionSetting> WorkflowactionSettings { get; set; }
    }
}