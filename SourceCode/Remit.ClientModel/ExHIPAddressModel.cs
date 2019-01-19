using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class ExHIPAddressModel
    {
        public int Id { get; set; }
        public string IPAddress { get; set; }
        public Nullable<System.DateTime> ActivtionDate { get; set; }
        public string ReferenceDocForIPRequest { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdateTime { get; set; }
        public Nullable<int> ExchangeHouseID { get; set; }
        public Nullable<System.DateTime> DiscontinuationDate { get; set; }
        //public ExchangeHouseModel ExchangeHouse { get; set; }
    }
}
