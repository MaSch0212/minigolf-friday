﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinigolfFriday.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullableLoginToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "active", table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "login_token",
                table: "users",
                type: "TEXT",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32
            );

            migrationBuilder.AlterColumn<string>(
                name: "alias",
                table: "users",
                type: "TEXT",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 150
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "login_token",
                table: "users",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 32,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "alias",
                table: "users",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 150,
                oldNullable: true
            );

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: true
            );
        }
    }
}
