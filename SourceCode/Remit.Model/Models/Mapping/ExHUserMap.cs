using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ExHUserMap : EntityTypeConfiguration<ExHUser>
    {
        public ExHUserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(t => t.ExUserName)
                .HasMaxLength(50);

            this.Property(t => t.ExUserPassword)
                .HasMaxLength(50);

            this.Property(t => t.ReferenceForUser)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("ExHUser");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ExchangeHouseId).HasColumnName("ExchangeHouseId");
            this.Property(t => t.ExUserName).HasColumnName("ExUserName");
            this.Property(t => t.ExUserPassword).HasColumnName("ExUserPassword");
            this.Property(t => t.ReferenceForUser).HasColumnName("ReferenceForUser");

            // Relationships
            this.HasOptional(t => t.ExchangeHouse)
                .WithMany(t => t.ExHUsers)
                .HasForeignKey(d => d.ExchangeHouseId);

        }
    }
}
