using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class ExchangeHouse
    {
        public ExchangeHouse()
        {
            this.BusinessUsers = new List<BusinessUser>();
            this.ExHIPAddresses = new List<ExHIPAddress>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactAddress { get; set; }
        public string ContactPhone { get; set; }
        public string Fax { get; set; }
        public string CountryOfOrigin { get; set; }
        public string DateOfBusinessWithBank { get; set; }
        public Nullable<int> OpenHours { get; set; }
        public Nullable<int> CloseHours { get; set; }
        public Nullable<double> BankGuranteeAmount { get; set; }
        public Nullable<System.DateTime> BankGuranteeExpiryDate { get; set; }
        public string BankGuranteeDescription { get; set; }
        public Nullable<double> MinimumBalance { get; set; }
        public Nullable<System.DateTime> ExchangeHouseLicenseExpiryDate { get; set; }
        public Nullable<System.DateTime> BangladeshBankApprovalDate { get; set; }
        public Nullable<System.DateTime> AMLQuestionaireReceiveDate { get; set; }
        public string CurrentStatus { get; set; }
        public string RemittanceTransactionMechanism { get; set; }
        public string ExHCode { get; set; }
        public virtual ICollection<BusinessUser> BusinessUsers { get; set; }
        public virtual Country Country { get; set; }
        public virtual ICollection<ExHIPAddress> ExHIPAddresses { get; set; }
    }
}
