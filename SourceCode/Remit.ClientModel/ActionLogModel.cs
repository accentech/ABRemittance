using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class ActionLogModel
    {
        public System.Guid Id { get; set; }
        public Nullable<int> Who { get; set; }
        public Nullable<System.DateTime> When { get; set; }
        public string AffectedRecordId { get; set; }
        public string What { get; set; }
        public string ActionCRUD { get; set; }
        public string Entity { get; set; }
        public string IPAddress { get; set; }
        public Nullable<int> BusinessUserId { get; set; }
    }
}
