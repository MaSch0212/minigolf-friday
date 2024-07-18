using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.MsSql.Migrations
{
    /// <inheritdoc />
    public partial class AddEventExternalUri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_uri",
                table: "events",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_uri",
                table: "events");
        }
    }
}
