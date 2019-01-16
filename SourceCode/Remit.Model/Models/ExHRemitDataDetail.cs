using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class ExHRemitDataDetail
    {
        public int Id { get; set; }
        public Nullable<int> ExHRemitDataId { get; set; }
        public string RawRemitData { get; set; }
        public string ParsedStatus { get; set; }
    }
}
