using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ModuleMap : EntityTypeConfiguration<Module>
    {
        public ModuleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            this.Property(t => t.ImageName)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Module");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.ImageName).HasColumnName("ImageName");
            this.Property(t => t.Ordering).HasColumnName("Ordering");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
        }
    }
}
