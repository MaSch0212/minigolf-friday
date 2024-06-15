using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class MapNotRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_timeslots_maps_map_id",
                table: "event_timeslots");

            migrationBuilder.AlterColumn<long>(
                name: "map_id",
                table: "event_timeslots",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_event_timeslots_maps_map_id",
                table: "event_timeslots",
                column: "map_id",
                principalTable: "maps",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_timeslots_maps_map_id",
                table: "event_timeslots");

            migrationBuilder.AlterColumn<long>(
                name: "map_id",
                table: "event_timeslots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_event_timeslots_maps_map_id",
                table: "event_timeslots",
                column: "map_id",
                principalTable: "maps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
