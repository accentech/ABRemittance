using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ExHRemitDataMap : EntityTypeConfiguration<ExHRemitData>
    {
        public ExHRemitDataMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.DataCreationMechanism)
                .HasMaxLength(50);

            this.Property(t => t.DataParsingStatus)
                .HasMaxLength(10);

            this.Property(t => t.CommentIfFailOrPartialParsed)
                .IsFixedLength()
                .HasMaxLength(10);

            // Table & Column Mappings
            this.ToTable("ExHRemitData");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.ExchangeHouseId).HasColumnName("ExchangeHouseId");
            this.Property(t => t.DataCreationMechanism).HasColumnName("DataCreationMechanism");
            this.Property(t => t.CreatedBy).HasColumnName("CreatedBy");
            this.Property(t => t.CreationDateTime).HasColumnName("CreationDateTime");
            this.Property(t => t.DataParsingDate).HasColumnName("DataParsingDate");
            this.Property(t => t.DataParsingStatus).HasColumnName("DataParsingStatus");
            this.Property(t => t.DataParsedBy).HasColumnName("DataParsedBy");
            this.Property(t => t.CommentIfFailOrPartialParsed).HasColumnName("CommentIfFailOrPartialParsed");
        }
    }
}
