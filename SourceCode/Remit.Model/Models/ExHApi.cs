using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class ExHApi
    {
        public int Id { get; set; }
        public Nullable<int> ExchangeHouseId { get; set; }
        public Nullable<int> APIId { get; set; }
        public Nullable<System.DateTime> ActivationDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public virtual Api Api { get; set; }
    }
}
