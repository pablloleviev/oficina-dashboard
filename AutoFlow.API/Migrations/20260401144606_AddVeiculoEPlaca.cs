using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddVeiculoEPlaca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Placa",
                table: "Servicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Veiculo",
                table: "Servicos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Placa",
                table: "Servicos");

            migrationBuilder.DropColumn(
                name: "Veiculo",
                table: "Servicos");
        }
    }
}
