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
                name: "UserSettingsId",
                table: "users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserSettingsEntity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EnableNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyOnEventPublish = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyOnEventStart = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyOnTimeslotStart = table.Column<bool>(type: "INTEGER", nullable: false),
                    SecondsToNotifyBeforeTimeslotStart = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettingsEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_UserSettingsId",
                table: "users",
                column: "UserSettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_UserSettingsEntity_UserSettingsId",
                table: "users",
                column: "UserSettingsId",
                principalTable: "UserSettingsEntity",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_UserSettingsEntity_UserSettingsId",
                table: "users");

            migrationBuilder.DropTable(
                name: "UserSettingsEntity");

            migrationBuilder.DropIndex(
                name: "IX_users_UserSettingsId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UserSettingsId",
                table: "users");
        }
    }
}
