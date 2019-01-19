using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class ExHRemitDataModel
    {
        public int Id { get; set; }
        public Nullable<int> ExchangeHouseId { get; set; }
        public string DataCreationMechanism { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreationDateTime { get; set; }
        public Nullable<System.DateTime> DataParsingDate { get; set; }
        public string DataParsingStatus { get; set; }
        public Nullable<int> DataParsedBy { get; set; }
        public string CommentIfFailOrPartialParsed { get; set; }
    }
}
