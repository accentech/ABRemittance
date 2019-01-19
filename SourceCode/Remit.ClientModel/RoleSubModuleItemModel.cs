using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class RoleSubModuleItemModel
    {
        public int Id { get; set; }
        public Nullable<int> RoleId { get; set; }
        public string RoleIdName { get; set; }
        public Nullable<int> ModuleId { get; set; }
        public string ModuleName { get; set; }
        public Nullable<int> SubModuleId { get; set; }
        public Nullable<int> SubModuleItemId { get; set; }
        public string SubModuleItemIdName { get; set; }
        public Nullable<bool> CreateOperation { get; set; }
        public string CreateOperationName { get; set; }
        public Nullable<bool> ReadOperation { get; set; }
        public string ReadOperationName { get; set; }
        public Nullable<bool> UpdateOperation { get; set; }
        public string UpdateOperationName { get; set; }
        public Nullable<bool> DeleteOperation { get; set; }
        public string DeleteOperationName { get; set; }
        public RoleModel Role { get; set; }
        public SubModuleItemModel SubModuleItem { get; set; }
    }
}
