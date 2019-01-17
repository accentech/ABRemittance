using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ExHRemitDataDetailMap : EntityTypeConfiguration<ExHRemitDataDetail>
    {
        public ExHRemitDataDetailMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.ParsedStatus)
                .HasMaxLength(10);

            // Table & Column Mappings
            this.ToTable("ExHRemitDataDetails");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ExHRemitDataId).HasColumnName("ExHRemitDataId");
            this.Property(t => t.RawRemitData).HasColumnName("RawRemitData");
            this.Property(t => t.ParsedStatus).HasColumnName("ParsedStatus");
        }
    }
}
