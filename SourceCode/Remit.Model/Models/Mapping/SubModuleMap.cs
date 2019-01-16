using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class SubModuleMap : EntityTypeConfiguration<SubModule>
    {
        public SubModuleMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(100);

            // Table & Column Mappings
            this.ToTable("SubModule");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ModuleId).HasColumnName("ModuleId");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Ordering).HasColumnName("Ordering");
            this.Property(t => t.IsActive).HasColumnName("IsActive");

            // Relationships
            this.HasOptional(t => t.Module)
                .WithMany(t => t.SubModules)
                .HasForeignKey(d => d.ModuleId);

        }
    }
}
