using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Decolei.net.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTabelaImagens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PacoteViagem_ImagemURL",
                table: "PacoteViagem");

            migrationBuilder.CreateTable(
                name: "Imagem",
                columns: table => new
                {
                    Imagem_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Imagem_Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PacoteViagem_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagem", x => x.Imagem_Id);
                    table.ForeignKey(
                        name: "FK_Imagem_PacoteViagem_PacoteViagem_Id",
                        column: x => x.PacoteViagem_Id,
                        principalTable: "PacoteViagem",
                        principalColumn: "PacoteViagem_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Imagem_PacoteViagem_Id",
                table: "Imagem",
                column: "PacoteViagem_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Imagem");

            migrationBuilder.AddColumn<string>(
                name: "PacoteViagem_ImagemURL",
                table: "PacoteViagem",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
