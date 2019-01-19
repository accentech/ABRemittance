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

            this.Property(t => t.EmailAddress)
                .HasMaxLength(100);

            this.Property(t => t.FullName)
                .HasMaxLength(100);

            this.Property(t => t.NID)
                .HasMaxLength(50);

            this.Property(t => t.Phone)
                .HasMaxLength(100);

            this.Property(t => t.ReasonForDeactivation)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("BusinessUser");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.LoginName).HasColumnName("LoginName");
            this.Property(t => t.Password).HasColumnName("Password");
            this.Property(t => t.RoleId).HasColumnName("RoleId");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
            this.Property(t => t.PwdTimeStamp).HasColumnName("PwdTimeStamp");
            this.Property(t => t.EmployeeId).HasColumnName("EmployeeId");
            this.Property(t => t.PwdExpiryDate).HasColumnName("PwdExpiryDate");
            this.Property(t => t.BranchId).HasColumnName("BranchId");
            this.Property(t => t.AgentId).HasColumnName("AgentId");
            this.Property(t => t.SubAgentId).HasColumnName("SubAgentId");
            this.Property(t => t.ExchangeHouseCode).HasColumnName("ExchangeHouseCode");
            this.Property(t => t.EmailAddress).HasColumnName("EmailAddress");
            this.Property(t => t.FullName).HasColumnName("FullName");
            this.Property(t => t.NID).HasColumnName("NID");
            this.Property(t => t.Phone).HasColumnName("Phone");
            this.Property(t => t.IsHeadOffice).HasColumnName("IsHeadOffice");
            this.Property(t => t.UserDeActivationDate).HasColumnName("UserDeActivationDate");
            this.Property(t => t.ReasonForDeactivation).HasColumnName("ReasonForDeactivation");

            // Relationships
            this.HasOptional(t => t.Agent)
                .WithMany(t => t.BusinessUsers)
                .HasForeignKey(d => d.AgentId);
            this.HasOptional(t => t.Branch)
                .WithMany(t => t.BusinessUsers)
                .HasForeignKey(d => d.BranchId);
            this.HasOptional(t => t.ExchangeHouse)
                .WithMany(t => t.BusinessUsers)
                .HasForeignKey(d => d.ExchangeHouseCode);
            this.HasOptional(t => t.SubAgent)
                .WithMany(t => t.BusinessUsers)
                .HasForeignKey(d => d.SubAgentId);
            this.HasOptional(t => t.Role)
                .WithMany(t => t.BusinessUsers)
                .HasForeignKey(d => d.RoleId);

        }
    }
}
