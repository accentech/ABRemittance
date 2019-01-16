using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class WorkflowactionMap : EntityTypeConfiguration<Workflowaction>
    {
        public WorkflowactionMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Workflowaction");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
        }
    }
}
