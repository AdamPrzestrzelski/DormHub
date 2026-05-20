using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DormHub.Migrations
{
    /// <inheritdoc />
    public partial class EnumEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_RoomTypes_PreferredRoomTypeId",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Payments",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "Faults",
                newName: "PriorityId");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Faults",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Applications",
                newName: "StatusId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "PreferredRoomTypeId",
                table: "Applications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "Enum_ApplicationStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enum_ApplicationStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enum_FaultCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enum_FaultCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enum_FaultPriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enum_FaultPriority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enum_PaymentStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enum_PaymentStatus", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Enum_ApplicationStatus",
                columns: new[] { "Id", "Name", "NameEn" },
                values: new object[,]
                {
                    { 1, "Oczekujacy", "Pending" },
                    { 2, "Zaakceptowany", "Accepted" },
                    { 3, "Odrzucony", "Rejected" }
                });

            migrationBuilder.InsertData(
                table: "Enum_FaultCategory",
                columns: new[] { "Id", "Name", "NameEn" },
                values: new object[,]
                {
                    { 1, "Hydraulika", "Plumbing" },
                    { 2, "Elektryka", "Electrical" },
                    { 3, "Meble", "Furniture" },
                    { 4, "Okna/Drzwi", "Windows" },
                    { 5, "Internet/TV", "Internet" },
                    { 6, "Inne", "Other" }
                });

            migrationBuilder.InsertData(
                table: "Enum_FaultPriority",
                columns: new[] { "Id", "Name", "NameEn" },
                values: new object[,]
                {
                    { 1, "Niski", "Low" },
                    { 2, "Sredni", "Medium" },
                    { 3, "Wysoki", "High" },
                    { 4, "Krytyczny", "Critical" }
                });

            migrationBuilder.InsertData(
                table: "Enum_PaymentStatus",
                columns: new[] { "Id", "Name", "NameEn" },
                values: new object[,]
                {
                    { 1, "Oczekujaca", "Pending" },
                    { 2, "Zaplacona", "Paid" },
                    { 3, "Zalegla", "Overdue" }
                });

            migrationBuilder.UpdateData(
                table: "RoomStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Dostepny");

            migrationBuilder.UpdateData(
                table: "RoomStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Zajety");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StatusId",
                table: "Payments",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Faults_CategoryId",
                table: "Faults",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Faults_PriorityId",
                table: "Faults",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StatusId",
                table: "Applications",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Enum_ApplicationStatus_StatusId",
                table: "Applications",
                column: "StatusId",
                principalTable: "Enum_ApplicationStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_RoomTypes_PreferredRoomTypeId",
                table: "Applications",
                column: "PreferredRoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Enum_FaultCategory_CategoryId",
                table: "Faults",
                column: "CategoryId",
                principalTable: "Enum_FaultCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Faults_Enum_FaultPriority_PriorityId",
                table: "Faults",
                column: "PriorityId",
                principalTable: "Enum_FaultPriority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Enum_PaymentStatus_StatusId",
                table: "Payments",
                column: "StatusId",
                principalTable: "Enum_PaymentStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Enum_ApplicationStatus_StatusId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_RoomTypes_PreferredRoomTypeId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Enum_FaultCategory_CategoryId",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_Faults_Enum_FaultPriority_PriorityId",
                table: "Faults");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Enum_PaymentStatus_StatusId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "Enum_ApplicationStatus");

            migrationBuilder.DropTable(
                name: "Enum_FaultCategory");

            migrationBuilder.DropTable(
                name: "Enum_FaultPriority");

            migrationBuilder.DropTable(
                name: "Enum_PaymentStatus");

            migrationBuilder.DropIndex(
                name: "IX_Payments_StatusId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Faults_CategoryId",
                table: "Faults");

            migrationBuilder.DropIndex(
                name: "IX_Faults_PriorityId",
                table: "Faults");

            migrationBuilder.DropIndex(
                name: "IX_Applications_StatusId",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Payments",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "PriorityId",
                table: "Faults",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Faults",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Applications",
                newName: "Status");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "PreferredRoomTypeId",
                table: "Applications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "RoomStatuses",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Dostępny");

            migrationBuilder.UpdateData(
                table: "RoomStatuses",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Zajęty");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_RoomTypes_PreferredRoomTypeId",
                table: "Applications",
                column: "PreferredRoomTypeId",
                principalTable: "RoomTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
