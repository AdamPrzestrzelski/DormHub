using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPartiallyOccupiedRoomStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Enum_RoomStatus",
                columns: new[] { "Id", "Name", "NameEn" },
                values: new object[] { 4, "Częściowo zajęty", "Partially Occupied" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Enum_RoomStatus",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
