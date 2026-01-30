using KrtBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KrtBank.Infrastructure.Mappings
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AccountHolderName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(a => a.Cpf)
                .IsRequired()
                .HasMaxLength(11);

            builder.Property(a => a.AccountStatus)
                 .HasConversion<int>()
                 .IsRequired();

            // Índice para performance e redução de custo de IOPS na nuvem
            builder.HasIndex(a => a.Cpf).IsUnique();
        }
    }
}
