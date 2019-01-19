using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class ExHRemitData
    {
        public ExHRemitData()
        {
            this.ExHRemitDataDetails = new List<ExHRemitDataDetail>();
        }

        public int Id { get; set; }
        public Nullable<int> ExchangeHouseId { get; set; }
        public string DataCreationMechanism { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreationDateTime { get; set; }
        public Nullable<System.DateTime> DataParsingDate { get; set; }
        public string DataParsingStatus { get; set; }
        public Nullable<int> DataParsedBy { get; set; }
        public string CommentIfFailOrPartialParsed { get; set; }
        public virtual ICollection<ExHRemitDataDetail> ExHRemitDataDetails { get; set; }
    }
}
