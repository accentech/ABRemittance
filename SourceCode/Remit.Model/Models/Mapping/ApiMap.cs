using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ApiMap : EntityTypeConfiguration<Api>
    {
        public ApiMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.APIName)
                .HasMaxLength(50);

            this.Property(t => t.EndPoint)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Api");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.APIName).HasColumnName("APIName");
            this.Property(t => t.EndPoint).HasColumnName("EndPoint");
        }
    }
}
