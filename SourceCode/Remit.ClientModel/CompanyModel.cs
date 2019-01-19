using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class CompanyModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string LogoName { get; set; }
        public string CompanyUrl { get; set; }
        public Nullable<int> BaseCurrency { get; set; }
        public string BaseCurrecySymbol { get; set; }
        public Nullable<int> LocalCurrency { get; set; }
        public string CompanyAddress { get; set; }
        public int CityId { get; set; }
        //public CurrencyModel Currency { get; set; }
        //public CurrencyModel Currency1 { get; set; }
    }
}
