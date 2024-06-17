using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MinigolfFriday.Migrations.PostgreSql.Migrations
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnableNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnEventPublish = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnEventStart = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnTimeslotStart = table.Column<bool>(type: "boolean", nullable: false),
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
