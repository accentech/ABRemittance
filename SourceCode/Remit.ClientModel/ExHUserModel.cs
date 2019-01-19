using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class ExHUserModel
    {
        public int Id { get; set; }
        public Nullable<int> ExchangeHouseId { get; set; }
        public string ExUserName { get; set; }
        public string ExUserPassword { get; set; }
        public string ReferenceForUser { get; set; }
        //public ExchangeHouseModel ExchangeHouse { get; set; }
    }
}
