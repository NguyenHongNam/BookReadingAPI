using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookReadingService.Migrations
{
    public partial class addCmtStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Ratings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Ratings");
        }
    }
}
