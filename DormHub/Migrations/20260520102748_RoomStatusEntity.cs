using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DormHub.Migrations
{
    /// <inheritdoc />
    public partial class RoomStatusEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Rooms",
                newName: "StatusId");

            migrationBuilder.CreateTable(
                name: "RoomStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomStatuses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RoomStatuses",
                columns: new[] { "Id", "Name", "NameEn" },
                values: new object[,]
                {
                    { 1, "Dostępny", "Available" },
                    { 2, "Zajęty", "Occupied" },
                    { 3, "W remoncie", "Under Maintenance" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_StatusId",
                table: "Rooms",
                column: "StatusId");

            // Remap old enum values (0=Available→1, 1=Occupied→2, 2=UnderMaintenance→3)
            migrationBuilder.Sql("UPDATE [Rooms] SET [StatusId] = [StatusId] + 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_RoomStatuses_StatusId",
                table: "Rooms",
                column: "StatusId",
                principalTable: "RoomStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_RoomStatuses_StatusId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "RoomStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_StatusId",
                table: "Rooms");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Rooms",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Rooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
