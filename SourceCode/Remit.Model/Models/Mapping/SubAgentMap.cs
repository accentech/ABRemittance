using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class SubAgentMap : EntityTypeConfiguration<SubAgent>
    {
        public SubAgentMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.LocationName)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("SubAgent");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.LocationName).HasColumnName("LocationName");
            this.Property(t => t.AgentId).HasColumnName("AgentId");
        }
    }
}
