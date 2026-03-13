using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.EmpresaId).IsRequired();

        // Email VO mapeado como owned type — coluna "EmailEndereco"
        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Endereco)
                .HasColumnName("EmailEndereco")
                .IsRequired()
                .HasMaxLength(200);
        });

        builder.Property(u => u.SenhaHash)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.NomeCompleto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.CriadoEm).IsRequired();
        builder.Property(u => u.Ativo).IsRequired().HasDefaultValue(true);

        builder.Property(u => u.RefreshToken).HasMaxLength(256);
        builder.Property(u => u.RefreshTokenExpiracao);

        // Índice único: um mesmo e-mail não pode existir duas vezes no mesmo tenant
        // Usamos SQL raw pois o owned type não suporta índice composto diretamente pelo fluent API
        builder.HasIndex("EmpresaId")
            .HasDatabaseName("IX_Usuarios_EmpresaId");
    }
}
