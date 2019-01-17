using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class DesignationMap : EntityTypeConfiguration<Designation>
    {
        public DesignationMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Designation");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.DepartmentId).HasColumnName("DepartmentId");
            this.Property(t => t.Name).HasColumnName("Name");

            // Relationships
            this.HasRequired(t => t.Department)
                .WithMany(t => t.Designations)
                .HasForeignKey(d => d.DepartmentId);

        }
    }
}
