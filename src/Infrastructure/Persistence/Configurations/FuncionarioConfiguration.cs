using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF Core 8 da entidade Funcionario para o Postgres.
/// TurnoContratual (Value Object) é mapeado como owned entity (colunas inline na mesma tabela).
/// </summary>
internal sealed class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("Funcionarios");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.EmpresaId)
            .IsRequired();

        builder.Property(f => f.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Cpf)
            .IsRequired()
            .HasMaxLength(14);

        builder.Property(f => f.Matricula)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(f => new { f.EmpresaId, f.Matricula })
            .IsUnique()
            .HasDatabaseName("IX_Funcionarios_EmpresaId_Matricula");

        builder.HasIndex(f => new { f.EmpresaId, f.Cpf })
            .IsUnique()
            .HasDatabaseName("IX_Funcionarios_EmpresaId_Cpf");

        builder.Property(f => f.DataAdmissao)
            .IsRequired();

        builder.Property(f => f.DataDemissao)
            .IsRequired(false);

        // ─── TurnoContratual (Value Object → Owned Entity) ────────────────────
        // Colunas armazenadas inline na tabela Funcionarios sem tabela separada
        builder.OwnsOne(f => f.TurnoContratual, turno =>
        {
            turno.Property(t => t.HoraEntrada)
                .HasColumnName("TurnoHoraEntrada")
                .IsRequired();

            turno.Property(t => t.HoraSaida)
                .HasColumnName("TurnoHoraSaida")
                .IsRequired();

            turno.Property(t => t.DuracaoIntervalo)
                .HasColumnName("TurnoDuracaoIntervalo")
                .IsRequired();
        });

        // ─── Navegação: Registros de Ponto ────────────────────────────────────
        builder.HasMany(f => f.RegistrosPonto)
            .WithOne()
            .HasForeignKey(r => r.FuncionarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
