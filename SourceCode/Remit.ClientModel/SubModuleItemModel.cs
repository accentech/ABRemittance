using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class SubModuleItemModel
    {
        public SubModuleItemModel()
        {
            this.RoleSubModuleItem = new RoleSubModuleItemModel();
        }

        public int Id { get; set; }
        public Nullable<int> ModuleId { get; set; }
        public string ModuleName { get; set; }
        public Nullable<int> SubModuleId { get; set; }
        public string SubModuleName { get; set; }
        public string Name { get; set; }
        public string UrlPath { get; set; }
        public Nullable<byte> Ordering { get; set; }
        public Nullable<bool> IsBaseItem { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> BaseItemId { get; set; }
        public string BaseItemName { get; set; }
        //public SubModuleModel SubModule { get; set; }
        //public SubModuleItemModel SubModuleItem2 { get; set; }
        public RoleSubModuleItemModel RoleSubModuleItem { get; set; }
    }
}
