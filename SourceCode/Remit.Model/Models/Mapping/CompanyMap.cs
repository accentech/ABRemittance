using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace Remit.Model.Models.Mapping
{
    public class CompanyMap : EntityTypeConfiguration<Company>
    {
        public CompanyMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Name)
                .HasMaxLength(50);

            this.Property(t => t.Phone)
                .HasMaxLength(50);

            this.Property(t => t.Fax)
                .HasMaxLength(50);

            this.Property(t => t.Email)
                .HasMaxLength(50);

            this.Property(t => t.ContactPerson)
                .HasMaxLength(50);

            this.Property(t => t.LogoName)
                .HasMaxLength(100);

            this.Property(t => t.CompanyAddress)
                .HasMaxLength(1000);

            // Table & Column Mappings
            this.ToTable("Company");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Phone).HasColumnName("Phone");
            this.Property(t => t.Fax).HasColumnName("Fax");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.ContactPerson).HasColumnName("ContactPerson");
            this.Property(t => t.LogoName).HasColumnName("LogoName");
            this.Property(t => t.CompanyUrl).HasColumnName("CompanyUrl");
            this.Property(t => t.BaseCurrency).HasColumnName("BaseCurrency");
            this.Property(t => t.LocalCurrency).HasColumnName("LocalCurrency");
            this.Property(t => t.CompanyAddress).HasColumnName("CompanyAddress");
            this.Property(t => t.CityId).HasColumnName("CityId");

            // Relationships
            this.HasOptional(t => t.Currency)
                .WithMany(t => t.Companies)
                .HasForeignKey(d => d.BaseCurrency);
            this.HasOptional(t => t.Currency1)
                .WithMany(t => t.Companies1)
                .HasForeignKey(d => d.LocalCurrency);

        }
    }
}
