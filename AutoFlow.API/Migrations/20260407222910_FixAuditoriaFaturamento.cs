using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class FixAuditoriaFaturamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DesfaturadoPorUserId",
                table: "OrdemServicos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FaturadoPorUserId",
                table: "OrdemServicos",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesfaturadoPorUserId",
                table: "OrdemServicos");

            migrationBuilder.DropColumn(
                name: "FaturadoPorUserId",
                table: "OrdemServicos");
        }
    }
}
