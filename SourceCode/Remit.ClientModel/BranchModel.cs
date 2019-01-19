using System;
using System.Collections.Generic;

namespace Remit.ClientModel
{
    public class BranchModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Nullable<int> BankId { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }
    }
}
