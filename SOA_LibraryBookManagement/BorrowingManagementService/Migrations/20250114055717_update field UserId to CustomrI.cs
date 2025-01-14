using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BorrowingManagementService.Migrations
{
    /// <inheritdoc />
    public partial class updatefieldUserIdtoCustomrI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Borrowings",
                newName: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Borrowings",
                newName: "UserId");
        }
    }
}
