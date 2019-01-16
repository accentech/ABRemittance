using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ExHIPAddressMap : EntityTypeConfiguration<ExHIPAddress>
    {
        public ExHIPAddressMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.IPAddress)
                .HasMaxLength(50);

            this.Property(t => t.ReferenceDocForIPRequest)
                .HasMaxLength(50);

            this.Property(t => t.CreatedBy)
                .HasMaxLength(50);

            this.Property(t => t.UpdatedBy)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("ExHIPAddress");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.IPAddress).HasColumnName("IPAddress");
            this.Property(t => t.ActivtionDate).HasColumnName("ActivtionDate");
            this.Property(t => t.ReferenceDocForIPRequest).HasColumnName("ReferenceDocForIPRequest");
            this.Property(t => t.CreatedBy).HasColumnName("CreatedBy");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
            this.Property(t => t.UpdatedBy).HasColumnName("UpdatedBy");
            this.Property(t => t.UpdateTime).HasColumnName("UpdateTime");
            this.Property(t => t.ExchangeHouseID).HasColumnName("ExchangeHouseID");
            this.Property(t => t.DiscontinuationDate).HasColumnName("DiscontinuationDate");

            // Relationships
            this.HasOptional(t => t.ExchangeHouse)
                .WithMany(t => t.ExHIPAddresses)
                .HasForeignKey(d => d.ExchangeHouseID);

        }
    }
}
