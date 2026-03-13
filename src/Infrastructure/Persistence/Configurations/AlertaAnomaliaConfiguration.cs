using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF Core 8 da entidade AlertaAnomalia para o Postgres.
/// </summary>
internal sealed class AlertaAnomaliaConfiguration : IEntityTypeConfiguration<AlertaAnomalia>
{
    public void Configure(EntityTypeBuilder<AlertaAnomalia> builder)
    {
        builder.ToTable("AlertasAnomalia");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmpresaId)
            .IsRequired();

        builder.Property(a => a.FuncionarioId)
            .IsRequired();

        builder.Property(a => a.TipoAnomalia)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.DataReferencia)
            .IsRequired();

        builder.Property(a => a.Descricao)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.Gravidade)
            .IsRequired();

        builder.Property(a => a.GeradoEm)
            .IsRequired();

        builder.Property(a => a.Resolvido)
            .IsRequired();

        builder.Property(a => a.ResolvidoEm)
            .IsRequired(false);

        // Índice de performance: dashboard de anomalias por empresa + funcionário + data
        builder.HasIndex(a => new { a.EmpresaId, a.FuncionarioId, a.DataReferencia })
            .HasDatabaseName("IX_AlertasAnomalia_EmpresaId_FuncionarioId_DataRef");

        // Índice para filtro por gravidade + pendentes (resolução via gestor)
        builder.HasIndex(a => new { a.EmpresaId, a.Resolvido, a.Gravidade })
            .HasDatabaseName("IX_AlertasAnomalia_EmpresaId_Resolvido_Gravidade");
    }
}
