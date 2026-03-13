using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF Core 8 da entidade Admissao.
/// Endereco (Value Object) é mapeado como owned entity inline na tabela Admissoes.
/// </summary>
internal sealed class AdmissaoConfiguration : IEntityTypeConfiguration<Admissao>
{
    public void Configure(EntityTypeBuilder<Admissao> builder)
    {
        builder.ToTable("Admissoes");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmpresaId)
            .IsRequired();

        builder.Property(a => a.FuncionarioId)
            .IsRequired();

        builder.Property(a => a.Cargo)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.SalarioBase)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(a => a.Regime)
            .IsRequired();

        builder.Property(a => a.DataAdmissao)
            .IsRequired();

        builder.Property(a => a.DataDemissao)
            .IsRequired(false);

        // ─── Endereco (Value Object → Owned Entity) ───────────────────────────
        builder.OwnsOne(a => a.EnderecoResidencial, end =>
        {
            end.Property(e => e.Logradouro)
                .HasColumnName("EndLogradouro")
                .IsRequired()
                .HasMaxLength(300);

            end.Property(e => e.Numero)
                .HasColumnName("EndNumero")
                .IsRequired()
                .HasMaxLength(20);

            end.Property(e => e.Bairro)
                .HasColumnName("EndBairro")
                .IsRequired()
                .HasMaxLength(100);

            end.Property(e => e.Cidade)
                .HasColumnName("EndCidade")
                .IsRequired()
                .HasMaxLength(100);

            end.Property(e => e.Uf)
                .HasColumnName("EndUf")
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength();

            end.Property(e => e.Cep)
                .HasColumnName("EndCep")
                .IsRequired()
                .HasMaxLength(8)
                .IsFixedLength();

            end.Property(e => e.Complemento)
                .HasColumnName("EndComplemento")
                .IsRequired(false)
                .HasMaxLength(200);
        });

        // ─── Índice de busca rápida por FuncionarioId + EmpresaId ─────────────
        builder.HasIndex(a => new { a.EmpresaId, a.FuncionarioId })
            .HasDatabaseName("IX_Admissoes_EmpresaId_FuncionarioId");
    }
}
