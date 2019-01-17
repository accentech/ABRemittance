using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class CityMap : EntityTypeConfiguration<City>
    {
        public CityMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.CountryId)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("City");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.CountryId).HasColumnName("CountryId");
            this.Property(t => t.Name).HasColumnName("Name");

            // Relationships
            this.HasRequired(t => t.Country)
                .WithMany(t => t.Cities)
                .HasForeignKey(d => d.CountryId);

        }
    }
}
