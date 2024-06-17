using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.MsSql.Migrations
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
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserSettingsEntity",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnableNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnEventPublish = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnEventStart = table.Column<bool>(type: "bit", nullable: false),
                    NotifyOnTimeslotStart = table.Column<bool>(type: "bit", nullable: false),
                    SecondsToNotifyBeforeTimeslotStart = table.Column<long>(type: "bigint", nullable: false)
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
