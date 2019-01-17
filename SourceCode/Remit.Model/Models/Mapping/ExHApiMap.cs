using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ExHApiMap : EntityTypeConfiguration<ExHApi>
    {
        public ExHApiMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("ExHApi");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ExchangeHouseId).HasColumnName("ExchangeHouseId");
            this.Property(t => t.APIId).HasColumnName("APIId");
            this.Property(t => t.ActivationDate).HasColumnName("ActivationDate");
            this.Property(t => t.CreatedBy).HasColumnName("CreatedBy");
            this.Property(t => t.CreationDate).HasColumnName("CreationDate");
        }
    }
}
