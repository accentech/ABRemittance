using System;
using System.Collections.Generic;

namespace Remit.Model.Models
{
    public partial class Employee
    {
        public Employee()
        {
            this.NotificationSettings = new List<NotificationSetting>();
            this.WorkflowactionSettings = new List<WorkflowactionSetting>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string NationalId { get; set; }
        public string PassportNo { get; set; }
        public string Email { get; set; }
        public string OfficePhone { get; set; }
        public string OfficeMobile { get; set; }
        public string ResidentPhone { get; set; }
        public string ResidentMobile { get; set; }
        public string BloodGroup { get; set; }
        public string PhotoPath { get; set; }
        public string AuthenticationCode { get; set; }
        public virtual ICollection<NotificationSetting> NotificationSettings { get; set; }
        public virtual ICollection<WorkflowactionSetting> WorkflowactionSettings { get; set; }
    }
}
