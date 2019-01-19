using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class ExHApiModel
    {
        public int Id { get; set; }
        public Nullable<int> ExchangeHouseId { get; set; }
        public Nullable<int> APIId { get; set; }
        public Nullable<System.DateTime> ActivationDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
    }
}
