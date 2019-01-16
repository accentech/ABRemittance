using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class CurrencyMap : EntityTypeConfiguration<Currency>
    {
        public CurrencyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(50);

            this.Property(t => t.Symbol)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Currency");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Symbol).HasColumnName("Symbol");
        }
    }
}
