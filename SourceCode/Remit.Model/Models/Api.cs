using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Api
    {
        public int Id { get; set; }
        public string APIName { get; set; }
        public string EndPoint { get; set; }
    }
}
