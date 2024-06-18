using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "settings_id",
                table: "users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    enable_notifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    notify_on_event_publish = table.Column<bool>(type: "INTEGER", nullable: false),
                    notify_on_event_start = table.Column<bool>(type: "INTEGER", nullable: false),
                    notify_on_event_updated = table.Column<bool>(type: "INTEGER", nullable: false),
                    notify_on_timeslot_start = table.Column<bool>(type: "INTEGER", nullable: false),
                    seconds_to_notify_before_timeslot_start = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_settings_id",
                table: "users",
                column: "settings_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_user_settings_settings_id",
                table: "users",
                column: "settings_id",
                principalTable: "user_settings",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_user_settings_settings_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropIndex(
                name: "IX_users_settings_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "settings_id",
                table: "users");
        }
    }
}
