using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormHub.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MoveOutDate",
                table: "Persons",
                newName: "MoveoutDate");

            migrationBuilder.RenameColumn(
                name: "MoveInDate",
                table: "Persons",
                newName: "MoveinDate");

            migrationBuilder.AlterColumn<string>(
                name: "MoveoutDate",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoveinDate",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Persons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "MoveInDate",
                table: "Persons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MoveOutDate",
                table: "Persons",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Persons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "MoveInDate",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "MoveOutDate",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "MoveoutDate",
                table: "Persons",
                newName: "MoveOutDate");

            migrationBuilder.RenameColumn(
                name: "MoveinDate",
                table: "Persons",
                newName: "MoveInDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MoveOutDate",
                table: "Persons",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "MoveInDate",
                table: "Persons",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
