using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class Sprint1_AdequacaoBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "Placa",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "Veiculo",
                table: "OrdemServicos");

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "OrdemServicos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "OrdemServicos",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "VeiculoId",
                table: "OrdemServicos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Veiculos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicos_ClienteId",
                table: "OrdemServicos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicos_VeiculoId",
                table: "OrdemServicos",
                column: "VeiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_ClienteId",
                table: "Veiculos",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicos_Clientes_ClienteId",
                table: "OrdemServicos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdemServicos_Veiculos_VeiculoId",
                table: "OrdemServicos",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicos_Clientes_ClienteId",
                table: "OrdemServicos");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdemServicos_Veiculos_VeiculoId",
                table: "OrdemServicos");

            migrationBuilder.DropTable(
                name: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_OrdemServicos_ClienteId",
                table: "OrdemServicos");

            migrationBuilder.DropIndex(
                name: "IX_OrdemServicos_VeiculoId",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "VeiculoId",
                table: "OrdemServicos");

            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "OrdemServicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Placa",
                table: "OrdemServicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "OrdemServicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Veiculo",
                table: "OrdemServicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
