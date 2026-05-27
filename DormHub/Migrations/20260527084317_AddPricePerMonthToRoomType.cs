using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormHub.Migrations
{
    /// <inheritdoc />
    public partial class AddPricePerMonthToRoomType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerMonth",
                table: "RoomTypes",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerMonth",
                table: "RoomTypes");
        }
    }
}
