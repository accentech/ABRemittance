using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class BusinessUserMap : EntityTypeConfiguration<BusinessUser>
    {
        public BusinessUserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.LoginName)
                .HasMaxLength(50);

            this.Property(t => t.Password)
                .HasMaxLength(50);

            this.Property(t => t.BranchCode)
                .HasMaxLength(20);

            this.Property(t => t.AgentCode)
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("BusinessUser");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.LoginName).HasColumnName("LoginName");
            this.Property(t => t.Password).HasColumnName("Password");
            this.Property(t => t.RoleId).HasColumnName("RoleId");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
            this.Property(t => t.PwdTimeStamp).HasColumnName("PwdTimeStamp");
            this.Property(t => t.EmployeeId).HasColumnName("EmployeeId");
            this.Property(t => t.ExpiryDate).HasColumnName("ExpiryDate");
            this.Property(t => t.BranchCode).HasColumnName("BranchCode");
            this.Property(t => t.AgentCode).HasColumnName("AgentCode");

            // Relationships
            this.HasOptional(t => t.Employee)
                .WithMany(t => t.BusinessUsers)
                .HasForeignKey(d => d.EmployeeId);
            this.HasOptional(t => t.Role)
                .WithMany(t => t.BusinessUsers)
                .HasForeignKey(d => d.RoleId);

        }
    }
}
