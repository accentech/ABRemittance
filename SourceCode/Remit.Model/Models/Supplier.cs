using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Nullable<int> CityId { get; set; }
        public string CountryId { get; set; }
        public string OfficePhone { get; set; }
        public string EmergencyContact { get; set; }
        public string Email { get; set; }
        public string Skype { get; set; }
        public string Website { get; set; }
        public string ItemLoadingPort { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocalSupplier { get; set; }
        public Nullable<int> CurrencyId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public virtual City City { get; set; }
        public virtual Country Country { get; set; }
        public virtual Currency Currency { get; set; }
    }
}
