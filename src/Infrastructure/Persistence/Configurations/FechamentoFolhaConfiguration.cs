using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class FechamentoFolhaConfiguration : IEntityTypeConfiguration<FechamentoFolha>
{
    public void Configure(EntityTypeBuilder<FechamentoFolha> builder)
    {
        builder.ToTable("FechamentosFolha");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.EmpresaId)
            .IsRequired();

        builder.Property(f => f.PeriodoInicio)
            .IsRequired();

        builder.Property(f => f.PeriodoFim)
            .IsRequired();

        builder.Property(f => f.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(f => f.TotalHorasExtras)
            .HasColumnType("numeric(8,2)")
            .IsRequired();

        builder.Property(f => f.TotalDescontos)
            .HasColumnType("numeric(8,2)")
            .IsRequired();

        builder.Property(f => f.TotalAnomaliasCriticas)
            .IsRequired();

        builder.Property(f => f.RelatorioNarrativo)
            .HasColumnType("text");

        builder.Property(f => f.CriadaEm)
            .IsRequired();

        builder.Property(f => f.FechadaEm);
        builder.Property(f => f.AprovadaEm);

        // Índice por tenant + período — evita duplicatas e acelera consultas
        builder.HasIndex(f => new { f.EmpresaId, f.PeriodoInicio, f.PeriodoFim })
            .IsUnique();
    }
}
