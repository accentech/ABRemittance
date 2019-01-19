using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class ModuleModel
    {
        public ModuleModel()
        {
            this.SubModules = new List<SubModuleModel>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageName { get; set; }
        public Nullable<byte> Ordering { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public List<SubModuleModel> SubModules { get; set; }
        
    }
}
