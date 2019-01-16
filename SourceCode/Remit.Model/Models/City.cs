using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class City
    {
        public int Id { get; set; }
        public string CountryId { get; set; }
        public string Name { get; set; }
        public virtual Country Country { get; set; }
    }
}
