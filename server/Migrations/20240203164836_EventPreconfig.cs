using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations
{
    /// <inheritdoc />
    public partial class EventPreconfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventInstancePreconfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventTimeslotId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInstancePreconfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventInstancePreconfigurations_EventTimeslots_EventTimeslotId",
                        column: x => x.EventTimeslotId,
                        principalTable: "EventTimeslots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventInstancePreconfigurationEntityUserEntity",
                columns: table => new
                {
                    EventInstancePreconfigurationsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayersId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInstancePreconfigurationEntityUserEntity", x => new { x.EventInstancePreconfigurationsId, x.PlayersId });
                    table.ForeignKey(
                        name: "FK_EventInstancePreconfigurationEntityUserEntity_EventInstancePreconfigurations_EventInstancePreconfigurationsId",
                        column: x => x.EventInstancePreconfigurationsId,
                        principalTable: "EventInstancePreconfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventInstancePreconfigurationEntityUserEntity_Users_PlayersId",
                        column: x => x.PlayersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventInstancePreconfigurationEntityUserEntity_PlayersId",
                table: "EventInstancePreconfigurationEntityUserEntity",
                column: "PlayersId");

            migrationBuilder.CreateIndex(
                name: "IX_EventInstancePreconfigurations_EventTimeslotId",
                table: "EventInstancePreconfigurations",
                column: "EventTimeslotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventInstancePreconfigurationEntityUserEntity");

            migrationBuilder.DropTable(
                name: "EventInstancePreconfigurations");
        }
    }
}
