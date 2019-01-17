using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class ExHUser
    {
        public int Id { get; set; }
        public Nullable<int> ExchangeHouseId { get; set; }
        public string ExUserName { get; set; }
        public string ExUserPassword { get; set; }
        public string ReferenceForUser { get; set; }
        public virtual ExchangeHouse ExchangeHouse { get; set; }
    }
}
