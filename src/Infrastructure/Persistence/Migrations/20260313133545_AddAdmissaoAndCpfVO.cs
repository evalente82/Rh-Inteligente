using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmissaoAndCpfVO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Funcionarios_EmpresaId_Cpf",
                table: "Funcionarios");

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Funcionarios",
                type: "character varying(11)",
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.CreateTable(
                name: "Admissoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cargo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SalarioBase = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Regime = table.Column<int>(type: "integer", nullable: false),
                    DataAdmissao = table.Column<DateOnly>(type: "date", nullable: false),
                    DataDemissao = table.Column<DateOnly>(type: "date", nullable: true),
                    EndLogradouro = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EndNumero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EndBairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EndCidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EndUf = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    EndCep = table.Column<string>(type: "character(8)", fixedLength: true, maxLength: 8, nullable: false),
                    EndComplemento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admissoes_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_EmpresaId_FuncionarioId",
                table: "Admissoes",
                columns: new[] { "EmpresaId", "FuncionarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_Admissoes_FuncionarioId",
                table: "Admissoes",
                column: "FuncionarioId");

            // Índice único EmpresaId + Cpf via SQL (owned type limitation no EF Core 8)
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Funcionarios_EmpresaId_Cpf\" " +
                "ON \"Funcionarios\" (\"EmpresaId\", \"Cpf\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admissoes");

            // Remove o índice único criado via SQL no Up
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"IX_Funcionarios_EmpresaId_Cpf\";");

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Funcionarios",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(11)",
                oldMaxLength: 11);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_EmpresaId_Cpf",
                table: "Funcionarios",
                columns: new[] { "EmpresaId", "Cpf" },
                unique: true);
        }
    }
}
