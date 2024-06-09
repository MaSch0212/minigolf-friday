﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Migrations.MsSql.Migrations
{
    /// <inheritdoc />
    public partial class HandleFallbackTimeslotDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_timeslot_registration_event_timeslots_fallback_event_timeslot_id",
                table: "event_timeslot_registration");

            migrationBuilder.AddForeignKey(
                name: "FK_event_timeslot_registration_event_timeslots_fallback_event_timeslot_id",
                table: "event_timeslot_registration",
                column: "fallback_event_timeslot_id",
                principalTable: "event_timeslots",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_event_timeslot_registration_event_timeslots_fallback_event_timeslot_id",
                table: "event_timeslot_registration");

            migrationBuilder.AddForeignKey(
                name: "FK_event_timeslot_registration_event_timeslots_fallback_event_timeslot_id",
                table: "event_timeslot_registration",
                column: "fallback_event_timeslot_id",
                principalTable: "event_timeslots",
                principalColumn: "id");
        }
    }
}
