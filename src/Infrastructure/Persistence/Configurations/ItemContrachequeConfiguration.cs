using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento EF Core 8 para a entidade <see cref="ItemContracheque"/>.
/// Tabela: ItensContracheque.
/// </summary>
public sealed class ItemContrachequeConfiguration : IEntityTypeConfiguration<ItemContracheque>
{
    public void Configure(EntityTypeBuilder<ItemContracheque> builder)
    {
        builder.ToTable("ItensContracheque");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ContrachequeId)
               .IsRequired();

        builder.Property(i => i.Tipo)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(i => i.Descricao)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(i => i.Valor)
               .HasColumnType("numeric(10,2)")
               .IsRequired();

        // EhDesconto é propriedade calculada — não persistida
        builder.Ignore(i => i.EhDesconto);

        // Índice de leitura por contracheque
        builder.HasIndex(i => i.ContrachequeId)
               .HasDatabaseName("IX_ItensContracheque_ContrachequeId");
    }
}
