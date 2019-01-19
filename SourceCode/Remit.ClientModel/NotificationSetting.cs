using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class NotificationSettingModel
    {
        public System.Guid Id { get; set; }
        public Nullable<int> SubModuleItemId { get; set; }
        public Nullable<int> NotifiedEmployeeId { get; set; }
        public string NotifiedEmployeeName { get; set; }
        public string SubModuleItemName { get; set; }
    }
}
