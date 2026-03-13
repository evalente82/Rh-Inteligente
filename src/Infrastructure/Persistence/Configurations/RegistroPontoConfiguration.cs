using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF Core 8 da entidade RegistroPonto para o Postgres.
/// </summary>
internal sealed class RegistroPontoConfiguration : IEntityTypeConfiguration<RegistroPonto>
{
    public void Configure(EntityTypeBuilder<RegistroPonto> builder)
    {
        builder.ToTable("RegistrosPonto");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.EmpresaId)
            .IsRequired();

        builder.Property(r => r.FuncionarioId)
            .IsRequired();

        builder.Property(r => r.DataHoraBatida)
            .IsRequired();

        builder.Property(r => r.TipoBatida)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.Origem)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.LancamentoManual)
            .IsRequired();

        builder.Property(r => r.Justificativa)
            .HasMaxLength(500)
            .IsRequired(false);

        // Índice de performance: busca por funcionário + período (uso frequente nos Use Cases)
        builder.HasIndex(r => new { r.EmpresaId, r.FuncionarioId, r.DataHoraBatida })
            .HasDatabaseName("IX_RegistrosPonto_EmpresaId_FuncionarioId_DataHora");
    }
}
