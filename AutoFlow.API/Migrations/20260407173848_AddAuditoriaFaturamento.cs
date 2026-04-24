using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditoriaFaturamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DesfaturadoPorUserId",
                table: "Servicos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaturadoPorUserId",
                table: "Servicos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesfaturadoPorUserId",
                table: "Servicos");

            migrationBuilder.DropColumn(
                name: "FaturadoPorUserId",
                table: "Servicos");
        }
    }
}
