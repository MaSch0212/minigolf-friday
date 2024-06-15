using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.MsSql.Migrations
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
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

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
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
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
