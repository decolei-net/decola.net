using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Decolei.net.Migrations
{
    /// <inheritdoc />
    public partial class NovaMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reserva_StatusPagamento",
                table: "Reserva",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reserva_StatusPagamento",
                table: "Reserva");
        }
    }
}
