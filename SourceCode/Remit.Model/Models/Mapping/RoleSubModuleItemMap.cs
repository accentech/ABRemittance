using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class RoleSubModuleItemMap : EntityTypeConfiguration<RoleSubModuleItem>
    {
        public RoleSubModuleItemMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("RoleSubModuleItem");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.RoleId).HasColumnName("RoleId");
            this.Property(t => t.SubModuleItemId).HasColumnName("SubModuleItemId");
            this.Property(t => t.CreateOperation).HasColumnName("CreateOperation");
            this.Property(t => t.ReadOperation).HasColumnName("ReadOperation");
            this.Property(t => t.UpdateOperation).HasColumnName("UpdateOperation");
            this.Property(t => t.DeleteOperation).HasColumnName("DeleteOperation");

            // Relationships
            this.HasOptional(t => t.Role)
                .WithMany(t => t.RoleSubModuleItems)
                .HasForeignKey(d => d.RoleId);
            this.HasOptional(t => t.SubModuleItem)
                .WithMany(t => t.RoleSubModuleItems)
                .HasForeignKey(d => d.SubModuleItemId);

        }
    }
}
