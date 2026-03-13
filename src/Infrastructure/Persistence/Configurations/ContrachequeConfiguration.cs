using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF Core 8 para a entidade <see cref="Contracheque"/>.
/// Tabela: Contracheques | multi-tenant via EmpresaId (Global Query Filter no DbContext).
/// </summary>
public sealed class ContrachequeConfiguration : IEntityTypeConfiguration<Contracheque>
{
    public void Configure(EntityTypeBuilder<Contracheque> builder)
    {
        builder.ToTable("Contracheques");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.EmpresaId)
               .IsRequired();

        builder.Property(c => c.FechamentoFolhaId)
               .IsRequired();

        builder.Property(c => c.FuncionarioId)
               .IsRequired();

        builder.Property(c => c.Competencia)
               .HasMaxLength(7)   // "MM/YYYY"
               .IsRequired();

        builder.Property(c => c.SalarioBruto)
               .HasColumnType("numeric(10,2)")
               .IsRequired();

        builder.Property(c => c.TotalDescontos)
               .HasColumnType("numeric(10,2)")
               .IsRequired();

        builder.Property(c => c.FgtsPatronal)
               .HasColumnType("numeric(10,2)")
               .IsRequired();

        builder.Property(c => c.GeradoEm)
               .IsRequired();

        // SalarioLiquido é calculado — não persistido (computed no domínio)
        builder.Ignore(c => c.SalarioLiquido);

        // Relacionamento 1:N com ItemContracheque
        // UsePropertyAccessMode(Field) instrui o EF Core 8 a usar o backing field _itens
        // em vez da propriedade pública IReadOnlyCollection<Itens>.
        builder.HasMany(c => c.Itens)
               .WithOne(i => i.Contracheque)
               .HasForeignKey(i => i.ContrachequeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Itens)
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Índice único: um contracheque por funcionário por fechamento
        builder.HasIndex(c => new { c.FuncionarioId, c.FechamentoFolhaId })
               .IsUnique()
               .HasDatabaseName("IX_Contracheques_FuncionarioId_FechamentoFolhaId");

        // Índice de leitura por fechamento (listagem)
        builder.HasIndex(c => c.FechamentoFolhaId)
               .HasDatabaseName("IX_Contracheques_FechamentoFolhaId");
    }
}
