using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class BranchMap : EntityTypeConfiguration<Branch>
    {
        public BranchMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(50);

            this.Property(t => t.Address)
                .HasMaxLength(150);

            this.Property(t => t.Phone)
                .HasMaxLength(50);

            this.Property(t => t.Email)
                .HasMaxLength(100);

            this.Property(t => t.CountryId)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Branch");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Address).HasColumnName("Address");
            this.Property(t => t.Phone).HasColumnName("Phone");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.BankId).HasColumnName("BankId");
            this.Property(t => t.CountryId).HasColumnName("CountryId");

            // Relationships
            this.HasOptional(t => t.Country)
                .WithMany(t => t.Branches)
                .HasForeignKey(d => d.CountryId);

        }
    }
}
