using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContracheque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracheques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechamentoFolhaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Competencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SalarioBruto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    TotalDescontos = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    FgtsPatronal = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    GeradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracheques", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItensContracheque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContrachequeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensContracheque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensContracheque_Contracheques_ContrachequeId",
                        column: x => x.ContrachequeId,
                        principalTable: "Contracheques",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracheques_FechamentoFolhaId",
                table: "Contracheques",
                column: "FechamentoFolhaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracheques_FuncionarioId_FechamentoFolhaId",
                table: "Contracheques",
                columns: new[] { "FuncionarioId", "FechamentoFolhaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensContracheque_ContrachequeId",
                table: "ItensContracheque",
                column: "ContrachequeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensContracheque");

            migrationBuilder.DropTable(
                name: "Contracheques");
        }
    }
}
