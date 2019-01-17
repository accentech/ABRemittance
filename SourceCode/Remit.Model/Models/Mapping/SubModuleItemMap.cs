using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class SubModuleItemMap : EntityTypeConfiguration<SubModuleItem>
    {
        public SubModuleItemMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(200);

            this.Property(t => t.UrlPath)
                .HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("SubModuleItem");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.SubModuleId).HasColumnName("SubModuleId");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.UrlPath).HasColumnName("UrlPath");
            this.Property(t => t.Ordering).HasColumnName("Ordering");
            this.Property(t => t.IsBaseItem).HasColumnName("IsBaseItem");
            this.Property(t => t.IsActive).HasColumnName("IsActive");
            this.Property(t => t.BaseItemId).HasColumnName("BaseItemId");

            // Relationships
            this.HasOptional(t => t.SubModule)
                .WithMany(t => t.SubModuleItems)
                .HasForeignKey(d => d.SubModuleId);
            this.HasOptional(t => t.SubModuleItem2)
                .WithMany(t => t.SubModuleItem1)
                .HasForeignKey(d => d.BaseItemId);

        }
    }
}
