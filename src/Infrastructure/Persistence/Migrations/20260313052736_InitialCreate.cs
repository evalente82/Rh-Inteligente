using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasAnomalia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoAnomalia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataReferencia = table.Column<DateOnly>(type: "date", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Gravidade = table.Column<int>(type: "integer", nullable: false),
                    GeradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Resolvido = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvidoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasAnomalia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Matricula = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataAdmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataDemissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TurnoHoraEntrada = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TurnoHoraSaida = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TurnoDuracaoIntervalo = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosPonto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataHoraBatida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoBatida = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Origem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LancamentoManual = table.Column<bool>(type: "boolean", nullable: false),
                    Justificativa = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosPonto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosPonto_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasAnomalia_EmpresaId_FuncionarioId_DataRef",
                table: "AlertasAnomalia",
                columns: new[] { "EmpresaId", "FuncionarioId", "DataReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasAnomalia_EmpresaId_Resolvido_Gravidade",
                table: "AlertasAnomalia",
                columns: new[] { "EmpresaId", "Resolvido", "Gravidade" });

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_EmpresaId_Cpf",
                table: "Funcionarios",
                columns: new[] { "EmpresaId", "Cpf" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_EmpresaId_Matricula",
                table: "Funcionarios",
                columns: new[] { "EmpresaId", "Matricula" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosPonto_EmpresaId_FuncionarioId_DataHora",
                table: "RegistrosPonto",
                columns: new[] { "EmpresaId", "FuncionarioId", "DataHoraBatida" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosPonto_FuncionarioId",
                table: "RegistrosPonto",
                column: "FuncionarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasAnomalia");

            migrationBuilder.DropTable(
                name: "RegistrosPonto");

            migrationBuilder.DropTable(
                name: "Funcionarios");
        }
    }
}
