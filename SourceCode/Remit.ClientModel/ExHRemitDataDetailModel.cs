using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class ExHRemitDataDetailModel
    {
        public int Id { get; set; }
        public Nullable<int> ExHRemitDataId { get; set; }
        public string RawRemitData { get; set; }
        public string ParsedStatus { get; set; }
    }
}
