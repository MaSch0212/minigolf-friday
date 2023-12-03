using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Alias = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    FacebookId = table.Column<string>(type: "TEXT", nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEntityPlayerEntity",
                columns: table => new
                {
                    AvoidId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AvoidedById = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEntityPlayerEntity", x => new { x.AvoidId, x.AvoidedById });
                    table.ForeignKey(
                        name: "FK_PlayerEntityPlayerEntity_Players_AvoidId",
                        column: x => x.AvoidId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerEntityPlayerEntity_Players_AvoidedById",
                        column: x => x.AvoidedById,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEntityPlayerEntity1",
                columns: table => new
                {
                    PreferId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreferredById = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEntityPlayerEntity1", x => new { x.PreferId, x.PreferredById });
                    table.ForeignKey(
                        name: "FK_PlayerEntityPlayerEntity1_Players_PreferId",
                        column: x => x.PreferId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerEntityPlayerEntity1_Players_PreferredById",
                        column: x => x.PreferredById,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEntityPlayerEntity_AvoidedById",
                table: "PlayerEntityPlayerEntity",
                column: "AvoidedById");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEntityPlayerEntity1_PreferredById",
                table: "PlayerEntityPlayerEntity1",
                column: "PreferredById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerEntityPlayerEntity");

            migrationBuilder.DropTable(
                name: "PlayerEntityPlayerEntity1");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
