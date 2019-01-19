using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class SubModuleModel
    {
        public SubModuleModel()
        {
            this.SubModuleItems = new List<SubModuleItemModel>();
        }

        public int Id { get; set; }
        public Nullable<int> ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Name { get; set; }
        public Nullable<byte> Ordering { get; set; }
        public Nullable<bool> IsActive { get; set; }
        //public ModuleModel Module { get; set; }
        public List<SubModuleItemModel> SubModuleItems { get; set; }
    }
}
