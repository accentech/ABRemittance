using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class NotificationSettingMap : EntityTypeConfiguration<NotificationSetting>
    {
        public NotificationSettingMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            // Table & Column Mappings
            this.ToTable("NotificationSetting");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.SubModuleItemId).HasColumnName("SubModuleItemId");
            this.Property(t => t.NotifiedEmployeeId).HasColumnName("NotifiedEmployeeId");
            this.Property(t => t.WorkflowactionId).HasColumnName("WorkflowactionId");

            // Relationships
            this.HasOptional(t => t.Employee)
                .WithMany(t => t.NotificationSettings)
                .HasForeignKey(d => d.NotifiedEmployeeId);
            this.HasOptional(t => t.SubModuleItem)
                .WithMany(t => t.NotificationSettings)
                .HasForeignKey(d => d.SubModuleItemId);
            this.HasOptional(t => t.Workflowaction)
                .WithMany(t => t.NotificationSettings)
                .HasForeignKey(d => d.WorkflowactionId);

        }
    }
}
