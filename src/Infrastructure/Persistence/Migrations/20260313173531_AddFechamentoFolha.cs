using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFechamentoFolha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FechamentosFolha",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodoInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodoFim = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalHorasExtras = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    TotalDescontos = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    TotalAnomaliasCriticas = table.Column<int>(type: "integer", nullable: false),
                    RelatorioNarrativo = table.Column<string>(type: "text", nullable: true),
                    CriadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AprovadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FechamentosFolha", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FechamentosFolha_EmpresaId_PeriodoInicio_PeriodoFim",
                table: "FechamentosFolha",
                columns: new[] { "EmpresaId", "PeriodoInicio", "PeriodoFim" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FechamentosFolha");
        }
    }
}
