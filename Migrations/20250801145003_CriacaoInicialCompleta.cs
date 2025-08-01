using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Decolei.net.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicialCompleta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Usuario_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario_Documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Usuario_Perfil = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Usuario_NomeCompleto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Usuario_LoginName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Usuario_LoginName_Normalizado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Usuario_Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Usuario_Email_Normalizado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    Usuario_Senha = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Usuario_Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Usuario_Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Usuario_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuario",
                        principalColumn: "Usuario_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Usuario_UserId",
                        column: x => x.UserId,
                        principalTable: "Usuario",
                        principalColumn: "Usuario_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PacoteViagem",
                columns: table => new
                {
                    PacoteViagem_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PacoteViagem_Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PacoteViagem_Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PacoteViagem_Destino = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PacoteViagem_Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PacoteViagem_DataInicio = table.Column<DateTime>(type: "datetime", nullable: true),
                    PacoteViagem_DataFim = table.Column<DateTime>(type: "datetime", nullable: true),
                    Usuario_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PacoteViagem", x => x.PacoteViagem_Id);
                    table.ForeignKey(
                        name: "PacoteViagem_Usuario_FK",
                        column: x => x.Usuario_Id,
                        principalTable: "Usuario",
                        principalColumn: "Usuario_Id");
                });

            migrationBuilder.CreateTable(
                name: "Avaliacao",
                columns: table => new
                {
                    Avaliacao_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario_Id = table.Column<int>(type: "int", nullable: false),
                    PacoteViagem_Id = table.Column<int>(type: "int", nullable: false),
                    Avaliacao_Nota = table.Column<int>(type: "int", nullable: true),
                    Avaliacao_Comentario = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Avaliacao_Data = table.Column<DateTime>(type: "date", nullable: true),
                    Avaliacao_Aprovada = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacao", x => x.Avaliacao_Id);
                    table.ForeignKey(
                        name: "Avaliacao_PacoteViagem_FK",
                        column: x => x.PacoteViagem_Id,
                        principalTable: "PacoteViagem",
                        principalColumn: "PacoteViagem_Id");
                    table.ForeignKey(
                        name: "Avaliacao_Usuario_FK",
                        column: x => x.Usuario_Id,
                        principalTable: "Usuario",
                        principalColumn: "Usuario_Id");
                });

            migrationBuilder.CreateTable(
                name: "Imagem",
                columns: table => new
                {
                    Imagem_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Imagem_Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Imagem_IsVideo = table.Column<bool>(type: "bit", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    Reserva_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario_Id = table.Column<int>(type: "int", nullable: false),
                    PacoteViagem_Id = table.Column<int>(type: "int", nullable: false),
                    Reserva_Data = table.Column<DateTime>(type: "date", nullable: true),
                    Reserva_ValorTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Reserva_Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Reserva_Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Reserva_StatusPagamento = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserva", x => x.Reserva_Id);
                    table.ForeignKey(
                        name: "Reserva_PacoteViagem_FK",
                        column: x => x.PacoteViagem_Id,
                        principalTable: "PacoteViagem",
                        principalColumn: "PacoteViagem_Id");
                    table.ForeignKey(
                        name: "Reserva_Usuario_FK",
                        column: x => x.Usuario_Id,
                        principalTable: "Usuario",
                        principalColumn: "Usuario_Id");
                });

            migrationBuilder.CreateTable(
                name: "Pagamento",
                columns: table => new
                {
                    Pagamento_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reserva_Id = table.Column<int>(type: "int", nullable: false),
                    Pagamento_Forma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Pagamento_Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Pagamento_ComprovanteURL = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Pagamento_Data = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamento", x => x.Pagamento_Id);
                    table.ForeignKey(
                        name: "Pagamento_Reserva_FK",
                        column: x => x.Reserva_Id,
                        principalTable: "Reserva",
                        principalColumn: "Reserva_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Viajante",
                columns: table => new
                {
                    Viajante_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Reserva_Id = table.Column<int>(type: "int", nullable: false),
                    Viajante_Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Viajante_Documento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viajante", x => x.Viajante_Id);
                    table.ForeignKey(
                        name: "Viajante_Reserva_FK",
                        column: x => x.Reserva_Id,
                        principalTable: "Reserva",
                        principalColumn: "Reserva_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_PacoteViagem_Id",
                table: "Avaliacao",
                column: "PacoteViagem_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacao_Usuario_Id",
                table: "Avaliacao",
                column: "Usuario_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Imagem_PacoteViagem_Id",
                table: "Imagem",
                column: "PacoteViagem_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PacoteViagem_Usuario_Id",
                table: "PacoteViagem",
                column: "Usuario_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_Reserva_Id",
                table: "Pagamento",
                column: "Reserva_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_PacoteViagem_Id",
                table: "Reserva",
                column: "PacoteViagem_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_Usuario_Id",
                table: "Reserva",
                column: "Usuario_Id");

            migrationBuilder.CreateIndex(
                name: "Reserva_Numero_UQ",
                table: "Reserva",
                column: "Reserva_Numero",
                unique: true,
                filter: "[Reserva_Numero] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Usuario",
                column: "Usuario_Email_Normalizado");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Usuario",
                column: "Usuario_LoginName_Normalizado",
                unique: true,
                filter: "[Usuario_LoginName_Normalizado] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "Usuario_Documento_UQ",
                table: "Usuario",
                column: "Usuario_Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Usuario_Email_Normalizado_UQ",
                table: "Usuario",
                column: "Usuario_Email_Normalizado",
                unique: true,
                filter: "[Usuario_Email_Normalizado] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "Usuario_Email_UQ",
                table: "Usuario",
                column: "Usuario_Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "Usuario_LoginName_Normalizado_UQ",
                table: "Usuario",
                column: "Usuario_LoginName_Normalizado",
                unique: true,
                filter: "[Usuario_LoginName_Normalizado] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Viajante_Reserva_Id",
                table: "Viajante",
                column: "Reserva_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "Avaliacao");

            migrationBuilder.DropTable(
                name: "Imagem");

            migrationBuilder.DropTable(
                name: "Pagamento");

            migrationBuilder.DropTable(
                name: "Viajante");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "PacoteViagem");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
