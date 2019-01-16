using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ActionLogMap : EntityTypeConfiguration<ActionLog>
    {
        public ActionLogMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.AffectedRecordId)
                .HasMaxLength(50);

            this.Property(t => t.ActionCRUD)
                .HasMaxLength(50);

            this.Property(t => t.Entity)
                .HasMaxLength(50);

            this.Property(t => t.IPAddress)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("ActionLog");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Who).HasColumnName("Who");
            this.Property(t => t.When).HasColumnName("When");
            this.Property(t => t.AffectedRecordId).HasColumnName("AffectedRecordId");
            this.Property(t => t.What).HasColumnName("What");
            this.Property(t => t.ActionCRUD).HasColumnName("ActionCRUD");
            this.Property(t => t.Entity).HasColumnName("Entity");
            this.Property(t => t.IPAddress).HasColumnName("IPAddress");

            // Relationships
            this.HasOptional(t => t.BusinessUser)
                .WithMany(t => t.ActionLogs)
                .HasForeignKey(d => d.Who);

        }
    }
}
