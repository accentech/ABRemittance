using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class ExchangeHouseMap : EntityTypeConfiguration<ExchangeHouse>
    {
        public ExchangeHouseMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(50);

            this.Property(t => t.ContactAddress)
                .HasMaxLength(200);

            this.Property(t => t.ContavtPhone)
                .HasMaxLength(50);

            this.Property(t => t.Fax)
                .HasMaxLength(50);

            this.Property(t => t.CountryOfOrigin)
                .IsFixedLength()
                .HasMaxLength(10);

            this.Property(t => t.DateOfBusinessWithBank)
                .HasMaxLength(50);

            this.Property(t => t.BankGuranteeDescription)
                .HasMaxLength(50);

            this.Property(t => t.CurrentStatus)
                .HasMaxLength(50);

            this.Property(t => t.RemittanceTransactionMechanism)
                .HasMaxLength(50);

            this.Property(t => t.ExHCode)
                .HasMaxLength(50);

            // Table & Column Mappings
            this.ToTable("ExchangeHouse");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.ContactAddress).HasColumnName("ContactAddress");
            this.Property(t => t.ContavtPhone).HasColumnName("ContavtPhone");
            this.Property(t => t.Fax).HasColumnName("Fax");
            this.Property(t => t.CountryOfOrigin).HasColumnName("CountryOfOrigin");
            this.Property(t => t.DateOfBusinessWithBank).HasColumnName("DateOfBusinessWithBank");
            this.Property(t => t.OpenHours).HasColumnName("OpenHours");
            this.Property(t => t.CloseHours).HasColumnName("CloseHours");
            this.Property(t => t.BankGuranteeAmount).HasColumnName("BankGuranteeAmount");
            this.Property(t => t.BankGuranteeExpiryDate).HasColumnName("BankGuranteeExpiryDate");
            this.Property(t => t.BankGuranteeDescription).HasColumnName("BankGuranteeDescription");
            this.Property(t => t.MinimumBalance).HasColumnName("MinimumBalance");
            this.Property(t => t.ExchangeHouseLicenseExpiryDate).HasColumnName("ExchangeHouseLicenseExpiryDate");
            this.Property(t => t.BangladeshBankApprovalDate).HasColumnName("BangladeshBankApprovalDate");
            this.Property(t => t.AMLQuestionaireReceiveDate).HasColumnName("AMLQuestionaireReceiveDate");
            this.Property(t => t.CurrentStatus).HasColumnName("CurrentStatus");
            this.Property(t => t.RemittanceTransactionMechanism).HasColumnName("RemittanceTransactionMechanism");
            this.Property(t => t.ExHCode).HasColumnName("ExHCode");
        }
    }
}
