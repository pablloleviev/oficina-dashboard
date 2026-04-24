using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFaturamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataFaturamento",
                table: "OrdemServicos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Faturado",
                table: "OrdemServicos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataFaturamento",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "Faturado",
                table: "OrdemServicos");
        }
    }
}
