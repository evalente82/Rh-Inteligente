using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomeFantasia)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Cnpj)
            .IsRequired()
            .HasMaxLength(14)
            .IsFixedLength();

        builder.Property(e => e.EmailContato)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CriadaEm)
            .IsRequired();

        builder.Property(e => e.Ativa)
            .IsRequired()
            .HasDefaultValue(true);

        // CNPJ único no sistema (não é tenant — é chave de negócio global)
        builder.HasIndex(e => e.Cnpj)
            .IsUnique();
    }
}
