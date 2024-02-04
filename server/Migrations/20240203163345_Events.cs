using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations
{
    /// <inheritdoc />
    public partial class Events : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerEntityPlayerEntity");

            migrationBuilder.DropTable(
                name: "PlayerEntityPlayerEntity1");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserEntityUserEntity",
                columns: table => new
                {
                    AvoidId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AvoidedById = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntityUserEntity", x => new { x.AvoidId, x.AvoidedById });
                    table.ForeignKey(
                        name: "FK_UserEntityUserEntity_Users_AvoidId",
                        column: x => x.AvoidId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEntityUserEntity_Users_AvoidedById",
                        column: x => x.AvoidedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEntityUserEntity1",
                columns: table => new
                {
                    PreferId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PreferredById = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntityUserEntity1", x => new { x.PreferId, x.PreferredById });
                    table.ForeignKey(
                        name: "FK_UserEntityUserEntity1_Users_PreferId",
                        column: x => x.PreferId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEntityUserEntity1_Users_PreferredById",
                        column: x => x.PreferredById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventTimeslots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Time = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    MapId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsFallbackAllowed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTimeslots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTimeslots_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventTimeslots_MinigolfMaps_MapId",
                        column: x => x.MapId,
                        principalTable: "MinigolfMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupCode = table.Column<string>(type: "TEXT", nullable: false),
                    EventTimeslotId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventInstances_EventTimeslots_EventTimeslotId",
                        column: x => x.EventTimeslotId,
                        principalTable: "EventTimeslots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventPlayerRegistration",
                columns: table => new
                {
                    EventTimeslotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FallbackEventTimeslotId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPlayerRegistration", x => new { x.EventTimeslotId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_EventPlayerRegistration_EventTimeslots_EventTimeslotId",
                        column: x => x.EventTimeslotId,
                        principalTable: "EventTimeslots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventPlayerRegistration_EventTimeslots_FallbackEventTimeslotId",
                        column: x => x.FallbackEventTimeslotId,
                        principalTable: "EventTimeslots",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventPlayerRegistration_Users_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventInstanceEntityUserEntity",
                columns: table => new
                {
                    EventInstancesId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayersId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventInstanceEntityUserEntity", x => new { x.EventInstancesId, x.PlayersId });
                    table.ForeignKey(
                        name: "FK_EventInstanceEntityUserEntity_EventInstances_EventInstancesId",
                        column: x => x.EventInstancesId,
                        principalTable: "EventInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventInstanceEntityUserEntity_Users_PlayersId",
                        column: x => x.PlayersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventInstanceEntityUserEntity_PlayersId",
                table: "EventInstanceEntityUserEntity",
                column: "PlayersId");

            migrationBuilder.CreateIndex(
                name: "IX_EventInstances_EventTimeslotId",
                table: "EventInstances",
                column: "EventTimeslotId");

            migrationBuilder.CreateIndex(
                name: "IX_EventPlayerRegistration_FallbackEventTimeslotId",
                table: "EventPlayerRegistration",
                column: "FallbackEventTimeslotId");

            migrationBuilder.CreateIndex(
                name: "IX_EventPlayerRegistration_PlayerId",
                table: "EventPlayerRegistration",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTimeslots_EventId_Time",
                table: "EventTimeslots",
                columns: new[] { "EventId", "Time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventTimeslots_MapId",
                table: "EventTimeslots",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntityUserEntity_AvoidedById",
                table: "UserEntityUserEntity",
                column: "AvoidedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntityUserEntity1_PreferredById",
                table: "UserEntityUserEntity1",
                column: "PreferredById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventInstanceEntityUserEntity");

            migrationBuilder.DropTable(
                name: "EventPlayerRegistration");

            migrationBuilder.DropTable(
                name: "UserEntityUserEntity");

            migrationBuilder.DropTable(
                name: "UserEntityUserEntity1");

            migrationBuilder.DropTable(
                name: "EventInstances");

            migrationBuilder.DropTable(
                name: "EventTimeslots");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Alias = table.Column<string>(type: "TEXT", nullable: true),
                    FacebookId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
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
    }
}
