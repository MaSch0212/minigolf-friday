using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.MsSql.Migrations
{
    /// <inheritdoc />
    public partial class EventInstanceEditorLock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "user_id_editing_instances",
                table: "events",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_user_id_editing_instances",
                table: "events",
                column: "user_id_editing_instances");

            migrationBuilder.AddForeignKey(
                name: "FK_events_users_user_id_editing_instances",
                table: "events",
                column: "user_id_editing_instances",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_events_users_user_id_editing_instances",
                table: "events");

            migrationBuilder.DropIndex(
                name: "IX_events_user_id_editing_instances",
                table: "events");

            migrationBuilder.DropColumn(
                name: "user_id_editing_instances",
                table: "events");
        }
    }
}
