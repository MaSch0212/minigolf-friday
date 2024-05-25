using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MinigolfFriday.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    registration_deadline = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "maps",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    login_token = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    alias = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_timeslots",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    time = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    is_fallback_allowed = table.Column<bool>(type: "INTEGER", nullable: false),
                    event_id = table.Column<long>(type: "INTEGER", nullable: false),
                    map_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_timeslots", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_timeslots_events_event_id",
                        column: x => x.event_id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_timeslots_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_to_avoided_users",
                columns: table => new
                {
                    avoided_user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_to_avoided_users", x => new { x.avoided_user_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_users_to_avoided_users_users_avoided_user_id",
                        column: x => x.avoided_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_to_avoided_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_to_preferred_users",
                columns: table => new
                {
                    preferred_user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_to_preferred_users", x => new { x.preferred_user_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_users_to_preferred_users_users_preferred_user_id",
                        column: x => x.preferred_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_to_preferred_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_to_roles",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_to_roles", x => new { x.role_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_users_to_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_to_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_instance_preconfigurations",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    event_timeslot_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_instance_preconfigurations", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_instance_preconfigurations_event_timeslots_event_timeslot_id",
                        column: x => x.event_timeslot_id,
                        principalTable: "event_timeslots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_instances",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    group_code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    timeslot_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_instances", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_instances_event_timeslots_timeslot_id",
                        column: x => x.timeslot_id,
                        principalTable: "event_timeslots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_timeslot_registration",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    event_timeslot_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    fallback_event_timeslot_id = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_timeslot_registration", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_timeslot_registration_event_timeslots_event_timeslot_id",
                        column: x => x.event_timeslot_id,
                        principalTable: "event_timeslots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_timeslot_registration_event_timeslots_fallback_event_timeslot_id",
                        column: x => x.fallback_event_timeslot_id,
                        principalTable: "event_timeslots",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_event_timeslot_registration_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_to_event_instance_preconfigurations",
                columns: table => new
                {
                    event_instance_preconfiguration_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_to_event_instance_preconfigurations", x => new { x.event_instance_preconfiguration_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_users_to_event_instance_preconfigurations_event_instance_preconfigurations_event_instance_preconfiguration_id",
                        column: x => x.event_instance_preconfiguration_id,
                        principalTable: "event_instance_preconfigurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_to_event_instance_preconfigurations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_instances_to_users",
                columns: table => new
                {
                    event_instance_id = table.Column<long>(type: "INTEGER", nullable: false),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_instances_to_users", x => new { x.event_instance_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_event_instances_to_users_event_instances_event_instance_id",
                        column: x => x.event_instance_id,
                        principalTable: "event_instances",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_event_instances_to_users_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 0, "Player" },
                    { 1, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_event_instance_preconfigurations_event_timeslot_id",
                table: "event_instance_preconfigurations",
                column: "event_timeslot_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_instances_timeslot_id",
                table: "event_instances",
                column: "timeslot_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_instances_to_users_user_id",
                table: "event_instances_to_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslot_registration_event_timeslot_id",
                table: "event_timeslot_registration",
                column: "event_timeslot_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslot_registration_fallback_event_timeslot_id",
                table: "event_timeslot_registration",
                column: "fallback_event_timeslot_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslot_registration_user_id",
                table: "event_timeslot_registration",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslots_event_id",
                table: "event_timeslots",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslots_map_id",
                table: "event_timeslots",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslots_time",
                table: "event_timeslots",
                column: "time",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_login_token",
                table: "users",
                column: "login_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_to_avoided_users_user_id",
                table: "users_to_avoided_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_to_event_instance_preconfigurations_user_id",
                table: "users_to_event_instance_preconfigurations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_to_preferred_users_user_id",
                table: "users_to_preferred_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_to_roles_user_id",
                table: "users_to_roles",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_instances_to_users");

            migrationBuilder.DropTable(
                name: "event_timeslot_registration");

            migrationBuilder.DropTable(
                name: "users_to_avoided_users");

            migrationBuilder.DropTable(
                name: "users_to_event_instance_preconfigurations");

            migrationBuilder.DropTable(
                name: "users_to_preferred_users");

            migrationBuilder.DropTable(
                name: "users_to_roles");

            migrationBuilder.DropTable(
                name: "event_instances");

            migrationBuilder.DropTable(
                name: "event_instance_preconfigurations");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "event_timeslots");

            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "maps");
        }
    }
}
