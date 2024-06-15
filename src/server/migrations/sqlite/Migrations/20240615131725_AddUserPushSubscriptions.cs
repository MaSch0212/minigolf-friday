using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPushSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_push_subscriptions",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<long>(type: "INTEGER", nullable: false),
                    lang = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    endpoint = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    p256dh = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    auth = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_push_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_push_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_push_subscriptions_endpoint",
                table: "user_push_subscriptions",
                column: "endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_push_subscriptions_user_id",
                table: "user_push_subscriptions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_push_subscriptions");
        }
    }
}
