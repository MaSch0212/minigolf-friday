using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixWrongTimeslotIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_timeslots_event_id",
                table: "event_timeslots"
            );

            migrationBuilder.DropIndex(name: "IX_event_timeslots_time", table: "event_timeslots");

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslots_event_id_time",
                table: "event_timeslots",
                columns: new[] { "event_id", "time" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_event_timeslots_event_id_time",
                table: "event_timeslots"
            );

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslots_event_id",
                table: "event_timeslots",
                column: "event_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_event_timeslots_time",
                table: "event_timeslots",
                column: "time",
                unique: true
            );
        }
    }
}
