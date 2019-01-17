using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class WorkflowactionSettingMap : EntityTypeConfiguration<WorkflowactionSetting>
    {
        public WorkflowactionSettingMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("WorkflowactionSetting");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.SubMouduleItemId).HasColumnName("SubMouduleItemId");
            this.Property(t => t.EmployeeId).HasColumnName("EmployeeId");
            this.Property(t => t.WorkflowactionId).HasColumnName("WorkflowactionId");

            // Relationships
            this.HasOptional(t => t.Employee)
                .WithMany(t => t.WorkflowactionSettings)
                .HasForeignKey(d => d.EmployeeId);
            this.HasOptional(t => t.SubModuleItem)
                .WithMany(t => t.WorkflowactionSettings)
                .HasForeignKey(d => d.SubMouduleItemId);
            this.HasOptional(t => t.Workflowaction)
                .WithMany(t => t.WorkflowactionSettings)
                .HasForeignKey(d => d.WorkflowactionId);

        }
    }
}
