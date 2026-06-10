using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormHub.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enum_ApplicationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enum_ApplicationType", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Enum_ApplicationType",
                columns: new[] { "Id", "Name", "NameEn" },
                values: new object[,]
                {
                    { 1, "Miejsce w akademiku", "Dormitory place" },
                    { 2, "Zmiana pokoju", "Room change" },
                    { 3, "Przedłużenie na wakacje", "Summer extension" },
                    { 4, "Miejsce w nowym roku", "Next academic year" },
                    { 5, "Wymeldowanie", "Check-out" }
                });

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Applications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreferredEndDate",
                table: "Applications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreferredStartDate",
                table: "Applications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecondRoomTypeId",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThirdRoomTypeId",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeId",
                table: "Applications",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_SecondRoomTypeId",
                table: "Applications",
                column: "SecondRoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ThirdRoomTypeId",
                table: "Applications",
                column: "ThirdRoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TypeId",
                table: "Applications",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Enum_ApplicationType_TypeId",
                table: "Applications",
                column: "TypeId",
                principalTable: "Enum_ApplicationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_RoomTypes_SecondRoomTypeId",
                table: "Applications",
                column: "SecondRoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_RoomTypes_ThirdRoomTypeId",
                table: "Applications",
                column: "ThirdRoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Enum_ApplicationType_TypeId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_RoomTypes_SecondRoomTypeId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_RoomTypes_ThirdRoomTypeId",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "Enum_ApplicationType");

            migrationBuilder.DropIndex(
                name: "IX_Applications_SecondRoomTypeId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ThirdRoomTypeId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_TypeId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PreferredEndDate",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "PreferredStartDate",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "SecondRoomTypeId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ThirdRoomTypeId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Applications");
        }
    }
}
