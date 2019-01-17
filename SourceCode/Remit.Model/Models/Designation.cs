using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Designation
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public virtual Department Department { get; set; }
    }
}
