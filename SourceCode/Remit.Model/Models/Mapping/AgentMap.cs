using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class AgentMap : EntityTypeConfiguration<Agent>
    {
        public AgentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.AgentName)
                .HasMaxLength(50);

            this.Property(t => t.AgentAddress)
                .HasMaxLength(100);

            this.Property(t => t.ContactPhone)
                .HasMaxLength(50);

            this.Property(t => t.Email)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("Agent");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.AgentName).HasColumnName("AgentName");
            this.Property(t => t.DateOfOperationStart).HasColumnName("DateOfOperationStart");
            this.Property(t => t.AgentAddress).HasColumnName("AgentAddress");
            this.Property(t => t.ContactPhone).HasColumnName("ContactPhone");
            this.Property(t => t.Email).HasColumnName("Email");
        }
    }
}
