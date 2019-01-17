using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class EmployeeMap : EntityTypeConfiguration<Employee>
    {
        public EmployeeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Code)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.FullName)
                .HasMaxLength(100);

            this.Property(t => t.PresentAddress)
                .HasMaxLength(200);

            this.Property(t => t.PermanentAddress)
                .HasMaxLength(200);

            this.Property(t => t.FatherName)
                .HasMaxLength(100);

            this.Property(t => t.MotherName)
                .HasMaxLength(100);

            this.Property(t => t.NationalId)
                .HasMaxLength(50);

            this.Property(t => t.PassportNo)
                .HasMaxLength(50);

            this.Property(t => t.Email)
                .HasMaxLength(100);

            this.Property(t => t.OfficePhone)
                .HasMaxLength(50);

            this.Property(t => t.OfficeMobile)
                .HasMaxLength(50);

            this.Property(t => t.ResidentPhone)
                .HasMaxLength(50);

            this.Property(t => t.ResidentMobile)
                .HasMaxLength(50);

            this.Property(t => t.BloodGroup)
                .HasMaxLength(10);

            this.Property(t => t.PhotoPath)
                .HasMaxLength(250);

            this.Property(t => t.AuthenticationCode)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Employee");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Code).HasColumnName("Code");
            this.Property(t => t.FullName).HasColumnName("FullName");
            this.Property(t => t.PresentAddress).HasColumnName("PresentAddress");
            this.Property(t => t.PermanentAddress).HasColumnName("PermanentAddress");
            this.Property(t => t.FatherName).HasColumnName("FatherName");
            this.Property(t => t.MotherName).HasColumnName("MotherName");
            this.Property(t => t.NationalId).HasColumnName("NationalId");
            this.Property(t => t.PassportNo).HasColumnName("PassportNo");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.OfficePhone).HasColumnName("OfficePhone");
            this.Property(t => t.OfficeMobile).HasColumnName("OfficeMobile");
            this.Property(t => t.ResidentPhone).HasColumnName("ResidentPhone");
            this.Property(t => t.ResidentMobile).HasColumnName("ResidentMobile");
            this.Property(t => t.BloodGroup).HasColumnName("BloodGroup");
            this.Property(t => t.PhotoPath).HasColumnName("PhotoPath");
            this.Property(t => t.AuthenticationCode).HasColumnName("AuthenticationCode");
        }
    }
}
